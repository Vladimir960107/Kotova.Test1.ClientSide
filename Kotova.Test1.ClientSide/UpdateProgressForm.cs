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
    public partial class UpdateProgressForm : Form
    {
        public UpdateProgressForm()
        {
            InitializeComponent();
        }

        public void UpdateProgress(int percent)
        {
            if (progressBar.InvokeRequired)
            {
                progressBar.Invoke(new Action(() => progressBar.Value = percent));
            }
            else
            {
                progressBar.Value = percent;
            }
        }

        public void SetMessage(string message)
        {
            if (labelMessage.InvokeRequired)
            {
                labelMessage.Invoke(new Action(() => labelMessage.Text = message));
            }
            else
            {
                labelMessage.Text = message;
            }
        }
    }
}
