// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include "detours.h"

#define DllExport extern "C" __declspec(dllexport)

NtQueryDirectoryFileFunc _NtQueryDirectoryFile = nullptr;
NtQueryDirectoryFileExFunc _NtQueryDirectoryFileEx = nullptr;

BOOL state;
WCHAR FilteredPath[EXTENDED_MAX_PATH];
DWORD FilteredPID;

DllExport void Begin()
{
	state = TRUE;
}

DllExport void FilterFile(LPCWSTR path)
{
	DWORD len = lstrlenW(path);
	RtlZeroMemory(FilteredPath, EXTENDED_MAX_PATH * sizeof(WCHAR));
	memcpy(FilteredPath, path, len);
}

DllExport void FilterProcess(DWORD processId)
{
	FilteredPID = processId;
}

BOOL cmpFileName(PWCHAR FileName, ULONG FileNameLength)
{
	std::wstring fn(FileName, FileName + FileNameLength);
	std::wstring target(FilteredPath);
	return !fn.compare(target);
}

template <class fiType> void CheckAndModifyMatchingDetails(fiType FileInformation)
{
	if (!state)
		return;

	fiType current = FileInformation;
	while (current->NextEntryOffset)
	{
		fiType next = (fiType)((LPBYTE)current + current->NextEntryOffset);

		if (cmpFileName(next->FileName, next->FileNameLength))
		{
			if (next->NextEntryOffset != 0)
			{
				next = (fiType)((LPBYTE)next + next->NextEntryOffset);
				current->NextEntryOffset += next->NextEntryOffset;
			}
			else
			{
				current->NextEntryOffset = 0;
			}
		}
		else
		{
			current = next;
		}
	}
}

BOOL ModifyFileInformation(FILE_INFORMATION_CLASS FileInformationClass, PVOID FileInformation)
{
	switch ((g_FILE_INFORMATION_CLASS)FileInformationClass)
	{
		case g_FILE_INFORMATION_CLASS::FileDirectoryInformation:
			CheckAndModifyMatchingDetails<PFILE_DIRECTORY_INFORMATION>((PFILE_DIRECTORY_INFORMATION)FileInformation);
			break;
		case g_FILE_INFORMATION_CLASS::FileIdBothDirectoryInformation:
			CheckAndModifyMatchingDetails<PFILE_ID_BOTH_DIR_INFORMATION>((PFILE_ID_BOTH_DIR_INFORMATION)FileInformation);
			break;
		case g_FILE_INFORMATION_CLASS::FileBothDirectoryInformation:
			CheckAndModifyMatchingDetails<PFILE_BOTH_DIR_INFORMATION>((PFILE_BOTH_DIR_INFORMATION)FileInformation);
			break;
		case g_FILE_INFORMATION_CLASS::FileIdFullDirectoryInformation:
			CheckAndModifyMatchingDetails<PFILE_ID_FULL_DIR_INFORMATION>((PFILE_ID_FULL_DIR_INFORMATION)FileInformation);
			break;
		case g_FILE_INFORMATION_CLASS::FileFullDirectoryInformation:
			CheckAndModifyMatchingDetails<PFILE_FULL_DIR_INFORMATION>((PFILE_FULL_DIR_INFORMATION)FileInformation);
			break;
		case g_FILE_INFORMATION_CLASS::FileIdGlobalTxDirectoryInformation:
			CheckAndModifyMatchingDetails<PFILE_ID_GLOBAL_TX_DIR_INFORMATION>((PFILE_ID_GLOBAL_TX_DIR_INFORMATION)FileInformation);
			break;
		case g_FILE_INFORMATION_CLASS::FileIdExtdDirectoryInformation:
			CheckAndModifyMatchingDetails<PFILE_ID_EXTD_DIR_INFORMATION>((PFILE_ID_EXTD_DIR_INFORMATION)FileInformation);
			break;
		case g_FILE_INFORMATION_CLASS::FileIdExtdBothDirectoryInformation:
			CheckAndModifyMatchingDetails<PFILE_ID_EXTD_BOTH_DIR_INFORMATION>((PFILE_ID_EXTD_BOTH_DIR_INFORMATION)FileInformation);
			break;
		default:
			return FALSE;
	}

	return TRUE;
}

