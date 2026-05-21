using System;
using System.Text;
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

            // Викликаємо асинхронне завантаження при старті вікна
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSystemStats();
            await LoadVirtualMachinesAsync();
        }

        private void LoadSystemStats()
        {
            try
            {
                ulong ram = NativeMethods.GetAvailableRAM_MB();
                uint cores = NativeMethods.GetTotalCPUCores();
                ulong vboxMemory = NativeMethods.GetTotalVBoxMemoryUsageMB();

                SystemStatsText.Text = $"Available RAM: {ram} MB | VM Consumption: {vboxMemory} MB | Available Cores: {cores}";

                // ВИКЛИК НОВОЇ АНАЛІТИКИ: Створюємо буфер на 4 КБ для списку процесів
                StringBuilder processBuffer = new StringBuilder(4096);
                NativeMethods.GetVBoxProcessAnalytics(processBuffer, processBuffer.Capacity);

                // Виводимо системні дані на екран
                ProcessAnalyticsText.Text = processBuffer.ToString();
            }
            catch (Exception ex)
            {
                SystemStatsText.Text = "Core communication error (C++ DLL)";
                ProcessAnalyticsText.Text = $"Failed to read processes.\nDetails: {ex.Message}";
            }
        }

        private async Task LoadVirtualMachinesAsync()
        {
            try
            {
                // Тимчасово блокуємо кнопки, поки йде фоновий запит до системи
                SetButtonsEnabled(false);
                SystemStatsText.Text += " (Оновлення списку...)";

                var vms = await _vboxManager.GetVirtualMachinesAsync();
                VmsDataGrid.ItemsSource = vms;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка отримання списку ВМ.\nДеталі: {ex.Message}", "Помилка VirtualBox", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetButtonsEnabled(true);
                LoadSystemStats(); // Оновлюємо цифри пам'яті
            }
        }

        private void SetButtonsEnabled(bool enabled)
        {
            RefreshBtn.IsEnabled = enabled;
            StartBtn.IsEnabled = enabled;
            StopBtn.IsEnabled = enabled;
            CreateBtn.IsEnabled = enabled;
        }

        private async void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            await LoadVirtualMachinesAsync();
        }

        private async void StartBtn_Click(object sender, RoutedEventArgs e)
        {
            if (VmsDataGrid.SelectedItem is VirtualMachineModel selectedVm)
            {
                try
                {
                    await _vboxManager.StartVmAsync(selectedVm.Uuid);
                    MessageBox.Show($"Віртуальну машину {selectedVm.Name} запущено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadVirtualMachinesAsync();
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

        private async void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            if (VmsDataGrid.SelectedItem is VirtualMachineModel selectedVm)
            {
                try
                {
                    await _vboxManager.StopVmAsync(selectedVm.Uuid);
                    MessageBox.Show($"Віртуальну машину {selectedVm.Name} зупинено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadVirtualMachinesAsync();
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

        private async void CreateBtn_Click(object sender, RoutedEventArgs e)
        {
            CreateVmWindow createWindow = new CreateVmWindow();
            createWindow.Owner = this;

            if (createWindow.ShowDialog() == true)
            {
                await LoadVirtualMachinesAsync();
            }
        }
    }
}