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
    /// Scans disks for deleted files and recoverable data
    /// </summary>
    public class DiskScanner
    {
        private CancellationToken _cancellationToken;
        private int _filesFound;
        private long _totalDataSize;

        public event EventHandler<ScanProgressEventArgs>? ProgressChanged;
        public event EventHandler<ScanCompletedEventArgs>? ScanCompleted;
        public event EventHandler<ErrorEventArgs>? ErrorOccurred;

        public async Task<List<RecoverableFile>> ScanDriveAsync(string drivePath, ScanOptions options, CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            _filesFound = 0;
            _totalDataSize = 0;
            var recoverableFiles = new List<RecoverableFile>();

            try
            {
                LoggerUtility.LogInfo($"Starting disk scan on drive: {drivePath}");
                OnProgressChanged("Initializing disk scan...", 0);

                if (!Directory.Exists(drivePath))
                {
                    throw new DirectoryNotFoundException($"Drive path not found: {drivePath}");
                }

                // Get all files on the drive
                var files = await Task.Run(() => GetAllFilesAsync(drivePath, options), cancellationToken);

                foreach (var file in files)
                {
                    _cancellationToken.ThrowIfCancellationRequested();

                    var recoverableFile = new RecoverableFile
                    {
                        FileName = Path.GetFileName(file),
                        FullPath = file,
                        FileSize = new FileInfo(file).Length,
                        FileExtension = Path.GetExtension(file),
                        CreatedDate = File.GetCreationTime(file),
                        ModifiedDate = File.GetLastWriteTime(file),
                        IsRecoverable = true,
                        RecoveryStatus = RecoveryStatus.Ready
                    };

                    recoverableFiles.Add(recoverableFile);
                    _filesFound++;
                    _totalDataSize += recoverableFile.FileSize;

                    int progress = (int)((double)_filesFound / Math.Max(files.Count, 1) * 100);
                    OnProgressChanged($"Found {_filesFound} files ({FormatFileSize(_totalDataSize)})", progress);
                }

                LoggerUtility.LogInfo($"Scan completed. Found {_filesFound} files ({FormatFileSize(_totalDataSize)})");
                OnScanCompleted(_filesFound, _totalDataSize, true);
            }
            catch (OperationCanceledException)
            {
                LoggerUtility.LogInfo("Disk scan cancelled by user");
                OnScanCompleted(_filesFound, _totalDataSize, false);
            }
            catch (Exception ex)
            {
                LoggerUtility.LogError("Error during disk scan", ex);
                OnErrorOccurred(ex.Message);
                OnScanCompleted(_filesFound, _totalDataSize, false);
            }

            return recoverableFiles;
        }

        private List<string> GetAllFilesAsync(string path, ScanOptions options)
        {
            var files = new List<string>();
            var queue = new Queue<string> { path };

            while (queue.Count > 0)
            {
                _cancellationToken.ThrowIfCancellationRequested();

                string currentPath = queue.Dequeue();

                try
                {
                    var directoryFiles = Directory.GetFiles(currentPath);
                    
                    foreach (var file in directoryFiles)
                    {
                        if (ShouldIncludeFile(file, options))
                        {
                            files.Add(file);
                        }
                    }

                    var subdirectories = Directory.GetDirectories(currentPath);
                    foreach (var subdir in subdirectories)
                    {
                        queue.Enqueue(subdir);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // Skip directories we don't have access to
                    continue;
                }
            }

            return files;
        }

        private bool ShouldIncludeFile(string filePath, ScanOptions options)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);

                // Filter by file type
                if (options.IncludedExtensions.Count > 0)
                {
                    var extension = Path.GetExtension(filePath).ToLower();
                    if (!options.IncludedExtensions.Contains(extension))
                    {
                        return false;
                    }
                }

                // Filter by minimum file size
                if (fileInfo.Length < options.MinimumFileSize)
                {
                    return false;
                }

                // Filter by maximum file size
                if (options.MaximumFileSize > 0 && fileInfo.Length > options.MaximumFileSize)
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
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
            ProgressChanged?.Invoke(this, new ScanProgressEventArgs(message, progressPercentage));
        }

        protected virtual void OnScanCompleted(int filesFound, long totalSize, bool success)
        {
            ScanCompleted?.Invoke(this, new ScanCompletedEventArgs(filesFound, totalSize, success));
        }

        protected virtual void OnErrorOccurred(string message)
        {
            ErrorOccurred?.Invoke(this, new ErrorEventArgs(message));
        }
    }

    public class ScanProgressEventArgs : EventArgs
    {
        public string Message { get; }
        public int ProgressPercentage { get; }

        public ScanProgressEventArgs(string message, int progressPercentage)
        {
            Message = message;
            ProgressPercentage = progressPercentage;
        }
    }

    public class ScanCompletedEventArgs : EventArgs
    {
        public int FilesFound { get; }
        public long TotalSize { get; }
        public bool Success { get; }

        public ScanCompletedEventArgs(int filesFound, long totalSize, bool success)
        {
            FilesFound = filesFound;
            TotalSize = totalSize;
            Success = success;
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
