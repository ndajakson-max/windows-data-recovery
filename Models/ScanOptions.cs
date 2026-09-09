using System.Collections.Generic;

namespace WindowsDataRecovery.Models
{
    /// <summary>
    /// Configuration options for disk scanning
    /// </summary>
    public class ScanOptions
    {
        public List<string> IncludedExtensions { get; set; } = new List<string>();
        public long MinimumFileSize { get; set; } = 0;
        public long MaximumFileSize { get; set; } = 0; // 0 = unlimited
        public bool ScanSubfolders { get; set; } = true;
        public bool DeepScan { get; set; } = false;

        public ScanOptions()
        {
        }

        public static ScanOptions CreateDefault()
        {
            return new ScanOptions
            {
                ScanSubfolders = true,
                DeepScan = false,
                MinimumFileSize = 0,
                MaximumFileSize = 0
            };
        }

        public static ScanOptions CreateDeepScan()
        {
            return new ScanOptions
            {
                ScanSubfolders = true,
                DeepScan = true,
                MinimumFileSize = 0,
                MaximumFileSize = 0
            };
        }
    }
}
