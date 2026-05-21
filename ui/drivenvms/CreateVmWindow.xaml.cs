using DrivenVMS;
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

        // Встановлюємо ліміти повзунків на основі реальних системних даних з C++ DLL
        private void InitializeSystemLimits()
        {
            try
            {
                ulong availableRam = NativeMethods.GetAvailableRAM_MB();
                uint totalCores = NativeMethods.GetTotalCPUCores();

                // Залишаємо 1024 МБ для роботи самої Windows (хоста), щоб уникнути зависання
                ulong maxSafeRam = availableRam > 1024 ? availableRam - 1024 : availableRam;

                RamSlider.Maximum = maxSafeRam;
                CpuSlider.Maximum = totalCores;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка читання системних лімітів через ядро. Встановлено стандартні обмеження.\n{ex.Message}", "Попередження", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RamSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (RamLabel != null)
                RamLabel.Text = $"Оперативна пам'ять (MB): {Math.Round(e.NewValue)}";
        }

        private void CpuSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (CpuLabel != null)
                CpuLabel.Text = $"Кількість ядер CPU: {Math.Round(e.NewValue)}";
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void CreateBtn_Click(object sender, RoutedEventArgs e)
        {
            string vmName = VmNameInput.Text.Trim();
            if (string.IsNullOrEmpty(vmName))
            {
                MessageBox.Show("Будь ласка, введіть назву віртуальної машини.", "Помилка валідації", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string osType = (OsTypeCombo.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Content.ToString() ?? "Ubuntu_64";
            int ram = (int)RamSlider.Value;
            int cpu = (int)CpuSlider.Value;

            try
            {
                // Звертаємося до контролера для генерації системних команд
                _vboxManager.CreateVm(vmName, osType, ram, cpu);
                MessageBox.Show($"Віртуальну машину '{vmName}' успішно створено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка створення ВМ: {ex.Message}", "Системна помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}