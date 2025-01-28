using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DocumentFormat.OpenXml.Spreadsheet;
using Kotova.CommonClasses;
using Microsoft.Extensions.Configuration;
using ShellLink;
using System.Linq;

using File = System.IO.File;

namespace Kotova.Test1.ClientSide
{
    internal static class Program
    {
        private static Mutex mutex = null;

        [STAThread]
        public static void Main(string[] args)
        { 
            [DllImport("kernel32.dll")]
            static extern bool AllocConsole();


            var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

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
                    finally
                    {
                        try
                        {
                            Process oldProcess = Process.GetProcessById(oldProcessId);
                            if (!oldProcess.HasExited)
                            {
                                oldProcess.Kill(); // Or decide how you want to handle this scenario
                                oldProcess.WaitForExit();
                            }
                        }
                        catch (ArgumentException)
                        {
                            // Process does not exist
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error while ensuring old process has exited: {ex.Message}");
                        }

                        DeleteOldApplication(oldExePath);
                        DeleteAllShortcutsInCurrentDirectoryAndDesktop(Path.GetDirectoryName(oldExePath));
                        CreateNewShortCutForApplicationInTheOldPathAndDesktop(oldExePath);
                    }

                }

                // Try to delete the old executable

            }

            VersionInfo embeddedVersionInfo = GetEmbeddedVersionInfo();

            if (embeddedVersionInfo is null)
            {
                MessageBox.Show("embeddedVersionInfo is null");
                throw new Exception("embeddedVersionInfo is null");
            }

            string currentExePath = Application.ExecutablePath;
            string currentExeDirectory = Path.GetDirectoryName(currentExePath);
            string serverExeDirectory = Path.GetDirectoryName(embeddedVersionInfo.ServerInternalFilePath);

            // Compare directories in a case-insensitive way.
            // Also check if appsettings.json is present in the same folder.
            if (IsSameDirectory(currentExeDirectory, serverExeDirectory) &&
                File.Exists(Path.Combine(currentExeDirectory, "appsettings.json")))
            {
                // We are running from the server share location
                CopyExeAndJsonToLocalFolderAndRestart();
                return; // So we don't continue the rest of Main
            }

            if (environment != "Development")
            {
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
            }



            if (environment == "Development")
            {
                MessageBox.Show("Application is running in Development mode", "Development Mode", MessageBoxButtons.OK, MessageBoxIcon.Information);
                AllocConsole();
                Console.WriteLine("Development Console is enabled.");
            }

            

            

            string externalJsonPath = embeddedVersionInfo.ServerVersionInternalPath;

