#pragma once
#include <windows.h>

#ifdef SYSTEMCORE_EXPORTS
#define SYSTEMCORE_API __declspec(dllexport)
#else
#define SYSTEMCORE_API __declspec(dllimport)
#endif

extern "C" {
    SYSTEMCORE_API unsigned int GetTotalCPUCores();
    SYSTEMCORE_API unsigned long GetAvailableRAM_MB();
    SYSTEMCORE_API unsigned long GetTotalVBoxMemoryUsageMB();
    SYSTEMCORE_API void ExecuteSystemCommand(const char* command, char* outputBuffer, int bufferSize);
    SYSTEMCORE_API void GetVBoxProcessAnalytics(char* outputBuffer, int bufferSize);
}