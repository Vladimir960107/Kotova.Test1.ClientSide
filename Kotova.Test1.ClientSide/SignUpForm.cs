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
        public SignUpForm(Login_Russian form)
        {
            InitializeComponent();
            form1 = form;
        }

        private void textBox1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "Введите логин")
            {
                textBox1.Text = "";
            }
            textBox1.BackColor = Color.White;
            textBox2.BackColor = SystemColors.Control;

        }
        private void textBox2_Click(object sender, EventArgs e)
        {
            if (textBox2.Text == "Введите пароль")
            {
                textBox2.Text = "";
            }
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

        private void LogInButton_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "" || textBox2.Text == "")
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

        private void returnButton_Click(object sender, EventArgs e)
        {
            form1.Location = this.Location;
            form1.Show();
            this.Hide(); 
        }

        private void RegistrationButton_Click(object sender, EventArgs e)
        {
            
        }
    }
}
