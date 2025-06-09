using Kotova.CommonClasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows; // For WPF Window
using WpfWindow = System.Windows.Window;
using WinFormsForm = System.Windows.Forms.Form;
using SystemColors = System.Drawing.SystemColors;
using MessageBox = System.Windows.Forms.MessageBox;
using Point = System.Drawing.Point;

namespace Kotova.Test1.ClientSide
{
    public partial class SignUpForm : Form
    {
        private static readonly string _changeCredUrl = ConfigurationClass.BASE_URL_DEVELOPMENT + "/change_credentials";
        private static readonly string _checkIfLoginAlreadyTaken = ConfigurationClass.BASE_URL_DEVELOPMENT + "check_login_already_taken";

        private Login_Russian? _loginForm;
        private WinFormsForm? _userForm;           // Original Windows Forms reference
        private WpfWindow? _userWpfWindow;         // NEW: WPF Window reference

        const string defaultLoginText = "Введите новый логин";
        const string defaultPasswordText = "Введите новый пароль";
        const string defaultPasswordRepeatText = "Повторите новый пароль";
        const string defaultEmailText = "Введите почту (Необязательно)";

        // Original constructor for Windows Forms compatibility
        public SignUpForm(Login_Russian form, WinFormsForm userForm)
        {
            InitializeComponent();
            _loginForm = form;
            _userForm = userForm;
            _userWpfWindow = null; // Clear WPF reference
        }

        // NEW: Constructor for WPF Window support
        public SignUpForm(Login_Russian form, WpfWindow userWpfWindow)
        {
            InitializeComponent();
            _loginForm = form;
            _userForm = null; // Clear Windows Forms reference
            _userWpfWindow = userWpfWindow;
        }



        // NEW: Method to determine which parent window type we have
        private bool IsWpfParent => _userWpfWindow != null;
        private bool IsFormsParent => _userForm != null;


        // NEW: Method to center SignUpForm over parent window
        private void CenterOverParent()
        {
            try
            {
                if (IsWpfParent && _userWpfWindow != null)
                {
                    // Ensure the WPF window position is available
                    if (_userWpfWindow.IsLoaded && _userWpfWindow.WindowState != System.Windows.WindowState.Minimized)
                    {
                        // Center over WPF window
                        this.StartPosition = FormStartPosition.Manual;

                        // Calculate center position
                        double wpfCenterX = _userWpfWindow.Left + (_userWpfWindow.Width / 2);
                        double wpfCenterY = _userWpfWindow.Top + (_userWpfWindow.Height / 2);

                        // Position SignUpForm centered over WPF window
                        this.Left = (int)(wpfCenterX - (this.Width / 2));
                        this.Top = (int)(wpfCenterY - (this.Height / 2));

                        // Ensure it's within screen bounds
                        var screen = Screen.FromPoint(new Point(this.Left, this.Top));

                        if (this.Left < screen.WorkingArea.Left)
                            this.Left = screen.WorkingArea.Left;
                        if (this.Top < screen.WorkingArea.Top)
                            this.Top = screen.WorkingArea.Top;
                        if (this.Left + this.Width > screen.WorkingArea.Right)
                            this.Left = screen.WorkingArea.Right - this.Width;
                        if (this.Top + this.Height > screen.WorkingArea.Bottom)
                            this.Top = screen.WorkingArea.Bottom - this.Height;

                        Console.WriteLine($"SignUpForm positioned at: ({this.Left}, {this.Top}) over WPF window at: ({_userWpfWindow.Left}, {_userWpfWindow.Top})");
                        return;
                    }
                }
                else if (IsFormsParent && _userForm != null && _userForm.Visible)
                {
                    // Center over Windows Forms
                    this.StartPosition = FormStartPosition.Manual;
                    this.Left = _userForm.Left + (_userForm.Width - this.Width) / 2;
                    this.Top = _userForm.Top + (_userForm.Height - this.Height) / 2;
                    return;
                }

                // Fallback to center screen
                this.StartPosition = FormStartPosition.CenterScreen;
                Console.WriteLine("SignUpForm: Using center screen fallback");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error centering SignUpForm over parent: {ex.Message}");
                this.StartPosition = FormStartPosition.CenterScreen;
            }
        }

        // NEW: Method to show parent window (works for both Forms and WPF)
        private void ShowParentWindow()
        {
            try
            {
                if (IsWpfParent && _userWpfWindow != null)
                {
                    // Show WPF window
                    _userWpfWindow.Show();
                    _userWpfWindow.WindowState = System.Windows.WindowState.Normal;
                    _userWpfWindow.Activate();
                }
                else if (IsFormsParent && _userForm != null)
                {
                    // Show Windows Forms window
                    _userForm.Show();
                    _userForm.WindowState = FormWindowState.Normal;
                    _userForm.BringToFront();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing parent window: {ex.Message}");
            }
        }

        // NEW: Method to hide parent window (works for both Forms and WPF)
        private void HideParentWindow()
        {
            try
            {
                if (IsWpfParent && _userWpfWindow != null)
                {
                    _userWpfWindow.Hide();
                }
                else if (IsFormsParent && _userForm != null)
                {
                    _userForm.Hide();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error hiding parent window: {ex.Message}");
            }
        }

        // NEW: Method to center SignUpForm over parent window

        // Override SetVisibleCore to center over parent when shown
        protected override void SetVisibleCore(bool value)
        {
            if (value && this.WindowState != FormWindowState.Minimized)
            {
                CenterOverParent();
            }
            base.SetVisibleCore(value);
        }

        // Rest of your existing SignUpForm methods remain exactly the same...
        // (All the existing event handlers, validation methods, etc.)

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
            var response = await Test.connectionToUrlPatch(_changeCredUrl, content, _loginForm._jwtToken);
            if (response == HttpStatusCode.OK)
            {
                MessageBox.Show("Вы успешно сменили логин и пароль!");

                if (_loginForm is not null)
                {
                    if (_userForm is not null)
                    {
                        _userForm.Dispose();
                    }
                    _loginForm.Show();

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

        private void SignUpForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                // Prevent the form from being disposed
                e.Cancel = true;

                // Hide the form instead of closing it
                this.Hide();
            }
        }
    }
}
