using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WindowsDataRecovery.Models;
using WindowsDataRecovery.Utilities;

namespace WindowsDataRecovery.Core
{
    /// <summary>
    /// Handles file recovery operations
    /// </summary>
    public class FileRecovery
    {
        private CancellationToken _cancellationToken;
        private int _filesRecovered;
        private int _filesFailed;
        private long _bytesRecovered;

        public event EventHandler<RecoveryProgressEventArgs>? ProgressChanged;
        public event EventHandler<RecoveryCompletedEventArgs>? RecoveryCompleted;
        public event EventHandler<ErrorEventArgs>? ErrorOccurred;

        public async Task<RecoveryResult> RecoverFilesAsync(
            List<RecoverableFile> filesToRecover,
            string destinationPath,
            CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            _filesRecovered = 0;
            _filesFailed = 0;
            _bytesRecovered = 0;

            var startTime = DateTime.Now;

            try
            {
                LoggerUtility.LogInfo($"Starting recovery to: {destinationPath}");
                OnProgressChanged("Initializing recovery...", 0);

                // Verify destination path
                if (!Directory.Exists(destinationPath))
                {
                    Directory.CreateDirectory(destinationPath);
                }

                int totalFiles = filesToRecover.Count;

                // Recover files
                for (int i = 0; i < totalFiles; i++)
                {
                    _cancellationToken.ThrowIfCancellationRequested();

                    var file = filesToRecover[i];
                    await RecoverFileAsync(file, destinationPath);

                    int progress = (int)((double)(i + 1) / totalFiles * 100);
                    OnProgressChanged(
                        $"Recovered {_filesRecovered}/{totalFiles} files ({FormatFileSize(_bytesRecovered)})",
                        progress);
                }

                var result = new RecoveryResult
                {
                    StartTime = startTime,
                    EndTime = DateTime.Now,
                    TotalFilesToRecover = totalFiles,
                    FilesRecovered = _filesRecovered,
                    BytesRecovered = _bytesRecovered,
                    DestinationPath = destinationPath,
                    Success = _filesFailed == 0
                };

                LoggerUtility.LogInfo($"Recovery completed. {_filesRecovered}/{totalFiles} files recovered.");
                OnRecoveryCompleted(result);

                return result;
            }
            catch (OperationCanceledException)
            {
                LoggerUtility.LogInfo("Recovery cancelled by user");
                var result = new RecoveryResult
                {
                    StartTime = startTime,
                    EndTime = DateTime.Now,
                    TotalFilesToRecover = filesToRecover.Count,
                    FilesRecovered = _filesRecovered,
                    BytesRecovered = _bytesRecovered,
                    DestinationPath = destinationPath,
                    Success = false
                };
                OnRecoveryCompleted(result);
                return result;
            }
            catch (Exception ex)
            {
                LoggerUtility.LogError("Error during recovery", ex);
                OnErrorOccurred(ex.Message);
                var result = new RecoveryResult
                {
                    StartTime = startTime,
                    EndTime = DateTime.Now,
                    TotalFilesToRecover = filesToRecover.Count,
                    FilesRecovered = _filesRecovered,
                    BytesRecovered = _bytesRecovered,
                    DestinationPath = destinationPath,
                    Success = false
                };
                OnRecoveryCompleted(result);
                return result;
            }
        }

        private async Task RecoverFileAsync(RecoverableFile file, string destinationPath)
        {
            try
            {
                if (!File.Exists(file.FullPath))
                {
                    _filesFailed++;
                    file.RecoveryStatus = RecoveryStatus.Failed;
                    return;
                }

                var fileName = Path.GetFileName(file.FullPath);
                var destFilePath = Path.Combine(destinationPath, fileName);

                // Handle file conflicts
                int counter = 1;
                string originalFileName = Path.GetFileNameWithoutExtension(fileName);
                string extension = Path.GetExtension(fileName);

                while (File.Exists(destFilePath))
                {
                    fileName = $"{originalFileName}_{counter}{extension}";
                    destFilePath = Path.Combine(destinationPath, fileName);
                    counter++;
                }

                // Copy file
                await Task.Run(() =>
                {
                    File.Copy(file.FullPath, destFilePath, false);
                    _filesRecovered++;
                    _bytesRecovered += file.FileSize;
                    file.RecoveryStatus = RecoveryStatus.Completed;
                });

                LoggerUtility.LogInfo($"File recovered: {destFilePath}");
            }
            catch (Exception ex)
            {
                _filesFailed++;
                file.RecoveryStatus = RecoveryStatus.Failed;
                LoggerUtility.LogError($"Failed to recover file: {file.FullPath}", ex);
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;

            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return $"{len:0.##} {sizes[order]}";
        }

        protected virtual void OnProgressChanged(string message, int progressPercentage)
        {
            ProgressChanged?.Invoke(this, new RecoveryProgressEventArgs(message, progressPercentage));
        }

        protected virtual void OnRecoveryCompleted(RecoveryResult result)
        {
            RecoveryCompleted?.Invoke(this, new RecoveryCompletedEventArgs(result));
        }

        protected virtual void OnErrorOccurred(string message)
        {
            ErrorOccurred?.Invoke(this, new ErrorEventArgs(message));
        }
    }

    public class RecoveryProgressEventArgs : EventArgs
    {
        public string Message { get; }
        public int ProgressPercentage { get; }

        public RecoveryProgressEventArgs(string message, int progressPercentage)
        {
            Message = message;
            ProgressPercentage = progressPercentage;
        }
    }

    public class RecoveryCompletedEventArgs : EventArgs
    {
        public RecoveryResult Result { get; }

        public RecoveryCompletedEventArgs(RecoveryResult result)
        {
            Result = result;
        }
    }

    public class ErrorEventArgs : EventArgs
    {
        public string Message { get; }

        public ErrorEventArgs(string message)
        {
            Message = message;
        }
    }
}
