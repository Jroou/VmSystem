using System.Runtime.InteropServices;
using System.Text;

namespace drivenvms
{
    public static class NativeMethods
    {
        private const string DllName = "VmsSystemCore.dll";

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong GetAvailableRAM_MB();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern uint GetTotalCPUCores();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void ExecuteSystemCommand(string command, StringBuilder outputBuffer, int bufferSize);

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        public static extern ulong GetTotalVBoxMemoryUsageMB();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        public static extern void GetVBoxProcessAnalytics(StringBuilder outputBuffer, int bufferSize);
    }
}