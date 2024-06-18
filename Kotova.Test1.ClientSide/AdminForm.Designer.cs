namespace Kotova.Test1.ClientSide
{
    partial class AdminForm
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
            button1 = new Button();
            panel2 = new Panel();
            label25 = new Label();
            button9 = new Button();
            button10 = new Button();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(72, 267);
            button1.Name = "button1";
            button1.Size = new Size(160, 23);
            button1.TabIndex = 0;
            button1.Text = "ХЗ зачем это кнопка :D";
            button1.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            panel2.Controls.Add(label25);
            panel2.Controls.Add(button9);
            panel2.Controls.Add(button10);
            panel2.Location = new Point(430, 119);
            panel2.Name = "panel2";
            panel2.Size = new Size(221, 143);
            panel2.TabIndex = 25;
            // 
            // label25
            // 
            label25.AutoSize = true;
            label25.Location = new Point(17, 16);
            label25.Name = "label25";
            label25.RightToLeft = RightToLeft.Yes;
            label25.Size = new Size(183, 15);
            label25.TabIndex = 13;
            label25.Text = "(перемести на администратора!";
            // 
            // button9
            // 
            button9.Location = new Point(17, 75);
            button9.Name = "button9";
            button9.Size = new Size(192, 53);
            button9.TabIndex = 12;
            button9.Text = "Синхронизация последнего файла с сервером";
            button9.UseVisualStyleBackColor = true;
            // 
            // button10
            // 
            button10.Location = new Point(17, 34);
            button10.Name = "button10";
            button10.Size = new Size(192, 35);
            button10.TabIndex = 11;
            button10.Text = "Загрузка excel на сервер";
            button10.UseVisualStyleBackColor = true;
            // 
            // AdminForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(panel2);
            Controls.Add(button1);
            Name = "AdminForm";
            Text = "AdminForm";
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Button button1;
        private Panel panel2;
        private Label label25;
        private Button button9;
        private Button button10;
    }
}