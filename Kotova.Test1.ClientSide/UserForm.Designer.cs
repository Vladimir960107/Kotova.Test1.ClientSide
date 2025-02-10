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
            LabelOfNotPassedInstr = new Label();
            PassInstruction = new CheckBox();
            FilesOfInstructionCheckedListBox = new CheckedListBox();
            AdditionalSettingsForUserContextMenuStrip = new ContextMenuStrip(components);
            UpdateInstructionsToolStripMenuItem = new ToolStripMenuItem();
            ChangeCredentialsToolStripMenuItem = new ToolStripMenuItem();
            signOutToolStripMenuItem = new ToolStripMenuItem();
            exitApplicationToolStripMenuItem = new ToolStripMenuItem();
            AdditionalSettingsPicture = new PictureBox();
            PassedOrNotInstrTabControl = new TabControl();
            NotPassedInstrTabPage = new TabPage();
            tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel2 = new TableLayoutPanel();
            PassedInstrTabPage = new TabPage();
            tableLayoutPanel5 = new TableLayoutPanel();
            tableLayoutPanel6 = new TableLayoutPanel();
            listBoxOfPathsOfPassedInstructions = new ListBox();
            dataGridViewPassedInstructions = new DataGridView();
            DateOfPassedInstr = new DataGridViewTextBoxColumn();
            TypeOfPassedInstr = new DataGridViewTextBoxColumn();
            CauseOfPassedInstr = new DataGridViewTextBoxColumn();
            LabelOfPassedInstr = new Label();
            tableLayoutPanel4 = new TableLayoutPanel();
            tableLayoutPanel3 = new TableLayoutPanel();
            tableLayoutPanel7 = new TableLayoutPanel();
            tableLayoutPanel8 = new TableLayoutPanel();
            tableLayoutPanel9 = new TableLayoutPanel();
            AdditionalSettingsForUserContextMenuStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)AdditionalSettingsPicture).BeginInit();
            PassedOrNotInstrTabControl.SuspendLayout();
            NotPassedInstrTabPage.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            tableLayoutPanel2.SuspendLayout();
            PassedInstrTabPage.SuspendLayout();
            tableLayoutPanel5.SuspendLayout();
            tableLayoutPanel6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewPassedInstructions).BeginInit();
            tableLayoutPanel7.SuspendLayout();
            tableLayoutPanel8.SuspendLayout();
            tableLayoutPanel9.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Location = new Point(3, 0);
            label1.Name = "label1";
            label1.Size = new Size(91, 38);
            label1.TabIndex = 22;
            label1.Text = "Вы вошли как:";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // UserLabel
            // 
            UserLabel.AutoSize = true;
            UserLabel.Dock = DockStyle.Fill;
            UserLabel.Location = new Point(100, 0);
            UserLabel.Name = "UserLabel";
            UserLabel.Size = new Size(91, 38);
            UserLabel.TabIndex = 23;
            UserLabel.Text = "UnknownUser";
            UserLabel.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // ListOfInstructionsForUser
            // 
            ListOfInstructionsForUser.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            ListOfInstructionsForUser.FormattingEnabled = true;
            ListOfInstructionsForUser.ItemHeight = 15;
            ListOfInstructionsForUser.Location = new Point(3, 3);
            ListOfInstructionsForUser.Name = "ListOfInstructionsForUser";
            ListOfInstructionsForUser.Size = new Size(353, 334);
            ListOfInstructionsForUser.TabIndex = 24;
            ListOfInstructionsForUser.SelectedValueChanged += ListOfInstructions_SelectedValueChanged;
            // 
            // LabelOfNotPassedInstr
            // 
            LabelOfNotPassedInstr.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            LabelOfNotPassedInstr.AutoSize = true;
            LabelOfNotPassedInstr.Location = new Point(3, 0);
            LabelOfNotPassedInstr.Name = "LabelOfNotPassedInstr";
            LabelOfNotPassedInstr.Size = new Size(718, 40);
            LabelOfNotPassedInstr.TabIndex = 25;
            LabelOfNotPassedInstr.Text = "Лист непройденных инструктажей:";
            LabelOfNotPassedInstr.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // PassInstruction
            // 
            PassInstruction.Anchor = AnchorStyles.None;
            PassInstruction.Location = new Point(238, 403);
            PassInstruction.Name = "PassInstruction";
            PassInstruction.Size = new Size(248, 19);
            PassInstruction.TabIndex = 28;
            PassInstruction.Text = "Подтверждаю, что инструктаж пройден";
            PassInstruction.UseVisualStyleBackColor = true;
            PassInstruction.CheckedChanged += PassInstruction_CheckedChanged;
            // 
            // FilesOfInstructionCheckedListBox
            // 
            FilesOfInstructionCheckedListBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            FilesOfInstructionCheckedListBox.FormattingEnabled = true;
            FilesOfInstructionCheckedListBox.HorizontalScrollbar = true;
            FilesOfInstructionCheckedListBox.Location = new Point(362, 3);
            FilesOfInstructionCheckedListBox.Name = "FilesOfInstructionCheckedListBox";
            FilesOfInstructionCheckedListBox.Size = new Size(353, 328);
            FilesOfInstructionCheckedListBox.TabIndex = 30;
            FilesOfInstructionCheckedListBox.ItemCheck += FilesOfInstructionCheckedListBox_ItemCheck;
            // 
            // AdditionalSettingsForUserContextMenuStrip
            // 
            AdditionalSettingsForUserContextMenuStrip.Items.AddRange(new ToolStripItem[] { UpdateInstructionsToolStripMenuItem, ChangeCredentialsToolStripMenuItem, signOutToolStripMenuItem, exitApplicationToolStripMenuItem });
            AdditionalSettingsForUserContextMenuStrip.Name = "AdditionalSettingsForUser";
            AdditionalSettingsForUserContextMenuStrip.Size = new Size(269, 92);
            AdditionalSettingsForUserContextMenuStrip.Text = "Доп. настройки:";
            AdditionalSettingsForUserContextMenuStrip.Opening += AdditionalSettingsForUserContextMenuStrip_Opening;
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
            AdditionalSettingsPicture.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            AdditionalSettingsPicture.Image = Properties.Resources.setting_line_icon;
            AdditionalSettingsPicture.Location = new Point(691, 3);
            AdditionalSettingsPicture.Name = "AdditionalSettingsPicture";
            AdditionalSettingsPicture.Size = new Size(44, 38);
            AdditionalSettingsPicture.SizeMode = PictureBoxSizeMode.Zoom;
            AdditionalSettingsPicture.TabIndex = 35;
            AdditionalSettingsPicture.TabStop = false;
            AdditionalSettingsPicture.Click += AdditionalSettingsPicture_Click;
            // 
            // PassedOrNotInstrTabControl
            // 
            PassedOrNotInstrTabControl.Controls.Add(NotPassedInstrTabPage);
            PassedOrNotInstrTabControl.Controls.Add(PassedInstrTabPage);
            PassedOrNotInstrTabControl.Dock = DockStyle.Fill;
            PassedOrNotInstrTabControl.Location = new Point(3, 53);
            PassedOrNotInstrTabControl.Name = "PassedOrNotInstrTabControl";
            PassedOrNotInstrTabControl.SelectedIndex = 0;
            PassedOrNotInstrTabControl.Size = new Size(738, 467);
            PassedOrNotInstrTabControl.TabIndex = 36;
            PassedOrNotInstrTabControl.SelectedIndexChanged += PassedOrNotInstrTabControl_SelectedIndexChanged;
            PassedOrNotInstrTabControl.TabIndexChanged += PassedOrNotInstrTabControl_TabIndexChanged;
            // 
            // NotPassedInstrTabPage
            // 
            NotPassedInstrTabPage.Controls.Add(tableLayoutPanel1);
            NotPassedInstrTabPage.Location = new Point(4, 24);
            NotPassedInstrTabPage.Name = "NotPassedInstrTabPage";
            NotPassedInstrTabPage.Padding = new Padding(3);
            NotPassedInstrTabPage.Size = new Size(730, 439);
            NotPassedInstrTabPage.TabIndex = 0;
            NotPassedInstrTabPage.Text = "Непройденные инструктажи";
            NotPassedInstrTabPage.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(tableLayoutPanel2, 0, 1);
            tableLayoutPanel1.Controls.Add(LabelOfNotPassedInstr, 0, 0);
            tableLayoutPanel1.Controls.Add(PassInstruction, 0, 2);
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(3, 3);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel1.Size = new Size(724, 433);
            tableLayoutPanel1.TabIndex = 31;
            // 
            // tableLayoutPanel2
            // 
            tableLayoutPanel2.ColumnCount = 2;
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel2.Controls.Add(ListOfInstructionsForUser, 0, 0);
            tableLayoutPanel2.Controls.Add(FilesOfInstructionCheckedListBox, 1, 0);
            tableLayoutPanel2.Dock = DockStyle.Fill;
            tableLayoutPanel2.Location = new Point(3, 43);
            tableLayoutPanel2.Name = "tableLayoutPanel2";
            tableLayoutPanel2.RowCount = 1;
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel2.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel2.Size = new Size(718, 347);
            tableLayoutPanel2.TabIndex = 0;
            // 
            // PassedInstrTabPage
            // 
            PassedInstrTabPage.Controls.Add(tableLayoutPanel5);
            PassedInstrTabPage.Location = new Point(4, 24);
            PassedInstrTabPage.Name = "PassedInstrTabPage";
            PassedInstrTabPage.Padding = new Padding(3);
            PassedInstrTabPage.Size = new Size(730, 439);
            PassedInstrTabPage.TabIndex = 1;
            PassedInstrTabPage.Text = "Пройденные инструктажи";
            PassedInstrTabPage.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel5
            // 
            tableLayoutPanel5.ColumnCount = 1;
            tableLayoutPanel5.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel5.Controls.Add(tableLayoutPanel6, 0, 1);
            tableLayoutPanel5.Controls.Add(LabelOfPassedInstr, 0, 0);
            tableLayoutPanel5.Dock = DockStyle.Fill;
            tableLayoutPanel5.Location = new Point(3, 3);
            tableLayoutPanel5.Name = "tableLayoutPanel5";
            tableLayoutPanel5.RowCount = 2;
            tableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel5.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel5.Size = new Size(724, 433);
            tableLayoutPanel5.TabIndex = 37;
            // 
            // tableLayoutPanel6
            // 
            tableLayoutPanel6.ColumnCount = 2;
            tableLayoutPanel6.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel6.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel6.Controls.Add(listBoxOfPathsOfPassedInstructions, 1, 0);
            tableLayoutPanel6.Controls.Add(dataGridViewPassedInstructions, 0, 0);
            tableLayoutPanel6.Dock = DockStyle.Fill;
            tableLayoutPanel6.Location = new Point(3, 43);
            tableLayoutPanel6.Name = "tableLayoutPanel6";
            tableLayoutPanel6.RowCount = 1;
            tableLayoutPanel6.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel6.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel6.Size = new Size(718, 387);
            tableLayoutPanel6.TabIndex = 0;
            // 
            // listBoxOfPathsOfPassedInstructions
            // 
            listBoxOfPathsOfPassedInstructions.Dock = DockStyle.Fill;
            listBoxOfPathsOfPassedInstructions.FormattingEnabled = true;
            listBoxOfPathsOfPassedInstructions.ItemHeight = 15;
            listBoxOfPathsOfPassedInstructions.Location = new Point(362, 3);
            listBoxOfPathsOfPassedInstructions.Name = "listBoxOfPathsOfPassedInstructions";
            listBoxOfPathsOfPassedInstructions.Size = new Size(353, 381);
            listBoxOfPathsOfPassedInstructions.TabIndex = 35;
            listBoxOfPathsOfPassedInstructions.DoubleClick += listBoxOfPathsOfPassedInstructions_DoubleClick;
            // 
            // dataGridViewPassedInstructions
            // 
            dataGridViewPassedInstructions.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGridViewPassedInstructions.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewPassedInstructions.Columns.AddRange(new DataGridViewColumn[] { DateOfPassedInstr, TypeOfPassedInstr, CauseOfPassedInstr });
            dataGridViewPassedInstructions.Location = new Point(3, 3);
            dataGridViewPassedInstructions.MultiSelect = false;
            dataGridViewPassedInstructions.Name = "dataGridViewPassedInstructions";
            dataGridViewPassedInstructions.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridViewPassedInstructions.Size = new Size(353, 381);
            dataGridViewPassedInstructions.TabIndex = 34;
            dataGridViewPassedInstructions.CellClick += dataGridViewPassedInstructions_CellClick;
            dataGridViewPassedInstructions.SelectionChanged += dataGridViewPassedInstructions_SelectionChanged;
            // 
            // DateOfPassedInstr
            // 
            DateOfPassedInstr.HeaderText = "Дата инструктажа";
            DateOfPassedInstr.Name = "DateOfPassedInstr";
            DateOfPassedInstr.ReadOnly = true;
            // 
            // TypeOfPassedInstr
            // 
            TypeOfPassedInstr.HeaderText = "Тип инструктажа";
            TypeOfPassedInstr.Name = "TypeOfPassedInstr";
            TypeOfPassedInstr.ReadOnly = true;
            // 
            // CauseOfPassedInstr
            // 
            CauseOfPassedInstr.HeaderText = "Причина инструктажа";
            CauseOfPassedInstr.Name = "CauseOfPassedInstr";
            CauseOfPassedInstr.ReadOnly = true;
            // 
            // LabelOfPassedInstr
            // 
            LabelOfPassedInstr.Anchor = AnchorStyles.Left;
            LabelOfPassedInstr.AutoSize = true;
            LabelOfPassedInstr.Location = new Point(3, 12);
            LabelOfPassedInstr.Name = "LabelOfPassedInstr";
            LabelOfPassedInstr.Size = new Size(188, 15);
            LabelOfPassedInstr.TabIndex = 32;
            LabelOfPassedInstr.Text = "Лист пройденных инструктажей:";
            // 
            // tableLayoutPanel4
            // 
            tableLayoutPanel4.ColumnCount = 2;
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel4.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel4.Dock = DockStyle.Fill;
            tableLayoutPanel4.Location = new Point(3, 3);
            tableLayoutPanel4.Name = "tableLayoutPanel4";
            tableLayoutPanel4.RowCount = 1;
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel4.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
            tableLayoutPanel4.Size = new Size(194, 276);
            tableLayoutPanel4.TabIndex = 0;
            // 
            // tableLayoutPanel3
            // 
            tableLayoutPanel3.ColumnCount = 1;
            tableLayoutPanel3.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel3.Location = new Point(0, 0);
            tableLayoutPanel3.Name = "tableLayoutPanel3";
            tableLayoutPanel3.RowCount = 3;
            tableLayoutPanel3.Size = new Size(200, 100);
            tableLayoutPanel3.TabIndex = 0;
            // 
            // tableLayoutPanel7
            // 
            tableLayoutPanel7.ColumnCount = 1;
            tableLayoutPanel7.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel7.Controls.Add(tableLayoutPanel8, 0, 0);
            tableLayoutPanel7.Controls.Add(PassedOrNotInstrTabControl, 0, 1);
            tableLayoutPanel7.Dock = DockStyle.Fill;
            tableLayoutPanel7.Location = new Point(0, 0);
            tableLayoutPanel7.Name = "tableLayoutPanel7";
            tableLayoutPanel7.RowCount = 2;
            tableLayoutPanel7.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            tableLayoutPanel7.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel7.Size = new Size(744, 523);
            tableLayoutPanel7.TabIndex = 37;
            // 
            // tableLayoutPanel8
            // 
            tableLayoutPanel8.ColumnCount = 3;
            tableLayoutPanel8.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F));
            tableLayoutPanel8.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel8.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50F));
            tableLayoutPanel8.Controls.Add(tableLayoutPanel9, 0, 0);
            tableLayoutPanel8.Controls.Add(AdditionalSettingsPicture, 2, 0);
            tableLayoutPanel8.Dock = DockStyle.Fill;
            tableLayoutPanel8.Location = new Point(3, 3);
            tableLayoutPanel8.Name = "tableLayoutPanel8";
            tableLayoutPanel8.RowCount = 1;
            tableLayoutPanel8.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutPanel8.Size = new Size(738, 44);
            tableLayoutPanel8.TabIndex = 0;
            // 
            // tableLayoutPanel9
            // 
            tableLayoutPanel9.ColumnCount = 2;
            tableLayoutPanel9.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel9.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel9.Controls.Add(label1, 0, 0);
            tableLayoutPanel9.Controls.Add(UserLabel, 1, 0);
            tableLayoutPanel9.Dock = DockStyle.Fill;
            tableLayoutPanel9.Location = new Point(3, 3);
            tableLayoutPanel9.Name = "tableLayoutPanel9";
            tableLayoutPanel9.RowCount = 1;
            tableLayoutPanel9.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel9.Size = new Size(194, 38);
            tableLayoutPanel9.TabIndex = 38;
            // 
            // UserForm
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(744, 523);
            Controls.Add(tableLayoutPanel7);
            Name = "UserForm";
            Text = "User";
            FormClosing += UserForm_FormClosing;
            Load += UserForm_Load;
            Resize += UserForm_Resize;
            AdditionalSettingsForUserContextMenuStrip.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)AdditionalSettingsPicture).EndInit();
            PassedOrNotInstrTabControl.ResumeLayout(false);
            NotPassedInstrTabPage.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            tableLayoutPanel2.ResumeLayout(false);
            PassedInstrTabPage.ResumeLayout(false);
            tableLayoutPanel5.ResumeLayout(false);
            tableLayoutPanel5.PerformLayout();
            tableLayoutPanel6.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridViewPassedInstructions).EndInit();
            tableLayoutPanel7.ResumeLayout(false);
            tableLayoutPanel8.ResumeLayout(false);
            tableLayoutPanel9.ResumeLayout(false);
            tableLayoutPanel9.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private Label label1;
        private Label UserLabel;
        private ListBox ListOfInstructionsForUser;
        private Label LabelOfNotPassedInstr;
        private CheckBox PassInstruction;
        private CheckedListBox FilesOfInstructionCheckedListBox;
        private Button SendMessageButton;
        private ContextMenuStrip AdditionalSettingsForUserContextMenuStrip;
        private ToolStripMenuItem UpdateInstructionsToolStripMenuItem;
        private ToolStripMenuItem ChangeCredentialsToolStripMenuItem;
        private ToolStripMenuItem exitApplicationToolStripMenuItem;
        private ToolStripMenuItem signOutToolStripMenuItem;
        private PictureBox AdditionalSettingsPicture;
        private TabControl PassedOrNotInstrTabControl;
        private TabPage NotPassedInstrTabPage;
        private TabPage PassedInstrTabPage;
        private Label LabelOfPassedInstr;
        private DataGridView dataGridViewPassedInstructions;
        private DataGridViewTextBoxColumn DateOfPassedInstr;
        private DataGridViewTextBoxColumn TypeOfPassedInstr;
        private DataGridViewTextBoxColumn CauseOfPassedInstr;
        private ListBox listBoxOfPathsOfPassedInstructions;
        private TableLayoutPanel tableLayoutPanel1;
        private TableLayoutPanel tableLayoutPanel2;
        private TableLayoutPanel tableLayoutPanel5;
        private TableLayoutPanel tableLayoutPanel6;
        private TableLayoutPanel tableLayoutPanel4;
        private TableLayoutPanel tableLayoutPanel3;
        private TableLayoutPanel tableLayoutPanel7;
        private TableLayoutPanel tableLayoutPanel8;
        private TableLayoutPanel tableLayoutPanel9;
    }
}