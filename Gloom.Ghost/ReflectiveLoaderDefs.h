#pragma once

#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#include "../Gloom.CShared/util.h"

#define DLL_QUERY_HMODULE		6

typedef BOOL(WINAPI *MyDllMain)(HINSTANCE, DWORD, LPVOID);
