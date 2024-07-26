using Kotova.CommonClasses;
using System.CodeDom;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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



        /*public Login_Russian()
        {
            InitializeComponent();
            _initTaskCompletionSource = new TaskCompletionSource<bool>();
            //bool initResult = InitializeAsync().Result;
            this.Shown += async (sender, args) => _initTaskCompletionSource.SetResult(await InitializeAsync());

            // Wait for the async initialization to complete
            bool initResult = _initTaskCompletionSource.Task.Result;
            // Wait for the async initialization to complete
            if (initResult)
            {
                // Initialization succeeded
                MessageBox.Show("Initialization succeeded");
                string token = Decryption_stuff.DecryptedJWTToken();
                HandleUserBasedOnRole(token);
            }
            else
            {
                // Initialization failed
                MessageBox.Show("Initialization failed");
                
            }
        }*/

        public Login_Russian()
        {
            this.Load += async (sender, args) => await Login_Russian_LoadAsync(sender, args);
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
            InitializeComponent();
            InitializeSettingsMenu();
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
            MessageBox.Show("Dark Theme включена.");
        }

        private void LightThemeItem_Click(object sender, EventArgs e)
        {
            // Logic for enabling light theme
            MessageBox.Show("Light Theme включена.");
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
                default:
                    MessageBox.Show("Oops, your role is invalid. Ask someone for help to resolve this issue :I");
                    break;
            }
        }

        private void OpenUserForm(string username)
        {
            UserForm userForm = new UserForm(this, username);
            userForm.Location = this.Location;
            this.Hide();
            userForm.Show();
            CheckDefaultUsername(userForm, username);
        }

        private void OpenChiefForm(string username)
        {
            ChiefForm chiefOfDepartmentForm = new ChiefForm(this, username);
            chiefOfDepartmentForm.Location = this.Location;
            chiefOfDepartmentForm.Show();
            this.Hide();
        }

        private void OpenCoordinatorForm(string username)
        {
            CoordinatorForm coordinatorForm = new CoordinatorForm(this, username);
            coordinatorForm.Location = this.Location;
            coordinatorForm.Show();
            this.Hide();
        }

        private void OpenManagementForm(string username)
        {
            ManagementForm managementForm = new ManagementForm(this, username);
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
                        MessageBox.Show("Login failed. result or token is invalid(null)");
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
                                    userForm.Location = this.Location;
                                    this.Hide();
                                    userForm.Show();

                                    DelayforRegistrationForm();
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
                                    chiefOfDepartmentForm.Location = this.Location;
                                    chiefOfDepartmentForm.Show();
                                    this.Hide();

                                    DelayforRegistrationForm();
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
                                    coordinatorForm.Location = this.Location;
                                    coordinatorForm.Show();
                                    this.Hide();

                                    DelayforRegistrationForm();
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
                                    managementForm.Location = this.Location;
                                    managementForm.Show();
                                    this.Hide();

                                    DelayforRegistrationForm();
                                    if (isDefaultUsername(GetUserNameFromToken(_jwtToken)))
                                    {
                                        if (managementForm._signUpForm is not null)
                                        {
                                            managementForm._signUpForm.Show();
                                        }
                                    }
                                    break;
                                default:
                                    MessageBox.Show("Oops, your role is invalid. Ask someone for help to resolve this issue :I");
                                    break;
                            }
                        }
                        else
                        {
                            MessageBox.Show("Login sucessfull, but JWT token was not saved and encoded!");

                        }

                    }

                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var errorResponse = JsonSerializer.Deserialize<ErrorResponse>(jsonResponse);
                    MessageBox.Show($"Login failed, most likely because current department already have Chief Authenticated. Ask him to close application and then (after 1 minute max) - open your application. {errorResponse.Message}", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    var message = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Login failed: {message}", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        private async void DelayforRegistrationForm()
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
                    MessageBox.Show("Login Successfull and JWT Token sucessfully encoded");
                }
                else
                {
                    MessageBox.Show("Login Successfull");
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





        /*private async void securedata_TEST_Button_Click(object sender, EventArgs e) // YOU CAN DELETE THIS PART OF CODE, DEPRECATED. IF YOU WANT
        {
            try
            {
                byte[] encryptedData = File.ReadAllBytes(filePath);

                // Decrypt the JWT using DPAPI
                byte[] decryptedData = ProtectedData.Unprotect(
                    encryptedData,
                    null,  // Same optional entropy as during encryption
                    DataProtectionScope.CurrentUser  // Or DataProtectionScope.LocalMachine
                );

                // Convert decrypted bytes to string
                string jwtToken = Encoding.UTF8.GetString(decryptedData);

                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        // Set the Authorization header with the Bearer token
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                        // Make a GET request to the secured endpoint
                        HttpResponseMessage response = await client.GetAsync(_securedataUrl);

                        response.EnsureSuccessStatusCode();

                        string responseData = await response.Content.ReadAsStringAsync();

                        MessageBox.Show(responseData, "Secure Data");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error fetching secure data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }





            }
            catch
            {
                MessageBox.Show("OOOops, something went wrong (check securedataButton_Click)!");
                return;
            }
        }*/
    }
}
