using System.ComponentModel;

namespace WindowsDataRecovery.Models
{
    /// <summary>
    /// Represents a file that can be recovered
    /// </summary>
    public class RecoverableFile : INotifyPropertyChanged
    {
        private bool _isSelected;
        private RecoveryStatus _recoveryStatus;

        public string FileName { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string FileExtension { get; set; } = string.Empty;
        public System.DateTime CreatedDate { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public bool IsRecoverable { get; set; }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        public RecoveryStatus RecoveryStatus
        {
            get { return _recoveryStatus; }
            set
            {
                if (_recoveryStatus != value)
                {
                    _recoveryStatus = value;
                    OnPropertyChanged(nameof(RecoveryStatus));
                }
            }
        }

        public string FormattedSize
        {
            get { return FormatFileSize(FileSize); }
        }

        public string FormattedCreatedDate
        {
            get { return CreatedDate.ToString("yyyy-MM-dd HH:mm:ss"); }
        }

        public string FormattedModifiedDate
        {
            get { return ModifiedDate.ToString("yyyy-MM-dd HH:mm:ss"); }
        }

        private static string FormatFileSize(long bytes)
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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Recovery status enumeration
    /// </summary>
    public enum RecoveryStatus
    {
        Ready,
        InProgress,
        Completed,
        Failed,
        Cancelled
    }
}
