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
    public partial class CoordinatorForm : Form
    {
        private Form? _loginForm;
        private string? _userName;
        public CoordinatorForm()
        {
            InitializeComponent();
        }

        public CoordinatorForm(Form loginForm, string userName)
        {
            _userName = userName;
            _loginForm = loginForm;
            InitializeComponent();
        }
    }
}
