using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DocumentFormat.OpenXml.Spreadsheet;
using Kotova.CommonClasses;
using ShellLink;

using File = System.IO.File;

namespace Kotova.Test1.ClientSide
{
    internal static class Program
    {
        private static Mutex mutex = null;

        [STAThread]
        public static void Main(string[] args)
        {
            if (IsUpdateInProgress())
            {
                MessageBox.Show("Обновление программы в процессе. Пожалуйста, дождитесь окончания обновления.");
                return;
            }

            if (args.Length > 0)
            {
                string oldExePath = args[0];

                int oldProcessId = -1;
                if (args.Length > 1)
                {
                    int.TryParse(args[1], out oldProcessId);
                }

                // Wait for the old process to exit
                if (oldProcessId > 0)
                {
                    try
                    {
                        Process oldProcess = Process.GetProcessById(oldProcessId);
                        oldProcess.WaitForExit();
                    }
                    catch (ArgumentException)
                    {
                        // Process has already exited
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error while waiting for old process to exit: {ex.Message}");
                    }
                }

                // Try to delete the old executable
                DeleteOldApplication(oldExePath);
                CreateNewShortCutForApplicationInTheOldPathAndDesktop(oldExePath);
            }

            string targetDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Lynks");
            string targetPath = Path.Combine(targetDirectory, Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName));

            // Check if the current path matches the target path
            if (!string.Equals(AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar), targetDirectory, StringComparison.OrdinalIgnoreCase))
            {
                // Ensure the target directory exists
                Directory.CreateDirectory(targetDirectory);

                // Copy the executable to the target directory
                string currentPath = Process.GetCurrentProcess().MainModule.FileName;
                System.IO.File.Copy(currentPath, targetPath, true);
                DeleteAllShortcutsInCurrentDirectoryAndDesktop(Path.GetDirectoryName(currentPath));

                int oldProcessId = Process.GetCurrentProcess().Id;

                // Start the application from the target directory
                Process.Start(new ProcessStartInfo
                {
                    FileName = targetPath,
                    Arguments = $"\"{Application.ExecutablePath}\" {oldProcessId}",
                    UseShellExecute = false
                });

                // Exit the current instance
                Environment.Exit(0);
            }

            var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

