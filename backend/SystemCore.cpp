#include "pch.h"
#include "SystemCore.h"
#include <string>
#include <TlHelp32.h>
#include <Psapi.h>
#include <memory>
#include <vector>

// Допоміжний метод для безпечної конвертації системних рядків WCHAR у стандартний string
std::string WStringToString(const std::wstring& wstr) {
    if (wstr.empty()) return "";
    int size_needed = WideCharToMultiByte(CP_UTF8, 0, &wstr[0], (int)wstr.size(), NULL, 0, NULL, NULL);
    std::string strTo(size_needed, 0);
    WideCharToMultiByte(CP_UTF8, 0, &wstr[0], (int)wstr.size(), &strTo[0], size_needed, NULL, NULL);
    return strTo;
}

unsigned int GetTotalCPUCores() {
    DWORD returnLength = 0;
    GetLogicalProcessorInformation(NULL, &returnLength);
    if (returnLength == 0) return 1;

    std::unique_ptr<SYSTEM_LOGICAL_PROCESSOR_INFORMATION[]> buffer(
        new SYSTEM_LOGICAL_PROCESSOR_INFORMATION[returnLength / sizeof(SYSTEM_LOGICAL_PROCESSOR_INFORMATION)]
    );

    if (!GetLogicalProcessorInformation(buffer.get(), &returnLength)) return 1;

    unsigned int physicalCores = 0;
    DWORD processorCoreCount = returnLength / sizeof(SYSTEM_LOGICAL_PROCESSOR_INFORMATION);

    for (DWORD i = 0; i < processorCoreCount; i++) {
        if (buffer[i].Relationship == RelationProcessorCore) {
            physicalCores++;
        }
    }
    return physicalCores > 0 ? physicalCores : 1;
}

unsigned long GetAvailableRAM_MB() {
    MEMORYSTATUSEX memInfo;
    memInfo.dwLength = sizeof(MEMORYSTATUSEX);
    GlobalMemoryStatusEx(&memInfo);
    return (unsigned long)(memInfo.ullAvailPhys / (1024 * 1024));
}

unsigned long GetTotalVBoxMemoryUsageMB() {
    unsigned long totalMemory = 0;
    HANDLE hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
    if (hSnapshot == INVALID_HANDLE_VALUE) return 0;

    PROCESSENTRY32 pe32;
    pe32.dwSize = sizeof(PROCESSENTRY32);

    if (Process32First(hSnapshot, &pe32)) {
        do {
            std::string name = WStringToString(pe32.szExeFile);
            if (name.find("VirtualBox") != std::string::npos || name.find("VBox") != std::string::npos) {
                HANDLE hProcess = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, FALSE, pe32.th32ProcessID);
                if (hProcess) {
                    PROCESS_MEMORY_COUNTERS pmc;
                    if (GetProcessMemoryInfo(hProcess, &pmc, sizeof(pmc))) {
                        totalMemory += pmc.WorkingSetSize;
                    }
                    CloseHandle(hProcess);
                }
            }
        } while (Process32Next(hSnapshot, &pe32));
    }
    CloseHandle(hSnapshot);
    return totalMemory / (1024 * 1024);
}

