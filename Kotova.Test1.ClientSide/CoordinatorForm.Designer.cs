﻿namespace Kotova.Test1.ClientSide
{
    partial class CoordinatorForm
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
            tabPage1 = new TabPage();
            PassInstruction = new CheckBox();
            button6 = new Button();
            DepartmentInitInstr = new TextBox();
            BirthDateInitInstr = new TextBox();
            ProfessionInitInstr = new TextBox();
            label1 = new Label();
            buttonSyncManualyInitialInstrWithDB = new Button();
            NamesOfPeopleForInitialInstr = new ListBox();
            datePickerEnd = new DateTimePicker();
            label6 = new Label();
            CoordinatorTabControl = new TabControl();
            tabPage2 = new TabPage();
            TelpEmployeesListView = new ListView();
            fullname = new ColumnHeader();
            department = new ColumnHeader();
            position = new ColumnHeader();
            email = new ColumnHeader();
            personnelNumber = new ColumnHeader();
            buttonRefreshTelpDatabase = new Button();
            tabPage3 = new TabPage();
            AddInitialInstructionToNewcomer = new CheckBox();
            label25 = new Label();
            RoleOfNewcomerListBox = new ListBox();
            InitialInstructionButton = new Button();
            InitialInstructionPathLabel = new Label();
            label23 = new Label();
            label22 = new Label();
            label21 = new Label();
            label20 = new Label();
            label19 = new Label();
            label18 = new Label();
            PasswordTextBox = new TextBox();
            loginTextBox = new TextBox();
            label17 = new Label();
            DepartmentForNewcomer = new ListBox();
            uploadNewcommer = new Button();
            WorkplaceNumberTextBox = new TextBox();
            personnelNumberTextBox = new TextBox();
            label5 = new Label();
            dateOfBirthDateTimePicker = new DateTimePicker();
            employeesPositionTextBox = new TextBox();
            employeeFullNameTextBox = new TextBox();
            tabPage4 = new TabPage();
            FilesOfInstructionCheckedListBox = new CheckedListBox();
            PassInstructionAsUser = new CheckBox();
            ListOfInstructionsForUser = new ListBox();
            HyperLinkForInstructionsFolder = new Button();
            label10 = new Label();
            tabPage5 = new TabPage();
            listBox5 = new ListBox();
            label16 = new Label();
            ExcelExportForCoordinatorButton = new Button();
            DepartmentsListBox = new ListBox();
            label15 = new Label();
            typesOfInstructionListBox = new ListBox();
            label14 = new Label();
            label13 = new Label();
            label12 = new Label();
            EndDateOfPassedInstructions = new DateTimePicker();
            BeginingDateOfPassedInstructions = new DateTimePicker();
            button1 = new Button();
            UserLabel = new Label();
            label11 = new Label();
            tabPage1.SuspendLayout();
            CoordinatorTabControl.SuspendLayout();
            tabPage2.SuspendLayout();
            tabPage3.SuspendLayout();
            tabPage4.SuspendLayout();
            tabPage5.SuspendLayout();
            SuspendLayout();
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(PassInstruction);
            tabPage1.Controls.Add(button6);
            tabPage1.Controls.Add(DepartmentInitInstr);
            tabPage1.Controls.Add(BirthDateInitInstr);
            tabPage1.Controls.Add(ProfessionInitInstr);
            tabPage1.Controls.Add(label1);
            tabPage1.Controls.Add(buttonSyncManualyInitialInstrWithDB);
            tabPage1.Controls.Add(NamesOfPeopleForInitialInstr);
            tabPage1.Controls.Add(datePickerEnd);
            tabPage1.Controls.Add(label6);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(981, 398);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Вводные инструктажи";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // PassInstruction
            // 
            PassInstruction.AutoSize = true;
            PassInstruction.Location = new Point(449, 334);
            PassInstruction.Name = "PassInstruction";
            PassInstruction.Size = new Size(245, 19);
            PassInstruction.TabIndex = 42;
            PassInstruction.Text = "Подтверждаю, что инструктаж пройден";
            PassInstruction.UseVisualStyleBackColor = true;
            // 
            // button6
            // 
            button6.Location = new Point(748, 46);
            button6.Name = "button6";
            button6.Size = new Size(178, 307);
            button6.TabIndex = 41;
            button6.Text = "Сформировать excel форму об инструктажах";
            button6.UseVisualStyleBackColor = true;
            // 
            // DepartmentInitInstr
            // 
            DepartmentInitInstr.Location = new Point(6, 330);
            DepartmentInitInstr.Name = "DepartmentInitInstr";
            DepartmentInitInstr.Size = new Size(403, 23);
            DepartmentInitInstr.TabIndex = 40;
            DepartmentInitInstr.Text = "Отдел(автомат)";
            // 
            // BirthDateInitInstr
            // 
            BirthDateInitInstr.Location = new Point(6, 286);
            BirthDateInitInstr.Name = "BirthDateInitInstr";
            BirthDateInitInstr.Size = new Size(403, 23);
            BirthDateInitInstr.TabIndex = 39;
            BirthDateInitInstr.Text = "Дата рождения(автомат)";
            // 
            // ProfessionInitInstr
            // 
            ProfessionInitInstr.Location = new Point(6, 241);
            ProfessionInitInstr.Name = "ProfessionInitInstr";
            ProfessionInitInstr.Size = new Size(403, 23);
            ProfessionInitInstr.TabIndex = 38;
            ProfessionInitInstr.Text = "Профессия(автомат)";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 104);
            label1.Name = "label1";
            label1.Size = new Size(130, 15);
            label1.TabIndex = 37;
            label1.Text = "Вводнный инструктаж";
            // 
            // buttonSyncManualyInitialInstrWithDB
            // 
            buttonSyncManualyInitialInstrWithDB.Location = new Point(3, 131);
            buttonSyncManualyInitialInstrWithDB.Margin = new Padding(3, 2, 3, 2);
            buttonSyncManualyInitialInstrWithDB.Name = "buttonSyncManualyInitialInstrWithDB";
            buttonSyncManualyInitialInstrWithDB.Size = new Size(216, 22);
            buttonSyncManualyInitialInstrWithDB.TabIndex = 33;
            buttonSyncManualyInitialInstrWithDB.Text = "Синхр. инструктажи с Базой Данных";
            buttonSyncManualyInitialInstrWithDB.UseVisualStyleBackColor = true;
            buttonSyncManualyInitialInstrWithDB.Click += buttonSyncNamesForInitialInstrWithDB_Click;
            // 
            // NamesOfPeopleForInitialInstr
            // 
            NamesOfPeopleForInitialInstr.FormattingEnabled = true;
            NamesOfPeopleForInitialInstr.ItemHeight = 15;
            NamesOfPeopleForInitialInstr.Location = new Point(6, 167);
            NamesOfPeopleForInitialInstr.Name = "NamesOfPeopleForInitialInstr";
            NamesOfPeopleForInitialInstr.Size = new Size(403, 49);
            NamesOfPeopleForInitialInstr.TabIndex = 1;
            NamesOfPeopleForInitialInstr.SelectedIndexChanged += NamesOfPeopleForInitialInstr_SelectedIndexChanged;
            // 
            // datePickerEnd
            // 
            datePickerEnd.Location = new Point(6, 46);
            datePickerEnd.Margin = new Padding(3, 2, 3, 2);
            datePickerEnd.Name = "datePickerEnd";
            datePickerEnd.Size = new Size(219, 23);
            datePickerEnd.TabIndex = 13;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(6, 16);
            label6.Name = "label6";
            label6.Size = new Size(173, 15);
            label6.TabIndex = 14;
            label6.Text = "Дата проведения инструктажа";
            // 
            // CoordinatorTabControl
            // 
            CoordinatorTabControl.Controls.Add(tabPage1);
            CoordinatorTabControl.Controls.Add(tabPage2);
            CoordinatorTabControl.Controls.Add(tabPage3);
            CoordinatorTabControl.Controls.Add(tabPage4);
            CoordinatorTabControl.Controls.Add(tabPage5);
            CoordinatorTabControl.Location = new Point(46, 62);
            CoordinatorTabControl.Name = "CoordinatorTabControl";
            CoordinatorTabControl.SelectedIndex = 0;
            CoordinatorTabControl.Size = new Size(989, 426);
            CoordinatorTabControl.TabIndex = 22;
            CoordinatorTabControl.SelectedIndexChanged += CoordinatorTabControl_SelectedIndexChanged;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(TelpEmployeesListView);
            tabPage2.Controls.Add(buttonRefreshTelpDatabase);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(981, 398);
            tabPage2.TabIndex = 6;
            tabPage2.Text = "Связь баз данных";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // TelpEmployeesListView
            // 
            TelpEmployeesListView.Columns.AddRange(new ColumnHeader[] { fullname, department, position, email, personnelNumber });
            TelpEmployeesListView.FullRowSelect = true;
            TelpEmployeesListView.Location = new Point(34, 18);
            TelpEmployeesListView.MultiSelect = false;
            TelpEmployeesListView.Name = "TelpEmployeesListView";
            TelpEmployeesListView.Size = new Size(831, 295);
            TelpEmployeesListView.TabIndex = 3;
            TelpEmployeesListView.UseCompatibleStateImageBehavior = false;
            TelpEmployeesListView.DoubleClick += TelpEmployeesListView_DoubleClick;
            // 
            // fullname
            // 
            fullname.Text = "ФИО";
            // 
            // department
            // 
            department.Text = "Отдел";
            // 
            // position
            // 
            position.Text = "Должность";
            // 
            // email
            // 
            email.Text = "Email";
            // 
            // personnelNumber
            // 
            personnelNumber.Text = "Табельный номер";
            // 
            // buttonRefreshTelpDatabase
            // 
            buttonRefreshTelpDatabase.Location = new Point(34, 337);
            buttonRefreshTelpDatabase.Name = "buttonRefreshTelpDatabase";
            buttonRefreshTelpDatabase.Size = new Size(155, 43);
            buttonRefreshTelpDatabase.TabIndex = 2;
            buttonRefreshTelpDatabase.Text = "Обновление базы данных";
            buttonRefreshTelpDatabase.UseVisualStyleBackColor = true;
            buttonRefreshTelpDatabase.Click += buttonRefreshTelpDatabase_Click;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(AddInitialInstructionToNewcomer);
            tabPage3.Controls.Add(label25);
            tabPage3.Controls.Add(RoleOfNewcomerListBox);
            tabPage3.Controls.Add(InitialInstructionButton);
            tabPage3.Controls.Add(InitialInstructionPathLabel);
            tabPage3.Controls.Add(label23);
            tabPage3.Controls.Add(label22);
            tabPage3.Controls.Add(label21);
            tabPage3.Controls.Add(label20);
            tabPage3.Controls.Add(label19);
            tabPage3.Controls.Add(label18);
            tabPage3.Controls.Add(PasswordTextBox);
            tabPage3.Controls.Add(loginTextBox);
            tabPage3.Controls.Add(label17);
            tabPage3.Controls.Add(DepartmentForNewcomer);
            tabPage3.Controls.Add(uploadNewcommer);
            tabPage3.Controls.Add(WorkplaceNumberTextBox);
            tabPage3.Controls.Add(personnelNumberTextBox);
            tabPage3.Controls.Add(label5);
            tabPage3.Controls.Add(dateOfBirthDateTimePicker);
            tabPage3.Controls.Add(employeesPositionTextBox);
            tabPage3.Controls.Add(employeeFullNameTextBox);
            tabPage3.Location = new Point(4, 24);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new Padding(3);
            tabPage3.Size = new Size(981, 398);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Данные сотрудника";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // AddInitialInstructionToNewcomer
            // 
            AddInitialInstructionToNewcomer.AutoSize = true;
            AddInitialInstructionToNewcomer.Location = new Point(306, 289);
            AddInitialInstructionToNewcomer.Name = "AddInitialInstructionToNewcomer";
            AddInitialInstructionToNewcomer.Size = new Size(284, 19);
            AddInitialInstructionToNewcomer.TabIndex = 30;
            AddInitialInstructionToNewcomer.Text = "Добавить ли сотруднику вводный инструктаж?";
            AddInitialInstructionToNewcomer.UseVisualStyleBackColor = true;
            // 
            // label25
            // 
            label25.AutoSize = true;
            label25.Location = new Point(306, 187);
            label25.Name = "label25";
            label25.Size = new Size(195, 15);
            label25.TabIndex = 29;
            label25.Text = "Выбрать роль нового сотрудника:";
            // 
            // RoleOfNewcomerListBox
            // 
            RoleOfNewcomerListBox.FormattingEnabled = true;
            RoleOfNewcomerListBox.ItemHeight = 15;
            RoleOfNewcomerListBox.Location = new Point(306, 212);
            RoleOfNewcomerListBox.Name = "RoleOfNewcomerListBox";
            RoleOfNewcomerListBox.Size = new Size(281, 64);
            RoleOfNewcomerListBox.TabIndex = 28;
            // 
            // InitialInstructionButton
            // 
            InitialInstructionButton.Location = new Point(750, 316);
            InitialInstructionButton.Name = "InitialInstructionButton";
            InitialInstructionButton.Size = new Size(210, 53);
            InitialInstructionButton.TabIndex = 26;
            InitialInstructionButton.Text = "Данные сотрудника заполнены";
            InitialInstructionButton.UseVisualStyleBackColor = true;
            InitialInstructionButton.Click += DataIsFilledButton_Click;
            // 
            // InitialInstructionPathLabel
            // 
            InitialInstructionPathLabel.AutoSize = true;
            InitialInstructionPathLabel.Location = new Point(24, 358);
            InitialInstructionPathLabel.Name = "InitialInstructionPathLabel";
            InitialInstructionPathLabel.Size = new Size(0, 15);
            InitialInstructionPathLabel.TabIndex = 25;
            // 
            // label23
            // 
            label23.AutoSize = true;
            label23.Location = new Point(306, 69);
            label23.Name = "label23";
            label23.Size = new Size(251, 15);
            label23.TabIndex = 23;
            label23.Text = "Номер рабочего места (если есть, или null):";
            // 
            // label22
            // 
            label22.AutoSize = true;
            label22.Location = new Point(306, 19);
            label22.Name = "label22";
            label22.Size = new Size(110, 15);
            label22.TabIndex = 22;
            label22.Text = "Табельный номер:";
            // 
            // label21
            // 
            label21.AutoSize = true;
            label21.Location = new Point(23, 69);
            label21.Name = "label21";
            label21.Size = new Size(138, 15);
            label21.TabIndex = 21;
            label21.Text = "Должность сотрудника:";
            // 
            // label20
            // 
            label20.AutoSize = true;
            label20.Location = new Point(24, 19);
            label20.Name = "label20";
            label20.Size = new Size(103, 15);
            label20.TabIndex = 20;
            label20.Text = "ФИО сотрудника:";
            // 
            // label19
            // 
            label19.AutoSize = true;
            label19.Location = new Point(698, 124);
            label19.Name = "label19";
            label19.Size = new Size(115, 15);
            label19.TabIndex = 19;
            label19.Text = "Пароль сотрудника";
            // 
            // label18
            // 
            label18.AutoSize = true;
            label18.Location = new Point(698, 45);
            label18.Name = "label18";
            label18.Size = new Size(107, 15);
            label18.TabIndex = 18;
            label18.Text = "Логин сотрудника";
            // 
            // PasswordTextBox
            // 
            PasswordTextBox.Location = new Point(698, 161);
            PasswordTextBox.Name = "PasswordTextBox";
            PasswordTextBox.Size = new Size(248, 23);
            PasswordTextBox.TabIndex = 17;
            // 
            // loginTextBox
            // 
            loginTextBox.Location = new Point(698, 84);
            loginTextBox.Name = "loginTextBox";
            loginTextBox.Size = new Size(248, 23);
            loginTextBox.TabIndex = 16;
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Location = new Point(24, 124);
            label17.Name = "label17";
            label17.Size = new Size(43, 15);
            label17.TabIndex = 15;
            label17.Text = "Отдел:";
            // 
            // DepartmentForNewcomer
            // 
            DepartmentForNewcomer.FormattingEnabled = true;
            DepartmentForNewcomer.ItemHeight = 15;
            DepartmentForNewcomer.Location = new Point(24, 142);
            DepartmentForNewcomer.Name = "DepartmentForNewcomer";
            DepartmentForNewcomer.Size = new Size(209, 64);
            DepartmentForNewcomer.TabIndex = 14;
            // 
            // uploadNewcommer
            // 
            uploadNewcommer.Enabled = false;
            uploadNewcommer.Location = new Point(306, 316);
            uploadNewcommer.Name = "uploadNewcommer";
            uploadNewcommer.Size = new Size(285, 57);
            uploadNewcommer.TabIndex = 13;
            uploadNewcommer.Text = "Сохранить сотрудника";
            uploadNewcommer.UseVisualStyleBackColor = true;
            uploadNewcommer.Click += uploadNewcommer_Click;
            // 
            // WorkplaceNumberTextBox
            // 
            WorkplaceNumberTextBox.Location = new Point(306, 90);
            WorkplaceNumberTextBox.Name = "WorkplaceNumberTextBox";
            WorkplaceNumberTextBox.Size = new Size(281, 23);
            WorkplaceNumberTextBox.TabIndex = 12;
            // 
            // personnelNumberTextBox
            // 
            personnelNumberTextBox.Location = new Point(306, 38);
            personnelNumberTextBox.Name = "personnelNumberTextBox";
            personnelNumberTextBox.Size = new Size(281, 23);
            personnelNumberTextBox.TabIndex = 11;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(306, 124);
            label5.Name = "label5";
            label5.Size = new Size(93, 15);
            label5.TabIndex = 10;
            label5.Text = "Дата рождения:";
            // 
            // dateOfBirthDateTimePicker
            // 
            dateOfBirthDateTimePicker.Location = new Point(306, 153);
            dateOfBirthDateTimePicker.Margin = new Padding(3, 2, 3, 2);
            dateOfBirthDateTimePicker.Name = "dateOfBirthDateTimePicker";
            dateOfBirthDateTimePicker.Size = new Size(219, 23);
            dateOfBirthDateTimePicker.TabIndex = 9;
            // 
            // employeesPositionTextBox
            // 
            employeesPositionTextBox.Location = new Point(24, 90);
            employeesPositionTextBox.Name = "employeesPositionTextBox";
            employeesPositionTextBox.Size = new Size(209, 23);
            employeesPositionTextBox.TabIndex = 1;
            // 
            // employeeFullNameTextBox
            // 
            employeeFullNameTextBox.Location = new Point(24, 38);
            employeeFullNameTextBox.Name = "employeeFullNameTextBox";
            employeeFullNameTextBox.Size = new Size(209, 23);
            employeeFullNameTextBox.TabIndex = 0;
            // 
            // tabPage4
            // 
            tabPage4.Controls.Add(FilesOfInstructionCheckedListBox);
            tabPage4.Controls.Add(PassInstructionAsUser);
            tabPage4.Controls.Add(ListOfInstructionsForUser);
            tabPage4.Controls.Add(HyperLinkForInstructionsFolder);
            tabPage4.Controls.Add(label10);
            tabPage4.Location = new Point(4, 24);
            tabPage4.Name = "tabPage4";
            tabPage4.Padding = new Padding(3);
            tabPage4.Size = new Size(981, 398);
            tabPage4.TabIndex = 5;
            tabPage4.Text = "Прохождение инструктажей";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // FilesOfInstructionCheckedListBox
            // 
            FilesOfInstructionCheckedListBox.FormattingEnabled = true;
            FilesOfInstructionCheckedListBox.HorizontalScrollbar = true;
            FilesOfInstructionCheckedListBox.Location = new Point(301, 82);
            FilesOfInstructionCheckedListBox.Name = "FilesOfInstructionCheckedListBox";
            FilesOfInstructionCheckedListBox.Size = new Size(195, 220);
            FilesOfInstructionCheckedListBox.TabIndex = 44;
            FilesOfInstructionCheckedListBox.ItemCheck += FilesOfInstructionCheckedListBox_ItemCheck;
            // 
            // PassInstructionAsUser
            // 
            PassInstructionAsUser.AutoSize = true;
            PassInstructionAsUser.Enabled = false;
            PassInstructionAsUser.Location = new Point(63, 322);
            PassInstructionAsUser.Name = "PassInstructionAsUser";
            PassInstructionAsUser.Size = new Size(245, 19);
            PassInstructionAsUser.TabIndex = 43;
            PassInstructionAsUser.Text = "Подтверждаю, что инструктаж пройден";
            PassInstructionAsUser.UseVisualStyleBackColor = true;
            PassInstructionAsUser.CheckedChanged += PassInstruction_CheckedChanged;
            // 
            // ListOfInstructionsForUser
            // 
            ListOfInstructionsForUser.FormattingEnabled = true;
            ListOfInstructionsForUser.ItemHeight = 15;
            ListOfInstructionsForUser.Location = new Point(38, 72);
            ListOfInstructionsForUser.Margin = new Padding(3, 2, 3, 2);
            ListOfInstructionsForUser.Name = "ListOfInstructionsForUser";
            ListOfInstructionsForUser.Size = new Size(244, 229);
            ListOfInstructionsForUser.TabIndex = 40;
            ListOfInstructionsForUser.SelectedValueChanged += InstructionsToPass_SelectedIndexChanged;
            // 
            // HyperLinkForInstructionsFolder
            // 
            HyperLinkForInstructionsFolder.Enabled = false;
            HyperLinkForInstructionsFolder.Location = new Point(511, 73);
            HyperLinkForInstructionsFolder.Name = "HyperLinkForInstructionsFolder";
            HyperLinkForInstructionsFolder.Size = new Size(111, 229);
            HyperLinkForInstructionsFolder.TabIndex = 42;
            HyperLinkForInstructionsFolder.Text = "Гиперссылка на инструктаж";
            HyperLinkForInstructionsFolder.UseVisualStyleBackColor = true;
            HyperLinkForInstructionsFolder.Click += HyperLinkForInstructionsFolder_Click;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(63, 30);
            label10.Name = "label10";
            label10.Size = new Size(201, 15);
            label10.TabIndex = 41;
            label10.Text = "Лист непройденных инструктажей:";
            // 
            // tabPage5
            // 
            tabPage5.Controls.Add(listBox5);
            tabPage5.Controls.Add(label16);
            tabPage5.Controls.Add(ExcelExportForCoordinatorButton);
            tabPage5.Controls.Add(DepartmentsListBox);
            tabPage5.Controls.Add(label15);
            tabPage5.Controls.Add(typesOfInstructionListBox);
            tabPage5.Controls.Add(label14);
            tabPage5.Controls.Add(label13);
            tabPage5.Controls.Add(label12);
            tabPage5.Controls.Add(EndDateOfPassedInstructions);
            tabPage5.Controls.Add(BeginingDateOfPassedInstructions);
            tabPage5.Location = new Point(4, 24);
            tabPage5.Name = "tabPage5";
            tabPage5.Padding = new Padding(3);
            tabPage5.Size = new Size(981, 398);
            tabPage5.TabIndex = 4;
            tabPage5.Text = "Контроль";
            tabPage5.UseVisualStyleBackColor = true;
            // 
            // listBox5
            // 
            listBox5.FormattingEnabled = true;
            listBox5.ItemHeight = 15;
            listBox5.Items.AddRange(new object[] { "ФИО 1", "ФИО 2", "ФИО 3" });
            listBox5.Location = new Point(427, 212);
            listBox5.Name = "listBox5";
            listBox5.Size = new Size(127, 94);
            listBox5.TabIndex = 10;
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Location = new Point(427, 173);
            label16.Name = "label16";
            label16.Size = new Size(196, 15);
            label16.TabIndex = 9;
            label16.Text = "Выбор сотрудника (опционально)";
            // 
            // ExcelExportForCoordinatorButton
            // 
            ExcelExportForCoordinatorButton.Location = new Point(630, 34);
            ExcelExportForCoordinatorButton.Name = "ExcelExportForCoordinatorButton";
            ExcelExportForCoordinatorButton.Size = new Size(299, 272);
            ExcelExportForCoordinatorButton.TabIndex = 8;
            ExcelExportForCoordinatorButton.Text = "Сформировать Excel отчёт";
            ExcelExportForCoordinatorButton.UseVisualStyleBackColor = true;
            ExcelExportForCoordinatorButton.Click += ExcelExportForCoordinatorButton_Click;
            // 
            // DepartmentsListBox
            // 
            DepartmentsListBox.FormattingEnabled = true;
            DepartmentsListBox.ItemHeight = 15;
            DepartmentsListBox.Items.AddRange(new object[] { "Тех. Отдел", "Общестрой" });
            DepartmentsListBox.Location = new Point(240, 212);
            DepartmentsListBox.Name = "DepartmentsListBox";
            DepartmentsListBox.Size = new Size(127, 94);
            DepartmentsListBox.TabIndex = 7;
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Location = new Point(240, 173);
            label15.Name = "label15";
            label15.Size = new Size(40, 15);
            label15.TabIndex = 6;
            label15.Text = "Отдел";
            // 
            // typesOfInstructionListBox
            // 
            typesOfInstructionListBox.FormattingEnabled = true;
            typesOfInstructionListBox.ItemHeight = 15;
            typesOfInstructionListBox.Items.AddRange(new object[] { "Вводный", "Внеплановый", "Первичный", "Повторный", "Повторный (для водителей)", "Целевой" });
            typesOfInstructionListBox.Location = new Point(57, 212);
            typesOfInstructionListBox.Name = "typesOfInstructionListBox";
            typesOfInstructionListBox.Size = new Size(127, 94);
            typesOfInstructionListBox.TabIndex = 5;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new Point(57, 173);
            label14.Name = "label14";
            label14.Size = new Size(103, 15);
            label14.TabIndex = 4;
            label14.Text = "Вид инструктажа:";
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(57, 92);
            label13.Name = "label13";
            label13.Size = new Size(90, 15);
            label13.TabIndex = 3;
            label13.Text = "Конец периода";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(57, 20);
            label12.Name = "label12";
            label12.Size = new Size(98, 15);
            label12.TabIndex = 2;
            label12.Text = "Начало периода";
            // 
            // EndDateOfPassedInstructions
            // 
            EndDateOfPassedInstructions.Location = new Point(57, 125);
            EndDateOfPassedInstructions.Name = "EndDateOfPassedInstructions";
            EndDateOfPassedInstructions.Size = new Size(245, 23);
            EndDateOfPassedInstructions.TabIndex = 1;
            // 
            // BeginingDateOfPassedInstructions
            // 
            BeginingDateOfPassedInstructions.Location = new Point(57, 52);
            BeginingDateOfPassedInstructions.Name = "BeginingDateOfPassedInstructions";
            BeginingDateOfPassedInstructions.Size = new Size(245, 23);
            BeginingDateOfPassedInstructions.TabIndex = 0;
            // 
            // button1
            // 
            button1.Location = new Point(1058, 62);
            button1.Name = "button1";
            button1.Size = new Size(120, 43);
            button1.TabIndex = 35;
            button1.Text = "Выйти из учётной записи";
            button1.UseVisualStyleBackColor = true;
            button1.Click += LogOut_Click;
            // 
            // UserLabel
            // 
            UserLabel.AutoSize = true;
            UserLabel.Location = new Point(140, 29);
            UserLabel.Name = "UserLabel";
            UserLabel.Size = new Size(81, 15);
            UserLabel.TabIndex = 37;
            UserLabel.Text = "UnknownUser";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(46, 29);
            label11.Name = "label11";
            label11.Size = new Size(88, 15);
            label11.TabIndex = 36;
            label11.Text = "Вы вошли как:";
            // 
            // CoordinatorForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1190, 543);
            Controls.Add(UserLabel);
            Controls.Add(label11);
            Controls.Add(button1);
            Controls.Add(CoordinatorTabControl);
            Name = "CoordinatorForm";
            Text = "Coordinator";
            FormClosing += CoordinatorForm_FormClosing;
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            CoordinatorTabControl.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            tabPage3.ResumeLayout(false);
            tabPage3.PerformLayout();
            tabPage4.ResumeLayout(false);
            tabPage4.PerformLayout();
            tabPage5.ResumeLayout(false);
            tabPage5.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private TabPage tabPage1;
        private DateTimePicker datePickerEnd;
        private Label label6;
        private TabControl CoordinatorTabControl;
        private Button buttonSyncManualyInitialInstrWithDB;
        private TabPage tabPage3;
        private TextBox employeesPositionTextBox;
        private TextBox employeeFullNameTextBox;
        private TextBox WorkplaceNumberTextBox;
        private TextBox personnelNumberTextBox;
        private Label label5;
        private DateTimePicker dateOfBirthDateTimePicker;
        private Label label1;
        public ListBox NamesOfPeopleForInitialInstr;
        private TextBox DepartmentInitInstr;
        private TextBox BirthDateInitInstr;
        private TextBox ProfessionInitInstr;
        private Button uploadNewcommer;
        private Button button6;
        private TabPage tabPage5;
        private ListBox typesOfInstructionListBox;
        private Label label14;
        private Label label13;
        private Label label12;
        private DateTimePicker EndDateOfPassedInstructions;
        private DateTimePicker BeginingDateOfPassedInstructions;
        private Button ExcelExportForCoordinatorButton;
        private ListBox DepartmentsListBox;
        private Label label15;
        private ListBox listBox5;
        private Label label16;
        private Label label17;
        private ListBox DepartmentForNewcomer;
        private Label label19;
        private Label label18;
        private TextBox PasswordTextBox;
        private TextBox loginTextBox;
        private Label label21;
        private Label label20;
        private Label label23;
        private Label label22;
        private Button InitialInstructionButton;
        private Label InitialInstructionPathLabel;
        private Button button1;
        private Label label25;
        private ListBox RoleOfNewcomerListBox;
        private CheckBox PassInstruction;
        private CheckBox AddInitialInstructionToNewcomer;
        private TabPage tabPage4;
        private CheckedListBox FilesOfInstructionCheckedListBox;
        private CheckBox PassInstructionAsUser;
        private ListBox ListOfInstructionsForUser;
        private Button HyperLinkForInstructionsFolder;
        private Label label10;
        private Label UserLabel;
        private Label label11;
        private TabPage tabPage2;
        private Button buttonRefreshTelpDatabase;
        private ListView TelpEmployeesListView;
        private ColumnHeader fullname;
        private ColumnHeader department;
        private ColumnHeader position;
        private ColumnHeader email;
        private ColumnHeader personnelNumber;
    }
}