            if (environment == "Development")
            {
                MessageBox.Show("Application is running in Development mode", "Development Mode", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            string currentExePath = Application.ExecutablePath;

            VersionInfo embeddedVersionInfo = GetEmbeddedVersionInfo();

            if (environment == "Development")
            {
                MessageBox.Show($"Embedded version of file is {embeddedVersionInfo.version}");
            }

            string externalJsonPath = embeddedVersionInfo.versionPath;

            VersionInfo embeddedVersionInfoFromServer = GetEmbeddedVersionInfoFromJson(externalJsonPath);
            if (embeddedVersionInfoFromServer != null)
            {
                if (environment == "Development")
                {
                    MessageBox.Show($"Version in external version.json: {embeddedVersionInfoFromServer.version}");
                }

                if (!string.IsNullOrEmpty(embeddedVersionInfo.filePath))
                {
                    if (!File.Exists(embeddedVersionInfo.filePath))
                    {
                        if (environment == "Development")
                        {
                            MessageBox.Show("File under 'filepath' from json file wasn't found!");
                        }
                    }
                    else
                    {
                        if (IsNewerVersion(embeddedVersionInfoFromServer.version, embeddedVersionInfo.version))
                        {
                            MessageBox.Show("Доступна новая версия приложения. Скачиваем и обновляем...");

                            string newFilePath = UpdateApplication(embeddedVersionInfo.filePath, embeddedVersionInfoFromServer.version);
                            if (newFilePath != null)
                            {
                                // The application will restart itself after the update
                                return;
                            }
                        }
                    }
                }
            }
            else
            {
                if (environment == "Development")
                {
                    MessageBox.Show("Не удалось загрузить версию из файла .json с сервера для обновления программы.");
                }
            }

            const string mutexName = "LynksApp";
            bool isNewInstance = false;

            mutex = new Mutex(true, mutexName, out isNewInstance);

            if (!isNewInstance)
            {
                NotifyExistingInstance(); // Notify the existing instance and exit
                return;
            }

            // Start the named pipe server to listen for further instances
            StartNamedPipeServer();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ApplicationConfiguration.Initialize();

            Login_Russian loginForm = new Login_Russian();
            Application.Run(loginForm);

            GC.KeepAlive(mutex);
        }

        

        private static void StartNamedPipeServer()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    using (var server = new NamedPipeServerStream("LynksAppPipe", PipeDirection.In))
                    {
                        server.WaitForConnection();
                        // Signal received from another instance; bring the main window to front
                        Login_Russian.Instance?.ShowForm();
                        server.Disconnect();
                    }
                }
            });
        }

        private static void NotifyExistingInstance()
        {
            try
            {
                using (var client = new NamedPipeClientStream(".", "LynksAppPipe", PipeDirection.Out))
                {
                    client.Connect(1000); // Timeout of 1 second
                    // Successfully notified the existing instance
                }
            }
            catch (TimeoutException)
            {
                // Could not connect to the existing instance in time
                MessageBox.Show("Failed to notify the existing instance.");
            }
        }

        public class VersionInfo
        {
            public string version { get; set; }
            public string versionPath { get; set; }
            public string filePath { get; set; }
        }

        public static VersionInfo GetEmbeddedVersionInfo()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Kotova.Test1.ClientSide.version.json"; // Replace with the actual namespace + filename

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string jsonContent = reader.ReadToEnd();
                        return JsonSerializer.Deserialize<VersionInfo>(jsonContent);
                    }
                }
            }

            return null; // Return null if the resource is not found
        }

        // Method to extract and read the embedded version.json from another .exe
        public static VersionInfo GetEmbeddedVersionInfoFromJson(string jsonFilePath)
        {
            if (File.Exists(jsonFilePath))
            {
                string jsonContent = System.IO.File.ReadAllText(jsonFilePath);
                VersionInfo versionInfo = JsonSerializer.Deserialize<VersionInfo>(jsonContent);

                // Output the version info
                return versionInfo;
            }
            else
            {
                return null;
            }
        }

        private static bool IsNewerVersion(string remoteVersion, string localVersion)
        {
            Version remote = new Version(remoteVersion);
            Version local = new Version(localVersion);
            return remote > local;
        }

        private static string UpdateApplication(string fileUrl, string version)
        {
            CreateUpdateLock();

            string newAppPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"LynKS (v{version}).exe");
            UpdateProgressForm progressForm = new UpdateProgressForm();

            try
            {
                progressForm.SetMessage("Скачивается обновление. Пожалуйста, подождите...");
                progressForm.Show();

                // Use WebClient or HttpClient to download the file from the server
                WebClient client = new WebClient();

                client.DownloadProgressChanged += (s, e) =>
                {
                    progressForm.UpdateProgress(e.ProgressPercentage);
                };

                client.DownloadFileCompleted += (s, e) =>
                {
                    if (e.Error != null)
                    {
                        MessageBox.Show($"Не удалось скачать обновление: {e.Error.Message}");
                        progressForm.Close();
                        RemoveUpdateLock();
                        return;
                    }

                    progressForm.SetMessage("Обновление скачено и установлено. Перезапускаю...");
                    Thread.Sleep(1000); // Wait to show the message
                    progressForm.Close();
                    RemoveUpdateLock();

                    TerminateOldInstances(Application.ExecutablePath);

                    // Start the new application
                    RestartApplication(newAppPath, Application.ExecutablePath);
                    Process.GetCurrentProcess().Kill();
                    Application.Exit();
                };

                // Start the download
                client.DownloadFileAsync(new Uri(fileUrl), newAppPath);

                // Keep the application running until the download completes
                Application.Run(progressForm);

                return newAppPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось обновить приложение: {ex.Message}");
                progressForm.Close();
                RemoveUpdateLock();
                return null;
            }
        }

        public static void RestartApplication(string newVersionFilePath, string currentExePath)
        {
            int oldProcessId = Process.GetCurrentProcess().Id;

            // Start the new version of the application
            Process.Start(new ProcessStartInfo
            {
                FileName = newVersionFilePath,
                Arguments = $"\"{currentExePath}\" {oldProcessId}",
                UseShellExecute = false
            });

            // Optionally: Delete the old executable after exiting the current application
        }

        public static void DeleteOldApplication(string oldExePath)
        {
            try
            {
                if (File.Exists(oldExePath))
                {
                    File.Delete(oldExePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete old application: {ex.Message}");
            }
        }

        private static void TerminateOldInstances(string currentExePath)
        {
            try
            {
                string currentProcessName = Path.GetFileNameWithoutExtension(currentExePath);
                int currentProcessId = Process.GetCurrentProcess().Id;

                // Get all processes with the same name
                var processes = Process.GetProcessesByName(currentProcessName);

                foreach (var process in processes)
                {
                    if (process.Id != currentProcessId)
                    {
                        // Try to close gracefully first
                        process.CloseMainWindow();
                        if (!process.WaitForExit(1000)) // Wait for 1 second
                        {
                            process.Kill();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to terminate old instances: {ex.Message}");
            }
        }

        public static bool IsUpdateInProgress()
        {
            string lockFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "update.lock");
            return File.Exists(lockFilePath);
        }

        public static void CreateUpdateLock()
        {
            string lockFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "update.lock");
            File.Create(lockFilePath).Dispose();
        }

        public static void RemoveUpdateLock()
        {
            string lockFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "update.lock");
            if (File.Exists(lockFilePath))
            {
                File.Delete(lockFilePath);
            }
        }

        private static void DeleteAllShortcutsInCurrentDirectoryAndDesktop(string currentPath)
        {
            try
            {
                // Get all .lnk (shortcut) files in the current directory
                string[] shortcutFilesOldPath = Directory.GetFiles(currentPath, "*.lnk");
                string[] shortcutFilesDesktop = Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "*.lnk");
                string[] shortcutFiles = shortcutFilesOldPath.Concat(shortcutFilesDesktop).ToArray();

                foreach (string shortcutFile in shortcutFiles)
                {
                    string fileName = Path.GetFileName(shortcutFile).ToLower();

                    // Check if the file name contains "kotova" or "lynks"
                    if (fileName.Contains("Kotova") || fileName.Contains("Lynks"))
                    {
                        try
                        {
                            File.Delete(shortcutFile);
                        }
                        catch (Exception ex)
                        {
                            // Log or handle individual file deletion exceptions
                            MessageBox.Show($"Error deleting {shortcutFile}: {ex.Message}");
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                // Handle exceptions related to directory access
                MessageBox.Show($"Error accessing directory {currentPath}: {ex.Message}");
            }
        }
        private static void CreateNewShortCutForApplicationInTheOldPathAndDesktop(string oldExePath)
        {
            try
            {
                string currentExePath = Process.GetCurrentProcess().MainModule.FileName;
                // Get the application name without extension
                string appName = Path.GetFileNameWithoutExtension(oldExePath);

                // Define the shortcut file name
                string shortcutName = appName + ".lnk";

                // Get the directory of the old executable
                string oldPathDirectory = Path.GetDirectoryName(oldExePath);

                // Paths for the shortcuts
                string oldPathShortcut = Path.Combine(oldPathDirectory, shortcutName);
                string desktopShortcut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), shortcutName);

                // Create the shortcut in the current path directory
                ShellLink.Shortcut.CreateShortcut(currentExePath)
                    .WriteToFile(oldPathShortcut);
                if (oldPathShortcut == desktopShortcut)
                {
                    return; //TODO: Проверь работает ли эта штука!
                            //Пропуск что если файл лежал на рабочем столе, то не нужно создавать на рабочем столе 2 ярлыка (потому что папки совпадают)
                }
                // Create the shortcut on the desktop
                ShellLink.Shortcut.CreateShortcut(currentExePath)
                    .WriteToFile(desktopShortcut);

                Console.WriteLine("Shortcuts have been created successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating shortcuts: {ex.Message}");
            }
        }


    }

}
