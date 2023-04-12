#pragma once
#include "pch.h"

#define DllExport extern "C" __declspec(dllexport)

#define COMBINE(hi, lo)			(ULONGLONG)(((hi) << 16) | (lo))
#define COMBINE_ERR(errType)	COMBINE((errType), GetLastError())

#define DEREF_PTR(name)	*(UINT_PTR*)(name)
#define DEREF_64(name)	*(DWORD64*)(name)
#define DEREF_32(name)	*(DWORD*)(name)
#define DEREF_16(name)	*(WORD*)(name)
#define DEREF_8(name)	*(BYTE*)(name)
