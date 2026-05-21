using DrivenVMS;
using System;
using System.Collections.Generic;
using System.Text;

namespace drivenvms
{
    public class VirtualBoxManager
    {
        // Стандартний шлях до утиліти управління VirtualBox
        private readonly string _vboxManagePath = @"C:\Program Files\Oracle\VirtualBox\VBoxManage.exe";


        // Формує повну команду і передає її в системне ядро C++
        public string ExecuteCommand(string arguments)
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
        }


        // Отримує список всіх ВМ та їхній поточний стан
        public List<VirtualMachineModel> GetVirtualMachines()
        {
            var vms = new List<VirtualMachineModel>();

            string output = ExecuteCommand("list vms");
            string[] lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var parts = line.Split(new[] { '"' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    string name = parts[0];
                    string uuid = parts[1].Trim(' ', '{', '}');

                    string stateOutput = ExecuteCommand($"showvminfo \"{uuid}\" --machinereadable");
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

        // Запускає віртуальну машину з графічним інтерфейсом
        public void StartVm(string uuid)
        {
            ExecuteCommand($"startvm \"{uuid}\" --type gui");
        }

        // Вимикання віртуальної машини
        public void StopVm(string uuid)
        {
            ExecuteCommand($"controlvm \"{uuid}\" poweroff");
        }

        public void CreateVm(string name, string osType, int ramMb, int cpuCores)
        {
            //Створення та реєстрація машини у VirtualBox
            ExecuteCommand($"createvm --name \"{name}\" --ostype \"{osType}\" --register");

            //Налаштування виділеної оперативної пам'яті та кількості ядер
            ExecuteCommand($"modifyvm \"{name}\" --memory {ramMb} --cpus {cpuCores}");
        }
    }
}