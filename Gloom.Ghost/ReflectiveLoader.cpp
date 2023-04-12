#include "pch.h"
#include "ReflectiveLoader.h"

#pragma intrinsic( _ReturnAddress )
__declspec(noinline) ULONG_PTR caller(VOID)
{
	return (ULONG_PTR)_ReturnAddress();
}

DllExport ULONG_PTR WINAPI BootstrapFunc(LPVOID parameter)
{
	// the functions we need
	MyLoadLibraryA lla = nullptr;
	MyGetProcAddress gpa = nullptr;
	MyVirtualAlloc va = nullptr;
	MyVirtualProtect vp = nullptr;
	MyVirtualLock vl = nullptr;
	MyNtFlushInstructionCache ntfic = nullptr;

#pragma region("STEP 0: calculate our images current base address")
	ULONG_PTR ibase = caller();
	while (TRUE)
	{
		if (((PIMAGE_DOS_HEADER)ibase)->e_magic == IMAGE_DOS_SIGNATURE)
		{
			UINT_PTR headerValue = ((PIMAGE_DOS_HEADER)ibase)->e_lfanew;
			if (headerValue >= sizeof(IMAGE_DOS_HEADER) && headerValue < 1024) // MZ header false positive prevention
			{
				headerValue += ibase;
				if (((PIMAGE_NT_HEADERS)headerValue)->Signature == IMAGE_NT_SIGNATURE)
					break;
			}
		}
		ibase--;
	}
#pragma endregion

#pragma region("STEP 1: process the kernels exports for the functions our loader needs...")
	//https://en.wikipedia.org/wiki/Win32_Thread_Information_Block
#ifdef _WIN64
	ULONG_PTR peb = __readgsqword(0x60);
#else
	ULONG_PTR peb = __readfsdword(0x30);
#endif

	auto ldrData = (ULONG_PTR)((_PPEB)peb)->pLdr;
	auto entry_ = (ULONG_PTR)((PPEB_LDR_DATA)ldrData)->InMemoryOrderModuleList.Flink;
	while (entry_)
	{
		auto moduleName = (ULONG_PTR)((PLDR_DATA_TABLE_ENTRY)entry_)->BaseDllName.pBuffer;
		DWORD moduleNameLength = ((PLDR_DATA_TABLE_ENTRY)entry_)->BaseDllName.Length;
		DWORD dllNameHash = 0;
		do
		{
			dllNameHash = ror(dllNameHash);
			if (DEREF_8(moduleName) >= 'a')
				dllNameHash += DEREF_8(moduleName) - 0x20;
			else
				dllNameHash += DEREF_8(moduleName);
			moduleName++;
		} while (--moduleNameLength);

		// compare the hash with that of kernel32.dll
		if (dllNameHash == KERNEL32DLL_HASH)
		{
#pragma region ("kernel32.dll - LoadLibraryA, GetProcAddress, VirtualAlloc, VirtualProtect, VirtualLock")
			auto dllBase = (ULONG_PTR)((PLDR_DATA_TABLE_ENTRY)entry_)->DllBase;
			ULONG_PTR ntHeader = dllBase + ((PIMAGE_DOS_HEADER)dllBase)->e_lfanew;
			auto exportDir = (ULONG_PTR) & ((PIMAGE_NT_HEADERS)ntHeader)->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_EXPORT];
			ULONG_PTR exportDirVA = (dllBase + ((PIMAGE_DATA_DIRECTORY)exportDir)->VirtualAddress);
			ULONG_PTR names = (dllBase + ((PIMAGE_EXPORT_DIRECTORY)exportDirVA)->AddressOfNames);
			ULONG_PTR nameOrdinals = (dllBase + ((PIMAGE_EXPORT_DIRECTORY)exportDirVA)->AddressOfNameOrdinals);

			DWORD funcCount = 5;
			while (funcCount > 0)
			{
				DWORD nameHash = hash((char *)(dllBase + DEREF_32(names)));
				if (nameHash == LOADLIBRARYA_HASH || nameHash == GETPROCADDRESS_HASH || nameHash == VIRTUALALLOC_HASH || nameHash == VIRTUALPROTECT_HASH || nameHash == VIRTUALLOCK_HASH)
				{
					UINT_PTR address = (dllBase + ((PIMAGE_EXPORT_DIRECTORY)exportDirVA)->AddressOfFunctions);
					address += (DEREF_16(nameOrdinals) * sizeof(DWORD));

					if (nameHash == LOADLIBRARYA_HASH)
						lla = (MyLoadLibraryA)(dllBase + DEREF_32(address));
					else if (nameHash == GETPROCADDRESS_HASH)
						gpa = (MyGetProcAddress)(dllBase + DEREF_32(address));
					else if (nameHash == VIRTUALALLOC_HASH)
						va = (MyVirtualAlloc)(dllBase + DEREF_32(address));
					else if (nameHash == VIRTUALPROTECT_HASH)
						vp = (MyVirtualProtect)(dllBase + DEREF_32(address));
					else if (nameHash == VIRTUALLOCK_HASH)
						vl = (MyVirtualLock)(dllBase + DEREF_32(address));

					funcCount--;
				}

				names += sizeof(DWORD);
				nameOrdinals += sizeof(WORD);
			}
#pragma endregion
		}
		else if (dllNameHash == NTDLLDLL_HASH)
		{
#pragma region("ntdll.dll - NtFlushInstructionCache")
			ULONG_PTR uiBaseAddress = (ULONG_PTR)((PLDR_DATA_TABLE_ENTRY)entry_)->DllBase;
			ULONG_PTR ntHeader = uiBaseAddress + ((PIMAGE_DOS_HEADER)uiBaseAddress)->e_lfanew;
			ULONG_PTR exportDir = (ULONG_PTR) & ((PIMAGE_NT_HEADERS)ntHeader)->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_EXPORT];
			ULONG_PTR exportDirVA = (uiBaseAddress + ((PIMAGE_DATA_DIRECTORY)exportDir)->VirtualAddress);
			ULONG_PTR names = (uiBaseAddress + ((PIMAGE_EXPORT_DIRECTORY)exportDirVA)->AddressOfNames);
			ULONG_PTR nameOrdinals = (uiBaseAddress + ((PIMAGE_EXPORT_DIRECTORY)exportDirVA)->AddressOfNameOrdinals);

			DWORD funcCount = 1;
			while (funcCount > 0)
			{
				DWORD nameHash = hash((char *)(uiBaseAddress + DEREF_32(names)));
				if (nameHash == NTFLUSHINSTRUCTIONCACHE_HASH)
				{
					ULONG_PTR uiAddressArray = (uiBaseAddress + ((PIMAGE_EXPORT_DIRECTORY)exportDirVA)->AddressOfFunctions);
					uiAddressArray += (DEREF_16(nameOrdinals) * sizeof(DWORD));
					if (nameHash == NTFLUSHINSTRUCTIONCACHE_HASH)
						ntfic = (MyNtFlushInstructionCache)(uiBaseAddress + DEREF_32(uiAddressArray));

					funcCount--;
				}

				names += sizeof(DWORD);
				nameOrdinals += sizeof(WORD);
			}
#pragma endregion
		}

		if (lla && gpa && va && vp && vl && ntfic)
			break;

		entry_ = DEREF_PTR(entry_);
	}
