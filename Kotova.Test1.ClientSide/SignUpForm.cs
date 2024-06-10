using Kotova.CommonClasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kotova.Test1.ClientSide
{
    public partial class SignUpForm : Form
    {
        private const string _changeCredUrl = ConfigurationClass.BASE_URL_DEVELOPMENT + "/change_credentials";
        private const string _checkIfLoginAlreadyTaken = ConfigurationClass.BASE_URL_DEVELOPMENT + "check_login_already_taken";
        private Login_Russian? _login_form;
        private UserForm? _userForm;
        const string defaultLoginText = "Введите новый логин";
        const string defaultPasswordText = "Введите новый пароль";
        const string defaultPasswordRepeatText = "Повторите новый пароль";
        const string defaultEmailText = "Введите почту (Необязательно)";
        public SignUpForm(Login_Russian form, UserForm userForm)
        {
            InitializeComponent();
            _login_form = form;
            _userForm = userForm;

        }

        private void textBox1_Click(object sender, EventArgs e)
        {
            if (loginTextBox.Text == "Введите логин")
            {
                loginTextBox.Text = "";
            }
            loginTextBox.BackColor = Color.White;
            RepeatPasswordTextBox.BackColor = SystemColors.Control;

        }
        private void textBox2_Click(object sender, EventArgs e)
        {
            if (RepeatPasswordTextBox.Text == "Введите пароль")
            {
                RepeatPasswordTextBox.Text = "";
            }
            RepeatPasswordTextBox.BackColor = Color.White;
            loginTextBox.BackColor = SystemColors.Control;
        }

        private void changeColorsOfTextBoxesToControl(object sender, MouseEventArgs e)
        {
            loginTextBox.BackColor = SystemColors.Control;
            RepeatPasswordTextBox.BackColor = SystemColors.Control;
        }

        private void lookPassword_MouseDown(object sender, MouseEventArgs e)
        {
            RepeatPasswordTextBox.UseSystemPasswordChar = false;
        }

        private void pictureBox4_MouseUp(object sender, MouseEventArgs e)
        {
            RepeatPasswordTextBox.UseSystemPasswordChar = true;
        }

        private void LogInButton_Click(object sender, EventArgs e)
        {
            if (loginTextBox.Text == "" || PasswordTextBox.Text == "")
            {
                MessageBox.Show("Пожалуйста, заполните Логин и Пароль", "Не указан Логин и/или Пароль", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

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
            this.Close();

        }

        private void skipButton_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private async void signUpButton_Click(object sender, EventArgs e)
        {
            signUpButton.Enabled = false;
            string login = loginTextBox.Text;
            string password = PasswordTextBox.Text;
            string email = emailTextBox.Text;
            string repeatedPassword = RepeatPasswordTextBox.Text;

            if (!CheckForValidation(login, password, repeatedPassword, email))
            {
                signUpButton.Enabled = true;
                return;
            }

            var userCredentials = new UserCredentials
            {
                Login = loginTextBox.Text,
                Password = PasswordTextBox.Text,
                Email = emailTextBox.Text
            };

            string jsonPayload = JsonConvert.SerializeObject(userCredentials);

            HttpContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            var response = await Test.connectionToUrlPatch(_changeCredUrl, content);
            if (response == HttpStatusCode.OK)
            {
                MessageBox.Show("Вы успешно сменили логин и пароль!");

                if (_login_form is not null)
                {
                    if (_userForm is not null)
                    {
                        _userForm.Dispose();
                    }
                    _login_form.Show();

                    this.Dispose();
                }
            }
            else
            {
                signUpButton.Enabled = true;
                MessageBox.Show("Что-то пошло не так :( Описание ошибки по идее на сервере.");

            }
            signUpButton.Enabled = true;
        }


        private bool CheckForValidation(string login, string password, string repeatedPassword, string email)
        {
            const string LoginRegex = @"^[a-zA-Z0-9_]+$";

            // Regex for validating password - at least one lowercase letter, one uppercase letter, one number, and is at least 8 characters long
            const string PasswordRegex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$";

            // Regex for validating email. A common regex for email validation.
            const string EmailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

            if (!Regex.IsMatch(login, LoginRegex) || string.IsNullOrWhiteSpace(login))
            {
                MessageBox.Show($"login:{login} is not valid");
                return false; // false MEANS NOT GOOD RESPONSE
            }
            else if (!Regex.IsMatch(password, PasswordRegex) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show($"password is not valid.");
                return false;
            }
            else if (!(password == repeatedPassword))
            {
                MessageBox.Show($"password is not equal to repeated password.");
                return false;
            }

            else if (string.IsNullOrWhiteSpace(email) || !Regex.IsMatch(email, EmailRegex))
            {
                MessageBox.Show($"email:{email} is not valid");
                return false;
            }
            return true; //Means good response
        }

        private void loginTextBox_DoubleClick(object sender, EventArgs e)
        {
            if (loginTextBox.Text == defaultLoginText)
            {
                loginTextBox.Text = "";
            }
        }

        private void PasswordTextBox_TextChanged(object sender, EventArgs e)
        {

            PasswordTextBox.UseSystemPasswordChar = true;
        }

        private void PasswordTextBox_DoubleClick(object sender, EventArgs e)
        {
            if (PasswordTextBox.Text == defaultPasswordText)
            {
                PasswordTextBox.Text = "";
            }
        }

        private void RepeatPassswordTextBox_TextChanged(object sender, EventArgs e)
        {
            RepeatPasswordTextBox.UseSystemPasswordChar = true;
        }

        private void RepeatPassswordTextBox_DoubleClick(object sender, EventArgs e)
        {
            if (RepeatPasswordTextBox.Text == defaultPasswordRepeatText)
            {
                RepeatPasswordTextBox.Text = "";
            }
        }

        private void emailTextBox_DoubleClick(object sender, EventArgs e)
        {
            if (emailTextBox.Text == defaultEmailText)
            {
                emailTextBox.Text = "";
            }
        }

        private void showPasswordPicture_MouseDown(object sender, MouseEventArgs e)
        {
            PasswordTextBox.UseSystemPasswordChar = false;
        }

        private void showPasswordPicture_MouseUp(object sender, MouseEventArgs e)
        {
            PasswordTextBox.UseSystemPasswordChar = true;
        }
    }
}
