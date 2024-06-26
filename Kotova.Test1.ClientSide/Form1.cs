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
                treeView1.Nodes.Clear();  // Clear the existing items in the TreeView
                TreeNode rootNode = new TreeNode(selectedFolder);
                treeView1.Nodes.Add(rootNode);  // Add a root node with the selected folder
                PopulateTreeView(selectedFolder, rootNode);  // Populate the TreeView
                //rootNode.Expand();  // Optionally expand the root node, enable if want to all the rootNode be collapsed (чтобы было видно все вложенные файлы сразу, не нажимая плюсик :))
            }
        }

        private void PopulateTreeView(string directoryValue, TreeNode parentNode)
        {
            // Processing directories
            string[] directoryArray = Directory.GetDirectories(directoryValue);
            try
            {
                foreach (string directory in directoryArray)
                {
                    string directoryName = Path.GetFileName(directory);
                    TreeNode myNode = new TreeNode(directoryName);
                    parentNode.Nodes.Add(myNode);
                    PopulateTreeView(directory, myNode);
                }
            }
            catch (UnauthorizedAccessException) { }

            // Processing files
            string[] fileArray = Directory.GetFiles(directoryValue);
            foreach (string file in fileArray)
            {
                string fileName = Path.GetFileName(file);
                TreeNode fileNode = new TreeNode(fileName);
                parentNode.Nodes.Add(fileNode);
            }
        }

        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            // Optional: Check/uncheck child nodes
            if (e.Action != TreeViewAction.Unknown) // Ensure the change was triggered by user interaction
            {
                foreach (TreeNode childNode in e.Node.Nodes)
                {
                    childNode.Checked = e.Node.Checked;
                }
            }
        }
    }
}