#pragma endregion

#pragma region("STEP 2: load our image into a new permanent location in memory...")
	UINT_PTR ntHeader = ibase + ((PIMAGE_DOS_HEADER)ibase)->e_lfanew;
	UINT_PTR buffer = (ULONG_PTR)va(NULL, ((PIMAGE_NT_HEADERS)ntHeader)->OptionalHeader.SizeOfImage, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE); //Disabled because it will cause program crash anyway.

	vl((LPVOID)buffer, ((PIMAGE_NT_HEADERS)ntHeader)->OptionalHeader.SizeOfImage);

	DWORD headerSize = ((PIMAGE_NT_HEADERS)ntHeader)->OptionalHeader.SizeOfHeaders;
	UINT_PTR localHeader = ibase;
	UINT_PTR remoteHeader = buffer;

	while (headerSize--)
		*(BYTE *)remoteHeader++ = *(BYTE *)localHeader++; // NOTE: Can't use memcpy because this code will be code-injected

#pragma endregion

#pragma region("STEP 3: load in all of our sections...")
	UINT_PTR sectionEntry = ((ULONG_PTR) & ((PIMAGE_NT_HEADERS)ntHeader)->OptionalHeader + ((PIMAGE_NT_HEADERS)ntHeader)->FileHeader.SizeOfOptionalHeader);

	WORD sectionCount = ((PIMAGE_NT_HEADERS)ntHeader)->FileHeader.NumberOfSections;
	while (sectionCount--)
	{
		UINT_PTR remoteSectionVA = (buffer + ((PIMAGE_SECTION_HEADER)sectionEntry)->VirtualAddress);
		UINT_PTR localSectionDataVA = (ibase + ((PIMAGE_SECTION_HEADER)sectionEntry)->PointerToRawData);

		DWORD sectionSize = ((PIMAGE_SECTION_HEADER)sectionEntry)->SizeOfRawData;
		while (sectionSize--)
			*(BYTE *)remoteSectionVA++ = *(BYTE *)localSectionDataVA++;

		sectionEntry += sizeof(IMAGE_SECTION_HEADER);
	}
