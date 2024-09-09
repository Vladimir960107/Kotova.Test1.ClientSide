using Kotova.CommonClasses;
using Microsoft.AspNetCore.SignalR.Client;
using System.CodeDom;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;


namespace Kotova.Test1.ClientSide
{
    public partial class Login_Russian : Form
    {

        private static readonly HttpClient _httpClient = new HttpClient();
        private const string _loginUrl = ConfigurationClass.BASE_URL_DEVELOPMENT + "/login"; //ADD IT TO THE 
        private const string _validateTokenUrl = ConfigurationClass.BASE_URL_DEVELOPMENT + "/validate-token";
        static string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        static string fileName = "encrypted_jwt.dat";
        string filePath = Path.Combine(documentsPath, fileName);
        public string? _jwtToken = null;
        public bool _jwtRemembered = false;
        private TaskCompletionSource<bool> _initTaskCompletionSource;
        private int timeForBeingAuthenticated = 30;

        public Form activeForm;
        private NotifyIcon notifyIcon;

        public Login_Russian()
        {
            InitializeComponent();
            InitializeSettingsMenu();
            InitializeNotifyIcon();

            activeForm = this;

            this.Load += async (sender, args) => await Login_Russian_LoadAsync(sender, args);
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
                HandleUserBasedOnRole(token);
            }
            else
            {
                // Initialization failed
                //MessageBox.Show("Initialization failed");
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

        private void ShowForm()
        {
            activeForm.Show();
            activeForm.WindowState = FormWindowState.Normal;
            activeForm.ShowInTaskbar = true;
        }

        public void ExitApplication()
        {
            this.Dispose(true);
            notifyIcon.Dispose();
            System.Windows.Forms.Application.Exit();
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

        private void HandleUserBasedOnRole(string token)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => HandleUserBasedOnRole(token)));
            }
            else
            {
                string role = GetRoleFromToken(token);
                string username = GetUserNameFromToken(token);
                switch (role)
                {
                    case "User":
                        OpenUserForm(username);
                        break;
                    case "ChiefOfDepartment":
                        OpenChiefForm(username);
                        break;
                    case "Coordinator":
                        OpenCoordinatorForm(username);
                        break;
                    case "Management":
                        OpenManagementForm(username);
                        break;
                    case "Administrator":
                        OpenAdminForm(username);
                        break;
                    default:
                        MessageBox.Show("Упс, роль не валидна. Попросите кого-то из тех. поддержки разрешить ситуацию :I");
                        break;
                }
            }
        }

        private void OpenUserForm(string username)
        {
            UserForm userForm = new UserForm(this, username);
            activeForm = userForm;
            userForm.Location = this.Location;
            this.Hide();
            userForm.Show();
            CheckDefaultUsername(userForm, username);
        }

        private void OpenChiefForm(string username)
        {
            ChiefForm chiefOfDepartmentForm = new ChiefForm(this, username);
            activeForm = chiefOfDepartmentForm;
            chiefOfDepartmentForm.Location = this.Location;
            chiefOfDepartmentForm.Show();
            this.Hide();
        }

        private void OpenCoordinatorForm(string username)
        {
            CoordinatorForm coordinatorForm = new CoordinatorForm(this, username);
            activeForm = coordinatorForm;
            coordinatorForm.Location = this.Location;
            coordinatorForm.Show();
            this.Hide();
        }

        private void OpenManagementForm(string username)
        {
            ManagementForm managementForm = new ManagementForm(this, username);
            activeForm = managementForm;
            managementForm.Location = this.Location;
            managementForm.Show();
            this.Hide();
        }

        private async void CheckDefaultUsername(UserForm userForm, string username)
        {
            await Task.Delay(1000);
            if (isDefaultUsername(username))
            {
                userForm._signUpForm?.Show();
            }
        }
        private void OpenAdminForm(string username)
        {
            AdminForm adminForm = new AdminForm(this, username);
            activeForm = adminForm;
            adminForm.Location = this.Location;
            adminForm.Show();
            this.Hide();
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

        private void changeColorsOfTextBoxesToControl(object sender, MouseEventArgs e)
        {
            LoginTextBox.BackColor = SystemColors.Control;
            PasswordTextBox.BackColor = SystemColors.Control;
        }

        private void lookPassword_MouseDown(object sender, MouseEventArgs e)
        {
            PasswordTextBox.UseSystemPasswordChar = false;
        }

        private void lookPassword_MouseUp(object sender, MouseEventArgs e)
        {
            PasswordTextBox.UseSystemPasswordChar = true;
        }

        private async void LogInButton_Click(object sender, EventArgs e) // TODO: Переписать эту функцию как выше описано, потому что повторяется!
        {
            LogInButton.Enabled = false;



            if (string.IsNullOrWhiteSpace(LoginTextBox.Text) || string.IsNullOrWhiteSpace(PasswordTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, заполните Логин и Пароль", "Не указан Логин и/или Пароль", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                LogInButton.Enabled = true;
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


            try
            {
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
                            switch (GetRoleFromToken(_jwtToken))
                            {
                                case "User":
                                    UserForm userForm = new UserForm(this, GetUserNameFromToken(_jwtToken)); // put here like UserForm(this)
                                    activeForm = userForm;
                                    userForm.Location = this.Location;
                                    this.Hide();
                                    userForm.Show();

                                    await DelayforRegistrationForm();
                                    if (isDefaultUsername(GetUserNameFromToken(_jwtToken)))
                                    {
                                        if (userForm._signUpForm is not null)
                                        {
                                            userForm._signUpForm.Show();
                                        }
                                    }

                                    break;
                                case "ChiefOfDepartment":
                                    ChiefForm chiefOfDepartmentForm = new ChiefForm(this, GetUserNameFromToken(_jwtToken));// put here like UserForm(this)
                                    activeForm = chiefOfDepartmentForm;
                                    chiefOfDepartmentForm.Location = this.Location;
                                    chiefOfDepartmentForm.Show();
                                    this.Hide();

                                    await DelayforRegistrationForm();
                                    if (isDefaultUsername(GetUserNameFromToken(_jwtToken)))
                                    {
                                        if (chiefOfDepartmentForm._signUpForm is not null)
                                        {
                                            chiefOfDepartmentForm._signUpForm.Show();
                                        }
                                    }

                                    break;
                                case "Coordinator":
                                    CoordinatorForm coordinatorForm = new CoordinatorForm(this, GetUserNameFromToken(_jwtToken));// put here like UserForm(this)
                                    activeForm = coordinatorForm;
                                    coordinatorForm.Location = this.Location;
                                    coordinatorForm.Show();
                                    this.Hide();

                                    await DelayforRegistrationForm();
                                    if (isDefaultUsername(GetUserNameFromToken(_jwtToken)))
                                    {
                                        if (coordinatorForm._signUpForm is not null)
                                        {
                                            coordinatorForm._signUpForm.Show();
                                        }
                                    }

                                    break;
                                case "Management":
                                    ManagementForm managementForm = new ManagementForm(this, GetUserNameFromToken(_jwtToken));
                                    activeForm = managementForm;
                                    managementForm.Location = this.Location;
                                    managementForm.Show();
                                    this.Hide();

                                    await DelayforRegistrationForm();
                                    if (isDefaultUsername(GetUserNameFromToken(_jwtToken)))
                                    {
                                        if (managementForm._signUpForm is not null)
                                        {
                                            managementForm._signUpForm.Show();
                                        }
                                    }
                                    break;
                                case "Administrator":
                                    AdminForm adminForm = new AdminForm(this, GetUserNameFromToken(_jwtToken));
                                    activeForm = adminForm;
                                    adminForm.Location = this.Location;
                                    adminForm.Show();
                                    this.Hide();

                                    await DelayforRegistrationForm();
                                    await DelayforRegistrationForm();
                                    break;
                                default:
                                    MessageBox.Show("Ваша роль не подходящая. Обратитесь в поддержку для разрешения этого вопроса :I");
                                    break;
                            }
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
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(jwtToken);
            var tokenS = jsonToken as JwtSecurityToken;

            var roleClaim = tokenS.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Role);
            return roleClaim?.Value;
        }
        public string GetUserNameFromToken(string jwtToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(jwtToken);
            var tokenS = jsonToken as JwtSecurityToken;

            var roleClaim = tokenS.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name);
            return roleClaim?.Value;
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

        private void Login_Russian_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            this.ShowInTaskbar = false;
        }
    }
}
