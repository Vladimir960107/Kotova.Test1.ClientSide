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
        private static readonly IConfiguration _configuration;
        public static readonly string BASE_URL_DEVELOPMENT;
        public static readonly string BASE_INSTRUCTIONS_URL_DEVELOPMENT;
        public static readonly string BASE_TASK_URL_DEVELOPMENT;
        public static readonly string BASE_SIGNALR_CONNECTION_URL_DEVELOPMENT;
        public const string DEFAULT_PATH_TO_INITIAL_INSTRUCTIONS = @"C:\Initial\Folder\Path";

        static ConfigurationClass()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "Kotova.Test1.ClientSide.appsettings.json"; // Adjust this to match your namespace and file TODO: Переименовать эту хрень, ну или использовать не self-contained file.

                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                        throw new FileNotFoundException($"Embedded resource '{resourceName}' not found.");

                    using (StreamReader reader = new StreamReader(stream))
                    {
                        var jsonString = reader.ReadToEnd();
                        var builder = new ConfigurationBuilder()
                            .AddJsonStream(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonString)));

                        _configuration = builder.Build();
                    }
                }

                BASE_URL_DEVELOPMENT = GetBaseUrlDevelopment();

                BASE_INSTRUCTIONS_URL_DEVELOPMENT = BASE_URL_DEVELOPMENT + "/Instructions";
                BASE_TASK_URL_DEVELOPMENT = BASE_URL_DEVELOPMENT + "/Tasks";
                BASE_SIGNALR_CONNECTION_URL_DEVELOPMENT = BASE_URL_DEVELOPMENT + "/notificationHub";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading configuration: " + ex.Message, "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


            BASE_URL_DEVELOPMENT = GetBaseUrlDevelopment();

            BASE_INSTRUCTIONS_URL_DEVELOPMENT = BASE_URL_DEVELOPMENT + "/Instructions";
            BASE_TASK_URL_DEVELOPMENT = BASE_URL_DEVELOPMENT + "/Tasks";
            BASE_SIGNALR_CONNECTION_URL_DEVELOPMENT = BASE_URL_DEVELOPMENT + "/notificationHub";
        }

        private static string GetBaseUrlDevelopment()
        {
            //return _configuration["ServerURLs:Development"] ?? throw new InvalidOperationException("Development URL not configured."); // ЭТО ДЛЯ РАЗРАБОТКИ!
            return _configuration["ServerURLs:Release"] ?? throw new InvalidOperationException("Release URL not configured."); // ЭТО ДЛЯ РЕЛИЗА!
        }
    }
}
