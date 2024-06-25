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
    public partial class AdminForm : Form
    {

        private Form? _loginForm;
        string? _userName;
        public AdminForm(Form loginForm, string userName)
        {
            _loginForm = loginForm;
            _userName = userName;
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _loginForm.Show();
            this.Dispose();
        }
    }
}
