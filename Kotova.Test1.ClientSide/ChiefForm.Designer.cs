﻿namespace Kotova.Test1.ClientSide
{
    partial class ChiefForm
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
            submitInstructionToPeople = new Button();
            SyncNamesWithDB = new Button();
            tabPage2 = new TabPage();
            checkedListBoxNamesOfPeopleCreatingInstr = new CheckedListBox();
            treeView1 = new TreeViewWithoutDoubleClick();
            PathToFolderOfInstruction = new Label();
            label7 = new Label();
            buttonChoosePathToInstruction = new Button();
            label2 = new Label();
            InstructionTextBox = new TextBox();
            label6 = new Label();
            datePickerEnd = new DateTimePicker();
            buttonCreateInstruction = new Button();
            label1 = new Label();
            typeOfInstructionListBox = new ListBox();
            testButton = new Button();
            tabPage1 = new TabPage();
            MissTheAssignmentOfInstrCheckedBox = new CheckBox();
            SelectAllThePeopleInListBoxButton = new Button();
            checkedListBoxNamesOfPeople = new CheckedListBox();
            ListOfInstructions = new ListBox();
            buttonSyncManualyInstrWithDB = new Button();
            ListOfInstructionsForUser = new ListBox();
            ChiefTabControl = new TabControl();
            tabPage3 = new TabPage();
            FilesOfInstructionCheckedListBox = new CheckedListBox();
            PassInstruction = new CheckBox();
            HyperLinkForInstructionsFolder = new Button();
            label8 = new Label();
            tabPage4 = new TabPage();
            listBoxOfPassedInstructions = new ListBox();
            dataGridViewPeopleThatNotPassedInstr = new DataGridView();
            Names = new DataGridViewTextBoxColumn();
            Passed = new DataGridViewTextBoxColumn();
            listBoxOfNotPassedByInstructions = new ListBox();
            TestButtonForInstructions = new Button();
            tabPage5 = new TabPage();
            DownloadAllEmployeesInTheDepartment = new Button();
            checkedListBoxTypesOfInstruction = new CheckedListBoxWithoutDoubleClick();
            label9 = new Label();
            endDateInstructionExportRequest = new DateTimePicker();
            label3 = new Label();
            startDateInstructionExportRequest = new DateTimePicker();
            ExportInstructionRequestButton = new Button();
            toolTip1 = new ToolTip(components);
            LogOutButton = new Button();
            LabelTray = new Label();
            RefreshTasksButton = new Button();
            TrayOfTasksList = new ListBox();
            usernameLabel = new Label();
            label10 = new Label();
            button1 = new Button();
            tabPage2.SuspendLayout();
            tabPage1.SuspendLayout();
            ChiefTabControl.SuspendLayout();
            tabPage3.SuspendLayout();
            tabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGridViewPeopleThatNotPassedInstr).BeginInit();
            tabPage5.SuspendLayout();
            SuspendLayout();
            // 
            // submitInstructionToPeople
            // 
            submitInstructionToPeople.Location = new Point(196, 488);
            submitInstructionToPeople.Margin = new Padding(3, 2, 3, 2);
            submitInstructionToPeople.Name = "submitInstructionToPeople";
            submitInstructionToPeople.Size = new Size(216, 71);
            submitInstructionToPeople.TabIndex = 28;
            submitInstructionToPeople.Text = "Отправить выбранным людям уведомление об инструктаже";
            submitInstructionToPeople.UseVisualStyleBackColor = true;
            submitInstructionToPeople.Click += submitInstructionToPeople_Click;
            // 
            // SyncNamesWithDB
            // 
            SyncNamesWithDB.Location = new Point(364, 43);
            SyncNamesWithDB.Margin = new Padding(3, 2, 3, 2);
            SyncNamesWithDB.Name = "SyncNamesWithDB";
            SyncNamesWithDB.Size = new Size(265, 38);
            SyncNamesWithDB.TabIndex = 26;
            SyncNamesWithDB.Text = "Синхронизировать ФИО с Базой Данных";
            SyncNamesWithDB.UseVisualStyleBackColor = true;
            SyncNamesWithDB.Click += SyncNamesWithDB_Click;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(checkedListBoxNamesOfPeopleCreatingInstr);
            tabPage2.Controls.Add(treeView1);
            tabPage2.Controls.Add(PathToFolderOfInstruction);
            tabPage2.Controls.Add(label7);
            tabPage2.Controls.Add(buttonChoosePathToInstruction);
            tabPage2.Controls.Add(label2);
            tabPage2.Controls.Add(InstructionTextBox);
            tabPage2.Controls.Add(label6);
            tabPage2.Controls.Add(datePickerEnd);
            tabPage2.Controls.Add(buttonCreateInstruction);
            tabPage2.Controls.Add(label1);
            tabPage2.Controls.Add(typeOfInstructionListBox);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(667, 588);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Создание инструктажа";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // checkedListBoxNamesOfPeopleCreatingInstr
            // 
            checkedListBoxNamesOfPeopleCreatingInstr.CheckOnClick = true;
            checkedListBoxNamesOfPeopleCreatingInstr.FormattingEnabled = true;
            checkedListBoxNamesOfPeopleCreatingInstr.HorizontalScrollbar = true;
            checkedListBoxNamesOfPeopleCreatingInstr.Location = new Point(47, 306);
            checkedListBoxNamesOfPeopleCreatingInstr.Name = "checkedListBoxNamesOfPeopleCreatingInstr";
            checkedListBoxNamesOfPeopleCreatingInstr.Size = new Size(289, 94);
            checkedListBoxNamesOfPeopleCreatingInstr.TabIndex = 35;
            // 
            // treeView1
            // 
            treeView1.CheckBoxes = true;
            treeView1.Location = new Point(364, 101);
            treeView1.Name = "treeView1";
            treeView1.Size = new Size(247, 178);
            treeView1.TabIndex = 15;
            treeView1.AfterCheck += treeView1_AfterCheck;
            // 
            // PathToFolderOfInstruction
            // 
            PathToFolderOfInstruction.AutoSize = true;
            PathToFolderOfInstruction.Location = new Point(364, 83);
            PathToFolderOfInstruction.Name = "PathToFolderOfInstruction";
            PathToFolderOfInstruction.Size = new Size(94, 15);
            PathToFolderOfInstruction.TabIndex = 14;
            PathToFolderOfInstruction.Text = "Путь не выбран";
            toolTip1.SetToolTip(PathToFolderOfInstruction, "Путь не выбран");
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(333, 49);
            label7.Name = "label7";
            label7.Size = new Size(0, 15);
            label7.TabIndex = 13;
            // 
            // buttonChoosePathToInstruction
            // 
            buttonChoosePathToInstruction.Location = new Point(364, 22);
            buttonChoosePathToInstruction.Margin = new Padding(3, 2, 3, 2);
            buttonChoosePathToInstruction.Name = "buttonChoosePathToInstruction";
            buttonChoosePathToInstruction.Size = new Size(172, 56);
            buttonChoosePathToInstruction.TabIndex = 12;
            buttonChoosePathToInstruction.Text = "Указать папку для инструктажа";
            buttonChoosePathToInstruction.UseVisualStyleBackColor = true;
            buttonChoosePathToInstruction.Click += buttonChooseHyperLinkToInstruction_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(47, 188);
            label2.Name = "label2";
            label2.Size = new Size(133, 15);
            label2.TabIndex = 11;
            label2.Text = "Причина инструктажа:";
            // 
            // InstructionTextBox
            // 
            InstructionTextBox.Location = new Point(47, 217);
            InstructionTextBox.Margin = new Padding(3, 2, 3, 2);
            InstructionTextBox.Multiline = true;
            InstructionTextBox.Name = "InstructionTextBox";
            InstructionTextBox.Size = new Size(219, 51);
            InstructionTextBox.TabIndex = 10;
            InstructionTextBox.Text = "Работа в зоне железнодорожных путей СТО-357";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(47, 121);
            label6.Name = "label6";
            label6.Size = new Size(183, 15);
            label6.TabIndex = 9;
            label6.Text = "До какой даты? (включительно)";
            // 
            // datePickerEnd
            // 
            datePickerEnd.Location = new Point(47, 150);
            datePickerEnd.Margin = new Padding(3, 2, 3, 2);
            datePickerEnd.Name = "datePickerEnd";
            datePickerEnd.Size = new Size(219, 23);
            datePickerEnd.TabIndex = 8;
            // 
            // buttonCreateInstruction
            // 
            buttonCreateInstruction.Enabled = false;
            buttonCreateInstruction.Location = new Point(239, 474);
            buttonCreateInstruction.Margin = new Padding(3, 2, 3, 2);
            buttonCreateInstruction.Name = "buttonCreateInstruction";
            buttonCreateInstruction.Size = new Size(219, 40);
            buttonCreateInstruction.TabIndex = 7;
            buttonCreateInstruction.Text = "Внести новый инструткаж";
            buttonCreateInstruction.UseVisualStyleBackColor = true;
            buttonCreateInstruction.Click += buttonCreateInstruction_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(47, 22);
            label1.Name = "label1";
            label1.Size = new Size(148, 15);
            label1.TabIndex = 1;
            label1.Text = "Выбор типа инструктажа:";
            // 
            // typeOfInstructionListBox
            // 
            typeOfInstructionListBox.FormattingEnabled = true;
            typeOfInstructionListBox.ItemHeight = 15;
            typeOfInstructionListBox.Items.AddRange(new object[] { "Первичный;", "Повторный;", "Повторный (для водителей);", "Целевой;" });
            typeOfInstructionListBox.Location = new Point(47, 49);
            typeOfInstructionListBox.Name = "typeOfInstructionListBox";
            typeOfInstructionListBox.Size = new Size(168, 64);
            typeOfInstructionListBox.TabIndex = 0;
            // 
            // testButton
            // 
            testButton.Location = new Point(708, 45);
            testButton.Name = "testButton";
            testButton.Size = new Size(176, 39);
            testButton.TabIndex = 15;
            testButton.Text = "Test (Получить ответ с сервера)";
            testButton.UseVisualStyleBackColor = true;
            testButton.Click += testButton_Click;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(MissTheAssignmentOfInstrCheckedBox);
            tabPage1.Controls.Add(SelectAllThePeopleInListBoxButton);
            tabPage1.Controls.Add(checkedListBoxNamesOfPeople);
            tabPage1.Controls.Add(ListOfInstructions);
            tabPage1.Controls.Add(buttonSyncManualyInstrWithDB);
            tabPage1.Controls.Add(submitInstructionToPeople);
            tabPage1.Controls.Add(SyncNamesWithDB);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(667, 588);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Внеплановые инструктажи";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // MissTheAssignmentOfInstrCheckedBox
            // 
            MissTheAssignmentOfInstrCheckedBox.AutoSize = true;
            MissTheAssignmentOfInstrCheckedBox.Location = new Point(20, 429);
            MissTheAssignmentOfInstrCheckedBox.Name = "MissTheAssignmentOfInstrCheckedBox";
            MissTheAssignmentOfInstrCheckedBox.Size = new Size(275, 19);
            MissTheAssignmentOfInstrCheckedBox.TabIndex = 36;
            MissTheAssignmentOfInstrCheckedBox.Text = "Не выбрать никого для данного инструктажа";
            MissTheAssignmentOfInstrCheckedBox.UseVisualStyleBackColor = true;
            MissTheAssignmentOfInstrCheckedBox.CheckedChanged += SkipTheAssignmentOfInstrCheckedBox_CheckedChanged;
            // 
            // SelectAllThePeopleInListBoxButton
            // 
            SelectAllThePeopleInListBoxButton.Location = new Point(431, 421);
            SelectAllThePeopleInListBoxButton.Name = "SelectAllThePeopleInListBoxButton";
            SelectAllThePeopleInListBoxButton.Size = new Size(138, 32);
            SelectAllThePeopleInListBoxButton.TabIndex = 35;
            SelectAllThePeopleInListBoxButton.Text = "Выбрать всех людей";
            SelectAllThePeopleInListBoxButton.UseVisualStyleBackColor = true;
            SelectAllThePeopleInListBoxButton.Click += SelectAllThePeopleInListBoxButton_Click;
            // 
            // checkedListBoxNamesOfPeople
            // 
            checkedListBoxNamesOfPeople.CheckOnClick = true;
            checkedListBoxNamesOfPeople.FormattingEnabled = true;
            checkedListBoxNamesOfPeople.HorizontalScrollbar = true;
            checkedListBoxNamesOfPeople.Location = new Point(364, 86);
            checkedListBoxNamesOfPeople.Name = "checkedListBoxNamesOfPeople";
            checkedListBoxNamesOfPeople.Size = new Size(265, 310);
            checkedListBoxNamesOfPeople.TabIndex = 34;
            // 
            // ListOfInstructions
            // 
            ListOfInstructions.FormattingEnabled = true;
            ListOfInstructions.ItemHeight = 15;
            ListOfInstructions.Location = new Point(20, 86);
            ListOfInstructions.Name = "ListOfInstructions";
            ListOfInstructions.Size = new Size(275, 304);
            ListOfInstructions.TabIndex = 33;
            // 
            // buttonSyncManualyInstrWithDB
            // 
            buttonSyncManualyInstrWithDB.Location = new Point(20, 43);
            buttonSyncManualyInstrWithDB.Margin = new Padding(3, 2, 3, 2);
            buttonSyncManualyInstrWithDB.Name = "buttonSyncManualyInstrWithDB";
            buttonSyncManualyInstrWithDB.Size = new Size(275, 38);
            buttonSyncManualyInstrWithDB.TabIndex = 32;
            buttonSyncManualyInstrWithDB.Text = "Синхронизировать инструктажи с Базой Данных";
            buttonSyncManualyInstrWithDB.UseVisualStyleBackColor = true;
            buttonSyncManualyInstrWithDB.Click += buttonSyncManualyInstrWithDB_Click;
            // 
            // ListOfInstructionsForUser
            // 
            ListOfInstructionsForUser.FormattingEnabled = true;
            ListOfInstructionsForUser.ItemHeight = 15;
            ListOfInstructionsForUser.Location = new Point(20, 59);
            ListOfInstructionsForUser.Margin = new Padding(3, 2, 3, 2);
            ListOfInstructionsForUser.Name = "ListOfInstructionsForUser";
            ListOfInstructionsForUser.Size = new Size(244, 229);
            ListOfInstructionsForUser.TabIndex = 29;
            ListOfInstructionsForUser.SelectedValueChanged += InstructionsToPass_SelectedIndexChanged;
            // 
            // ChiefTabControl
            // 
            ChiefTabControl.Controls.Add(tabPage1);
            ChiefTabControl.Controls.Add(tabPage2);
            ChiefTabControl.Controls.Add(tabPage3);
            ChiefTabControl.Controls.Add(tabPage4);
            ChiefTabControl.Controls.Add(tabPage5);
            ChiefTabControl.Location = new Point(12, 23);
            ChiefTabControl.Name = "ChiefTabControl";
            ChiefTabControl.SelectedIndex = 0;
            ChiefTabControl.Size = new Size(675, 616);
            ChiefTabControl.TabIndex = 33;
            ChiefTabControl.SelectedIndexChanged += ChiefTabControl_SelectedIndexChanged;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(FilesOfInstructionCheckedListBox);
            tabPage3.Controls.Add(PassInstruction);
            tabPage3.Controls.Add(ListOfInstructionsForUser);
            tabPage3.Controls.Add(HyperLinkForInstructionsFolder);
            tabPage3.Controls.Add(label8);
            tabPage3.Location = new Point(4, 24);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new Padding(3);
            tabPage3.Size = new Size(667, 588);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Прохождение инструктажей";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // FilesOfInstructionCheckedListBox
            // 
            FilesOfInstructionCheckedListBox.CheckOnClick = true;
            FilesOfInstructionCheckedListBox.FormattingEnabled = true;
            FilesOfInstructionCheckedListBox.HorizontalScrollbar = true;
            FilesOfInstructionCheckedListBox.Location = new Point(283, 69);
            FilesOfInstructionCheckedListBox.Name = "FilesOfInstructionCheckedListBox";
            FilesOfInstructionCheckedListBox.Size = new Size(195, 220);
            FilesOfInstructionCheckedListBox.TabIndex = 39;
            FilesOfInstructionCheckedListBox.ItemCheck += FilesOfInstructionCheckedListBox_ItemCheck;
            // 
            // PassInstruction
            // 
            PassInstruction.AutoSize = true;
            PassInstruction.Enabled = false;
            PassInstruction.Location = new Point(45, 309);
            PassInstruction.Name = "PassInstruction";
            PassInstruction.Size = new Size(245, 19);
            PassInstruction.TabIndex = 38;
            PassInstruction.Text = "Подтверждаю, что инструктаж пройден";
            PassInstruction.UseVisualStyleBackColor = true;
            PassInstruction.CheckedChanged += PassInstruction_CheckedChanged;
            // 
            // HyperLinkForInstructionsFolder
            // 
            HyperLinkForInstructionsFolder.Enabled = false;
            HyperLinkForInstructionsFolder.Location = new Point(493, 60);
            HyperLinkForInstructionsFolder.Name = "HyperLinkForInstructionsFolder";
            HyperLinkForInstructionsFolder.Size = new Size(111, 229);
            HyperLinkForInstructionsFolder.TabIndex = 37;
            HyperLinkForInstructionsFolder.Text = "Гиперссылка на инструктаж";
            HyperLinkForInstructionsFolder.UseVisualStyleBackColor = true;
            HyperLinkForInstructionsFolder.Click += HyperLinkForInstructionsFolder_Click;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(45, 17);
            label8.Name = "label8";
            label8.Size = new Size(201, 15);
            label8.TabIndex = 36;
            label8.Text = "Лист непройденных инструктажей:";
            // 
            // tabPage4
            // 
            tabPage4.Controls.Add(listBoxOfPassedInstructions);
            tabPage4.Controls.Add(dataGridViewPeopleThatNotPassedInstr);
            tabPage4.Controls.Add(listBoxOfNotPassedByInstructions);
            tabPage4.Controls.Add(TestButtonForInstructions);
            tabPage4.Location = new Point(4, 24);
            tabPage4.Name = "tabPage4";
            tabPage4.Padding = new Padding(3);
            tabPage4.Size = new Size(667, 588);
            tabPage4.TabIndex = 3;
            tabPage4.Text = "Контроль прохождения инстр.";
            tabPage4.UseVisualStyleBackColor = true;
            // 
            // listBoxOfPassedInstructions
            // 
            listBoxOfPassedInstructions.FormattingEnabled = true;
            listBoxOfPassedInstructions.ItemHeight = 15;
            listBoxOfPassedInstructions.Location = new Point(25, 278);
            listBoxOfPassedInstructions.Name = "listBoxOfPassedInstructions";
            listBoxOfPassedInstructions.Size = new Size(280, 184);
            listBoxOfPassedInstructions.TabIndex = 3;
            // 
            // dataGridViewPeopleThatNotPassedInstr
            // 
            dataGridViewPeopleThatNotPassedInstr.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewPeopleThatNotPassedInstr.Columns.AddRange(new DataGridViewColumn[] { Names, Passed });
            dataGridViewPeopleThatNotPassedInstr.Location = new Point(324, 59);
            dataGridViewPeopleThatNotPassedInstr.Name = "dataGridViewPeopleThatNotPassedInstr";
            dataGridViewPeopleThatNotPassedInstr.ReadOnly = true;
            dataGridViewPeopleThatNotPassedInstr.Size = new Size(322, 184);
            dataGridViewPeopleThatNotPassedInstr.TabIndex = 2;
            // 
            // Names
            // 
            Names.HeaderText = "ФИО";
            Names.Name = "Names";
            Names.ReadOnly = true;
            // 
            // Passed
            // 
            Passed.HeaderText = "Пройден ли инструктаж?";
            Passed.Name = "Passed";
            Passed.ReadOnly = true;
            // 
            // listBoxOfNotPassedByInstructions
            // 
            listBoxOfNotPassedByInstructions.FormattingEnabled = true;
            listBoxOfNotPassedByInstructions.ItemHeight = 15;
            listBoxOfNotPassedByInstructions.Location = new Point(25, 59);
            listBoxOfNotPassedByInstructions.Name = "listBoxOfNotPassedByInstructions";
            listBoxOfNotPassedByInstructions.Size = new Size(280, 184);
            listBoxOfNotPassedByInstructions.TabIndex = 1;
            listBoxOfNotPassedByInstructions.SelectedIndexChanged += listBoxOfNotPassedByInstructions_SelectedIndexChanged;
            // 
            // TestButtonForInstructions
            // 
            TestButtonForInstructions.Location = new Point(192, 20);
            TestButtonForInstructions.Name = "TestButtonForInstructions";
            TestButtonForInstructions.Size = new Size(240, 23);
            TestButtonForInstructions.TabIndex = 0;
            TestButtonForInstructions.Text = "Пока что тестовая кнопка";
            TestButtonForInstructions.UseVisualStyleBackColor = true;
            TestButtonForInstructions.Click += TestButtonForInstructions_Click;
            // 
            // tabPage5
            // 
            tabPage5.Controls.Add(DownloadAllEmployeesInTheDepartment);
            tabPage5.Controls.Add(checkedListBoxTypesOfInstruction);
            tabPage5.Controls.Add(label9);
            tabPage5.Controls.Add(endDateInstructionExportRequest);
            tabPage5.Controls.Add(label3);
            tabPage5.Controls.Add(startDateInstructionExportRequest);
            tabPage5.Controls.Add(ExportInstructionRequestButton);
            tabPage5.Location = new Point(4, 24);
            tabPage5.Name = "tabPage5";
            tabPage5.Padding = new Padding(3);
            tabPage5.Size = new Size(667, 588);
            tabPage5.TabIndex = 4;
            tabPage5.Text = "Учёт сотрудников";
            tabPage5.UseVisualStyleBackColor = true;
            // 
            // DownloadAllEmployeesInTheDepartment
            // 
            DownloadAllEmployeesInTheDepartment.Location = new Point(36, 371);
            DownloadAllEmployeesInTheDepartment.Name = "DownloadAllEmployeesInTheDepartment";
            DownloadAllEmployeesInTheDepartment.Size = new Size(207, 71);
            DownloadAllEmployeesInTheDepartment.TabIndex = 6;
            DownloadAllEmployeesInTheDepartment.Text = "Скачать данные обо всех сотрудниках";
            DownloadAllEmployeesInTheDepartment.UseVisualStyleBackColor = true;
            DownloadAllEmployeesInTheDepartment.Click += DownloadAllEmployeesInTheDepartment_Click;
            // 
            // checkedListBoxTypesOfInstruction
            // 
            checkedListBoxTypesOfInstruction.FormattingEnabled = true;
            checkedListBoxTypesOfInstruction.Items.AddRange(new object[] { "Внеплановые;", "Первичные;", "Повторные;", "Повторные(для водителей);", "Целевые;" });
            checkedListBoxTypesOfInstruction.Location = new Point(43, 116);
            checkedListBoxTypesOfInstruction.Name = "checkedListBoxTypesOfInstruction";
            checkedListBoxTypesOfInstruction.Size = new Size(334, 94);
            checkedListBoxTypesOfInstruction.TabIndex = 5;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(369, 28);
            label9.Name = "label9";
            label9.Size = new Size(105, 15);
            label9.TabIndex = 4;
            label9.Text = "По такую-то дату:";
            // 
            // endDateInstructionExportRequest
            // 
            endDateInstructionExportRequest.Location = new Point(369, 56);
            endDateInstructionExportRequest.Name = "endDateInstructionExportRequest";
            endDateInstructionExportRequest.Size = new Size(200, 23);
            endDateInstructionExportRequest.TabIndex = 3;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(43, 28);
            label3.Name = "label3";
            label3.Size = new Size(98, 15);
            label3.TabIndex = 2;
            label3.Text = "С такой-то даты:";
            // 
            // startDateInstructionExportRequest
            // 
            startDateInstructionExportRequest.Location = new Point(43, 56);
            startDateInstructionExportRequest.Name = "startDateInstructionExportRequest";
            startDateInstructionExportRequest.Size = new Size(200, 23);
            startDateInstructionExportRequest.TabIndex = 1;
            // 
            // ExportInstructionRequestButton
            // 
            ExportInstructionRequestButton.Location = new Point(234, 237);
            ExportInstructionRequestButton.Name = "ExportInstructionRequestButton";
            ExportInstructionRequestButton.Size = new Size(162, 23);
            ExportInstructionRequestButton.TabIndex = 0;
            ExportInstructionRequestButton.Text = "Экспортировать данные";
            ExportInstructionRequestButton.UseVisualStyleBackColor = true;
            ExportInstructionRequestButton.Click += ExportInstructionRequestButton_Click;
            // 
            // LogOutButton
            // 
            LogOutButton.Location = new Point(928, 47);
            LogOutButton.Name = "LogOutButton";
            LogOutButton.Size = new Size(120, 43);
            LogOutButton.TabIndex = 34;
            LogOutButton.Text = "Выйти из учётной записи";
            LogOutButton.UseVisualStyleBackColor = true;
            LogOutButton.Click += LogOutForm_Click;
            // 
            // LabelTray
            // 
            LabelTray.AutoSize = true;
            LabelTray.Location = new Point(16, 660);
            LabelTray.Name = "LabelTray";
            LabelTray.Size = new Size(87, 15);
            LabelTray.TabIndex = 37;
            LabelTray.Text = "!Список задач:";
            // 
            // RefreshTasksButton
            // 
            RefreshTasksButton.Location = new Point(833, 609);
            RefreshTasksButton.Name = "RefreshTasksButton";
            RefreshTasksButton.Size = new Size(215, 66);
            RefreshTasksButton.TabIndex = 38;
            RefreshTasksButton.Text = "Обновить список задач";
            RefreshTasksButton.UseVisualStyleBackColor = true;
            RefreshTasksButton.Click += RefreshTasksButton_Click;
            // 
            // TrayOfTasksList
            // 
            TrayOfTasksList.FormattingEnabled = true;
            TrayOfTasksList.ItemHeight = 15;
            TrayOfTasksList.Location = new Point(16, 688);
            TrayOfTasksList.Name = "TrayOfTasksList";
            TrayOfTasksList.Size = new Size(1032, 94);
            TrayOfTasksList.TabIndex = 40;
            // 
            // usernameLabel
            // 
            usernameLabel.AutoSize = true;
            usernameLabel.Location = new Point(813, 150);
            usernameLabel.Name = "usernameLabel";
            usernameLabel.Size = new Size(44, 15);
            usernameLabel.TabIndex = 41;
            usernameLabel.Text = "label10";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(719, 150);
            label10.Name = "label10";
            label10.Size = new Size(88, 15);
            label10.TabIndex = 42;
            label10.Text = "Вы вошли как:";
            // 
            // button1
            // 
            button1.Location = new Point(380, 811);
            button1.Name = "button1";
            button1.Size = new Size(205, 23);
            button1.TabIndex = 43;
            button1.Text = "Задание выполнено";
            button1.UseVisualStyleBackColor = true;
            // 
            // ChiefForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1102, 845);
            Controls.Add(button1);
            Controls.Add(label10);
            Controls.Add(usernameLabel);
            Controls.Add(TrayOfTasksList);
            Controls.Add(RefreshTasksButton);
            Controls.Add(LabelTray);
            Controls.Add(testButton);
            Controls.Add(LogOutButton);
            Controls.Add(ChiefTabControl);
            Name = "ChiefForm";
            Text = "ChiefOfDepartment";
            FormClosing += ChiefForm_FormClosing;
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            ChiefTabControl.ResumeLayout(false);
            tabPage3.ResumeLayout(false);
            tabPage3.PerformLayout();
            tabPage4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dataGridViewPeopleThatNotPassedInstr).EndInit();
            tabPage5.ResumeLayout(false);
            tabPage5.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button submitInstructionToPeople;
        private Button SyncNamesWithDB;
        private CheckBox checkBoxIsForDrivers;
        private TabPage tabPage2;
        private Label label1;
        private ListBox typeOfInstructionListBox;
        private TabPage tabPage1;
        private Button buttonSyncManualyInstrWithDB;
        private ListBox ListOfInstructionsForUser;
        private Button Download_file_excel;
        private Button buttonTest;
        private TabControl ChiefTabControl;
        private Button buttonChoosePathToInstruction;
        private Label label2;
        private TextBox InstructionTextBox;
        private Label label6;
        private DateTimePicker datePickerEnd;
        private Button buttonCreateInstruction;
        private Label PathToFolderOfInstruction;
        private Label label7;
        private ToolTip toolTip1;
        private Button testButton;
        private Button LogOutButton;
        private TabPage tabPage3;
        private CheckBox PassInstruction;
        private Button HyperLinkForInstructionsFolder;
        private Label label8;
        private ListBox ListOfInstructions;
        private CheckedListBox FilesOfInstructionCheckedListBox;
        private TabPage tabPage4;
        private Button TestButtonForInstructions;
        private ListBox listBoxOfNotPassedByInstructions;
        private CheckedListBox checkedListBoxNamesOfPeople;
        private DataGridView dataGridViewPeopleThatNotPassedInstr;
        private DataGridViewTextBoxColumn Names;
        private DataGridViewTextBoxColumn Passed;
        private CheckedListBox checkedListBoxNamesOfPeopleCreatingInstr;
        private ListBox listBoxOfPassedInstructions;
        private TreeViewWithoutDoubleClick treeView1;
        private TabPage tabPage5;
        private Label LabelTray;
        private Button RefreshTasksButton;
        private Label label3;
        private DateTimePicker startDateInstructionExportRequest;
        private Button ExportInstructionRequestButton;
        private CheckedListBoxWithoutDoubleClick checkedListBoxTypesOfInstruction;
        private Label label9;
        private DateTimePicker endDateInstructionExportRequest;
        private ListBox TrayOfTasksList;
        private Label usernameLabel;
        private Label label10;
        private Button button1;
        private Button SelectAllThePeopleInListBoxButton;
        private Button DownloadAllEmployeesInTheDepartment;
        private CheckBox MissTheAssignmentOfInstrCheckedBox;
    }
}