﻿using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;


namespace Kotova.Test1.ClientSide
{
    public partial class Login_Russian : Form
    {

        private static readonly HttpClient _httpClient = new HttpClient();
        private const string _loginUrl = ConfigurationClass.BASE_URL_DEVELOPMENT+"/login"; //ADD IT TO THE 
        static string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        static string fileName = "encrypted_jwt.dat";
        string filePath = Path.Combine(documentsPath, fileName);

        public Login_Russian()
        {
            InitializeComponent();

        }

        private void textBox1_Click(object sender, EventArgs e)
        {
            textBox1.BackColor = Color.White;
            textBox2.BackColor = SystemColors.Control;

        }
        private void textBox2_Click(object sender, EventArgs e)
        {
            textBox2.BackColor = Color.White;
            textBox1.BackColor = SystemColors.Control;
        }

        private void changeColorsOfTextBoxesToControl(object sender, MouseEventArgs e)
        {
            textBox1.BackColor = SystemColors.Control;
            textBox2.BackColor = SystemColors.Control;
        }

        private void lookPassword_MouseDown(object sender, MouseEventArgs e)
        {
            textBox2.UseSystemPasswordChar = false;
        }

        private void pictureBox4_MouseUp(object sender, MouseEventArgs e)
        {
            textBox2.UseSystemPasswordChar = true;
        }

        private async void LogInButton_Click(object sender, EventArgs e)
        {
            LogInButton.Enabled = false;


            if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("Пожалуйста, заполните Логин и Пароль", "Не указан Логин и/или Пароль", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                LogInButton.Enabled = true;
                return;
            }

            var username = textBox1.Text;
            var password = textBox2.Text;
            var loginModel = new
            {
                username = username,
                password = password
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
                        if (EncodeJWTToken(result.token))
                        {
                            MessageBox.Show("Login Successfull and JWT Token sucessfully encoded");
                            switch (GetRoleFromToken(result.token))
                            {
                                case "User":
                                    UserForm userForm = new UserForm(this, GetUserNameFromToken(result.token)); // put here like UserForm(this)
                                    userForm.Location = this.Location;
                                    this.Hide();
                                    userForm.Show();
                                    await Task.Delay(1000);
                                    if (userForm._signUpForm is not null)
                                    {
                                        userForm._signUpForm.Show();
                                    }
                                    break;
                                case "Chief Of Department":
                                    ChiefForm chiefOfDepartmentForm = new ChiefForm(this, GetUserNameFromToken(result.token));// put here like UserForm(this)
                                    chiefOfDepartmentForm.Location = this.Location;
                                    chiefOfDepartmentForm.Show();
                                    this.Hide();
                                    break;
                                case "Coordinator":
                                    CoordinatorForm coordinatorForm = new CoordinatorForm(this, GetUserNameFromToken(result.token));// put here like UserForm(this)
                                    coordinatorForm.Location = this.Location;
                                    coordinatorForm.Show();
                                    this.Hide();
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
                else
                {
                    var message = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Login failed: {message}", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                MessageBox.Show(ex.ToString() );
            }
            finally
            {
                LogInButton.Enabled = true;
            }

        }

        private bool EncodeJWTToken(string token)
        {
            try
            {
                byte[] encryptedData = ProtectedData.Protect(
                            Encoding.UTF8.GetBytes(token),
                            null,  // Optional entropy (additional data) to increase encryption complexity
                            DataProtectionScope.CurrentUser  // Or DataProtectionScope.LocalMachine
                        );
                File.WriteAllBytes(filePath, encryptedData);
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

        /*private void button1_Click(object sender, EventArgs e) //НЕ НУЖНО БОЛЬШЕ, МОЖНО СТЕРЕТЬ!
        {
            SignUpForm signUpForm = new SignUpForm(this);
            signUpForm.Location = this.Location;
            signUpForm.Show();
            this.Hide();


        }*/

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
