using System;
using Microsoft.Extensions.Configuration;
using System.Windows.Forms; // For MessageBox (if used in Windows Forms)
using System;
using System.IO;
using System.Reflection;

namespace Kotova.Test1.ClientSide
{
    internal static class ConfigurationClass
    {
        internal static readonly IConfiguration _configuration; // TODO: check if removing the readonly - does it change the behavior? cause readonly is thread-safe.
        public static readonly string BASE_URL_DEVELOPMENT;
        public static readonly string BASE_INSTRUCTIONS_URL_DEVELOPMENT;
        public static readonly string BASE_TASK_URL_DEVELOPMENT;
        public static readonly string BASE_SIGNALR_CONNECTION_URL_DEVELOPMENT;
        public static readonly string BASE_VERSION;
        public static readonly string BASE_VERSION_FILEPATH;
        public static readonly string BASE_FILEPATH_WHERE_DOWNLOAD_UPDATE_FROM;
        public const string DEFAULT_PATH_TO_INITIAL_INSTRUCTIONS = @"C:\Initial\Folder\Path";

        static ConfigurationClass()
        {
            try
            {
                // Get the directory where the executable is located
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string configPath = Path.Combine(baseDirectory, "appsettings.json");

                // Verify the configuration file exists
                if (!File.Exists(configPath))
                {
                    throw new FileNotFoundException(
                        $"Configuration file not found at: {configPath}\n" +
                        "Please ensure appsettings.json is set to 'Copy to Output Directory' in Visual Studio.");
                }

                // Build configuration
                var builder = new ConfigurationBuilder()
                    .SetBasePath(baseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                _configuration = builder.Build();

                // 2. Initialize your static fields based on configuration:
                var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
                if (environment == "Development")
                {
                    BASE_URL_DEVELOPMENT = GetBaseUrl(true) ?? throw new InvalidOperationException("Not configured GetBaseUrl(true)"); ;
                    BASE_VERSION_FILEPATH = GetInternalVersionFilePath(true) ?? throw new InvalidOperationException("Not configured GetInternalVersionFilePath(true)"); ;
                    BASE_FILEPATH_WHERE_DOWNLOAD_UPDATE_FROM = GetInternalFilePath(true) ?? throw new InvalidOperationException("Not configured GetInternalFilePath(true)"); ;

                }
                else
                {
                    BASE_URL_DEVELOPMENT = GetBaseUrl() ?? throw new InvalidOperationException("Not configured GetBaseUrl()"); ;
                    BASE_VERSION_FILEPATH = GetInternalVersionFilePath() ?? throw new InvalidOperationException("Not configured GetInternalVersionFilePath()"); ;
                    BASE_FILEPATH_WHERE_DOWNLOAD_UPDATE_FROM = GetInternalFilePath() ?? throw new InvalidOperationException("Not configured GetInternalFilePath()"); ;
                }

                BASE_VERSION = GetInternalVersion() ?? throw new InvalidOperationException("Not configured GetInternalVersion()"); ;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading configuration: " + ex.Message, "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }

            BASE_INSTRUCTIONS_URL_DEVELOPMENT = BASE_URL_DEVELOPMENT + "/Instructions";
            BASE_TASK_URL_DEVELOPMENT = BASE_URL_DEVELOPMENT + "/Tasks";
            BASE_SIGNALR_CONNECTION_URL_DEVELOPMENT = BASE_URL_DEVELOPMENT + "/notificationHub";
        }

        internal static string GetBaseUrl(bool isDevelopmentMode = false)
        {
            if (isDevelopmentMode)
            {
                return _configuration["ServerURLs:Development"] ?? throw new InvalidOperationException("Development URL not configured.");
            }
            return _configuration["ServerURLs:Release"] ?? throw new InvalidOperationException("Release URL not configured.");
        }

        internal static string GetInternalVersion(bool isDevelopmentMode = false)
        {
            return _configuration["Version"] ?? throw new InvalidOperationException("Release internal version not configured.");
        }

        internal static string GetInternalVersionFilePath(bool isDevelopmentMode = false)
        {
            if (isDevelopmentMode)
            {
                return _configuration["ServerVersionInternalPath:Development"] ?? throw new InvalidOperationException("Development internal version PATH not configured.");
            }
            return _configuration["ServerVersionInternalPath:Release"] ?? throw new InvalidOperationException("Release internal version PATH not configured.");
        }

        internal static string GetInternalFilePath(bool isDevelopmentMode = false)
        {
            if (isDevelopmentMode)
            {
                return _configuration["ServerInternalFilePath:Development"] ?? throw new InvalidOperationException("Development internal file PATH not configured.");
            }
            return _configuration["ServerInternalFilePath:Release"] ?? throw new InvalidOperationException("Release internal file PATH not configured.");
        } 
    }
}
