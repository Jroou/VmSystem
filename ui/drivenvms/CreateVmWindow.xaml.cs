using System;
using System.Windows;

namespace drivenvms
{
    public partial class CreateVmWindow : Window
    {
        private VirtualBoxManager _vboxManager;

        public CreateVmWindow()
        {
            InitializeComponent();
            _vboxManager = new VirtualBoxManager();
            InitializeSystemLimits();
        }

        private void InitializeSystemLimits()
        {
            try
            {
                ulong availableRam = NativeMethods.GetAvailableRAM_MB();
                uint totalCores = NativeMethods.GetTotalCPUCores();

                ulong maxSafeRam = availableRam > 1024 ? availableRam - 1024 : availableRam;

                RamSlider.Maximum = maxSafeRam;
                CpuSlider.Maximum = totalCores;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка читання системних лімітів: {ex.Message}", "Попередження", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private async void CreateBtn_Click(object sender, RoutedEventArgs e)
        {
            string vmName = VmNameInput.Text.Trim();
            if (string.IsNullOrEmpty(vmName))
            {
                MessageBox.Show("Please enter a virtual machine name", "Validation error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string osType = (OsTypeCombo.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString() ?? "Ubuntu_64";
            int ram = (int)RamSlider.Value;
            int cpu = (int)CpuSlider.Value;

            try
            {
                CreateBtn.IsEnabled = false; // Блокуємо кнопку на час створення

                // Викликаємо оновлений асинхронний метод
                await _vboxManager.CreateVmAsync(vmName, osType, ram, cpu);

                MessageBox.Show($"Virtual machine '{vmName}' created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create VM. Details: {ex.Message}", "System error", MessageBoxButton.OK, MessageBoxImage.Error);
                CreateBtn.IsEnabled = true;
            }
        }
    }
}