﻿using System;
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
    public partial class Login : Form
    {

        public Login()
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
    }
}
