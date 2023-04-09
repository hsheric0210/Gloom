#include "SilentProcAddr.h"
#include "util.h"

FARPROC SilentProcAddr(std::wstring libName, std::string procName)  // Intentional copy
{
	int i, libNameLen = libName.length(), procNameLen = procName.length();
	for (i = 0; i < libNameLen; i++)
		libName[i] ^= XOR_KEY;
	for (i = 0; i < procNameLen; i++)
		procName[i] = procName[i] ^ XOR_KEY;

	transform(libName.begin(), libName.end(), libName.begin(), towupper);

	// PEB
	ULONG_PTR peb;
	//https://en.wikipedia.org/wiki/Win32_Thread_Information_Block
#ifdef _WIN64
	peb = __readgsqword(0x60);
#else
	peb = __readfsdword(0x30);
#endif

	// _PEB_LDR_DATA
	auto ldrData = (ULONG_PTR)((_PPEB)peb)->pLdr;
	auto beginEntry = (ULONG_PTR)((PPEB_LDR_DATA)ldrData)->InMemoryOrderModuleList.Flink;
	ULONG_PTR entry = beginEntry;
	while (entry)
	{
		// For each all loaded modules
		auto dataEntry = (PLDR_DATA_TABLE_ENTRY)entry;
		auto moduleNameBuffer = dataEntry->BaseDllName.pBuffer;
		size_t moduleNameLength1 = min(dataEntry->BaseDllName.Length, libNameLen);
		std::wstring currentLibraryName(moduleNameBuffer, moduleNameBuffer + moduleNameLength1);
		std::transform(currentLibraryName.begin(), currentLibraryName.end(), currentLibraryName.begin(), towupper);

		if (libName == currentLibraryName)
		{
			// For each all of its exported procs
			auto base = (ULONG_PTR)dataEntry->DllBase;
			ULONG_PTR ntHeader = base + ((PIMAGE_DOS_HEADER)base)->e_lfanew;
			PIMAGE_DATA_DIRECTORY exportDirOffset = (&(((PIMAGE_NT_HEADERS)ntHeader)->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_EXPORT]));
			auto exportDir = (PIMAGE_EXPORT_DIRECTORY)(base + exportDirOffset->VirtualAddress);
			ULONG_PTR names = (base + exportDir->AddressOfNames);
			ULONG_PTR nameOrdinals = (base + exportDir->AddressOfNameOrdinals);
			DWORD count = exportDir->NumberOfNames;
			while (count--)
			{
				if (procName == (LPSTR)(base + DEREF_32(names)))
				{
					UINT_PTR addr = (base + exportDir->AddressOfFunctions);
					addr += (DEREF_16(nameOrdinals) * sizeof(DWORD));
					addr = base + DEREF_32(addr);
					return (FARPROC)addr;
				}

				names += sizeof(DWORD);
				nameOrdinals += sizeof(WORD);
			}

			return nullptr;
		}

		entry = DEREF_PTR(dataEntry);
		if (entry == beginEntry) // Because it's an cyclic doubly linked list
			break;
	}

	return nullptr;
}
