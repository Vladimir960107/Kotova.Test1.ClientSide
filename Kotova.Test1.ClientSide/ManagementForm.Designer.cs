namespace Kotova.Test1.ClientSide
{
    partial class ManagementForm
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
            ManagementLabel = new Label();
            label1 = new Label();
            ManagementTabControl = new TabControl();
            tabPage1 = new TabPage();
            PathToFolderOfInstruction = new Label();
            treeView1 = new TreeViewWithoutDoubleClick();
            buttonChoosePathToInstruction = new Button();
            InstructionTextBox = new TextBox();
            datePickerEnd = new DateTimePicker();
            PeopleAndDepartmentsTreeView = new TreeViewWithoutDoubleClick();
            label5 = new Label();
            typeOfInstructionListBox = new ListBox();
            DepartmentsCheckedListBox = new CheckedListBox();
            button5 = new Button();
            buttonCreateInstruction = new Button();
            label9 = new Label();
            label4 = new Label();
            label3 = new Label();
            label2 = new Label();
            tabPage2 = new TabPage();
            FilesOfInstructionCheckedListBox = new CheckedListBox();
            PassInstructionAsUser = new CheckBox();
            ListOfInstructionsForUser = new ListBox();
            HyperLinkForInstructionsFolder = new Button();
            label10 = new Label();
            signUpButton = new Button();
            LogOutButton = new Button();
            ManagementTabControl.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            SuspendLayout();
            // 
            // ManagementLabel
            // 
            ManagementLabel.AutoSize = true;
            ManagementLabel.Location = new Point(136, 44);
            ManagementLabel.Name = "ManagementLabel";
            ManagementLabel.Size = new Size(81, 15);
            ManagementLabel.TabIndex = 25;
            ManagementLabel.Text = "UnknownUser";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(42, 44);
            label1.Name = "label1";
            label1.Size = new Size(88, 15);
            label1.TabIndex = 24;
            label1.Text = "Вы вошли как:";
            // 
            // ManagementTabControl
            // 
            ManagementTabControl.Controls.Add(tabPage1);
            ManagementTabControl.Controls.Add(tabPage2);
            ManagementTabControl.Location = new Point(42, 79);
            ManagementTabControl.Name = "ManagementTabControl";
            ManagementTabControl.SelectedIndex = 0;
            ManagementTabControl.Size = new Size(1180, 633);
            ManagementTabControl.TabIndex = 30;
            ManagementTabControl.SelectedIndexChanged += ManagementTabControl_SelectedIndexChanged;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(PathToFolderOfInstruction);
            tabPage1.Controls.Add(treeView1);
            tabPage1.Controls.Add(buttonChoosePathToInstruction);
            tabPage1.Controls.Add(InstructionTextBox);
            tabPage1.Controls.Add(datePickerEnd);
            tabPage1.Controls.Add(PeopleAndDepartmentsTreeView);
            tabPage1.Controls.Add(label5);
            tabPage1.Controls.Add(typeOfInstructionListBox);
            tabPage1.Controls.Add(DepartmentsCheckedListBox);
            tabPage1.Controls.Add(button5);
            tabPage1.Controls.Add(buttonCreateInstruction);
            tabPage1.Controls.Add(label9);
            tabPage1.Controls.Add(label4);
            tabPage1.Controls.Add(label3);
            tabPage1.Controls.Add(label2);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(1172, 605);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Создание инструктажей";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // PathToFolderOfInstruction
            // 
            PathToFolderOfInstruction.AutoSize = true;
            PathToFolderOfInstruction.Location = new Point(306, 202);
            PathToFolderOfInstruction.Name = "PathToFolderOfInstruction";
            PathToFolderOfInstruction.Size = new Size(94, 15);
            PathToFolderOfInstruction.TabIndex = 40;
            PathToFolderOfInstruction.Text = "Путь не выбран";
            // 
            // treeView1
            // 
            treeView1.CheckBoxes = true;
            treeView1.Location = new Point(306, 240);
            treeView1.Name = "treeView1";
            treeView1.Size = new Size(247, 178);
            treeView1.TabIndex = 39;
            treeView1.AfterCheck += treeView1_AfterCheck;
            // 
            // buttonChoosePathToInstruction
            // 
            buttonChoosePathToInstruction.Location = new Point(306, 118);
            buttonChoosePathToInstruction.Margin = new Padding(3, 2, 3, 2);
            buttonChoosePathToInstruction.Name = "buttonChoosePathToInstruction";
            buttonChoosePathToInstruction.Size = new Size(172, 56);
            buttonChoosePathToInstruction.TabIndex = 38;
            buttonChoosePathToInstruction.Text = "Указать папку для инструктажа";
            buttonChoosePathToInstruction.UseVisualStyleBackColor = true;
            buttonChoosePathToInstruction.Click += buttonChoosePathToInstruction_Click;
            // 
            // InstructionTextBox
            // 
            InstructionTextBox.Location = new Point(38, 503);
            InstructionTextBox.Margin = new Padding(3, 2, 3, 2);
            InstructionTextBox.Multiline = true;
            InstructionTextBox.Name = "InstructionTextBox";
            InstructionTextBox.Size = new Size(219, 51);
            InstructionTextBox.TabIndex = 36;
            InstructionTextBox.Text = "Работа в зоне железнодорожных путей СТО-357";
            // 
            // datePickerEnd
            // 
            datePickerEnd.Location = new Point(38, 422);
            datePickerEnd.Margin = new Padding(3, 2, 3, 2);
            datePickerEnd.Name = "datePickerEnd";
            datePickerEnd.Size = new Size(219, 23);
            datePickerEnd.TabIndex = 33;
            // 
            // PeopleAndDepartmentsTreeView
            // 
            PeopleAndDepartmentsTreeView.CheckBoxes = true;
            PeopleAndDepartmentsTreeView.Location = new Point(670, 230);
            PeopleAndDepartmentsTreeView.Name = "PeopleAndDepartmentsTreeView";
            PeopleAndDepartmentsTreeView.Size = new Size(261, 130);
            PeopleAndDepartmentsTreeView.TabIndex = 32;
            PeopleAndDepartmentsTreeView.AfterCheck += peopleAndDepartmentsTreeView_AfterCheck;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(38, 107);
            label5.Name = "label5";
            label5.Size = new Size(148, 15);
            label5.TabIndex = 30;
            label5.Text = "Выбор типа инструктажа:";
            // 
            // typeOfInstructionListBox
            // 
            typeOfInstructionListBox.FormattingEnabled = true;
            typeOfInstructionListBox.ItemHeight = 15;
            typeOfInstructionListBox.Items.AddRange(new object[] { "Первичный;", "Повторный;", "Целевой;", "Внеплановый;" });
            typeOfInstructionListBox.Location = new Point(38, 134);
            typeOfInstructionListBox.Name = "typeOfInstructionListBox";
            typeOfInstructionListBox.Size = new Size(120, 64);
            typeOfInstructionListBox.TabIndex = 29;
            typeOfInstructionListBox.SelectedValueChanged += typeOfInstructionListBox_SelectedValueChanged;
            // 
            // DepartmentsCheckedListBox
            // 
            DepartmentsCheckedListBox.FormattingEnabled = true;
            DepartmentsCheckedListBox.Location = new Point(38, 240);
            DepartmentsCheckedListBox.Name = "DepartmentsCheckedListBox";
            DepartmentsCheckedListBox.Size = new Size(213, 130);
            DepartmentsCheckedListBox.TabIndex = 28;
            // 
            // button5
            // 
            button5.Location = new Point(970, 67);
            button5.Name = "button5";
            button5.Size = new Size(178, 286);
            button5.TabIndex = 27;
            button5.Text = "Сформировать excel форму об инструктажах";
            button5.UseVisualStyleBackColor = true;
            // 
            // buttonCreateInstruction
            // 
            buttonCreateInstruction.Location = new Point(670, 450);
            buttonCreateInstruction.Name = "buttonCreateInstruction";
            buttonCreateInstruction.Size = new Size(214, 74);
            buttonCreateInstruction.TabIndex = 26;
            buttonCreateInstruction.Text = "Отправить инструктаж сотрудникам";
            buttonCreateInstruction.UseVisualStyleBackColor = true;
            buttonCreateInstruction.Click += buttonCreateInstruction_Click;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(670, 202);
            label9.Name = "label9";
            label9.Size = new Size(295, 15);
            label9.TabIndex = 24;
            label9.Text = "ОТДЕЛЫ И ФИО людей, отвечающих за инструктаж:";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(38, 468);
            label4.Name = "label4";
            label4.Size = new Size(133, 15);
            label4.TabIndex = 18;
            label4.Text = "Причина инструктажа:";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(38, 212);
            label3.Name = "label3";
            label3.Size = new Size(52, 15);
            label3.TabIndex = 16;
            label3.Text = "Отделы:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(38, 394);
            label2.Name = "label2";
            label2.Size = new Size(239, 15);
            label2.TabIndex = 15;
            label2.Text = "Дата окончания проведения инструктажа:";
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(FilesOfInstructionCheckedListBox);
            tabPage2.Controls.Add(PassInstructionAsUser);
            tabPage2.Controls.Add(ListOfInstructionsForUser);
            tabPage2.Controls.Add(HyperLinkForInstructionsFolder);
            tabPage2.Controls.Add(label10);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(1172, 605);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Прохождение инструктажей";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // FilesOfInstructionCheckedListBox
            // 
            FilesOfInstructionCheckedListBox.FormattingEnabled = true;
            FilesOfInstructionCheckedListBox.HorizontalScrollbar = true;
            FilesOfInstructionCheckedListBox.Location = new Point(340, 92);
            FilesOfInstructionCheckedListBox.Name = "FilesOfInstructionCheckedListBox";
            FilesOfInstructionCheckedListBox.Size = new Size(195, 220);
            FilesOfInstructionCheckedListBox.TabIndex = 49;
            FilesOfInstructionCheckedListBox.ItemCheck += FilesOfInstructionCheckedListBox_ItemCheck;
            // 
            // PassInstructionAsUser
            // 
            PassInstructionAsUser.AutoSize = true;
            PassInstructionAsUser.Enabled = false;
            PassInstructionAsUser.Location = new Point(102, 332);
            PassInstructionAsUser.Name = "PassInstructionAsUser";
            PassInstructionAsUser.Size = new Size(245, 19);
            PassInstructionAsUser.TabIndex = 48;
            PassInstructionAsUser.Text = "Подтверждаю, что инструктаж пройден";
            PassInstructionAsUser.UseVisualStyleBackColor = true;
            PassInstructionAsUser.CheckedChanged += PassInstruction_CheckedChanged;
            // 
            // ListOfInstructionsForUser
            // 
            ListOfInstructionsForUser.FormattingEnabled = true;
            ListOfInstructionsForUser.ItemHeight = 15;
            ListOfInstructionsForUser.Location = new Point(77, 82);
            ListOfInstructionsForUser.Margin = new Padding(3, 2, 3, 2);
            ListOfInstructionsForUser.Name = "ListOfInstructionsForUser";
            ListOfInstructionsForUser.Size = new Size(244, 229);
            ListOfInstructionsForUser.TabIndex = 45;
            ListOfInstructionsForUser.SelectedValueChanged += InstructionsToPass_SelectedIndexChanged;
            // 
            // HyperLinkForInstructionsFolder
            // 
            HyperLinkForInstructionsFolder.Enabled = false;
            HyperLinkForInstructionsFolder.Location = new Point(550, 83);
            HyperLinkForInstructionsFolder.Name = "HyperLinkForInstructionsFolder";
            HyperLinkForInstructionsFolder.Size = new Size(111, 229);
            HyperLinkForInstructionsFolder.TabIndex = 47;
            HyperLinkForInstructionsFolder.Text = "Гиперссылка на инструктаж";
            HyperLinkForInstructionsFolder.UseVisualStyleBackColor = true;
            HyperLinkForInstructionsFolder.Click += HyperLinkForInstructionsFolder_Click;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(102, 40);
            label10.Name = "label10";
            label10.Size = new Size(201, 15);
            label10.TabIndex = 46;
            label10.Text = "Лист непройденных инструктажей:";
            // 
            // signUpButton
            // 
            signUpButton.Location = new Point(629, 23);
            signUpButton.Name = "signUpButton";
            signUpButton.Size = new Size(148, 57);
            signUpButton.TabIndex = 31;
            signUpButton.Text = "смена регистрационных даных";
            signUpButton.UseVisualStyleBackColor = true;
            signUpButton.Click += signUpButton_Click;
            // 
            // LogOutButton
            // 
            LogOutButton.Location = new Point(1083, 23);
            LogOutButton.Name = "LogOutButton";
            LogOutButton.Size = new Size(112, 56);
            LogOutButton.TabIndex = 32;
            LogOutButton.Text = "Выйти из учётной записи";
            LogOutButton.UseVisualStyleBackColor = true;
            LogOutButton.Click += LogOutButton_Click;
            // 
            // ManagementForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1260, 826);
            Controls.Add(LogOutButton);
            Controls.Add(signUpButton);
            Controls.Add(ManagementTabControl);
            Controls.Add(ManagementLabel);
            Controls.Add(label1);
            Name = "ManagementForm";
            Text = "ManagementForm";
            FormClosing += ManagementForm_FormClosing;
            ManagementTabControl.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label ManagementLabel;
        private Label label1;
        private TabControl ManagementTabControl;
        private TabPage tabPage1;
        private Button button5;
        private Button buttonCreateInstruction;
        private Label label4;
        private Label label3;
        private Label label2;
        private TabPage tabPage2;
        private CheckedListBox FilesOfInstructionCheckedListBox;
        private CheckBox PassInstructionAsUser;
        private ListBox ListOfInstructionsForUser;
        private Button HyperLinkForInstructionsFolder;
        private Label label10;
        private CheckedListBox DepartmentsCheckedListBox;
        private Label label5;
        private ListBox typeOfInstructionListBox;
        private Label label9;
        private Button signUpButton;
        private TreeViewWithoutDoubleClick PeopleAndDepartmentsTreeView;
        private Button LogOutButton;
        private DateTimePicker datePickerEnd;
        private TreeViewWithoutDoubleClick treeView1;
        private Button buttonChoosePathToInstruction;
        private TextBox InstructionTextBox;
        private Label PathToFolderOfInstruction;
    }
}