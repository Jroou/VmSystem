# Project Context & Refactoring Request

## Overview
We are developing a hybrid, architecture-driven desktop application named **DrivenVMS** (Virtual Machine Management utility). It interacts with Oracle VirtualBox using a low-level WinAPI C++ DLL wrapper and a C# WPF frontend interface.

The solution is organized as a monolithic repository with the following intended directory structure:
- `C:\Users\furu1\source\repos\VmSystem\backend\` -> Contains C++ Source Files (`SystemCore.cpp`, `SystemCore.h`, `dllmain.cpp`, `pch.h`) and `VmsSystemCore.vcxproj`.
- `C:\Users\furu1\source\repos\VmSystem\ui\drivenvms\` -> Contains C# WPF Source Files (`MainWindow.xaml`, `VirtualBoxManager.cs`, `NativeMethods.cs`, `VirtualMachineModel.cs`) and `drivenvms.csproj`.
- `C:\Users\furu1\source\repos\VmSystem\VmSystem.sln` -> Consolidated Visual Studio Solution file uniting both projects.

---

## Technical Stack & Logic
1. **C++ Core (`VmsSystemCore.dll`)**: Interacts directly with Windows API. It uses `GetLogicalProcessorInformation` (filtering by `RelationProcessorCore`) to query the exact number of **physical CPU cores** (the host CPU has 6 physical cores / 12 logical threads).
2. **C# WPF (`drivenvms.exe`)**: P/Invokes functions from `VmsSystemCore.dll` using `NativeMethods.cs`. It runs asynchronous commands (`Task.Run`) to invoke `VBoxManage.exe` via the C++ `ExecuteSystemCommand` wrapper.

---

## Current Problem: C++ Build Isolation & Hardcoded Cache
We recently refactored our repository layout (removed Git submodules, flattened nested duplicate folders, and migrated project files to a single `VmSystem` repository). 

However, we are facing an issue where the C# frontend **still displays 12 CPU cores instead of 6 physical cores**. 

### Root Cause Analysis:
- When running or building `drivenvms.csproj` directly, Visual Studio does not trigger an incremental build for the C++ backend (`VmsSystemCore.vcxproj`). 
- As a result, the C# binary target directory keeps loading an outdated, cached version of `VmsSystemCore.dll` (which was compiled before our physical core filtering fix).
- Visual Studio project configuration paths, dependencies, or Post-Build copy scripts might be broken or pointing to missing/ghost folders due to the recent file movement.

---

## Target Objectives for Copilot:
1. **Cleanup Solution Metadata**: Verify and clean up the `VmSystem.sln` solution and `.csproj`/`.vcxproj` files. Remove any obsolete paths, missing file links, or broken references caused by directory restructuring.
2. **Setup Build Dependencies**: Configure the Solution Build Dependencies so that compiling/running the C# WPF project (`drivenvms`) **automatically triggers** a rebuild of the C++ project (`VmsSystemCore`) if C++ files change.
3. **Automate Post-Build DLL Copying**: Ensure that a robust, relative-path Cross-Platform XML build target exists in `drivenvms.csproj` to copy the fresh `VmsSystemCore.dll` from the active build configuration output (e.g., `..\backend\x64\Debug\VmsSystemCore.dll`) straight into the WPF binary folder (`$(TargetDir)`) automatically on every successful build.
4. **Fix Configuration Targets**: Ensure both projects align to compile explicitly under **Debug/Release | x64** target platform architectures to prevent `BadImageFormatException`.

Please inspect my current configuration files, resolve target tracking issues, and provide step-by-step instructions or fixed XML/C++ project structures to make the solution completely cohesive and production-ready.