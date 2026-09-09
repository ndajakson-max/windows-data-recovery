using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WindowsDataRecovery.Utilities
{
    /// <summary>
    /// Utility for drive and volume operations
    /// </summary>
    public static class DriveUtility
    {
        public class DriveInfo
        {
            public string DriveLetter { get; set; } = string.Empty;
            public string VolumeName { get; set; } = string.Empty;
            public string FileSystem { get; set; } = string.Empty;
            public long TotalSize { get; set; }
            public long FreeSpace { get; set; }
            public long UsedSpace { get; set; }

            public string FormattedTotalSize
            {
                get { return FormatBytes(TotalSize); }
            }

            public string FormattedFreeSpace
            {
                get { return FormatBytes(FreeSpace); }
            }

            public string FormattedUsedSpace
            {
                get { return FormatBytes(UsedSpace); }
            }

            public double UsagePercentage
            {
                get
                {
                    if (TotalSize == 0)
                        return 0;
                    return (double)UsedSpace / TotalSize * 100;
                }
            }

            private static string FormatBytes(long bytes)
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

        public static List<DriveInfo> GetAvailableDrives()
        {
            var drives = new List<DriveInfo>();

            try
            {
                var driveInfos = System.IO.DriveInfo.GetDrives();

                foreach (var drive in driveInfos)
                {
                    try
                    {
                        if (drive.IsReady)
                        {
                            drives.Add(new DriveInfo
                            {
                                DriveLetter = drive.Name.TrimEnd('\\'),
                                VolumeName = drive.VolumeLabel,
                                FileSystem = drive.DriveFormat,
                                TotalSize = drive.TotalSize,
                                FreeSpace = drive.AvailableFreeSpace,
                                UsedSpace = drive.TotalSize - drive.AvailableFreeSpace
                            });
                        }
                    }
                    catch
                    {
                        // Skip drives we can't access
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggerUtility.LogError("Error retrieving drives", ex);
            }

            return drives.OrderBy(d => d.DriveLetter).ToList();
        }

        public static bool IsDriveAccessible(string drivePath)
        {
            try
            {
                var driveInfo = new System.IO.DriveInfo(Path.GetPathRoot(drivePath) ?? drivePath);
                return driveInfo.IsReady;
            }
            catch
            {
                return false;
            }
        }

        public static long GetDriveFreeSpace(string drivePath)
        {
            try
            {
                var driveInfo = new System.IO.DriveInfo(Path.GetPathRoot(drivePath) ?? drivePath);
                return driveInfo.AvailableFreeSpace;
            }
            catch
            {
                return 0;
            }
        }
    }
}