            VersionAndReleaseDateInfo embeddedVersionInfoFromServer = VersionChecker.GetEmbeddedVersionInfoFromJson(externalJsonPath);
            if (embeddedVersionInfoFromServer != null)
            {
                if (environment == "Development")
                {
                    MessageBox.Show($"Version in external version.json: {embeddedVersionInfoFromServer.Version}");
                }

                if (!string.IsNullOrEmpty(embeddedVersionInfo.ServerInternalFilePath))
                {
                    if (!File.Exists(embeddedVersionInfo.ServerInternalFilePath))
                    {
                        if (environment == "Development")
                        {
                            MessageBox.Show("File under 'filepath' from json file wasn't found!");
                        }
                    }
                    else
                    {
                        if (IsNewerVersion(embeddedVersionInfoFromServer.Version, embeddedVersionInfo.Version))
                        {
                            // Ask the user if they want to download and install the update
                            MessageBox.Show(
                                "Доступна новая версия приложения. Скачиваем и обновляем.",
                                "Обновление"
                            );
                                // The user confirmed to update
                            if (embeddedVersionInfo.ServerVersionInternalPath == null)
                            {
                                MessageBox.Show("embeddedVersionInfo.ServerInternalFilePath is null");
                                throw new Exception("embeddedVersionInfo.ServerInternalFilePath is null");
                            }

                            string newFilePath = UpdateApplication(
                                    embeddedVersionInfo.ServerInternalFilePath,
                                    embeddedVersionInfoFromServer.Version
                                );

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

        private static bool IsSameDirectory(string dir1, string dir2)
        {
            if (string.IsNullOrEmpty(dir1) || string.IsNullOrEmpty(dir2))
                return false;

            // Normalize (remove trailing slashes, etc.)
            dir1 = Path.GetFullPath(dir1).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            dir2 = Path.GetFullPath(dir2).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            return dir1.Equals(dir2, StringComparison.OrdinalIgnoreCase);
        }

        private static void CopyExeAndJsonToLocalFolderAndRestart()
        {
            try
            {
                // 1. Identify the local Lynks folder
                string targetDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "Lynks"
                );
                Directory.CreateDirectory(targetDirectory);

                // 2. Identify the current EXE and its folder
                string currentExePath = Application.ExecutablePath;
                string currentExeDirectory = Path.GetDirectoryName(currentExePath);
                string exeName = Path.GetFileName(currentExePath);

                // 3. Copy the EXE
                string targetExePath = Path.Combine(targetDirectory, exeName);
                File.Copy(currentExePath, targetExePath, true);

                // 4. Copy appsettings.json
                string sourceJsonPath = Path.Combine(currentExeDirectory, "appsettings.json");
                string targetJsonPath = Path.Combine(targetDirectory, "appsettings.json");

                if (File.Exists(sourceJsonPath))
                {
                    File.Copy(sourceJsonPath, targetJsonPath, true);
                }

                // 5. Start the app from local folder
                Process.Start(new ProcessStartInfo
                {
                    FileName = targetExePath,
                    // If you want to pass any arguments, add them here, e.g.:
                    // Arguments = "someArg if needed",
                    UseShellExecute = false
                });

                // 6. Close the current instance immediately
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error copying files to local folder: {ex.Message}");
                // Possibly handle error or just run normally
            }
        }

        private static VersionInfo GetEmbeddedVersionInfo()
        {
            var embeddedVersionInfo = new VersionInfo()
            {
                Version = ConfigurationClass.BASE_VERSION,
                ServerVersionInternalPath = ConfigurationClass.BASE_VERSION_FILEPATH,
                ServerInternalFilePath = ConfigurationClass.BASE_FILEPATH_WHERE_DOWNLOAD_UPDATE_FROM
            };
            return embeddedVersionInfo;
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
            public string Version { get; set; }
            public string ServerVersionInternalPath { get; set; }
            public string ServerInternalFilePath { get; set; }
        }

        public class AppSettings
        {
            public ServerURLs ServerURLs { get; set; }
            public string Version { get; set; }
            public string ReleaseDate { get; set; }
            public ServerPaths ServerVersionInternalPath { get; set; }
            public ServerPaths ServerInternalFilePath { get; set; }
        }

        public class ServerURLs
        {
            public string Development { get; set; }
            public string Release { get; set; }
        }

        public class ServerPaths
        {
            public string Development { get; set; }
            public string Release { get; set; }
        }

        public class VersionAndReleaseDateInfo
        {
            public string Version { get; set; }
            public string ReleaseDate { get; set; }
        }

        public static class VersionChecker
        {
            public static AppSettings LoadAppSettings(string appSettingsFilePath)
            {
                if (File.Exists(appSettingsFilePath))
                {
                    try
                    {
                        string jsonContent = File.ReadAllText(appSettingsFilePath);
                        return JsonSerializer.Deserialize<AppSettings>(jsonContent);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reading or deserializing appsettings.json: {ex.Message}");
                        return null;
                    }
                }
                else
                {
                    Console.WriteLine("appsettings.json file not found.");
                    return null;
                }
            }

            public static VersionAndReleaseDateInfo GetEmbeddedVersionInfoFromJson(string jsonFilePath)
            {
                if (File.Exists(jsonFilePath))
                {
                    try
                    {
                        string jsonContent = File.ReadAllText(jsonFilePath);
                        return JsonSerializer.Deserialize<VersionAndReleaseDateInfo>(jsonContent);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reading or deserializing version file: {ex.Message}");
                        return null;
                    }
                }
                else
                {
                    Console.WriteLine("Version file not found.");
                    return null;
                }
            }

            /*public static void CheckVersion(string appSettingsFilePath, string environment) // NOT CURRENTLY IN USE, SO COMMENTED. IN CASE OF NEED - CAN DE-COMMENT AND USE :)
            {
                // Load appsettings.json
                var appSettings = LoadAppSettings(appSettingsFilePath);
                if (appSettings == null)
                {
                    Console.WriteLine("Failed to load appsettings.json.");
                    return;
                }

                // Determine the version file path based on the environment
                string versionFilePath = environment switch
                {
                    "Development" => appSettings.ServerVersionInternalPath.Development,
                    "Release" => appSettings.ServerVersionInternalPath.Release,
                    _ => null
                };

                if (string.IsNullOrEmpty(versionFilePath))
                {
                    Console.WriteLine($"No version file path found for environment: {environment}");
                    return;
                }

                // Get version info from the version file
                var versionInfo = GetEmbeddedVersionInfoFromJson(versionFilePath);
                if (versionInfo != null)
                {
                    Console.WriteLine($"Version: {versionInfo.Version}");
                    Console.WriteLine($"Release Date: {versionInfo.ReleaseDate}");
                }
                else
                {
                    Console.WriteLine("Could not retrieve version information.");
                }
            }*/
        }



        private static bool IsNewerVersion(string remoteVersion, string localVersion)
        {
            if (remoteVersion == null || remoteVersion.Length == 0)
            {
                MessageBox.Show("Удалённая версия файла сервера не найдена. (Скорее всего нет подключения к удалённому серверу)");
                return false;
            }
            Version remote = new Version(remoteVersion);
            Version local = new Version(localVersion);
            return remote > local;
        }

        private static string? UpdateApplication(string fileUrl, string version)
        {
            CreateUpdateLock();

            string applicationDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string newAppFileName = $"LynKS (v{version}).exe";
            string newAppPath = Path.Combine(applicationDirectory, newAppFileName);

            // Adjust if your appsettings file name or location differs:
            string newAppSettingsName = "appsettings.json";
            string newAppSettingsPath = Path.Combine(applicationDirectory, newAppSettingsName);

            UpdateProgressForm progressForm = new UpdateProgressForm();

            try
            {
                progressForm.SetMessage("Скачивается обновление. Пожалуйста, подождите...");
                progressForm.Show();

                // --- STEP 1: Download the EXE file ---
                WebClient exeClient = new WebClient();

                exeClient.DownloadProgressChanged += (s, e) =>
                {
                    progressForm.UpdateProgress(e.ProgressPercentage);
                };

                // When the EXE completes, we begin the JSON download
                exeClient.DownloadFileCompleted += (s, e) =>
                {
                    if (e.Error != null)
                    {
                        MessageBox.Show($"Не удалось скачать EXE-файл: {e.Error.Message}");
                        progressForm.Close();
                        RemoveUpdateLock();
                        return;
                    }

                    // --- STEP 2: Now download the appsettings.json ---
                    progressForm.SetMessage("EXE-файл загружен. Скачивается appsettings.json...");
                    progressForm.UpdateProgress(0);

                    WebClient jsonClient = new WebClient();
                    jsonClient.DownloadProgressChanged += (snd, evt) =>
                    {
                        // You could reuse the same progress bar for the JSON download
                        progressForm.UpdateProgress(evt.ProgressPercentage);
                    };

                    jsonClient.DownloadFileCompleted += (snd, evt) =>
                    {
                        if (evt.Error != null)
                        {
                            MessageBox.Show($"Не удалось скачать appsettings.json: {evt.Error.Message}");
                            progressForm.Close();
                            RemoveUpdateLock();
                            return;
                        }

                        // --- STEP 3: Both files are now downloaded. Proceed with update steps ---
                        progressForm.SetMessage("Обновление скачено и установлено. Перезапуск...");
                        Thread.Sleep(1000); // Show the message briefly
                        progressForm.Close();

                        // Delete old version files
                        DeleteOldVersionFiles(applicationDirectory, newAppFileName);

                        RemoveUpdateLock();

                        // Terminate old instances
                        TerminateOldInstances(Application.ExecutablePath);

                        // Start the new application
                        RestartApplication(newAppPath, Application.ExecutablePath);

                        // Kill current process to ensure a clean restart
                        Process.GetCurrentProcess().Kill();
                        Application.Exit();
                    };

                    // Actually start downloading the JSON
                    // You might build the URL differently if the JSON is on a different path.
                    // For instance: string jsonUrl = fileUrl.Replace(".exe", ".json");
                    // or pass the JSON URL as a parameter. Here, assume same server folder:
                    string jsonUrl = fileUrl.Replace("Kotova.Test1.ClientSide.exe", "appsettings.json");
                    jsonClient.DownloadFileAsync(new Uri(jsonUrl), newAppSettingsPath);
                };

                // Start downloading the EXE
                exeClient.DownloadFileAsync(new Uri(fileUrl), newAppPath);

                // Keep the application running until the downloads complete
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
                    if (fileName.Contains("kotova") || fileName.Contains("lynks"))
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

                //MessageBox.Show("Shortcuts have been created successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating shortcuts: {ex.Message}");
            }
        }

        private static void DeleteOldVersionFiles(string applicationDirectory, string newVersionFileName)
        {
            try
            {
                // Get all .exe files in the application directory
                string[] exeFiles = Directory.GetFiles(applicationDirectory, "*.exe");

                foreach (string exeFile in exeFiles)
                {
                    string fileName = Path.GetFileName(exeFile);

                    // Skip the new version executable
                    if (string.Equals(fileName, newVersionFileName, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    // Skip the current running executable
                    string currentExeName = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
                    if (string.Equals(fileName, currentExeName, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    // Optionally, check if the file matches a versioned executable pattern
                    if ((fileName.StartsWith("LynKS (v", StringComparison.OrdinalIgnoreCase) && fileName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))||(fileName.StartsWith("Kotova") && fileName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)))
                    {
                        // Delete the old version file
                        try
                        {
                            File.Delete(exeFile);
                        }
                        catch (Exception ex)
                        {
                            // Handle exception (e.g., log the error or show a message)
                            MessageBox.Show($"Error deleting old version file {exeFile}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions related to directory access
                MessageBox.Show($"Error accessing directory {applicationDirectory}: {ex.Message}");
            }
        }



    }

}
