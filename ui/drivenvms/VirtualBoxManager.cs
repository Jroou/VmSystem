using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace drivenvms
{
    public class VirtualBoxManager
    {
        private readonly string _vboxManagePath = @"C:\Program Files\Oracle\VirtualBox\VBoxManage.exe";


        // Асинхронно формує повну команду і передає її в системне ядро C++ у фоновому потоці
        public async Task<string> ExecuteCommandAsync(string arguments)
        {
            return await Task.Run(() =>
            {
                string fullCommand = $"\"{_vboxManagePath}\" {arguments}";
                StringBuilder output = new StringBuilder(8192);

                NativeMethods.ExecuteSystemCommand(fullCommand, output, output.Capacity);

                string result = output.ToString();
                if (result.StartsWith("Error:"))
                {
                    throw new Exception($"System API Error: {result}");
                }
                return result;
            });
        }

        
        // Асинхронно отримує список всіх ВМ та їхній поточний стан
        public async Task<List<VirtualMachineModel>> GetVirtualMachinesAsync()
        {
            var vms = new List<VirtualMachineModel>();

            string output = await ExecuteCommandAsync("list vms");
            string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var parts = line.Split(new[] { '"' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    string name = parts[0];
                    string uuid = parts[1].Trim(' ', '{', '}');

                    string stateOutput = await ExecuteCommandAsync($"showvminfo \"{uuid}\" --machinereadable");
                    string state = "Unknown";

                    foreach (var infoLine in stateOutput.Split('\n'))
                    {
                        if (infoLine.StartsWith("VMState="))
                        {
                            state = infoLine.Split('=')[1].Trim('"', '\r');
                            break;
                        }
                    }

                    vms.Add(new VirtualMachineModel { Name = name, Uuid = uuid, State = state });
                }
            }
            return vms;
        }


        // Асинхронно запускає віртуальну машину
        public async Task StartVmAsync(string uuid)
        {
            await ExecuteCommandAsync($"startvm \"{uuid}\" --type gui");
        }

        // Асинхронно зупиняє віртуальну машину
        public async Task StopVmAsync(string uuid)
        {
            await ExecuteCommandAsync($"controlvm \"{uuid}\" poweroff");
        }

        // Асинхронно створює нову віртуальну машину
        public async Task CreateVmAsync(string name, string osType, int ramMb, int cpuCores)
        {
            await ExecuteCommandAsync($"createvm --name \"{name}\" --ostype \"{osType}\" --register");
            await ExecuteCommandAsync($"modifyvm \"{name}\" --memory {ramMb} --cpus {cpuCores}");
        }
    }
}