namespace Kotova.Test1.ClientSide
{
    partial class UserForm
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
            UserLabel = new Label();
            ListOfInstructionsForUser = new ListBox();
            label3 = new Label();
            HyperLinkForInstructionsFolder = new Button();
            PassInstruction = new CheckBox();
            SignOut = new Button();
            FilesOfInstructionCheckedListBox = new CheckedListBox();
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
            label1.Location = new Point(15, 22);
            label1.Name = "label1";
            label1.Size = new Size(88, 15);
            label1.TabIndex = 22;
            label1.Text = "Вы вошли как:";
            // 
            // UserLabel
            // 
            UserLabel.AutoSize = true;
            UserLabel.Location = new Point(109, 22);
            UserLabel.Name = "UserLabel";
            UserLabel.Size = new Size(81, 15);
            UserLabel.TabIndex = 23;
            UserLabel.Text = "UnknownUser";
            // 
            // ListOfInstructionsForUser
            // 
            ListOfInstructionsForUser.FormattingEnabled = true;
            ListOfInstructionsForUser.ItemHeight = 15;
            ListOfInstructionsForUser.Location = new Point(12, 170);
            ListOfInstructionsForUser.Name = "ListOfInstructionsForUser";
            ListOfInstructionsForUser.Size = new Size(236, 229);
            ListOfInstructionsForUser.TabIndex = 24;
            ListOfInstructionsForUser.SelectedValueChanged += ListOfInstructions_SelectedValueChanged;
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
            // HyperLinkForInstructionsFolder
            // 
            HyperLinkForInstructionsFolder.Enabled = false;
            HyperLinkForInstructionsFolder.Location = new Point(662, 170);
            HyperLinkForInstructionsFolder.Name = "HyperLinkForInstructionsFolder";
            HyperLinkForInstructionsFolder.Size = new Size(111, 229);
            HyperLinkForInstructionsFolder.TabIndex = 26;
            HyperLinkForInstructionsFolder.Text = "Гиперссылка на инструктаж";
            HyperLinkForInstructionsFolder.UseVisualStyleBackColor = true;
            HyperLinkForInstructionsFolder.Click += HyperLinkForInstructionsFolder_Click;
            // 
            // PassInstruction
            // 
            PassInstruction.AutoSize = true;
            PassInstruction.Location = new Point(214, 419);
            PassInstruction.Name = "PassInstruction";
            PassInstruction.Size = new Size(245, 19);
            PassInstruction.TabIndex = 28;
            PassInstruction.Text = "Подтверждаю, что инструктаж пройден";
            PassInstruction.UseVisualStyleBackColor = true;
            PassInstruction.CheckedChanged += PassInstruction_CheckedChanged;
            // 
            // SignOut
            // 
            SignOut.Location = new Point(678, 12);
            SignOut.Name = "SignOut";
            SignOut.Size = new Size(110, 39);
            SignOut.TabIndex = 29;
            SignOut.Text = "Выйти из учётной записи";
            SignOut.UseVisualStyleBackColor = true;
            SignOut.Click += SignOut_Click;
            // 
            // FilesOfInstructionCheckedListBox
            // 
            FilesOfInstructionCheckedListBox.FormattingEnabled = true;
            FilesOfInstructionCheckedListBox.HorizontalScrollbar = true;
            FilesOfInstructionCheckedListBox.Location = new Point(296, 179);
            FilesOfInstructionCheckedListBox.Name = "FilesOfInstructionCheckedListBox";
            FilesOfInstructionCheckedListBox.Size = new Size(213, 220);
            FilesOfInstructionCheckedListBox.TabIndex = 30;
            FilesOfInstructionCheckedListBox.ItemCheck += FilesOfInstructionCheckedListBox_ItemCheck;
            // 
            // UserForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(800, 450);
            Controls.Add(FilesOfInstructionCheckedListBox);
            Controls.Add(SignOut);
            Controls.Add(PassInstruction);
            Controls.Add(HyperLinkForInstructionsFolder);
            Controls.Add(label3);
            Controls.Add(ListOfInstructionsForUser);
            Controls.Add(UserLabel);
            Controls.Add(label1);
            Controls.Add(CheckForNewInstructions);
            Name = "UserForm";
            Text = "User";
            FormClosed += UserForm_FormClosed;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button CheckForNewInstructions;
        private Label label1;
        private Label UserLabel;
        private ListBox ListOfInstructionsForUser;
        private Label label3;
        private Button HyperLinkForInstructionsFolder;
        private CheckBox PassInstruction;
        private Button SignOut;
        private CheckedListBox FilesOfInstructionCheckedListBox;
    }
}