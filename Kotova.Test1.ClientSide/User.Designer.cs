namespace Kotova.Test1.ClientSide
{
    partial class User
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
            CheckForNewInstructions = new Button();
            label1 = new Label();
            label2 = new Label();
            ListOfInstructions = new ListBox();
            label3 = new Label();
            button1 = new Button();
            PassInstruction = new RadioButton();
            SuspendLayout();
            // 
            // CheckForNewInstructions
            // 
            CheckForNewInstructions.Location = new Point(214, 53);
            CheckForNewInstructions.Name = "CheckForNewInstructions";
            CheckForNewInstructions.Size = new Size(273, 55);
            CheckForNewInstructions.TabIndex = 21;
            CheckForNewInstructions.Text = "Синхронизация с базой данных на проверку есть ли непройденные инструктажи";
            CheckForNewInstructions.UseVisualStyleBackColor = true;
            CheckForNewInstructions.Click += CheckForNewInstructions_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(28, 27);
            label1.Name = "label1";
            label1.Size = new Size(88, 15);
            label1.TabIndex = 22;
            label1.Text = "Вы вошли как:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(122, 27);
            label2.Name = "label2";
            label2.Size = new Size(0, 15);
            label2.TabIndex = 23;
            // 
            // ListOfInstructions
            // 
            ListOfInstructions.FormattingEnabled = true;
            ListOfInstructions.ItemHeight = 15;
            ListOfInstructions.Location = new Point(214, 170);
            ListOfInstructions.Name = "ListOfInstructions";
            ListOfInstructions.Size = new Size(430, 229);
            ListOfInstructions.TabIndex = 24;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(214, 127);
            label3.Name = "label3";
            label3.Size = new Size(201, 15);
            label3.TabIndex = 25;
            label3.Text = "Лист непройденных инструктажей:";
            // 
            // button1
            // 
            button1.Location = new Point(662, 170);
            button1.Name = "button1";
            button1.Size = new Size(111, 229);
            button1.TabIndex = 26;
            button1.Text = "Гиперссылка на инструктаж";
            button1.UseVisualStyleBackColor = true;
            // 
            // PassInstruction
            // 
            PassInstruction.AutoSize = true;
            PassInstruction.Enabled = false;
            PassInstruction.Location = new Point(214, 419);
            PassInstruction.Name = "PassInstruction";
            PassInstruction.Size = new Size(244, 19);
            PassInstruction.TabIndex = 27;
            PassInstruction.TabStop = true;
            PassInstruction.Text = "Подтверждаю, что интсруктаж пройден";
            PassInstruction.UseVisualStyleBackColor = true;
            // 
            // User
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(PassInstruction);
            Controls.Add(button1);
            Controls.Add(label3);
            Controls.Add(ListOfInstructions);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(CheckForNewInstructions);
            Name = "User";
            Text = "User";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button CheckForNewInstructions;
        private Label label1;
        private Label label2;
        private ListBox ListOfInstructions;
        private Label label3;
        private Button button1;
        private RadioButton PassInstruction;
    }
}