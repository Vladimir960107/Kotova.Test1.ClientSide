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
            components = new System.ComponentModel.Container();
            label1 = new Label();
            UserLabel = new Label();
            ListOfInstructionsForUser = new ListBox();
            label3 = new Label();
            PassInstruction = new CheckBox();
            FilesOfInstructionCheckedListBox = new CheckedListBox();
            AdditionalSettingsForUserContextMenuStrip = new ContextMenuStrip(components);
            UpdateInstructionsToolStripMenuItem = new ToolStripMenuItem();
            ChangeCredentialsToolStripMenuItem = new ToolStripMenuItem();
            signOutToolStripMenuItem = new ToolStripMenuItem();
            exitApplicationToolStripMenuItem = new ToolStripMenuItem();
            AdditionalSettingsPicture = new PictureBox();
            AdditionalSettingsForUserContextMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)AdditionalSettingsPicture).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(489, 33);
            label1.Name = "label1";
            label1.Size = new Size(88, 15);
            label1.TabIndex = 22;
            label1.Text = "Вы вошли как:";
            // 
            // UserLabel
            // 
            UserLabel.AutoSize = true;
            UserLabel.Location = new Point(592, 33);
            UserLabel.Name = "UserLabel";
            UserLabel.Size = new Size(81, 15);
            UserLabel.TabIndex = 23;
            UserLabel.Text = "UnknownUser";
            // 
            // ListOfInstructionsForUser
            // 
            ListOfInstructionsForUser.FormattingEnabled = true;
            ListOfInstructionsForUser.ItemHeight = 15;
            ListOfInstructionsForUser.Location = new Point(24, 131);
            ListOfInstructionsForUser.Name = "ListOfInstructionsForUser";
            ListOfInstructionsForUser.Size = new Size(351, 259);
            ListOfInstructionsForUser.TabIndex = 24;
            ListOfInstructionsForUser.SelectedValueChanged += ListOfInstructions_SelectedValueChanged;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(24, 95);
            label3.Name = "label3";
            label3.Size = new Size(201, 15);
            label3.TabIndex = 25;
            label3.Text = "Лист непройденных инструктажей:";
            // 
            // PassInstruction
            // 
            PassInstruction.AutoSize = true;
            PassInstruction.Location = new Point(267, 443);
            PassInstruction.Name = "PassInstruction";
            PassInstruction.Size = new Size(245, 19);
            PassInstruction.TabIndex = 28;
            PassInstruction.Text = "Подтверждаю, что инструктаж пройден";
            PassInstruction.UseVisualStyleBackColor = true;
            PassInstruction.CheckedChanged += PassInstruction_CheckedChanged;
            // 
            // FilesOfInstructionCheckedListBox
            // 
            FilesOfInstructionCheckedListBox.FormattingEnabled = true;
            FilesOfInstructionCheckedListBox.HorizontalScrollbar = true;
            FilesOfInstructionCheckedListBox.Location = new Point(411, 131);
            FilesOfInstructionCheckedListBox.Name = "FilesOfInstructionCheckedListBox";
            FilesOfInstructionCheckedListBox.Size = new Size(396, 256);
            FilesOfInstructionCheckedListBox.TabIndex = 30;
            FilesOfInstructionCheckedListBox.ItemCheck += FilesOfInstructionCheckedListBox_ItemCheck;
            // 
            // AdditionalSettingsForUserContextMenuStrip
            // 
            AdditionalSettingsForUserContextMenuStrip.Items.AddRange(new ToolStripItem[] { UpdateInstructionsToolStripMenuItem, ChangeCredentialsToolStripMenuItem, signOutToolStripMenuItem, exitApplicationToolStripMenuItem });
            AdditionalSettingsForUserContextMenuStrip.Name = "AdditionalSettingsForUser";
            AdditionalSettingsForUserContextMenuStrip.Size = new Size(269, 114);
            AdditionalSettingsForUserContextMenuStrip.Text = "Доп. настройки:";
            // 
            // UpdateInstructionsToolStripMenuItem
            // 
            UpdateInstructionsToolStripMenuItem.Name = "UpdateInstructionsToolStripMenuItem";
            UpdateInstructionsToolStripMenuItem.Size = new Size(268, 22);
            UpdateInstructionsToolStripMenuItem.Text = "Обновить инструктажи";
            UpdateInstructionsToolStripMenuItem.Click += UpdateInstructionsToolStripMenuItem_Click;
            // 
            // ChangeCredentialsToolStripMenuItem
            // 
            ChangeCredentialsToolStripMenuItem.Name = "ChangeCredentialsToolStripMenuItem";
            ChangeCredentialsToolStripMenuItem.Size = new Size(268, 22);
            ChangeCredentialsToolStripMenuItem.Text = "Сменить регистрационные данные";
            ChangeCredentialsToolStripMenuItem.Click += changeCredentialsToolStripMenuItem_Click;
            // 
            // signOutToolStripMenuItem
            // 
            signOutToolStripMenuItem.Name = "signOutToolStripMenuItem";
            signOutToolStripMenuItem.Size = new Size(268, 22);
            signOutToolStripMenuItem.Text = "Выйти из учётной записи";
            signOutToolStripMenuItem.Click += signOutToolStripMenuItem_Click;
            // 
            // exitApplicationToolStripMenuItem
            // 
            exitApplicationToolStripMenuItem.Name = "exitApplicationToolStripMenuItem";
            exitApplicationToolStripMenuItem.Size = new Size(268, 22);
            exitApplicationToolStripMenuItem.Text = "Выйти из программы";
            exitApplicationToolStripMenuItem.Click += exitApplicationToolStripMenuItem_Click;
            // 
            // AdditionalSettingsPicture
            // 
            AdditionalSettingsPicture.Image = Properties.Resources.setting_line_icon;
            AdditionalSettingsPicture.Location = new Point(22, 22);
            AdditionalSettingsPicture.Name = "AdditionalSettingsPicture";
            AdditionalSettingsPicture.Size = new Size(34, 36);
            AdditionalSettingsPicture.SizeMode = PictureBoxSizeMode.Zoom;
            AdditionalSettingsPicture.TabIndex = 35;
            AdditionalSettingsPicture.TabStop = false;
            AdditionalSettingsPicture.Click += AdditionalSettingsPicture_Click;
            // 
            // UserForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(829, 527);
            Controls.Add(AdditionalSettingsPicture);
            Controls.Add(FilesOfInstructionCheckedListBox);
            Controls.Add(PassInstruction);
            Controls.Add(label3);
            Controls.Add(ListOfInstructionsForUser);
            Controls.Add(UserLabel);
            Controls.Add(label1);
            Name = "UserForm";
            Text = "User";
            FormClosing += UserForm_FormClosing;
            AdditionalSettingsForUserContextMenuStrip.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)AdditionalSettingsPicture).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label label1;
        private Label UserLabel;
        private ListBox ListOfInstructionsForUser;
        private Label label3;
        private CheckBox PassInstruction;
        private CheckedListBox FilesOfInstructionCheckedListBox;
        private Button SendMessageButton;
        private ContextMenuStrip AdditionalSettingsForUserContextMenuStrip;
        private ToolStripMenuItem UpdateInstructionsToolStripMenuItem;
        private ToolStripMenuItem ChangeCredentialsToolStripMenuItem;
        private ToolStripMenuItem exitApplicationToolStripMenuItem;
        private ToolStripMenuItem signOutToolStripMenuItem;
        private PictureBox AdditionalSettingsPicture;
    }
}