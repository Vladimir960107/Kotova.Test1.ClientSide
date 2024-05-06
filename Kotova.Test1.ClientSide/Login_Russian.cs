﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Security.Cryptography;
using System.Net.Http.Headers;


namespace Kotova.Test1.ClientSide
{
    public partial class Login_Russian : Form
    {

        private static readonly HttpClient _httpClient = new HttpClient();
        private const string _loginUrl = "https://localhost:7052/login";
        private const string _securedataUrl = "https://localhost:7052/notifications";
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



            if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("Пожалуйста, заполните Логин и Пароль", "Не указан Логин и/или Пароль", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
                            Form2 FormMain = new Form2(this);
                            FormMain.Location = this.Location;
                            FormMain.Show();
                            this.Hide();
                        }
                        else
                        {
                            MessageBox.Show("Login sucessfull, but JWT token was not saved and encoded!");
                            
                        }

                    }

                }
                else
                {
                    MessageBox.Show($"Login failed: {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
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

        private void button1_Click(object sender, EventArgs e)
        {
            SignUpForm signUpForm = new SignUpForm(this);
            signUpForm.Location = this.Location;
            signUpForm.Show();
            this.Hide();


        }

        private async void securedataButton_Click(object sender, EventArgs e)
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

                        // Ensure that the request was successful
                        response.EnsureSuccessStatusCode();

                        // Read the response content as a string
                        string responseData = await response.Content.ReadAsStringAsync();

                        // Display the result in a message box or another UI element
                        MessageBox.Show(responseData, "Secure Data");
                    }
                }
                catch (Exception ex)
                {
                    // Handle any errors that occurred during the request
                    MessageBox.Show($"Error fetching secure data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }





            }
            catch
            {
                MessageBox.Show("OOOops, something went wrong (check securedataButton_Click)!");
                return;
            }
        }
    }
}
