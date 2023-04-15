#include "pch.h"
#include "Clipboard.h"

LPVOID ClipboardData(UINT format)
{
	if (!((MyOpenClipboard)SilentProcAddr(MODULE_USER32, PROC_OpenClipboard))(NULL))
		return (LPVOID)-1;

	HANDLE hClip = ((MyGetClipboardData)SilentProcAddr(MODULE_USER32, PROC_GetClipboardData))(format);
	if (!hClip)
		return (LPVOID)-2;

	LPVOID data = ((MyGlobalLock)SilentProcAddr(MODULE_KERNEL32, PROC_GlobalLock))(hClip);
	if (!data)
		return (LPVOID)-3;

	return data; // REMEMBER CLEANING UP THIS BY CALLING ClipboardCleanup()
}

void ClipboardCleanup(HANDLE clipboardHandle)
{
	((MyGlobalLock)SilentProcAddr(MODULE_KERNEL32, PROC_GlobalUnlock))(clipboardHandle);
	((MyCloseClipboard)SilentProcAddr(MODULE_USER32, PROC_CloseClipboard))();
}

DWORD ClipIndex()
{
	return ((MyGetClipboardSequenceNumber)SilentProcAddr(MODULE_USER32, PROC_GetClipboardSequenceNumber))();
}

INT ClipTextSize()
{
	LPVOID data = ClipboardData(CF_UNICODETEXT);
	if ((UINT_PTR)data < 0) // error
		return (INT)data;
	auto length = (INT)wcslen((LPWSTR)data);
	ClipboardCleanup(data);
	return length;
}

void ClipTextCopy(DWORD bufferSize, LPWSTR buffer)
{
	LPVOID data = ClipboardData(CF_UNICODETEXT);
	wcscpy_s(buffer, bufferSize, (LPWSTR)data);
	ClipboardCleanup(data);
}