using System;

namespace WindowsDataRecovery.Models
{
    /// <summary>
    /// Results of a file recovery operation
    /// </summary>
    public class RecoveryResult
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int TotalFilesToRecover { get; set; }
        public int FilesRecovered { get; set; }
        public long BytesRecovered { get; set; }
        public string DestinationPath { get; set; } = string.Empty;
        public bool Success { get; set; }

        public TimeSpan Duration
        {
            get { return EndTime - StartTime; }
        }

        public int FailedFiles
        {
            get { return TotalFilesToRecover - FilesRecovered; }
        }

        public double SuccessRate
        {
            get
            {
                if (TotalFilesToRecover == 0)
                    return 0;
                return (double)FilesRecovered / TotalFilesToRecover * 100;
            }
        }

        public string FormattedBytesRecovered
        {
            get { return FormatFileSize(BytesRecovered); }
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
    }
}