void ExecuteSystemCommand(const char* command, char* outputBuffer, int bufferSize) {
    if (!command || !outputBuffer || bufferSize <= 0) return;

    SECURITY_ATTRIBUTES saAttr;
    saAttr.nLength = sizeof(SECURITY_ATTRIBUTES);
    saAttr.bInheritHandle = TRUE;
    saAttr.lpSecurityDescriptor = NULL;

    HANDLE hChildStd_OUT_Rd = NULL;
    HANDLE hChildStd_OUT_Wr = NULL;

    if (!CreatePipe(&hChildStd_OUT_Rd, &hChildStd_OUT_Wr, &saAttr, 0)) {
        strcpy_s(outputBuffer, bufferSize, "Error: CreatePipe failed");
        return;
    }

    if (!SetHandleInformation(hChildStd_OUT_Rd, HANDLE_FLAG_INHERIT, 0)) {
        CloseHandle(hChildStd_OUT_Wr);
        CloseHandle(hChildStd_OUT_Rd);
        strcpy_s(outputBuffer, bufferSize, "Error: SetHandleInformation failed");
        return;
    }

    STARTUPINFOA siStartInfo;
    PROCESS_INFORMATION piProcInfo;
    ZeroMemory(&siStartInfo, sizeof(STARTUPINFOA));
    siStartInfo.cb = sizeof(STARTUPINFOA);
    siStartInfo.hStdError = hChildStd_OUT_Wr;
    siStartInfo.hStdOutput = hChildStd_OUT_Wr;
    siStartInfo.dwFlags |= STARTF_USESTDHANDLES;

    ZeroMemory(&piProcInfo, sizeof(PROCESS_INFORMATION));

    std::string cmdStr(command);
    if (!CreateProcessA(NULL, &cmdStr[0], NULL, NULL, TRUE, CREATE_NO_WINDOW, NULL, NULL, &siStartInfo, &piProcInfo)) {
        CloseHandle(hChildStd_OUT_Wr);
        CloseHandle(hChildStd_OUT_Rd);
        strcpy_s(outputBuffer, bufferSize, "Error: CreateProcess failed");
        return;
    }

    CloseHandle(hChildStd_OUT_Wr);

    std::string result = "";
    char chBuf[4096];
    DWORD dwRead;
    bool bSuccess = false;

    while (true) {
        bSuccess = ReadFile(hChildStd_OUT_Rd, chBuf, sizeof(chBuf) - 1, &dwRead, NULL);
        if (!bSuccess || dwRead == 0) break;
        chBuf[dwRead] = '\0';
        result += chBuf;
    }

    CloseHandle(hChildStd_OUT_Rd);
    CloseHandle(piProcInfo.hProcess);
    CloseHandle(piProcInfo.hThread);

    if (result.length() >= (size_t)bufferSize) {
        result = result.substr(0, bufferSize - 1);
    }
    strcpy_s(outputBuffer, bufferSize, result.c_str());
}

// НОВА ФУНКЦІЯ: Детальна аналітика кожного фонового процесу окремо
void GetVBoxProcessAnalytics(char* outputBuffer, int bufferSize) {
    if (!outputBuffer || bufferSize <= 0) return;

    std::string analytics = "";
    HANDLE hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
    if (hSnapshot == INVALID_HANDLE_VALUE) {
        strcpy_s(outputBuffer, bufferSize, "WinAPI Error Snapshot");
        return;
    }

    PROCESSENTRY32 pe32;
    pe32.dwSize = sizeof(PROCESSENTRY32);

    if (!Process32First(hSnapshot, &pe32)) {
        CloseHandle(hSnapshot);
        strcpy_s(outputBuffer, bufferSize, "Process Reading Error");
        return;
    }

    int count = 0;
    do {
        std::string name = WStringToString(pe32.szExeFile);
        std::string lowerName = name;
        for (auto& c : lowerName) c = tolower(c);

        if (lowerName.find("vbox") != std::string::npos || lowerName.find("virtualbox") != std::string::npos) {
            HANDLE hProcess = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, FALSE, pe32.th32ProcessID);
            size_t memoryMB = 0;

            if (hProcess) {
                PROCESS_MEMORY_COUNTERS pmc;
                if (GetProcessMemoryInfo(hProcess, &pmc, sizeof(pmc))) {
                    memoryMB = pmc.WorkingSetSize / (1024 * 1024);
                }
                CloseHandle(hProcess);
            }

            // Формуємо красивий рядок аналітики для бічної панелі
            analytics += "• " + name + "\n  [PID: " + std::to_string(pe32.th32ProcessID) + "] → " + std::to_string(memoryMB) + " MB\n\n";
            count++;
        }
    } while (Process32Next(hSnapshot, &pe32));

    CloseHandle(hSnapshot);

    if (count == 0) {
        analytics = "No active VirtualBox processes\nfound in the system.";
    }

    if (analytics.length() >= (size_t)bufferSize) {
        analytics = analytics.substr(0, bufferSize - 1);
    }
    strcpy_s(outputBuffer, bufferSize, analytics.c_str());
}