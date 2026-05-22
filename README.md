# DrivenVMS - Native Virtual Machine Manager

![Platform](https://img.shields.io/badge/Platform-Windows%20x64-blue)
![C%23](https://img.shields.io/badge/C%23-WPF%20%7C%20.NET%208-239120)
![C++](https://img.shields.io/badge/C++-WinAPI-00599C)
![License](https://img.shields.io/badge/License-MIT-green)

**DrivenVMS** is a high-performance, hybrid-architecture desktop application designed for streamlined management of VirtualBox virtual machines. It combines a modern, asynchronous Fluent UI with a low-level C++ native core to interact directly with the Windows API for real-time hardware telemetry.

## 🚀 Key Features

* **Hybrid Architecture:** Seamless Interop (P/Invoke) between a managed C# WPF frontend and an unmanaged C++ DLL.
* **Native OS Telemetry:** Real-time tracking of physical CPU cores and available host RAM using raw `WinAPI` (`GetLogicalProcessorInformation`, `GlobalMemoryStatusEx`).
* **Process Analytics Engine:** Built-in tracker that inspects the Windows process tree via `CreateToolhelp32Snapshot` to monitor exact memory consumption (Working Set) of individual VirtualBox instances (`VBoxSVC`, `VirtualBoxVM`).
* **Fail-Safe Hardware Allocation:** Dynamic resource sliders that automatically prevent the host OS from being starved of RAM during VM creation.
* **Modern Fluent UI:** Clean, responsive, and minimalist user interface.

## 🏗️ Project Structure

The repository is structured as a monolithic solution with deterministic MSBuild compilation:

    VmSystem/
    ├── backend/                  # C++ Native Core (VmsSystemCore.dll)
    │   ├── SystemCore.cpp        # WinAPI memory and processor analytics
    │   └── dllmain.cpp           # DLL entry point
    ├── ui/                       # C# Managed UI (drivenvms.exe)
    │   ├── drivenvms/
    │   │   ├── MainWindow.xaml   # WPF Fluent UI layout
    │   │   └── NativeMethods.cs  # DllImport P/Invoke definitions
    └── VmSystem.sln              # Unified Visual Studio Solution

## 🛠️ Build Instructions

The project uses deterministic build targets. Building the UI project will automatically trigger a compilation of the native C++ core and deploy the resulting DLL to the correct output directory.

### Prerequisites
* Visual Studio 2022
* .NET 8.0 SDK
* Desktop Development with C++ workload installed
* Oracle VirtualBox installed on the host machine

### Compilation
1. Clone the repository:
   git clone https://github.com/Jroou/VmSystem.git
2. Open `VmSystem.sln` in Visual Studio.
3. Set the build configuration to **Release | x64**.
4. Right-click the Solution and select **Rebuild Solution**.
5. Run `drivenvms.exe`.

## 🧠 Technical Highlights for Engineering
* **Memory Management:** Safe translation of wide-character strings and management of unmanaged memory buffers between C++ and C#.
* **Process Handling:** Utilization of `CreatePipe` and `CreateProcess` in C++ for secure, invisible execution of `VBoxManage` CLI commands.
* **Architecture:** Separation of Concerns (SoC) ensuring that UI components run asynchronously from native OS resource monitoring.

## 👨‍💻 Author
**Jrou** ## 📄 License
This project is licensed under the MIT License - see the LICENSE file for details.