#pragma endregion

#pragma region("STEP 4: process our images import table...")
	auto importDir = (ULONG_PTR) & ((PIMAGE_NT_HEADERS)ntHeader)->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_IMPORT];
	UINT_PTR iatEntry = (buffer + ((PIMAGE_DATA_DIRECTORY)importDir)->VirtualAddress);
	while (((PIMAGE_IMPORT_DESCRIPTOR)iatEntry)->Name)
	{
		auto moduleBase = (ULONG_PTR)lla((LPCSTR)(buffer + ((PIMAGE_IMPORT_DESCRIPTOR)iatEntry)->Name));
		UINT_PTR firstThunkVA = (buffer + ((PIMAGE_IMPORT_DESCRIPTOR)iatEntry)->OriginalFirstThunk);
		ULONG_PTR iatVA = (buffer + ((PIMAGE_IMPORT_DESCRIPTOR)iatEntry)->FirstThunk);
		while (DEREF_PTR(iatVA))
		{
			if (firstThunkVA && ((PIMAGE_THUNK_DATA)firstThunkVA)->u1.Ordinal & IMAGE_ORDINAL_FLAG)
			{
				UINT_PTR ntHeader = moduleBase + ((PIMAGE_DOS_HEADER)moduleBase)->e_lfanew;
				auto exportDir = (ULONG_PTR) & ((PIMAGE_NT_HEADERS)ntHeader)->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_EXPORT];
				UINT_PTR exportDirVA = (moduleBase + ((PIMAGE_DATA_DIRECTORY)exportDir)->VirtualAddress);
				UINT_PTR address = (moduleBase + ((PIMAGE_EXPORT_DIRECTORY)exportDirVA)->AddressOfFunctions);
				address += ((IMAGE_ORDINAL(((PIMAGE_THUNK_DATA)firstThunkVA)->u1.Ordinal) - ((PIMAGE_EXPORT_DIRECTORY)exportDirVA)->Base) * sizeof(DWORD));
				DEREF_PTR(iatVA) = (moduleBase + DEREF_32(address));
			}
			else
			{
				UINT_PTR uiValueB = (buffer + DEREF_PTR(iatVA));
				DEREF_PTR(iatVA) = (ULONG_PTR)gpa((HMODULE)moduleBase, (LPCSTR)((PIMAGE_IMPORT_BY_NAME)uiValueB)->Name);
			}

			iatVA += sizeof(ULONG_PTR);
			if (firstThunkVA)
				firstThunkVA += sizeof(ULONG_PTR);
		}

		iatEntry += sizeof(IMAGE_IMPORT_DESCRIPTOR);
	}
#pragma endregion

