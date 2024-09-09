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
        protected async override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            if (_hubConnection != null)
            {
                await _hubConnection.StopAsync();
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
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
            ExitTheProgrammEntirelyButton = new Button();
            title = new TextBox();
            text = new TextBox();
            showNotification = new Button();
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
            // ExitTheProgrammEntirelyButton
            // 
            ExitTheProgrammEntirelyButton.Location = new Point(678, 57);
            ExitTheProgrammEntirelyButton.Name = "ExitTheProgrammEntirelyButton";
            ExitTheProgrammEntirelyButton.Size = new Size(110, 51);
            ExitTheProgrammEntirelyButton.TabIndex = 32;
            ExitTheProgrammEntirelyButton.Text = "Выйти из программы";
            ExitTheProgrammEntirelyButton.UseVisualStyleBackColor = true;
            ExitTheProgrammEntirelyButton.Click += ExitTheProgrammEntirelyButton_Click;
            // 
            // title
            // 
            title.Location = new Point(541, 70);
            title.Name = "title";
            title.Size = new Size(100, 23);
            title.TabIndex = 33;
            // 
            // text
            // 
            text.Location = new Point(541, 119);
            text.Name = "text";
            text.Size = new Size(100, 23);
            text.TabIndex = 34;
            // 
            // showNotification
            // 
            showNotification.Location = new Point(551, 170);
            showNotification.Name = "showNotification";
            showNotification.Size = new Size(75, 23);
            showNotification.TabIndex = 35;
            showNotification.Text = "button1";
            showNotification.UseVisualStyleBackColor = true;
            showNotification.Click += showNotification_Click;
            // 
            // UserForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(800, 450);
            Controls.Add(showNotification);
            Controls.Add(text);
            Controls.Add(title);
            Controls.Add(ExitTheProgrammEntirelyButton);
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
            FormClosing += UserForm_FormClosing;
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
        private Button SendMessageButton;
        private Button ExitTheProgrammEntirelyButton;
        private TextBox title;
        private TextBox text;
        private Button showNotification;
    }
}