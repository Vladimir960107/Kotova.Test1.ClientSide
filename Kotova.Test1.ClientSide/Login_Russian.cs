using DocumentFormat.OpenXml.Drawing;
using Kotova.CommonClasses;
using Kotova.Test1.ClientSide.ManagementWPF;
using Microsoft.AspNetCore.SignalR.Client;
using System.CodeDom;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows; 
using System.Windows.Forms.Integration;
using System.Windows.Input;
using Windows.UI.WindowManagement;
using static Kotova.Test1.ClientSide.Program;
using static System.Net.Mime.MediaTypeNames;
using MessageBox = System.Windows.Forms.MessageBox;
using SystemColors = System.Drawing.SystemColors;

namespace Kotova.Test1.ClientSide
{
    public partial class Login_Russian : Form
    {

        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly string _loginUrl = ConfigurationClass.BASE_URL_DEVELOPMENT + "/login"; //ADD IT TO THE 
        private static readonly string _validateTokenUrl = ConfigurationClass.BASE_URL_DEVELOPMENT + "/validate-token";
        static string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        static string fileName = "encrypted_jwt.dat";
        string filePath = System.IO.Path.Combine(documentsPath, fileName);
        public string? _jwtToken = null;
        public bool _jwtRemembered = false;
        private TaskCompletionSource<bool> _initTaskCompletionSource;
        private int timeForBeingAuthenticated = 600;


        private DepartmentCache _departmentCache;

        public static Login_Russian Instance { get; private set; }

        public Form? activeForm;
        public Window? activeWpfWindow;
        private NotifyIcon notifyIcon;

        public Login_Russian()
        {
            Instance = this;
            InitializeComponent();
            InitializeSettingsMenu();
            InitializeNotifyIcon();


            _departmentCache = new DepartmentCache();
            _ = InitializeDepartmentCache();  

            try
            {
                versionLabel.Text = ConfigurationClass.BASE_VERSION;
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Error loading version information: {ex.Message}", "Version Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                versionLabel.Text = "Version Unknown";
            }
            activeForm = this;
            this.Text = "Login_Russian";
            this.Activate();
            this.BringToFront();
            this.Load += async (sender, args) => await Login_Russian_LoadAsync(sender, args);
        }