NTSTATUS NTAPI NtQueryDirectoryFile(
	HANDLE FileHandle,
	HANDLE Event,
	PIO_APC_ROUTINE ApcRoutine,
	PVOID ApcContext,
	PIO_STATUS_BLOCK	IoStatusBlock,
	PVOID FileInformation,
	ULONG Length,
	FILE_INFORMATION_CLASS FileInformationClass,
	BOOLEAN ReturnSingleEntry,
	PUNICODE_STRING FileName,
	BOOLEAN RestartScan)
{
	NTSTATUS ret = _NtQueryDirectoryFile(FileHandle, Event, ApcRoutine, ApcContext, IoStatusBlock, FileInformation, Length, FileInformationClass, ReturnSingleEntry, FileName, RestartScan);

	if (ret != STATUS_SUCCESS)
		return ret;

	ModifyFileInformation(FileInformationClass, FileInformation);

	return ret;
}

NTSTATUS NTAPI NtQueryDirectoryFileEx(
	HANDLE FileHandle,
	HANDLE Event,
	PIO_APC_ROUTINE ApcRoutine,
	PVOID ApcContext,
	PIO_STATUS_BLOCK IoStatusBlock,
	PVOID FileInformation,
	ULONG Length,
	FILE_INFORMATION_CLASS FileInformationClass,
	ULONG QueryFlags,
	PUNICODE_STRING FileName)
{
	NTSTATUS ret = _NtQueryDirectoryFileEx(FileHandle, Event, ApcRoutine, ApcContext, IoStatusBlock, FileInformation, Length, FileInformationClass, QueryFlags, FileName);

	if (ret != STATUS_SUCCESS)
		return ret;

	ModifyFileInformation(FileInformationClass, FileInformation);

	return ret;
}

BOOL APIENTRY DllMain(HINSTANCE, DWORD dwReason, LPVOID)
{
	LONG error;

	if (DetourIsHelperProcess())
		return TRUE;

	if (dwReason == DLL_PROCESS_ATTACH)
	{
		DetourRestoreAfterWith();

#ifdef DEBUG_TRACE
		printf("WinHideEx" DETOURS_STRINGIFY(DETOURS_BITS) ".dll: starting.\n");
		fflush(stdout);
#endif // DEBUG_TRACE

		DetourTransactionBegin();
		DetourUpdateThread(GetCurrentThread());

		// Get our handle to the WinNT API DLL
		HMODULE hNtDLL = GetModuleHandle(TEXT("ntdll.dll"));

		if (hNtDLL == NULL)
		{
#ifdef DEBUG_TRACE
			printf("Error detouring NtQueryDirectoryFile, could not get handle to ntdll!");
#endif // DEBUG_TRACE

			return TRUE;
		}

		_NtQueryDirectoryFile = (NtQueryDirectoryFileFunc)GetProcAddress(hNtDLL, "NtQueryDirectoryFile");
		_NtQueryDirectoryFileEx = (NtQueryDirectoryFileExFunc)GetProcAddress(hNtDLL, "NtQueryDirectoryFileEx");

		DetourAttach(&(PVOID &)_NtQueryDirectoryFile, NtQueryDirectoryFile);
		DetourAttach(&(PVOID &)_NtQueryDirectoryFileEx, NtQueryDirectoryFileEx);
		error = DetourTransactionCommit();

#ifdef DEBUG_TRACE
		if (error == NO_ERROR)
			printf("WinHideEx" DETOURS_STRINGIFY(DETOURS_BITS) ".dll: detoured!\n");
		else
			printf("WinHideEx" DETOURS_STRINGIFY(DETOURS_BITS) ".dll: error detouring: %ld\n", error);
#endif // DEBUG_TRACE
	}
	else if (dwReason == DLL_PROCESS_DETACH)
	{
		DetourTransactionBegin();
		DetourUpdateThread(GetCurrentThread());
		DetourDetach(&(PVOID &)_NtQueryDirectoryFile, NtQueryDirectoryFile);
		DetourDetach(&(PVOID &)_NtQueryDirectoryFileEx, NtQueryDirectoryFileEx);
		error = DetourTransactionCommit();

#ifdef DEBUG_TRACE
		printf("WinHideEx" DETOURS_STRINGIFY(DETOURS_BITS) ".dll: removed (result=%ld).\n", error);
		fflush(stdout);
#endif // DEBUG_TRACE
	}

	return TRUE;
}

