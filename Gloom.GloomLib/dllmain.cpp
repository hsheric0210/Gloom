#include "pch.h"

BOOL APIENTRY DllMain(HMODULE moduleHandle, DWORD  callReason, LPVOID reserved)
{
	switch (callReason)
	{
		case DLL_PROCESS_ATTACH:
		case DLL_THREAD_ATTACH:
		case DLL_THREAD_DETACH:
		case DLL_PROCESS_DETACH:
			break;
	}
	return TRUE;
}

