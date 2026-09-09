using System;
using System.IO;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace WindowsDataRecovery.Utilities
{
    /// <summary>
    /// Logging utility for application events
    /// </summary>
    public static class LoggerUtility
    {
        private static ILogger _logger = LogManager.GetCurrentClassLogger();
        private static string _logDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WindowsDataRecovery",
            "Logs");

        public static void Initialize()
        {
            try
            {
                // Ensure log directory exists
                if (!Directory.Exists(_logDirectory))
                {
                    Directory.CreateDirectory(_logDirectory);
                }

                // Configure NLog
                var config = new LoggingConfiguration();
                var fileTarget = new FileTarget("logfile")
                {
                    FileName = Path.Combine(_logDirectory, "${date:format=yyyy-MM-dd}.log"),
                    Layout = "${longdate}|${level:uppercase=true}|${message}${exception:format=tostring}",
                    ArchiveFileName = Path.Combine(_logDirectory, "archive", "${date:format=yyyy-MM-dd}_${sequencenumber}.log"),
                    ArchiveEvery = FileArchivePeriod.Day,
                    MaxArchiveFiles = 30
                };

                var consoleTarget = new ConsoleTarget("console")
                {
                    Layout = "${longdate}|${level:uppercase=true}|${message}"
                };

                config.AddRule(LogLevel.Trace, LogLevel.Fatal, consoleTarget);
                config.AddRule(LogLevel.Info, LogLevel.Fatal, fileTarget);

                LogManager.Configuration = config;
                _logger = LogManager.GetCurrentClassLogger();

                LogInfo("Logger initialized");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to initialize logger: {ex.Message}");
            }
        }

        public static void LogInfo(string message)
        {
            _logger.Info(message);
        }

        public static void LogDebug(string message)
        {
            _logger.Debug(message);
        }

        public static void LogWarning(string message)
        {
            _logger.Warn(message);
        }

        public static void LogError(string message, Exception ex)
        {
            _logger.Error(ex, message);
        }

        public static void LogError(string message)
        {
            _logger.Error(message);
        }

        public static string GetLogDirectory()
        {
            return _logDirectory;
        }
    }
}
