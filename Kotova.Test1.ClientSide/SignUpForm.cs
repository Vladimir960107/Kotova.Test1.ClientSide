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

        private void loginTextBox_Click(object sender, EventArgs e)
        {
            if (loginTextBox.Text == defaultLoginText)
            {
                loginTextBox.Text = "";
            }
            changeAllToSytemColors();
            CheckForEmptyStringAndTypeReminders(sender);
            loginTextBox.BackColor = Color.White;

        }

        private void PasswordTextBox_Click(object sender, EventArgs e)
        {
            if (PasswordTextBox.Text == defaultPasswordText)
            {
                PasswordTextBox.Text = "";
            }
            changeAllToSytemColors();
            CheckForEmptyStringAndTypeReminders(sender);
            PasswordTextBox.BackColor = Color.White;
        }
        private void RepeatPasswordTextBox_Click(object sender, EventArgs e)
        {
            if (RepeatPasswordTextBox.Text == defaultPasswordRepeatText)
            {
                RepeatPasswordTextBox.Text = "";
            }
            if (string.IsNullOrWhiteSpace(loginTextBox.Text))

            changeAllToSytemColors();
            CheckForEmptyStringAndTypeReminders(sender);
            RepeatPasswordTextBox.BackColor = Color.White;
        }

        private void emailTextBox_Click(object sender, EventArgs e)
        {
            if (emailTextBox.Text == defaultEmailText)
            {
                emailTextBox.Text = "";
            }
            changeAllToSytemColors();
            CheckForEmptyStringAndTypeReminders(sender);
            emailTextBox.BackColor = Color.White;
            
        }
        private void changeAllToSytemColors()
        {
            RepeatPasswordTextBox.BackColor = SystemColors.Control;
            loginTextBox.BackColor = SystemColors.Control;
            PasswordTextBox.BackColor = SystemColors.Control;
            emailTextBox.BackColor = SystemColors.Control;
        }
        private void CheckForEmptyStringAndTypeReminders(object sender)
        {
            List<TextBox> listOfObjects = new List<TextBox>(); // ИСПРАВЬ ТУТ 2 ЛИСТА на Dictionary ИЛИ ЧТО-ТО ПОХОЖЕЕ.
            listOfObjects.Add(loginTextBox);
            listOfObjects.Add(PasswordTextBox);
            listOfObjects.Add(RepeatPasswordTextBox);
            listOfObjects.Add(emailTextBox);

            List<string> listOfDefaultReminder = new List<string>();
            listOfDefaultReminder.Add(defaultLoginText);
            listOfDefaultReminder.Add(defaultPasswordText);
            listOfDefaultReminder.Add(defaultPasswordRepeatText);
            listOfDefaultReminder.Add(defaultEmailText);

            for (int i = 0; i < listOfObjects.Count; i++)
            {
                if (listOfObjects[i] != sender)
                { 
                    if (string.IsNullOrWhiteSpace(listOfObjects[i].Text))
                    {
                        if (i != 1 && i != 2) //Если не пароль и не повторение пароля короче. Иначе там баг и сложно! Разберись если хочешь
                        {
                            listOfObjects[i].Text = listOfDefaultReminder[i];
                        }
                        
                    }
                    
                }
            }

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
            string? email = emailTextBox.Text;
            string repeatedPassword = RepeatPasswordTextBox.Text;

            if (!CheckForValidation(login, password, repeatedPassword, ref email))
            {
                signUpButton.Enabled = true;
                return;
            }
            var userCredentials = new UserCredentials
            {
                Login = login,
                Password = password,
                Email = email
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


        private bool CheckForValidation(string login, string password, string repeatedPassword, ref string? email)
        {
            const string LoginRegex = @"^[a-zA-Z0-9_]+$";

            // Regex for validating password - at least one lowercase letter, one uppercase letter, one number, and is at least 8 characters long
            const string PasswordRegex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$";

            // A common regex for email validation.
            const string EmailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

            if (!Regex.IsMatch(login, LoginRegex) || string.IsNullOrWhiteSpace(login))
            {
                MessageBox.Show($"login:{login} is not valid");
                return false;
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
            else if (string.IsNullOrWhiteSpace(email) || email == defaultEmailText)
            {
                if (ConfirmAction("You have not entered email, are you sure?"))
                {
                    email = null;
                    return true;
                }
                return false;
            }
            else if (!Regex.IsMatch(email, EmailRegex))
            {
                MessageBox.Show("email is not valid, try again.");
                return false;
            }
            return true; //Means good response
        }

        private bool ConfirmAction(string confirmMessage)
        {
            var result = MessageBox.Show(confirmMessage, "Confirm action", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                return true;
            }
            else
            {
                return false;
            }
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

        private void showrepeatedPasswordPicture_MouseDown(object sender, MouseEventArgs e)
        {
            RepeatPasswordTextBox.UseSystemPasswordChar = false;
        }

        private void showRepeatedPasswordPicture_MouseUp(object sender, MouseEventArgs e)
        {
            RepeatPasswordTextBox.UseSystemPasswordChar = true;
        }


    }
}
