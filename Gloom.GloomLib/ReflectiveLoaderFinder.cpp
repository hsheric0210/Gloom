#include "ReflectiveLoaderFinder.h"

template<typename _ntHeaderStruct>
DWORD Rva2OffsetInternal(DWORD rva, UINT_PTR baseAddress)
{
	auto ntHeader = (_ntHeaderStruct)(baseAddress + ((PIMAGE_DOS_HEADER)baseAddress)->e_lfanew);
	auto sectionHeader = (PIMAGE_SECTION_HEADER)((UINT_PTR)(&ntHeader->OptionalHeader) + ntHeader->FileHeader.SizeOfOptionalHeader);
	if (rva < sectionHeader[0].PointerToRawData)
		return rva;

	for (WORD i = 0; i < ntHeader->FileHeader.NumberOfSections; i++)
		if (rva >= sectionHeader[i].VirtualAddress && rva < (sectionHeader[i].VirtualAddress + sectionHeader[i].SizeOfRawData))
			return (rva - sectionHeader[i].VirtualAddress + sectionHeader[i].PointerToRawData);

	return 0;
}

DWORD Rva2Offset(DWORD rva, UINT_PTR baseAddress, bool is64)
{
	return is64 ? Rva2OffsetInternal<PIMAGE_NT_HEADERS64>(rva, baseAddress) : Rva2OffsetInternal<PIMAGE_NT_HEADERS32>(rva, baseAddress);
}

DWORD GetReflectiveLoaderOffset(UINT_PTR baseAddress, LPCSTR procName)
{
	UINT_PTR ntHeader = baseAddress + ((PIMAGE_DOS_HEADER)baseAddress)->e_lfanew;
	BOOL is64 = FALSE;
	UINT_PTR exportDirOffset;
	WORD magic = ((PIMAGE_NT_HEADERS)ntHeader)->OptionalHeader.Magic;
	if (magic == 0x010B) // PE32
	{
		exportDirOffset = (UINT_PTR) & ((PIMAGE_NT_HEADERS32)ntHeader)->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_EXPORT];
	}
	else if (magic == 0x020B) // PE64
	{
		is64 = TRUE;
		exportDirOffset = (UINT_PTR) & ((PIMAGE_NT_HEADERS64)ntHeader)->OptionalHeader.DataDirectory[IMAGE_DIRECTORY_ENTRY_EXPORT];
	}
	else
		return 0;

	UINT_PTR exportDirectoryOffset = baseAddress + Rva2Offset(((PIMAGE_DATA_DIRECTORY)exportDirOffset)->VirtualAddress, baseAddress, is64);
	UINT_PTR names = baseAddress + Rva2Offset(((PIMAGE_EXPORT_DIRECTORY)exportDirectoryOffset)->AddressOfNames, baseAddress, is64);
	UINT_PTR nameOrdinals = baseAddress + Rva2Offset(((PIMAGE_EXPORT_DIRECTORY)exportDirectoryOffset)->AddressOfNameOrdinals, baseAddress, is64);
	UINT_PTR address = baseAddress + Rva2Offset(((PIMAGE_EXPORT_DIRECTORY)exportDirectoryOffset)->AddressOfFunctions, baseAddress, is64);

	if (!((DWORD_PTR)procName >> 16)) // By ordinal
	{
		address += ((IMAGE_ORDINAL((DWORD)procName) - ((PIMAGE_EXPORT_DIRECTORY)exportDirectoryOffset)->Base) * sizeof(DWORD));
		return Rva2Offset(DEREF_32(address), address, is64);
	}

	DWORD count = ((PIMAGE_EXPORT_DIRECTORY)exportDirectoryOffset)->NumberOfNames;
	while (count--)
	{
		auto currentProcName = (PSTR)(baseAddress + Rva2Offset(DEREF_32(names), baseAddress, is64));
		if (strstr(currentProcName, currentProcName))
			return Rva2Offset(DEREF_32(address + (DEREF_16(nameOrdinals) * sizeof(DWORD))), baseAddress, is64);

		names += sizeof(DWORD);
		nameOrdinals += sizeof(WORD);
	}

	return 0;
}
