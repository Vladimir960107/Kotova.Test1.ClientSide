namespace Kotova.Test1.ClientSide
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            EncryptByBCrypt = new Button();
            textBox1 = new TextBox();
            button1 = new Button();
            treeView1 = new TreeView();
            SuspendLayout();
            // 
            // EncryptByBCrypt
            // 
            EncryptByBCrypt.Location = new Point(40, 251);
            EncryptByBCrypt.Name = "EncryptByBCrypt";
            EncryptByBCrypt.Size = new Size(170, 23);
            EncryptByBCrypt.TabIndex = 0;
            EncryptByBCrypt.Text = "Encrypt by Bcrypt";
            EncryptByBCrypt.UseVisualStyleBackColor = true;
            EncryptByBCrypt.Click += button1_Click;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(40, 294);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(735, 23);
            textBox1.TabIndex = 1;
            // 
            // button1
            // 
            button1.Location = new Point(51, 54);
            button1.Name = "button1";
            button1.Size = new Size(159, 62);
            button1.TabIndex = 2;
            button1.Text = "test button for folder selection";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click_1;
            // 
            // treeView1
            // 
            treeView1.CheckBoxes = true;
            treeView1.Location = new Point(392, 79);
            treeView1.Name = "treeView1";
            treeView1.Size = new Size(247, 178);
            treeView1.TabIndex = 3;
            treeView1.AfterCheck += treeView1_AfterCheck;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(treeView1);
            Controls.Add(button1);
            Controls.Add(textBox1);
            Controls.Add(EncryptByBCrypt);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button EncryptByBCrypt;
        private TextBox textBox1;
        private Button button1;
        private TreeView treeView1;
    }
}