#pragma region("STEP 5: process all of our images relocations...")
	ibase = buffer - ((PIMAGE_NT_HEADERS)ntHeader)->OptionalHeader.ImageBase;
	UINT_PTR relocDir = (ULONG_PTR) & ((PIMAGE_NT_HEADERS)ntHeader)->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_BASERELOC];
	if (((PIMAGE_DATA_DIRECTORY)relocDir)->Size)
	{
		DWORD sizeOfBlock = ((PIMAGE_BASE_RELOCATION)relocDir)->SizeOfBlock;
		UINT_PTR remoteRelocEntry = (buffer + ((PIMAGE_DATA_DIRECTORY)relocDir)->VirtualAddress);
		while (sizeOfBlock && ((PIMAGE_BASE_RELOCATION)remoteRelocEntry)->SizeOfBlock)
		{
			UINT_PTR remoteRelocVA = (buffer + ((PIMAGE_BASE_RELOCATION)remoteRelocEntry)->VirtualAddress);
			DWORD remoteRelocBlockCount = (((PIMAGE_BASE_RELOCATION)remoteRelocEntry)->SizeOfBlock - sizeof(IMAGE_BASE_RELOCATION)) / sizeof(IMAGE_RELOC);
			UINT_PTR remoteRelocBlock = remoteRelocEntry + sizeof(IMAGE_BASE_RELOCATION);
			while (remoteRelocBlockCount--)
			{
				if (((PIMAGE_RELOC)remoteRelocBlock)->type == IMAGE_REL_BASED_DIR64)
					*(ULONG_PTR *)(remoteRelocVA + ((PIMAGE_RELOC)remoteRelocBlock)->offset) += ibase;
				else if (((PIMAGE_RELOC)remoteRelocBlock)->type == IMAGE_REL_BASED_HIGHLOW)
					*(DWORD *)(remoteRelocVA + ((PIMAGE_RELOC)remoteRelocBlock)->offset) += (DWORD)ibase;
				else if (((PIMAGE_RELOC)remoteRelocBlock)->type == IMAGE_REL_BASED_HIGH)
					*(WORD *)(remoteRelocVA + ((PIMAGE_RELOC)remoteRelocBlock)->offset) += HIWORD(ibase);
				else if (((PIMAGE_RELOC)remoteRelocBlock)->type == IMAGE_REL_BASED_LOW)
					*(WORD *)(remoteRelocVA + ((PIMAGE_RELOC)remoteRelocBlock)->offset) += LOWORD(ibase);

				remoteRelocBlock += sizeof(IMAGE_RELOC);
			}

			sizeOfBlock -= ((PIMAGE_BASE_RELOCATION)remoteRelocEntry)->SizeOfBlock;
			remoteRelocEntry = remoteRelocEntry + ((PIMAGE_BASE_RELOCATION)remoteRelocEntry)->SizeOfBlock;
		}
	}
#pragma endregion

#pragma region("STEP 6: iterate through all sections, applying protections")
	auto sectionHeaderEntry = (PIMAGE_SECTION_HEADER)((ULONG_PTR) & ((PIMAGE_NT_HEADERS)ntHeader)->OptionalHeader + ((PIMAGE_NT_HEADERS)ntHeader)->FileHeader.SizeOfOptionalHeader);
	sectionCount = ((PIMAGE_NT_HEADERS)ntHeader)->FileHeader.NumberOfSections;
	while (sectionCount--)
	{
		UINT_PTR remoteSectionVA = (buffer + sectionHeaderEntry->VirtualAddress);
		DWORD protect = 0;
		if (sectionHeaderEntry->Characteristics & IMAGE_SCN_MEM_WRITE)
			protect = PAGE_WRITECOPY;
		if (sectionHeaderEntry->Characteristics & IMAGE_SCN_MEM_READ)
			protect = PAGE_READONLY;
		if ((sectionHeaderEntry->Characteristics & IMAGE_SCN_MEM_WRITE) && (sectionHeaderEntry->Characteristics & IMAGE_SCN_MEM_READ))
			protect = PAGE_READWRITE;
		if (sectionHeaderEntry->Characteristics & IMAGE_SCN_MEM_EXECUTE)
			protect = PAGE_EXECUTE;
		if ((sectionHeaderEntry->Characteristics & IMAGE_SCN_MEM_EXECUTE) && (sectionHeaderEntry->Characteristics & IMAGE_SCN_MEM_WRITE))
			protect = PAGE_EXECUTE_WRITECOPY;
		if ((sectionHeaderEntry->Characteristics & IMAGE_SCN_MEM_EXECUTE) && (sectionHeaderEntry->Characteristics & IMAGE_SCN_MEM_READ))
			protect = PAGE_EXECUTE_READ;
		if ((sectionHeaderEntry->Characteristics & IMAGE_SCN_MEM_EXECUTE) && (sectionHeaderEntry->Characteristics & IMAGE_SCN_MEM_WRITE) && (sectionHeaderEntry->Characteristics & IMAGE_SCN_MEM_READ))
			protect = PAGE_EXECUTE_READWRITE;

		DWORD sectionSize = sectionHeaderEntry->SizeOfRawData;

		if (sectionSize)
			vp((LPVOID)remoteSectionVA, sectionSize, protect, &protect);

		sectionHeaderEntry += sizeof(IMAGE_SECTION_HEADER);
	}
#pragma endregion

#pragma region("STEP 7: call our images entry point")
	UINT_PTR entryPointVA = (buffer + ((PIMAGE_NT_HEADERS)ntHeader)->OptionalHeader.AddressOfEntryPoint);
	ntfic((HANDLE)-1, NULL, 0);
	((MyDllMain)entryPointVA)((HINSTANCE)buffer, DLL_PROCESS_ATTACH, parameter);
#pragma endregion
	return entryPointVA;
}
