#include "pch.h"
#include "ReflectiveDllInject.h"
#include "ReflectiveLoaderFinder.h"

// You must allocate an heap or virtual memory and copy DLL data on it.
ULONGLONG InjectToInternal(DWORD pid, LPCSTR reflectiveLoaderProcName, size_t dllDataSize, LPVOID dllDataHeap, size_t dllEntryParameterSize, LPVOID dllEntryParameterHeap)
{
	auto myOpenProcess = (MyOpenProcess)SilentProcAddr(MODULE_KERNEL32, PROC_OpenProcess);
	auto myVirtualAllocEx = (MyVirtualAllocEx)SilentProcAddr(MODULE_KERNEL32, PROC_VirtualAllocEx);
	auto myWriteProcessMemory = (MyWriteProcessMemory)SilentProcAddr(MODULE_KERNEL32, PROC_WriteProcessMemory);
	auto myReadProcessMemory = (MyReadProcessMemory)SilentProcAddr(MODULE_KERNEL32, PROC_ReadProcessMemory);
	auto myVirtualProtectEx = (MyVirtualProtectEx)SilentProcAddr(MODULE_KERNEL32, PROC_VirtualProtectEx);
	auto myVirtualFreeEx = (MyVirtualFreeEx)SilentProcAddr(MODULE_KERNEL32, PROC_VirtualFreeEx);
	auto myGetCurrentProcess = (MyGetCurrentProcess)SilentProcAddr(MODULE_KERNEL32, PROC_GetCurrentProcess);
	auto myCreateRemoteThread = (MyCreateRemoteThread)SilentProcAddr(MODULE_KERNEL32, PROC_CreateRemoteThread);
	auto myWaitForSingleObject = (MyWaitForSingleObject)SilentProcAddr(MODULE_KERNEL32, PROC_WaitForSingleObject);
	auto myCloseHandle = (MyCloseHandle)SilentProcAddr(MODULE_KERNEL32, PROC_CloseHandle);
	LPVOID myBTITAddr = SilentProcAddr(MODULE_KERNEL32, PROC_BaseThreadInitThunk);

	DWORD loaderOffset = GetReflectiveLoaderOffset((UINT_PTR)dllDataHeap, reflectiveLoaderProcName);
	if (!loaderOffset)
		return COMBINE(RINJ_ERR_LOADER_UNAVAILABLE, 0);

	// Must have SeDebugPrivilege
	HANDLE processHandle = myOpenProcess(PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_READ | PROCESS_VM_WRITE, FALSE, pid);
	if (!processHandle)
		return COMBINE_ERR(RINJ_ERR_OPEN_PROECSS);

	std::random_device rd;
	std::mt19937_64 mt(rd());
	std::uniform_int_distribution<int> shiftAmount(1, 8);
	std::uniform_int_distribution<int> fillData(0, UCHAR_MAX);
	auto pageShift = 4096 * shiftAmount(mt);
	LPVOID remoteDLLMemory = myVirtualAllocEx(processHandle, nullptr, dllDataSize + pageShift, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);
	if (!remoteDLLMemory)
		return COMBINE_ERR(RINJ_ERR_ALLOC_PROCMEM_DLL);

	auto random = new BYTE[pageShift];
	for (int i = 0; i < pageShift; i++)
		random[i] = fillData(mt);
	myWriteProcessMemory(processHandle, remoteDLLMemory, random, pageShift, nullptr);
	delete[] random;

	remoteDLLMemory = (LPSTR)remoteDLLMemory + pageShift;

	BOOL state = myWriteProcessMemory(processHandle, remoteDLLMemory, dllDataHeap, dllDataSize, nullptr);
	if (!state)
		return COMBINE_ERR(RINJ_ERR_ALLOC_PROCMEM_DLL);

	DWORD oldProtect = 0;

	if (myBTITAddr)
	{
		UCHAR myBTITData[16];
		SIZE_T readBTIT = 0, writtenBTIT = 0;
		HANDLE handleOfMyself = myGetCurrentProcess();
		state = myReadProcessMemory(handleOfMyself, myBTITAddr, myBTITData, sizeof(myBTITData), &readBTIT);
		if (!state || readBTIT != sizeof(myBTITData))
			return COMBINE_ERR(RINJ_ERR_READ_MY_BTIT);

		// my BTIT address = their BTIT address (because base address of kernel32.dll is same across all processes)
		state = myVirtualProtectEx(processHandle, myBTITAddr, sizeof(myBTITData), PAGE_EXECUTE_READWRITE, &oldProtect);
		if (!state)
			return COMBINE_ERR(RINJ_ERR_UNLOCK_PROC_BTIT);

		state = myWriteProcessMemory(processHandle, myBTITAddr, myBTITData, sizeof(myBTITData), &writtenBTIT);
		if (!state || writtenBTIT != sizeof(myBTITData))
			return COMBINE_ERR(RINJ_ERR_OVERWRITE_PROC_BTIT);

		state = myVirtualProtectEx(processHandle, myBTITAddr, sizeof(myBTITData), oldProtect, &oldProtect);
		if (!state)
			return COMBINE_ERR(RINJ_ERR_RELOCK_PROC_BTIT);
	}

	LPVOID remoteParameterMemory = myVirtualAllocEx(processHandle, nullptr, dllDataSize, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
	if (!processHandle)
		return COMBINE_ERR(RINJ_ERR_ALLOC_PROCMEM_EPARAM);

	state = myWriteProcessMemory(processHandle, remoteParameterMemory, dllEntryParameterHeap, dllEntryParameterSize, nullptr);
	if (!state)
		return COMBINE_ERR(RINJ_ERR_WRITE_PROCMEM_EPARAM);

	state = myVirtualProtectEx(processHandle, remoteDLLMemory, dllDataSize, PAGE_EXECUTE_READ, &oldProtect);
	if (!state)
		return COMBINE_ERR(RINJ_ERR_UNLOCK_PROCMEM_DLL);

	HANDLE threadHandle = myCreateRemoteThread(processHandle, nullptr, REFLECTIVE_LOADER_SIZE, (LPTHREAD_START_ROUTINE)((ULONG_PTR)remoteDLLMemory + loaderOffset), remoteParameterMemory, 0, nullptr);
	if (!threadHandle)
		return COMBINE_ERR(RINJ_ERR_CREATE_THREAD);

	myWaitForSingleObject(threadHandle, INFINITE);

	myVirtualFreeEx(processHandle, remoteDLLMemory, 0, MEM_RELEASE);
	myVirtualFreeEx(processHandle, remoteParameterMemory, 0, MEM_RELEASE);

	myCloseHandle(threadHandle);
	return COMBINE(RINJ_SUCCESS, 0);

}

ULONGLONG RefInject(DWORD pid, LPCSTR reflectiveLoaderProcName, size_t dllDataSize, LPVOID dllDataHeap, size_t dllEntryParameterSize, LPVOID dllEntryParameterHeap)
{
	__try
	{
		return InjectToInternal(pid, reflectiveLoaderProcName, dllDataSize, dllDataHeap, dllEntryParameterSize, dllEntryParameterHeap);
	}
	__except (EXCEPTION_EXECUTE_HANDLER)
	{
		return COMBINE(RINJ_ERR_UNKNOWN, 0);
	}
}

