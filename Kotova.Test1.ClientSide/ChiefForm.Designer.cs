namespace Kotova.Test1.ClientSide
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
            components = new System.ComponentModel.Container();
            submitInstructionToPeople = new Button();
            label3 = new Label();
            SyncNamesWithDB = new Button();
            ListBoxNamesOfPeople = new ListBox();
            checkBoxIsForDrivers = new CheckBox();
            tabPage2 = new TabPage();
            treeView1 = new TreeView();
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
            ListOfInstructions = new ListBox();
            buttonSyncManualyInstrWithDB = new Button();
            label5 = new Label();
            label4 = new Label();
            Download_file_excel = new Button();
            buttonTest = new Button();
            ListOfInstructionsForUser = new ListBox();
            ChiefTabControl = new TabControl();
            tabPage3 = new TabPage();
            PassInstruction = new CheckBox();
            HyperLinkForInstructionsFolder = new Button();
            label8 = new Label();
            toolTip1 = new ToolTip(components);
            LogOutButton = new Button();
            consoleTextBox = new TextBox();
            tabPage2.SuspendLayout();
            tabPage1.SuspendLayout();
            ChiefTabControl.SuspendLayout();
            tabPage3.SuspendLayout();
            SuspendLayout();
            // 
            // submitInstructionToPeople
            // 
            submitInstructionToPeople.Location = new Point(398, 226);
            submitInstructionToPeople.Margin = new Padding(3, 2, 3, 2);
            submitInstructionToPeople.Name = "submitInstructionToPeople";
            submitInstructionToPeople.Size = new Size(216, 71);
            submitInstructionToPeople.TabIndex = 28;
            submitInstructionToPeople.Text = "Отправить выбранным людям уведомление об инструктаже";
            submitInstructionToPeople.UseVisualStyleBackColor = true;
            submitInstructionToPeople.Click += submitInstructionToPeople_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(304, 170);
            label3.Name = "label3";
            label3.Size = new Size(310, 15);
            label3.TabIndex = 27;
            label3.Text = "Чтобы отменить выбор - ctrl+ЛКМ(левый клик мыши)";
            // 
            // SyncNamesWithDB
            // 
            SyncNamesWithDB.Location = new Point(349, 59);
            SyncNamesWithDB.Margin = new Padding(3, 2, 3, 2);
            SyncNamesWithDB.Name = "SyncNamesWithDB";
            SyncNamesWithDB.Size = new Size(265, 22);
            SyncNamesWithDB.TabIndex = 26;
            SyncNamesWithDB.Text = "Синхронизировать ФИО с Базой Данных";
            SyncNamesWithDB.UseVisualStyleBackColor = true;
            SyncNamesWithDB.Click += SyncNamesWithDB_Click;
            // 
            // ListBoxNamesOfPeople
            // 
            ListBoxNamesOfPeople.FormattingEnabled = true;
            ListBoxNamesOfPeople.ItemHeight = 15;
            ListBoxNamesOfPeople.Items.AddRange(new object[] { "Список ФИО" });
            ListBoxNamesOfPeople.Location = new Point(304, 85);
            ListBoxNamesOfPeople.Margin = new Padding(3, 2, 3, 2);
            ListBoxNamesOfPeople.Name = "ListBoxNamesOfPeople";
            ListBoxNamesOfPeople.SelectionMode = SelectionMode.MultiExtended;
            ListBoxNamesOfPeople.Size = new Size(346, 79);
            ListBoxNamesOfPeople.TabIndex = 21;
            // 
            // checkBoxIsForDrivers
            // 
            checkBoxIsForDrivers.AutoSize = true;
            checkBoxIsForDrivers.Location = new Point(388, 33);
            checkBoxIsForDrivers.Margin = new Padding(3, 2, 3, 2);
            checkBoxIsForDrivers.Name = "checkBoxIsForDrivers";
            checkBoxIsForDrivers.Size = new Size(152, 19);
            checkBoxIsForDrivers.TabIndex = 20;
            checkBoxIsForDrivers.Text = "Только для водителей?";
            checkBoxIsForDrivers.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
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
            tabPage2.Size = new Size(667, 374);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Создание инструктажа";
            tabPage2.UseVisualStyleBackColor = true;
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
            buttonCreateInstruction.Location = new Point(199, 306);
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
            typeOfInstructionListBox.Items.AddRange(new object[] { "Первичный;", "Повторный;", "Целевой;" });
            typeOfInstructionListBox.Location = new Point(47, 49);
            typeOfInstructionListBox.Name = "typeOfInstructionListBox";
            typeOfInstructionListBox.Size = new Size(120, 49);
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
            tabPage1.Controls.Add(ListOfInstructions);
            tabPage1.Controls.Add(buttonSyncManualyInstrWithDB);
            tabPage1.Controls.Add(submitInstructionToPeople);
            tabPage1.Controls.Add(label5);
            tabPage1.Controls.Add(label3);
            tabPage1.Controls.Add(ListBoxNamesOfPeople);
            tabPage1.Controls.Add(SyncNamesWithDB);
            tabPage1.Controls.Add(label4);
            tabPage1.Controls.Add(Download_file_excel);
            tabPage1.Controls.Add(checkBoxIsForDrivers);
            tabPage1.Controls.Add(buttonTest);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(667, 374);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Обработка инструктажей";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // ListOfInstructions
            // 
            ListOfInstructions.FormattingEnabled = true;
            ListOfInstructions.ItemHeight = 15;
            ListOfInstructions.Location = new Point(20, 57);
            ListOfInstructions.Name = "ListOfInstructions";
            ListOfInstructions.Size = new Size(232, 79);
            ListOfInstructions.TabIndex = 33;
            // 
            // buttonSyncManualyInstrWithDB
            // 
            buttonSyncManualyInstrWithDB.Location = new Point(27, 10);
            buttonSyncManualyInstrWithDB.Margin = new Padding(3, 2, 3, 2);
            buttonSyncManualyInstrWithDB.Name = "buttonSyncManualyInstrWithDB";
            buttonSyncManualyInstrWithDB.Size = new Size(216, 38);
            buttonSyncManualyInstrWithDB.TabIndex = 32;
            buttonSyncManualyInstrWithDB.Text = "Синхронизировать инструктажи с Базой Данных";
            buttonSyncManualyInstrWithDB.UseVisualStyleBackColor = true;
            buttonSyncManualyInstrWithDB.Click += buttonSyncManualyInstrWithDB_Click;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(150, 142);
            label5.Name = "label5";
            label5.Size = new Size(74, 15);
            label5.TabIndex = 31;
            label5.Text = "Не выбрано";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(20, 142);
            label4.Name = "label4";
            label4.Size = new Size(124, 15);
            label4.TabIndex = 30;
            label4.Text = "Дата инструктажа до:";
            // 
            // Download_file_excel
            // 
            Download_file_excel.Location = new Point(20, 226);
            Download_file_excel.Margin = new Padding(3, 2, 3, 2);
            Download_file_excel.Name = "Download_file_excel";
            Download_file_excel.Size = new Size(216, 71);
            Download_file_excel.TabIndex = 24;
            Download_file_excel.Text = "Скачать Excel файл для отдела";
            Download_file_excel.UseVisualStyleBackColor = true;
            // 
            // buttonTest
            // 
            buttonTest.Location = new Point(20, 166);
            buttonTest.Margin = new Padding(3, 2, 3, 2);
            buttonTest.Name = "buttonTest";
            buttonTest.Size = new Size(131, 22);
            buttonTest.TabIndex = 22;
            buttonTest.Text = "Тест сервера";
            buttonTest.UseVisualStyleBackColor = true;
            // 
            // ListOfInstructionsForUser
            // 
            ListOfInstructionsForUser.FormattingEnabled = true;
            ListOfInstructionsForUser.ItemHeight = 15;
            ListOfInstructionsForUser.Location = new Point(20, 59);
            ListOfInstructionsForUser.Margin = new Padding(3, 2, 3, 2);
            ListOfInstructionsForUser.Name = "ListOfInstructionsForUser";
            ListOfInstructionsForUser.Size = new Size(467, 229);
            ListOfInstructionsForUser.TabIndex = 29;
            ListOfInstructionsForUser.SelectedValueChanged += InstructionsToPass_SelectedIndexChanged;
            // 
            // ChiefTabControl
            // 
            ChiefTabControl.Controls.Add(tabPage1);
            ChiefTabControl.Controls.Add(tabPage2);
            ChiefTabControl.Controls.Add(tabPage3);
            ChiefTabControl.Location = new Point(12, 23);
            ChiefTabControl.Name = "ChiefTabControl";
            ChiefTabControl.SelectedIndex = 0;
            ChiefTabControl.Size = new Size(675, 402);
            ChiefTabControl.TabIndex = 33;
            ChiefTabControl.SelectedIndexChanged += ChiefTabControl_SelectedIndexChanged;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(PassInstruction);
            tabPage3.Controls.Add(ListOfInstructionsForUser);
            tabPage3.Controls.Add(HyperLinkForInstructionsFolder);
            tabPage3.Controls.Add(label8);
            tabPage3.Location = new Point(4, 24);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new Padding(3);
            tabPage3.Size = new Size(667, 374);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Прохождение инструктажей";
            tabPage3.UseVisualStyleBackColor = true;
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
            // consoleTextBox
            // 
            consoleTextBox.Location = new Point(708, 106);
            consoleTextBox.Multiline = true;
            consoleTextBox.Name = "consoleTextBox";
            consoleTextBox.ReadOnly = true;
            consoleTextBox.ScrollBars = ScrollBars.Both;
            consoleTextBox.Size = new Size(340, 305);
            consoleTextBox.TabIndex = 35;
            // 
            // ChiefForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1085, 450);
            Controls.Add(testButton);
            Controls.Add(consoleTextBox);
            Controls.Add(LogOutButton);
            Controls.Add(ChiefTabControl);
            Name = "ChiefForm";
            Text = "ChiefOfDepartment";
            FormClosed += ChiefForm_FormClosed;
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            ChiefTabControl.ResumeLayout(false);
            tabPage3.ResumeLayout(false);
            tabPage3.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button submitInstructionToPeople;
        private Label label3;
        private Button SyncNamesWithDB;
        private ListBox ListBoxNamesOfPeople;
        private CheckBox checkBoxIsForDrivers;
        private TabPage tabPage2;
        private Label label1;
        private ListBox typeOfInstructionListBox;
        private TabPage tabPage1;
        private Button buttonSyncManualyInstrWithDB;
        private Label label5;
        private ListBox ListOfInstructionsForUser;
        private Label label4;
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
        private TextBox consoleTextBox;
        private TreeView treeView1;
    }
}