using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using WindowsDataRecovery.Core;
using WindowsDataRecovery.Models;
using WindowsDataRecovery.Utilities;

namespace WindowsDataRecovery.UI
{
    public partial class MainWindow : Window
    {
        private DiskScanner _diskScanner;
        private FileRecovery _fileRecovery;
        private CancellationTokenSource _scanCancellationTokenSource;
        private CancellationTokenSource _recoveryCancellationTokenSource;
        private ObservableCollection<RecoverableFile> _recoverableFiles;
        private string _selectedDrive = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
            InitializeApplication();
        }

        private void InitializeApplication()
        {
            try
            {
                _diskScanner = new DiskScanner();
                _fileRecovery = new FileRecovery();
                _recoverableFiles = new ObservableCollection<RecoverableFile>();

                FilesListBox.ItemsSource = _recoverableFiles;
                RecoveryListBox.ItemsSource = _recoverableFiles;

                // Wire up event handlers
                _diskScanner.ProgressChanged += DiskScanner_ProgressChanged;
                _diskScanner.ScanCompleted += DiskScanner_ScanCompleted;
                _diskScanner.ErrorOccurred += DiskScanner_ErrorOccurred;

                _fileRecovery.ProgressChanged += FileRecovery_ProgressChanged;
                _fileRecovery.RecoveryCompleted += FileRecovery_RecoveryCompleted;
                _fileRecovery.ErrorOccurred += FileRecovery_ErrorOccurred;

                RefreshDrives();
                LoggerUtility.LogInfo("Application initialized");
            }
            catch (Exception ex)
            {
                LoggerUtility.LogError("Error initializing application", ex);
                MessageBox.Show($"Error initializing application: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshDrives()
        {
            try
            {
                DriveComboBox.Items.Clear();
                var drives = DriveUtility.GetAvailableDrives();

                foreach (var drive in drives)
                {
                    DriveComboBox.Items.Add($"{drive.DriveLetter} ({drive.VolumeName ?? "Local Disk"}) - {drive.FormattedTotalSize}");
                }

                if (DriveComboBox.Items.Count > 0)
                {
                    DriveComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                LoggerUtility.LogError("Error refreshing drives", ex);
            }
        }

        private void RefreshDrives_Click(object sender, RoutedEventArgs e)
        {
            RefreshDrives();
            MessageBox.Show("Drives refreshed", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void StartScan_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DriveComboBox.SelectedIndex == -1)
                {
                    MessageBox.Show("Please select a drive to scan", "No Drive Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _selectedDrive = GetSelectedDrivePath();
                _recoverableFiles.Clear();

                // Prepare scan options
                var options = DeepScanCheckBox.IsChecked == true ? ScanOptions.CreateDeepScan() : ScanOptions.CreateDefault();

                // Parse file types
                if (!string.IsNullOrWhiteSpace(FileTypesTextBox.Text))
                {
                    options.IncludedExtensions = FileTypesTextBox.Text
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .ToList();
                }

                // Parse minimum file size
                if (long.TryParse(MinFileSizeTextBox.Text, out long minSize))
                {
                    options.MinimumFileSize = minSize * 1024; // Convert KB to bytes
                }

                // Update UI
                StartScanButton.IsEnabled = false;
                CancelScanButton.IsEnabled = true;
                ScanStatusText.Text = "Scanning in progress...";

                _scanCancellationTokenSource = new CancellationTokenSource();
                var files = await _diskScanner.ScanDriveAsync(_selectedDrive, options, _scanCancellationTokenSource.Token);

                foreach (var file in files)
                {
                    _recoverableFiles.Add(file);
                }

                StartRecoveryButton.IsEnabled = _recoverableFiles.Count > 0;
            }
            catch (Exception ex)
            {
                LoggerUtility.LogError("Error starting scan", ex);
                MessageBox.Show($"Error during scan: {ex.Message}", "Scan Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StartScanButton.IsEnabled = true;
                CancelScanButton.IsEnabled = false;
            }
        }

        private void CancelScan_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _scanCancellationTokenSource?.Cancel();
                StartScanButton.IsEnabled = true;
                CancelScanButton.IsEnabled = false;
                ScanStatusText.Text = "Scan cancelled";
            }
            catch (Exception ex)
            {
                LoggerUtility.LogError("Error cancelling scan", ex);
            }
        }

        private void BrowseDestination_Click(object sender, RoutedEventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select destination folder for recovered files";
                if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    DestinationPathTextBox.Text = folderDialog.SelectedPath;
                }
            }
        }

        private async void StartRecovery_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(DestinationPathTextBox.Text))
                {
                    MessageBox.Show("Please select a destination folder", "No Destination", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var selectedFiles = _recoverableFiles.Where(f => f.IsSelected).ToList();
                if (selectedFiles.Count == 0)
                {
                    MessageBox.Show("Please select at least one file to recover", "No Files Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Update UI
                StartRecoveryButton.IsEnabled = false;
                CancelRecoveryButton.IsEnabled = true;
                RecoveryStatusText.Text = "Recovery in progress...";

                _recoveryCancellationTokenSource = new CancellationTokenSource();
                var result = await _fileRecovery.RecoverFilesAsync(
                    selectedFiles,
                    DestinationPathTextBox.Text,
                    _recoveryCancellationTokenSource.Token);

                UpdateRecoverySummary(result);
            }
            catch (Exception ex)
            {
                LoggerUtility.LogError("Error starting recovery", ex);
                MessageBox.Show($"Error during recovery: {ex.Message}", "Recovery Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StartRecoveryButton.IsEnabled = true;
                CancelRecoveryButton.IsEnabled = false;
            }
        }

        private void CancelRecovery_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _recoveryCancellationTokenSource?.Cancel();
                StartRecoveryButton.IsEnabled = true;
                CancelRecoveryButton.IsEnabled = false;
                RecoveryStatusText.Text = "Recovery cancelled";
            }
            catch (Exception ex)
            {
                LoggerUtility.LogError("Error cancelling recovery", ex);
            }
        }

        private void FileItem_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var file = (sender as System.Windows.FrameworkElement)?.DataContext as RecoverableFile;
            if (file != null)
            {
                file.IsSelected = !file.IsSelected;
            }
        }

        private void ViewLogs_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var logDirectory = LoggerUtility.GetLogDirectory();
                if (Directory.Exists(logDirectory))
                {
                    System.Diagnostics.Process.Start("explorer.exe", logDirectory);
                }
                else
                {
                    MessageBox.Show("Log directory not found", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                LoggerUtility.LogError("Error opening logs", ex);
                MessageBox.Show($"Error opening logs: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetSelectedDrivePath()
        {
            if (DriveComboBox.SelectedItem is string item)
            {
                return item.Split('(')[0].Trim() + "\\";
            }
            return string.Empty;
        }

        private void UpdateRecoverySummary(RecoveryResult result)
        {
            FilesRecoveredText.Text = result.FilesRecovered.ToString();
            DataRecoveredText.Text = result.FormattedBytesRecovered;
            SuccessRateText.Text = $"{result.SuccessRate:F1}%";
            RecoveryStatusText.Text = result.Success ? "Recovery completed successfully" : "Recovery completed with errors";
        }

        private void DiskScanner_ProgressChanged(object sender, DiskScanner.ScanProgressEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                ScanProgressBar.Value = e.ProgressPercentage;
                ScanStatusText.Text = e.Message;
            });
        }

        private void DiskScanner_ScanCompleted(object sender, DiskScanner.ScanCompletedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                StartScanButton.IsEnabled = true;
                CancelScanButton.IsEnabled = false;
                ScanStatusText.Text = e.Success ? $"Scan completed. Found {e.FilesFound} files" : "Scan cancelled";
            });
        }

        private void DiskScanner_ErrorOccurred(object sender, DiskScanner.ErrorEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                MessageBox.Show(e.Message, "Scan Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StartScanButton.IsEnabled = true;
                CancelScanButton.IsEnabled = false;
            });
        }

        private void FileRecovery_ProgressChanged(object sender, FileRecovery.RecoveryProgressEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                RecoveryProgressBar.Value = e.ProgressPercentage;
                RecoveryStatusText.Text = e.Message;
            });
        }

        private void FileRecovery_RecoveryCompleted(object sender, FileRecovery.RecoveryCompletedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                StartRecoveryButton.IsEnabled = true;
                CancelRecoveryButton.IsEnabled = false;
                UpdateRecoverySummary(e.Result);
            });
        }

        private void FileRecovery_ErrorOccurred(object sender, FileRecovery.ErrorEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                MessageBox.Show(e.Message, "Recovery Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StartRecoveryButton.IsEnabled = true;
                CancelRecoveryButton.IsEnabled = false;
            });
        }
    }
}
