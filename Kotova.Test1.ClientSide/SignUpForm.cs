using Kotova.CommonClasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kotova.Test1.ClientSide
{
    public partial class SignUpForm : Form
    {
        private Login_Russian form1;
        const string defaultLoginText = "Введите новый логин";
        const string defaultPasswordText = "Введите новый пароль";
        const string defaultPasswordRepeatText = "Повторите новый пароль";
        const string defaultEmailText = "Введите почту (Необязательно)";
        public SignUpForm(Login_Russian form)
        {
            InitializeComponent();
            form1 = form;
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
            form1.Location = this.Location;
            form1.Show();
            this.Hide();
        }

        private async void signUpButton_Click(object sender, EventArgs e)
        {
            string login = loginTextBox.Text;
            string password = PasswordTextBox.Text;
            string email = emailTextBox.Text;

            var userCredentials = new UserCredentials
            {
                Login = loginTextBox.Text,
                Password = PasswordTextBox.Text,
                Email = emailTextBox.Text
            };

            string jsonPayload = JsonConvert.SerializeObject(userCredentials);

            if (await isSuccesfullyUpdateCredentialsInDB(jsonPayload))
            {
                MessageBox.Show("Вы успешно сменили логин и пароль!");
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
    }
}
