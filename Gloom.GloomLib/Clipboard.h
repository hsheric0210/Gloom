#pragma once
#include "pch.h"
#include "SilentProcAddr.h"
#include "ProcedureDefs.h"
#include "util.h"

DllExport DWORD ClipTextSize();
DllExport void ClipTextCopy(DWORD bufferSize, LPWSTR buffer);
