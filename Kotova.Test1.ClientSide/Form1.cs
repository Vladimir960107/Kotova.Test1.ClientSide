using Kotova.CommonClasses;
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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = Encryption_Kotova.HashPassword(textBox1.Text);
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                string selectedFolder = folderBrowser.SelectedPath;
                // You can now do something with the selected folder
            }
        }

        private void PopulateTreeView(string directoryValue, TreeNode parentNode)
        {
            string[] directoryArray = Directory.GetDirectories(directoryValue);
            try
            {
                if (directoryArray.Length != 0)
                {
                    foreach (string directory in directoryArray)
                    {
                        string directoryName = Path.GetFileName(directory);
                        TreeNode myNode = new TreeNode(directoryName);
                        parentNode.Nodes.Add(myNode);
                        PopulateTreeView(directory, myNode);
                    }
                }
            }
            catch (UnauthorizedAccessException) { }
        }
    }
}
