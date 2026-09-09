using System;
using System.Diagnostics;
using System.Security.Principal;

namespace WindowsDataRecovery.Utilities
{
    /// <summary>
    /// Helper for administrator privilege operations
    /// </summary>
    public static class AdminHelper
    {
        public static bool IsRunningAsAdmin()
        {
            try
            {
                using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
                {
                    WindowsPrincipal principal = new WindowsPrincipal(identity);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool RestartAsAdmin()
        {
            try
            {
                if (!IsRunningAsAdmin())
                {
                    ProcessStartInfo proc = new ProcessStartInfo
                    {
                        UseShellExecute = true,
                        FileName = Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty,
                        Verb = "runas"
                    };
                    Process.Start(proc);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
