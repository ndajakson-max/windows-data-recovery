using System;
using System.Windows;
using WindowsDataRecovery.Utilities;

namespace WindowsDataRecovery
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // Initialize logger
                LoggerUtility.Initialize();

                // Check for administrator privileges
                if (!AdminHelper.IsRunningAsAdmin())
                {
                    MessageBox.Show(
                        "This application requires Administrator privileges.\n\n" +
                        "The application will now restart with elevated privileges.",
                        "Administrator Privileges Required",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    if (AdminHelper.RestartAsAdmin())
                    {
                        Application.Current.Shutdown();
                    }
                    else
                    {
                        MessageBox.Show(
                            "Failed to restart with administrator privileges.",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        Application.Current.Shutdown();
                    }
                }

                LoggerUtility.LogInfo("Application started successfully");
            }
            catch (Exception ex)
            {
                LoggerUtility.LogError("Error during application startup", ex);
                MessageBox.Show(
                    $"An error occurred during startup: {ex.Message}",
                    "Startup Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            LoggerUtility.LogInfo("Application closed");
            base.OnExit(e);
        }
    }
}
