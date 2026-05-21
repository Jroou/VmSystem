using DrivenVMS;
using System;
using System.Windows;

namespace drivenvms
{
    public partial class MainWindow : Window
    {
        private VirtualBoxManager _vboxManager;

        public MainWindow()
        {
            InitializeComponent();
            _vboxManager = new VirtualBoxManager();

            LoadSystemStats();
            LoadVirtualMachines();
        }

        private void LoadSystemStats()
        {
            try
            {
                ulong ram = NativeMethods.GetAvailableRAM_MB();
                uint cores = NativeMethods.GetTotalCPUCores();

                SystemStatsText.Text = $"Вільна RAM: {ram} MB | Доступно ядер: {cores}";
            }
            catch (Exception ex)
            {
                SystemStatsText.Text = "Помилка зв'язку з ядром (C++ DLL)";
                MessageBox.Show($"Не вдалося завантажити системну інформацію.\nПомилка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadVirtualMachines()
        {
            try
            {
                var vms = _vboxManager.GetVirtualMachines();
                VmsDataGrid.ItemsSource = vms;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка отримання списку ВМ.\nДеталі: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadSystemStats();
            LoadVirtualMachines();
        }

        private void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            if (VmsDataGrid.SelectedItem is VirtualMachineModel selectedVm)
            {
                try
                {
                    _vboxManager.StartVm(selectedVm.Uuid);
                    MessageBox.Show($"Віртуальну машину {selectedVm.Name} запущено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadVirtualMachines(); // Оновлюємо статус у таблиці
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не вдалося запустити ВМ.\nДеталі: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Будь ласка, виберіть віртуальну машину зі списку.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            if (VmsDataGrid.SelectedItem is VirtualMachineModel selectedVm)
            {
                try
                {
                    _vboxManager.StopVm(selectedVm.Uuid);
                    MessageBox.Show($"Віртуальну машину {selectedVm.Name} зупинено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadVirtualMachines(); // Оновлюємо статус у таблиці
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Не вдалося зупинити ВМ.\nДеталі: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Будь ласка, виберіть віртуальну машину зі списку.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void CreateBtn_Click(object sender, RoutedEventArgs e)
        {
            CreateVmWindow createWindow = new CreateVmWindow();
            createWindow.Owner = this; // Робить вікно модальним відносно головного

            // Якщо вікно повернуло True (машина створена успішно), оновлюємо таблицю
            if (createWindow.ShowDialog() == true)
            {
                LoadVirtualMachines();
            }
        }
    }
}