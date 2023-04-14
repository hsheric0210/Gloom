#pragma once
#include "pch.h"
#include "util.h"
#include "ProcedureDefs.h"
#include "SilentProcAddr.h"
#include <random>

#define REFLECTIVE_LOADER_SIZE			(1024 * 1024)

#define RINJ_ERR_UNKNOWN			-1
#define RINJ_SUCCESS					0
#define RINJ_ERR_LOADER_UNAVAILABLE		1
#define RINJ_ERR_OPEN_PROECSS			2
#define RINJ_ERR_ALLOC_PROCMEM_DLL		3
#define RINJ_ERR_WRITE_PROCMEM_DLL		4
#define RINJ_ERR_READ_MY_BTIT			5
#define RINJ_ERR_ALLOC_PROCMEM_EPARAM	6
#define RINJ_ERR_WRITE_PROCMEM_EPARAM	7
#define RINJ_ERR_UNLOCK_PROC_BTIT		8
#define RINJ_ERR_OVERWRITE_PROC_BTIT	9
#define RINJ_ERR_RELOCK_PROC_BTIT		10
#define RINJ_ERR_UNLOCK_PROCMEM_DLL		11
#define RINJ_ERR_CREATE_THREAD			12

DllExport ULONGLONG RefInject(DWORD pid, LPCSTR reflectiveLoaderProcName, size_t dllDataSize, LPVOID dllDataHeap, size_t dllEntryParameterSize, LPVOID dllEntryParameterHeap);