        public void ShowForm()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => ShowForm()));
            }
            else
            {
                // Show either Windows Form or WPF Window
                if (activeForm != null)
                {
                    if (activeForm == this && activeWpfWindow != null) //Made just for MANAGER AND USER, cause their active form = login form and activewpfwindow just his or something. AT LEAST SHOULD BE! CHECK IT!
                    {
                        activeWpfWindow.Show();
                        activeWpfWindow.WindowState = System.Windows.WindowState.Normal;
                        activeWpfWindow.Activate();
                    }
                    else
                    {
                        activeForm.Show();
                        activeForm.WindowState = FormWindowState.Normal;
                        activeForm.ShowInTaskbar = true;
                        activeForm.BringToFront();
                    }
                }
                else if (activeWpfWindow != null)
                {
                    // Restore the WPF window from tray if minimized or hidden
                    activeWpfWindow.Show();
                    activeWpfWindow.WindowState = System.Windows.WindowState.Normal;
                    activeWpfWindow.Activate();
                }
                else
                {
                    // Fallback to showing login form
                    this.Show();
                    this.WindowState = FormWindowState.Normal;
                    this.ShowInTaskbar = true;
                    this.BringToFront();
                }
            }
        }


        private void InitializeNotifyIcon()
        {
            notifyIcon = new NotifyIcon();

            Stream iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(Constants.favicon_path);
            if (iconStream != null)
            {
                using (iconStream)
                {
                    notifyIcon.Icon = new Icon(iconStream);
                }
            }

            notifyIcon.Text = "Система Управление Инструктажей";
            notifyIcon.Visible = true;

            notifyIcon.DoubleClick += NotifyIcon_DoubleClick;

            ContextMenuStrip contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Показать", null, (s, e) => { ShowForm(); });
            contextMenu.Items.Add("Закрыть", null, (s, e) => { ExitApplication(); });
            notifyIcon.ContextMenuStrip = contextMenu;
        }

        private async Task Login_Russian_LoadAsync(object sender, EventArgs e)
        {
            bool initThroughJWTResult = await InitializeAsync();
            if (initThroughJWTResult)
            {
                // Initialization succeeded
                //MessageBox.Show("Initialization succeeded");
                string token = Decryption_stuff.DecryptedJWTToken();
                ProcessSuccessfulAuthentication(token);
            }
            else
            {
                // Initialization failed
                //MessageBox.Show("Initialization failed");
            }


        }

        /*public class VersionInfo // TODO: ИСПРАВЬ КЛАСС НА НОВЫЙ ТИП И ВОЗЬМИ ЕГО ИЗ PROGRAM.CS, (А ТАМ ВОЗЬМИ И СДЕЛАЙ ЕГО В ОБЩИЙ ФАЙЛ!)
        {
            public string version { get; set; }
            public string versionPath { get; set; }
            public string filePath { get; set; }
        }*/

        private static VersionInfo GetEmbeddedVersionInfo() //TODO: Убери, если лишнее.
        {
            // Determine if we're in development mode
            var isDevelopment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "Development";

            try
            {
                var embeddedVersionInfo = new VersionInfo()
                {
                    // Use the existing configuration methods with the development flag
                    Version = ConfigurationClass.GetInternalVersion(isDevelopment),
                    ServerVersionInternalPath = ConfigurationClass.GetInternalVersionFilePath(isDevelopment),
                    ServerInternalFilePath = ConfigurationClass.GetInternalFilePath(isDevelopment)
                };

                return embeddedVersionInfo;
            }
            catch (InvalidOperationException ex)
            {
                // Add more context to the configuration error
                throw new InvalidOperationException($"Failed to initialize version information: {ex.Message}", ex);
            }
        }

        private void InitializeSettingsMenu()
        {
            // Create the ContextMenuStrip
            ContextMenuStrip settingsMenu = new ContextMenuStrip();

            // Create menu items
            ToolStripMenuItem timeToLoginItem = new ToolStripMenuItem("Время сохранения аутентификации");
            ToolStripMenuItem darkThemeItem = new ToolStripMenuItem("Тёмная тема (Пока не работает)");
            ToolStripMenuItem lightThemeItem = new ToolStripMenuItem("Светлая тема (Пока не работает)");

            // Add menu items to the ContextMenuStrip
            settingsMenu.Items.Add(timeToLoginItem);
            settingsMenu.Items.Add(darkThemeItem);
            settingsMenu.Items.Add(lightThemeItem);

            // Attach event handlers
            timeToLoginItem.Click += TimeToLoginItem_Click;
            darkThemeItem.Click += DarkThemeItem_Click;
            lightThemeItem.Click += LightThemeItem_Click;

            // Assign the ContextMenuStrip to the PictureBox
            pictureBox5.ContextMenuStrip = settingsMenu;
        }
        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            ShowForm();
        }

        private void TimeToLoginItem_Click(object sender, EventArgs e)
        {
            using (var dialog = new InputDialog("Введите время (в минутах):"))
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    int? loginTime = dialog.Result;
                    if (loginTime.HasValue)
                    {
                        MessageBox.Show($"время сохранения аутентификации установлено - {loginTime.Value} минут.");
                        timeForBeingAuthenticated = loginTime.Value;
                    }
                }
            }
        }

        private void DarkThemeItem_Click(object sender, EventArgs e)
        {
            // Logic for enabling dark theme
            MessageBox.Show("Тёмная тема включена.");
        }

        private void LightThemeItem_Click(object sender, EventArgs e)
        {
            // Logic for enabling light theme
            MessageBox.Show("Светлая тема включена.");
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            // Show the context menu manually if needed
            pictureBox5.ContextMenuStrip.Show(pictureBox5, new System.Drawing.Point(0, pictureBox5.Height));
        }


        /*private System.Windows.Window CreateWPFManagementWindow(string username, string fullName, string departmentName)
        {
            try
            {
                // For standalone WPF windows, only need basic interop
                WindowsFormsHost.EnableWindowsFormsInterop();

                // Create API service with your existing authentication
                var apiService = new ManagementWPF.Services.ApiService(
                    ConfigurationClass.BASE_URL_DEVELOPMENT + "/api/instructions");
                apiService.SetAuthToken(_jwtToken);

                // Create ViewModel with user info
                var mainViewModel = new ManagementWPF.ViewModels.MainViewModel(apiService, fullName);

                // Create and configure WPF window
                var mainWindow = new ManagementWPF.Views.MainWindow(mainViewModel);

                // Set window properties
                mainWindow.Title = $"Система управления инструктажами - {fullName}";
                mainWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;

                // IMPORTANT: Ensure the window can receive keyboard input
                mainWindow.Focusable = true;
                System.Windows.Input.KeyboardNavigation.SetTabNavigation(mainWindow, System.Windows.Input.KeyboardNavigationMode.Local);

                // Enable Input Method Editor for international characters (Russian)
                System.Windows.Input.InputMethod.SetIsInputMethodEnabled(mainWindow, true);

                // Handle the Loaded event to set initial focus
                mainWindow.Loaded += (s, e) => {
                    // Set focus with delay to ensure window is fully rendered
                    mainWindow.Dispatcher.BeginInvoke(new Action(() => {
                        mainWindow.Focus();
                        System.Windows.Input.Keyboard.Focus(mainWindow);

                        // Try to focus the first text input control
                        var firstTextBox = FindFirstTextBox(mainWindow);
                        if (firstTextBox != null)
                        {
                            firstTextBox.Focus();
                            System.Windows.Input.Keyboard.Focus(firstTextBox);
                        }
                    }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                };

                // Handle window activation to restore focus
                mainWindow.Activated += (s, e) => {
                    mainWindow.Focus();
                };

                // Handle window closing to return to login
                mainWindow.Closed += (s, e) => {
                    this.ShowForm();
                };

                return mainWindow;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка запуска системы управления: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }*/



        private void LaunchWPFManagementApplication(string username, string fullName, string departmentName)
        {
            try
            {
                // Hide the current form
                this.Hide();

                // Check if there's already a WPF Application running
                var wpfApp = CommonFunctions.GetOrCreateWpfApp();

                // Create API service
                var apiService = new ManagementWPF.Services.ApiService(
                    ConfigurationClass.BASE_URL_DEVELOPMENT + "/api/instructions");
                apiService.SetAuthToken(_jwtToken);

                // Create ViewModel with login form reference for sign out
                var mainViewModel = new ManagementWPF.ViewModels.MainViewModel(apiService, fullName, this);

                // Create main window
                var mainWindow = new ManagementWPF.Views.MainWindow(mainViewModel);
                mainWindow.Title = $"Система управления инструктажами - {fullName}";

                // Handle window events
                mainWindow.OnWindowHidden += () =>
                {
                    this.Invoke(new Action(() =>
                    {
                        this.ShowForm();
                    }));
                };

                // Handle when the window is closed normally
                mainWindow.Closed += (s, e) =>
                {
                    // This will be called when window is properly closed
                    this.Invoke(new Action(() =>
                    {
                        this.ShowForm();
                    }));
                };

                // Show the window
                mainWindow.Show();

                // Store reference for potential future use
                activeWpfWindow = mainWindow;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка запуска системы управления: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.ShowForm();
            }
        }



        private void OpenFormBasedOnRole(string role, string username, string fullName, string departmentName)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => OpenFormBasedOnRole(role, username, fullName, departmentName)));
                return;
            }

            Form formToOpen = null;
            Window wpfWindow = null;

            switch (role)
            {
                case "User":
                    // STEP 1: Launch WPF InstructionViewerWindow instead of UserForm
                    LaunchWPFInstructionViewer(username, fullName, departmentName, isChief: false);
                    return; // Return early since we're handling WPF differently

                case "ChiefOfDepartment":
                    // STEP 1: Launch WPF InstructionViewerWindow for chiefs too
                    formToOpen = new ChiefForm(this, username, fullName, departmentName);
                    break;

                case "Coordinator":
                    formToOpen = new CoordinatorForm(this, username, fullName, departmentName);
                    break;

                case "DeputyChief":
                    // STEP 1: Launch WPF InstructionViewerWindow for deputy chiefs
                    formToOpen = new ChiefForm(this, username, fullName, departmentName);
                    break;

                case "Management":
                    LaunchWPFManagementApplication(username, fullName, departmentName);
                    return; // Return early since this is already WPF

                case "Admin":
                    formToOpen = new AdminForm(this, username, fullName);
                    break;

                default:
                    MessageBox.Show($"Упс, роль '{role}' не валидна. Попросите кого-то из тех. поддержки разрешить ситуацию :I");
                    return;
            }

            // Handle remaining Windows Forms (Coordinator, Admin)
            if (formToOpen != null)
            {
                activeForm = formToOpen;          // Set Windows Forms active form
                activeWpfWindow = null;           // Clear WPF window reference
                formToOpen.Location = this.Location;
                this.Hide();
                formToOpen.Show();

                // Check if this is a default username after a delay
                if (isDefaultUsername(username))
                {
                    Task.Run(async () =>
                    {
                        await Task.Delay(1000);

                        this.Invoke(new Action(() =>
                        {
                            if (formToOpen is CoordinatorForm coordinatorForm && coordinatorForm._signUpForm != null)
                                coordinatorForm._signUpForm.Show();
                            /*else if (formToOpen is AdminForm adminForm && adminForm._signUpForm != null)
                                adminForm._signUpForm.Show();*/
                        }));
                    });
                }
            }
        }

        /// <summary>
        /// Launches the WPF InstructionViewerWindow for Users, Chiefs, and Deputy Chiefs
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="fullName">The full name</param>
        /// <param name="departmentName">The department name</param>
        /// <param name="isChief">Whether the user is a chief/deputy chief</param>
        private void LaunchWPFInstructionViewer(string username, string fullName, string departmentName, bool isChief = false)
        {
            try
            {
                // Ensure WPF interop is enabled
                WindowsFormsHost.EnableWindowsFormsInterop();

                // Create the enhanced WPF instruction viewer window
                var wpfWindow = new InstructionViewerWindow(
                    jwtToken: _jwtToken,
                    userName: fullName, // Use full name for display
                    isChief: isChief,
                    allowCompletion: true, // Allow chiefs to complete instructions if needed
                    loginForm: this,
                    signUpForm: null // Will be created after window is shown
                );

                // Set window properties
                wpfWindow.Title = $"Система управления инструктажами - {fullName}";
                wpfWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;

                // Center over login form if possible
                if (this.Visible)
                {
                    wpfWindow.Left = this.Location.X + (this.Width - wpfWindow.Width) / 2;
                    wpfWindow.Top = this.Location.Y + (this.Height - wpfWindow.Height) / 2;
                }

                // Handle WPF window closed event
                wpfWindow.Closed += (sender, e) =>
                {
                    // When WPF window closes, show login form again
                    this.Invoke(new Action(() =>
                    {
                        activeForm = this;           // Set login form as active
                        activeWpfWindow = null;      // Clear WPF window reference
                        this.Show();
                        this.WindowState = FormWindowState.Normal;
                        this.BringToFront();
                    }));
                };

                // Show the WPF window
                wpfWindow.Show();

                // Set WPF window as active and clear Windows Forms reference
                activeForm = null;              // Clear Windows Forms reference  
                activeWpfWindow = wpfWindow;    // Set WPF window as active
                this.Hide();

                // Create SignUpForm after window is shown
                try
                {
                    // Create a wrapper Form to satisfy SignUpForm constructor requirements
                    var wrapperForm = new Form() { Visible = false, ShowInTaskbar = false };
                    var signUpForm = new SignUpForm(this, wrapperForm);

                    // Update the WPF window's SignUpForm reference
                    wpfWindow.SetSignUpForm(signUpForm);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not create SignUpForm: {ex.Message}");
                }

                // Handle default username scenario
                if (isDefaultUsername(username))
                {
                    Task.Run(async () =>
                    {
                        await Task.Delay(1000);

                        wpfWindow.Dispatcher.Invoke(() =>
                        {
                            // Note: SignUpForm functionality will need to be adapted for WPF
                            // For now, we can show a WPF message or implement a WPF-based signup
                            System.Windows.MessageBox.Show("Пожалуйста, обновите свои учётные данные в настройках.",
                                "Обновление данных", MessageBoxButton.OK, MessageBoxImage.Information);
                        });
                    });
                }

                // Log successful launch
                Console.WriteLine($"Successfully launched WPF InstructionViewerWindow for {fullName} (Role: {(isChief ? "Chief" : "User")})");
            }
            catch (Exception ex)
            {
                // Fallback to Windows Forms if WPF fails
                Console.WriteLine($"Failed to launch WPF InstructionViewerWindow: {ex.Message}");
                MessageBox.Show($"Ошибка при запуске WPF интерфейса: {ex.Message}\n\nПереход к стандартному интерфейсу...",
                    "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                // Fallback to original Windows Forms
                LaunchWindowsFormsFallback(username, fullName, departmentName, isChief);
            }
        }

        /// <summary>
        /// Fallback method to launch Windows Forms UserForm/ChiefForm if WPF fails
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="fullName">The full name</param>
        /// <param name="departmentName">The department name</param>
        /// <param name="isChief">Whether the user is a chief</param>
        private void LaunchWindowsFormsFallback(string username, string fullName, string departmentName, bool isChief)
        {
            try
            {
                Form formToOpen = null;

                if (isChief)
                {
                    formToOpen = new ChiefForm(this, username, fullName, departmentName);
                }
                else
                {
                    formToOpen = new UserForm(this, username, fullName, departmentName);
                }

                if (formToOpen != null)
                {
                    activeForm = formToOpen;        // Set Windows Forms as active
                    activeWpfWindow = null;         // Clear WPF reference
                    formToOpen.Location = this.Location;
                    this.Hide();
                    formToOpen.Show();

                    // Handle default username scenario
                    if (isDefaultUsername(username))
                    {
                        Task.Run(async () =>
                        {
                            await Task.Delay(1000);

                            this.Invoke(new Action(() =>
                            {
                                if (formToOpen is UserForm userForm && userForm._signUpForm != null)
                                    userForm._signUpForm.Show();
                                else if (formToOpen is ChiefForm chiefForm && chiefForm._signUpForm != null)
                                    chiefForm._signUpForm.Show();
                            }));
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Критическая ошибка при запуске интерфейса: {ex.Message}",
                    "Критическая ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Add ExitApplication method to handle both Forms and WPF Windows
        public void ExitApplication()
        {
            try
            {
                // Close any active WPF windows
                if (activeWpfWindow != null)
                {
                    if (activeWpfWindow is ManagementWPF.Views.MainWindow mainWin)
                    {
                        mainWin.ForceClose();
                    }
                    else
                    {
                        activeWpfWindow.Close();
                    }
                    activeWpfWindow = null;
                }

                // Close any active Windows Forms
                if (activeForm != null && activeForm != this)
                {
                    activeForm.Close();
                    activeForm.Dispose();
                    activeForm = null;
                }

                // Delete JWT token
                Decryption_stuff.DeleteJWTToken();

                // Close application
                this.Dispose();
                System.Windows.Forms.Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при закрытии приложения: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Windows.Forms.Application.Exit();
            }
        }

        private async void LogInButton_Click(object sender, EventArgs e)
        {
            LogInButton.Enabled = false;

            try
            {
                if (string.IsNullOrWhiteSpace(LoginTextBox.Text) || string.IsNullOrWhiteSpace(PasswordTextBox.Text))
                {
                    MessageBox.Show("Пожалуйста, заполните Логин и Пароль", "Не указан Логин и/или Пароль", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                var username = LoginTextBox.Text;
                var password = PasswordTextBox.Text;
                var loginModel = new
                {
                    username = username,
                    password = password,
                    time_for_being_authenticated = timeForBeingAuthenticated,
                };

                var response = await _httpClient.PostAsJsonAsync(_loginUrl, loginModel);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<LoginResponse>(jsonResponse);

                    if (result is null || string.IsNullOrWhiteSpace(result.token))
                    {
                        MessageBox.Show("Вход не удался. Результат или токен не валиден(нуль)");
                    }
                    else
                    {
                        _jwtToken = result.token;
                        if (EncodeJWTTokenSuccessfully(_jwtToken))
                        {
                            PasswordTextBox.Text = "";
                            //Console.WriteLine($"JWTToken: {_jwtToken}"); //THIS SHOULDN'T BE SHOWING IN FREAKING PRODUCTION FOR SURE!
                            ProcessSuccessfulAuthentication(_jwtToken);
                        }
                        else
                        {
                            MessageBox.Show("Что-то пошло не так. Обратитесь в поддержку.");
                        }
                    }
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(jsonResponse);
                    MessageBox.Show($"Вход не успешен, так как начальник для данного отдела уже авторизован. Попросите его закрыть приложение и после этого - спустя минуту авторизуйтесь заново.", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    var message = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"{message}", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                LogInButton.Enabled = true;
            }
        }

        private void ProcessSuccessfulAuthentication(string token)
        {
            string role = GetRoleFromToken(token);
            string userName = GetUserNameFromToken(token);
            string fullName = GetFullNameFromToken(token);

            int departmentId = GetDepartmentIdFromToken(token);

            // Get department name from cache
            string departmentName = "Unknown Department";
            if (departmentId > 0)
            {
                departmentName = _departmentCache.GetDepartmentName(departmentId);
            }

            OpenFormBasedOnRole(role, userName, fullName, departmentName);
        }

        public string GetFullNameFromToken(string jwtToken)
        {
            if (string.IsNullOrEmpty(jwtToken))
                return string.Empty;

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(jwtToken) as JwtSecurityToken;

                if (jsonToken == null)
                    return string.Empty;

                var fullNameClaim = jsonToken.Claims.FirstOrDefault(claim => claim.Type == "FullName");
                return fullNameClaim?.Value ?? string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }





        public class TokenRequest
        {
            public string Token { get; set; }
        }

        private async Task<bool> InitializeAsync()
        {
            _jwtToken = Decryption_stuff.DecryptedJWTToken();



            if (string.IsNullOrEmpty(_jwtToken))
            {
                return false;
            }




            if (_jwtToken != null)
            {

                try
                {
                    var response = await _httpClient.PostAsJsonAsync(_validateTokenUrl, _jwtToken); //Зашифруй токен, если считаешь, что это важно or something. 
                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return true;
                    }
                    else
                    {
                        var message = await response.Content.ReadAsStringAsync();
                        MessageBox.Show(message);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return false;
                }
            }
            return false;
        }


        private void textBox1_Click(object sender, EventArgs e)
        {
            LoginTextBox.BackColor = Color.White;
            PasswordTextBox.BackColor = SystemColors.Control;

        }
        private void textBox2_Click(object sender, EventArgs e)
        {
            PasswordTextBox.BackColor = Color.White;
            LoginTextBox.BackColor = SystemColors.Control;
        }

        private void changeColorsOfTextBoxesToControl(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            LoginTextBox.BackColor = SystemColors.Control;
            PasswordTextBox.BackColor = SystemColors.Control;
        }

        private void lookPassword_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            PasswordTextBox.UseSystemPasswordChar = false;
        }

        private void lookPassword_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            PasswordTextBox.UseSystemPasswordChar = true;
        }

        private async Task DelayforRegistrationForm()
        {
            await Task.Delay(1000);
        }

        private bool isDefaultUsername(string v)
        {
            string pattern = @"^User\d+$";
            Regex regex = new Regex(pattern);
            if (regex.IsMatch(v))
            {
                return true;
            }
            return false;
        }

        private bool EncodeJWTTokenSuccessfully(string token)
        {
            try
            {
                byte[] encryptedData = ProtectedData.Protect(
                            Encoding.UTF8.GetBytes(token),
                            null,  // Optional entropy (additional data) to increase encryption complexity
                            DataProtectionScope.CurrentUser  // Or DataProtectionScope.LocalMachine
                        );
                if (RememberCredentialsCheckBox.Checked)
                {
                    File.WriteAllBytes(filePath, encryptedData);
                    MessageBox.Show("Вход с запоминанем данных успешен.");
                }
                else
                {
                    MessageBox.Show("Вход успешен.");
                }
                return true;
            }
            catch
            {
                return false;
            }

        }
        public string GetRoleFromToken(string jwtToken)
        {
            if (string.IsNullOrEmpty(jwtToken))
                return string.Empty;

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(jwtToken) as JwtSecurityToken;

                if (jsonToken == null)
                    return string.Empty;

                // Try standard role claim first
                var roleClaim = jsonToken.Claims.FirstOrDefault(claim =>
                     claim.Type == ClaimTypes.Role ||  // Full URI format
                     claim.Type == "role" ||           // Short format common in newer JWT implementations
                     claim.Type == "roles" ||          // Plural version sometimes used
                     claim.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"); // Explicit URI

                return roleClaim?.Value ?? string.Empty;
            }
            catch (Exception)
            {
                return string.Empty; // Return empty string for any parsing errors
            }
        }
        public string GetUserNameFromToken(string jwtToken)
        {
            if (string.IsNullOrEmpty(jwtToken))
                return string.Empty;

            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(jwtToken) as JwtSecurityToken;

            if (jsonToken == null)
                return string.Empty;

            // Look for "unique_name" instead of ClaimTypes.Name
            var nameClaim = jsonToken.Claims.FirstOrDefault(claim => claim.Type == "unique_name");
            return nameClaim?.Value ?? string.Empty;
        }

        public class LoginResponse
        {
            public string token { get; set; }
            public string message { get; set; }
        }

        private void ForgotPasswordLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            return;
        }

        private void SupportEmail_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            return;
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                LogInButton_Click(sender, e);
            }
        }

        private async Task InitializeDepartmentCache()
        {
            try
            {
                // Make sure _departmentCache is initialized
                if (_departmentCache == null)
                {
                    _departmentCache = new DepartmentCache();
                }

                var response = await _httpClient.GetAsync($"{ConfigurationClass.BASE_URL_DEVELOPMENT}/api/departments");
                if (response.IsSuccessStatusCode)
                {
                    var departments = await response.Content.ReadFromJsonAsync<List<DepartmentDto>>();
                    var deptDict = departments.ToDictionary(d => d.Id, d => d.Name);

                    _departmentCache.UpdateCache(deptDict);
                }
            }
            catch (Exception ex)
            {
                // Log error but continue application startup
                
                Console.WriteLine($"Failed to initialize department cache: {ex.Message}");
            }
        }

        private void Login_Russian_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;  // Prevent the form from closing
            this.Hide();  // Hide the form
            this.ShowInTaskbar = false;  // Hide from the taskbar
        }

        public int GetDepartmentIdFromToken(string jwtToken)
        {
            if (string.IsNullOrEmpty(jwtToken))
                return -1;

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(jwtToken) as JwtSecurityToken;

                if (jsonToken == null)
                    return -1;

                // Look for department ID claim
                var departmentIdClaim = jsonToken.Claims.FirstOrDefault(claim =>
                    claim.Type == "DepartmentId" ||
                    claim.Type == "department_id");

                if (departmentIdClaim != null && int.TryParse(departmentIdClaim.Value, out int departmentId))
                {
                    return departmentId;
                }

                return -1;
            }
            catch (Exception)
            {
                return -1; // Return -1 for any parsing errors
            }
        }

    }
}
