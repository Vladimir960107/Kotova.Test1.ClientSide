namespace Kotova.Test1.ClientSide
{
    partial class Form2
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            button1 = new Button();
            datePickerEnd = new DateTimePicker();
            label1 = new Label();
            InstructionTextBox = new TextBox();
            label2 = new Label();
            buttonChoosePathToInstruction = new Button();
            checkBoxIsForDrivers = new CheckBox();
            ListBoxNamesOfPeople = new ListBox();
            buttonTest = new Button();
            UploadFileToServer = new Button();
            Download_file_excel = new Button();
            syncExcelAndDB = new Button();
            SyncNamesWithDB = new Button();
            label3 = new Label();
            submitInstructionToPeople = new Button();
            listOfInstructions = new ListBox();
            label4 = new Label();
            label5 = new Label();
            buttonSyncManualyInstrWithDB = new Button();
            tableLayoutPanel1 = new TableLayoutPanel();
            button2 = new Button();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(30, 250);
            button1.Margin = new Padding(3, 2, 3, 2);
            button1.Name = "button1";
            button1.Size = new Size(219, 40);
            button1.TabIndex = 0;
            button1.Text = "Внести новый инструткаж";
            button1.UseVisualStyleBackColor = true;
            button1.Click += buttonCreateNotification_Click;
            // 
            // datePickerEnd
            // 
            datePickerEnd.Location = new Point(30, 70);
            datePickerEnd.Margin = new Padding(3, 2, 3, 2);
            datePickerEnd.Name = "datePickerEnd";
            datePickerEnd.Size = new Size(219, 23);
            datePickerEnd.TabIndex = 2;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(30, 41);
            label1.Name = "label1";
            label1.Size = new Size(175, 15);
            label1.TabIndex = 3;
            label1.Text = "До какой даты включительно?";
            // 
            // InstructionTextBox
            // 
            InstructionTextBox.Location = new Point(30, 137);
            InstructionTextBox.Margin = new Padding(3, 2, 3, 2);
            InstructionTextBox.Multiline = true;
            InstructionTextBox.Name = "InstructionTextBox";
            InstructionTextBox.Size = new Size(219, 51);
            InstructionTextBox.TabIndex = 4;
            InstructionTextBox.Text = "Работа в зоне железнодорожных путей СТО-357";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(30, 108);
            label2.Name = "label2";
            label2.Size = new Size(133, 15);
            label2.TabIndex = 5;
            label2.Text = "Причина инструктажа:";
            // 
            // buttonChoosePathToInstruction
            // 
            buttonChoosePathToInstruction.Location = new Point(78, 192);
            buttonChoosePathToInstruction.Margin = new Padding(3, 2, 3, 2);
            buttonChoosePathToInstruction.Name = "buttonChoosePathToInstruction";
            buttonChoosePathToInstruction.Size = new Size(121, 50);
            buttonChoosePathToInstruction.TabIndex = 6;
            buttonChoosePathToInstruction.Text = "Указать папку для инструктажа";
            buttonChoosePathToInstruction.UseVisualStyleBackColor = true;
            buttonChoosePathToInstruction.Click += buttonChoosePathToInstruction_Click;
            // 
            // checkBoxIsForDrivers
            // 
            checkBoxIsForDrivers.AutoSize = true;
            checkBoxIsForDrivers.Location = new Point(351, 17);
            checkBoxIsForDrivers.Margin = new Padding(3, 2, 3, 2);
            checkBoxIsForDrivers.Name = "checkBoxIsForDrivers";
            checkBoxIsForDrivers.Size = new Size(152, 19);
            checkBoxIsForDrivers.TabIndex = 7;
            checkBoxIsForDrivers.Text = "Только для водителей?";
            checkBoxIsForDrivers.UseVisualStyleBackColor = true;
            // 
            // ListBoxNamesOfPeople
            // 
            ListBoxNamesOfPeople.FormattingEnabled = true;
            ListBoxNamesOfPeople.ItemHeight = 15;
            ListBoxNamesOfPeople.Location = new Point(749, 70);
            ListBoxNamesOfPeople.Margin = new Padding(3, 2, 3, 2);
            ListBoxNamesOfPeople.Name = "ListBoxNamesOfPeople";
            ListBoxNamesOfPeople.SelectionMode = SelectionMode.MultiExtended;
            ListBoxNamesOfPeople.Size = new Size(346, 79);
            ListBoxNamesOfPeople.TabIndex = 8;
            // 
            // buttonTest
            // 
            buttonTest.Location = new Point(351, 192);
            buttonTest.Margin = new Padding(3, 2, 3, 2);
            buttonTest.Name = "buttonTest";
            buttonTest.Size = new Size(131, 22);
            buttonTest.TabIndex = 9;
            buttonTest.Text = "Тест сервера";
            buttonTest.UseVisualStyleBackColor = true;
            buttonTest.Click += buttonTest_Click;
            // 
            // UploadFileToServer
            // 
            UploadFileToServer.Location = new Point(351, 218);
            UploadFileToServer.Margin = new Padding(3, 2, 3, 2);
            UploadFileToServer.Name = "UploadFileToServer";
            UploadFileToServer.Size = new Size(168, 22);
            UploadFileToServer.TabIndex = 10;
            UploadFileToServer.Text = "Upload file to server";
            UploadFileToServer.UseVisualStyleBackColor = true;
            UploadFileToServer.Click += UploadFileToServer_Click;
            // 
            // Download_file_excel
            // 
            Download_file_excel.Location = new Point(351, 271);
            Download_file_excel.Margin = new Padding(3, 2, 3, 2);
            Download_file_excel.Name = "Download_file_excel";
            Download_file_excel.Size = new Size(216, 22);
            Download_file_excel.TabIndex = 11;
            Download_file_excel.Text = "Download last file from server";
            Download_file_excel.UseVisualStyleBackColor = true;
            Download_file_excel.Click += Download_file_excel_Click;
            // 
            // syncExcelAndDB
            // 
            syncExcelAndDB.Location = new Point(351, 244);
            syncExcelAndDB.Margin = new Padding(3, 2, 3, 2);
            syncExcelAndDB.Name = "syncExcelAndDB";
            syncExcelAndDB.Size = new Size(192, 22);
            syncExcelAndDB.TabIndex = 12;
            syncExcelAndDB.Text = "Sync last excel file and DB";
            syncExcelAndDB.UseVisualStyleBackColor = true;
            syncExcelAndDB.Click += syncExcelAndDB_Click;
            // 
            // SyncNamesWithDB
            // 
            SyncNamesWithDB.Location = new Point(749, 36);
            SyncNamesWithDB.Margin = new Padding(3, 2, 3, 2);
            SyncNamesWithDB.Name = "SyncNamesWithDB";
            SyncNamesWithDB.Size = new Size(216, 22);
            SyncNamesWithDB.TabIndex = 13;
            SyncNamesWithDB.Text = "Синхр. ФИО с Базой Данных";
            SyncNamesWithDB.UseVisualStyleBackColor = true;
            SyncNamesWithDB.Click += SyncNamesWithDB_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(749, 150);
            label3.Name = "label3";
            label3.Size = new Size(310, 15);
            label3.TabIndex = 14;
            label3.Text = "Чтобы отменить выбор - ctrl+ЛКМ(левый клик мыши)";
            // 
            // submitInstructionToPeople
            // 
            submitInstructionToPeople.Location = new Point(749, 182);
            submitInstructionToPeople.Margin = new Padding(3, 2, 3, 2);
            submitInstructionToPeople.Name = "submitInstructionToPeople";
            submitInstructionToPeople.Size = new Size(216, 71);
            submitInstructionToPeople.TabIndex = 15;
            submitInstructionToPeople.Text = "Отправить выбранным людям уведомление об инструктаже";
            submitInstructionToPeople.UseVisualStyleBackColor = true;
            submitInstructionToPeople.Click += submitInstructionToPeople_Click;
            // 
            // listOfInstructions
            // 
            listOfInstructions.FormattingEnabled = true;
            listOfInstructions.ItemHeight = 15;
            listOfInstructions.Location = new Point(351, 70);
            listOfInstructions.Margin = new Padding(3, 2, 3, 2);
            listOfInstructions.Name = "listOfInstructions";
            listOfInstructions.Size = new Size(249, 79);
            listOfInstructions.TabIndex = 16;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(351, 164);
            label4.Name = "label4";
            label4.Size = new Size(124, 15);
            label4.TabIndex = 17;
            label4.Text = "Дата инструктажа до:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(493, 164);
            label5.Name = "label5";
            label5.Size = new Size(74, 15);
            label5.TabIndex = 18;
            label5.Text = "Не выбрано";
            // 
            // buttonSyncManualyInstrWithDB
            // 
            buttonSyncManualyInstrWithDB.Location = new Point(351, 38);
            buttonSyncManualyInstrWithDB.Margin = new Padding(3, 2, 3, 2);
            buttonSyncManualyInstrWithDB.Name = "buttonSyncManualyInstrWithDB";
            buttonSyncManualyInstrWithDB.Size = new Size(216, 22);
            buttonSyncManualyInstrWithDB.TabIndex = 19;
            buttonSyncManualyInstrWithDB.Text = "Синхр. инструктажи с Базой Данных";
            buttonSyncManualyInstrWithDB.UseVisualStyleBackColor = true;
            buttonSyncManualyInstrWithDB.Click += buttonSyncManualyInstrWithDB_Click;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 3;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 95F));
            tableLayoutPanel1.Controls.Add(button2, 0, 0);
            tableLayoutPanel1.Location = new Point(638, 460);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 41F));
            tableLayoutPanel1.Size = new Size(327, 135);
            tableLayoutPanel1.TabIndex = 20;
            // 
            // button2
            // 
            button2.Anchor = AnchorStyles.None;
            button2.Location = new Point(20, 12);
            button2.Name = "button2";
            button2.Size = new Size(75, 23);
            button2.TabIndex = 0;
            button2.Text = "button2";
            button2.UseVisualStyleBackColor = true;
            // 
            // Form2
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1708, 894);
            Controls.Add(tableLayoutPanel1);
            Controls.Add(buttonSyncManualyInstrWithDB);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(listOfInstructions);
            Controls.Add(submitInstructionToPeople);
            Controls.Add(label3);
            Controls.Add(SyncNamesWithDB);
            Controls.Add(syncExcelAndDB);
            Controls.Add(Download_file_excel);
            Controls.Add(UploadFileToServer);
            Controls.Add(buttonTest);
            Controls.Add(ListBoxNamesOfPeople);
            Controls.Add(checkBoxIsForDrivers);
            Controls.Add(buttonChoosePathToInstruction);
            Controls.Add(label2);
            Controls.Add(InstructionTextBox);
            Controls.Add(label1);
            Controls.Add(datePickerEnd);
            Controls.Add(button1);
            Margin = new Padding(3, 2, 3, 2);
            Name = "Form2";
            Text = "Form2";
            tableLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private DateTimePicker datePickerEnd;
        private Label label1;
        private TextBox InstructionTextBox;
        private Label label2;
        private Button buttonChoosePathToInstruction;
        private CheckBox checkBoxIsForDrivers;
        private ListBox ListBoxNamesOfPeople;
        private Button buttonTest;
        private Button UploadFileToServer;
        private Button Download_file_excel;
        private Button syncExcelAndDB;
        private Button SyncNamesWithDB;
        private Label label3;
        private Button submitInstructionToPeople;
        private ListBox listOfInstructions;
        private Label label4;
        private Label label5;
        private Button buttonSyncManualyInstrWithDB;
        private TableLayoutPanel tableLayoutPanel1;
        private Button button2;
    }
}
