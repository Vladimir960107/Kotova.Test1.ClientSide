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
            listBox1 = new ListBox();
            buttonTest = new Button();
            UploadFileToServer = new Button();
            Download_file_excel = new Button();
            syncExcelAndDB = new Button();
            SyncNamesWithDB = new Button();
            label3 = new Label();
            button2 = new Button();
            listOfInstructions = new ListBox();
            label4 = new Label();
            label5 = new Label();
            buttonSyncManualyInstrWithDB = new Button();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(34, 333);
            button1.Name = "button1";
            button1.Size = new Size(250, 54);
            button1.TabIndex = 0;
            button1.Text = "Внести новый инструткаж";
            button1.UseVisualStyleBackColor = true;
            button1.Click += buttonCreateNotification_Click;
            // 
            // datePickerEnd
            // 
            datePickerEnd.Location = new Point(34, 93);
            datePickerEnd.Name = "datePickerEnd";
            datePickerEnd.Size = new Size(250, 27);
            datePickerEnd.TabIndex = 2;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(34, 55);
            label1.Name = "label1";
            label1.Size = new Size(220, 20);
            label1.TabIndex = 3;
            label1.Text = "До какой даты включительно?";
            // 
            // InstructionTextBox
            // 
            InstructionTextBox.Location = new Point(34, 183);
            InstructionTextBox.Multiline = true;
            InstructionTextBox.Name = "InstructionTextBox";
            InstructionTextBox.Size = new Size(250, 67);
            InstructionTextBox.TabIndex = 4;
            InstructionTextBox.Text = "Работа в зоне железнодорожных путей СТО-357";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(34, 144);
            label2.Name = "label2";
            label2.Size = new Size(165, 20);
            label2.TabIndex = 5;
            label2.Text = "Название инструкции:";
            // 
            // buttonChoosePathToInstruction
            // 
            buttonChoosePathToInstruction.Location = new Point(89, 256);
            buttonChoosePathToInstruction.Name = "buttonChoosePathToInstruction";
            buttonChoosePathToInstruction.Size = new Size(138, 67);
            buttonChoosePathToInstruction.TabIndex = 6;
            buttonChoosePathToInstruction.Text = "Указать путь для инструктажа";
            buttonChoosePathToInstruction.UseVisualStyleBackColor = true;
            buttonChoosePathToInstruction.Click += buttonChoosePathToInstruction_Click;
            // 
            // checkBoxIsForDrivers
            // 
            checkBoxIsForDrivers.AutoSize = true;
            checkBoxIsForDrivers.Location = new Point(401, 23);
            checkBoxIsForDrivers.Name = "checkBoxIsForDrivers";
            checkBoxIsForDrivers.Size = new Size(192, 24);
            checkBoxIsForDrivers.TabIndex = 7;
            checkBoxIsForDrivers.Text = "Только для водителей?";
            checkBoxIsForDrivers.UseVisualStyleBackColor = true;
            // 
            // listBox1
            // 
            listBox1.FormattingEnabled = true;
            listBox1.ItemHeight = 20;
            listBox1.Location = new Point(856, 93);
            listBox1.Name = "listBox1";
            listBox1.SelectionMode = SelectionMode.MultiExtended;
            listBox1.Size = new Size(284, 104);
            listBox1.TabIndex = 8;
            // 
            // buttonTest
            // 
            buttonTest.Location = new Point(401, 256);
            buttonTest.Name = "buttonTest";
            buttonTest.Size = new Size(150, 29);
            buttonTest.TabIndex = 9;
            buttonTest.Text = "Тест сервера";
            buttonTest.UseVisualStyleBackColor = true;
            buttonTest.Click += buttonTest_Click;
            // 
            // UploadFileToServer
            // 
            UploadFileToServer.Location = new Point(401, 291);
            UploadFileToServer.Name = "UploadFileToServer";
            UploadFileToServer.Size = new Size(192, 29);
            UploadFileToServer.TabIndex = 10;
            UploadFileToServer.Text = "Upload file to server";
            UploadFileToServer.UseVisualStyleBackColor = true;
            UploadFileToServer.Click += UploadFileToServer_Click;
            // 
            // Download_file_excel
            // 
            Download_file_excel.Location = new Point(401, 361);
            Download_file_excel.Name = "Download_file_excel";
            Download_file_excel.Size = new Size(247, 29);
            Download_file_excel.TabIndex = 11;
            Download_file_excel.Text = "Download last file from server";
            Download_file_excel.UseVisualStyleBackColor = true;
            Download_file_excel.Click += Download_file_excel_Click;
            // 
            // syncExcelAndDB
            // 
            syncExcelAndDB.Location = new Point(401, 326);
            syncExcelAndDB.Name = "syncExcelAndDB";
            syncExcelAndDB.Size = new Size(219, 29);
            syncExcelAndDB.TabIndex = 12;
            syncExcelAndDB.Text = "Sync last excel file and DB";
            syncExcelAndDB.UseVisualStyleBackColor = true;
            syncExcelAndDB.Click += syncExcelAndDB_Click;
            // 
            // SyncNamesWithDB
            // 
            SyncNamesWithDB.Location = new Point(856, 48);
            SyncNamesWithDB.Name = "SyncNamesWithDB";
            SyncNamesWithDB.Size = new Size(247, 29);
            SyncNamesWithDB.TabIndex = 13;
            SyncNamesWithDB.Text = "Синхр. ФИО с Базой Данных";
            SyncNamesWithDB.UseVisualStyleBackColor = true;
            SyncNamesWithDB.Click += SyncNamesWithDB_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(856, 200);
            label3.Name = "label3";
            label3.Size = new Size(387, 20);
            label3.TabIndex = 14;
            label3.Text = "Чтобы отменить выбор - ctrl+ЛКМ(левый клик мыши)";
            // 
            // button2
            // 
            button2.Location = new Point(856, 242);
            button2.Name = "button2";
            button2.Size = new Size(247, 95);
            button2.TabIndex = 15;
            button2.Text = "Отправить выбранным людям уведомление об инструктаже";
            button2.UseVisualStyleBackColor = true;
            // 
            // listOfInstructions
            // 
            listOfInstructions.FormattingEnabled = true;
            listOfInstructions.ItemHeight = 20;
            listOfInstructions.Location = new Point(401, 93);
            listOfInstructions.Name = "listOfInstructions";
            listOfInstructions.Size = new Size(284, 104);
            listOfInstructions.TabIndex = 16;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(401, 218);
            label4.Name = "label4";
            label4.Size = new Size(156, 20);
            label4.TabIndex = 17;
            label4.Text = "Дата инструктажа до:";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(563, 218);
            label5.Name = "label5";
            label5.Size = new Size(95, 20);
            label5.TabIndex = 18;
            label5.Text = "Не выбрано";
            // 
            // buttonSyncManualyInstrwithDB
            // 
            buttonSyncManualyInstrWithDB.Location = new Point(401, 51);
            buttonSyncManualyInstrWithDB.Name = "buttonSyncManualyInstrwithDB";
            buttonSyncManualyInstrWithDB.Size = new Size(247, 29);
            buttonSyncManualyInstrWithDB.TabIndex = 19;
            buttonSyncManualyInstrWithDB.Text = "Синхр. инструктажи с Базой Данных";
            buttonSyncManualyInstrWithDB.UseVisualStyleBackColor = true;
            buttonSyncManualyInstrWithDB.Click += buttonSyncManualyInstrWithDB_Click;
            // 
            // Form2
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1265, 450);
            Controls.Add(buttonSyncManualyInstrWithDB);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(listOfInstructions);
            Controls.Add(button2);
            Controls.Add(label3);
            Controls.Add(SyncNamesWithDB);
            Controls.Add(syncExcelAndDB);
            Controls.Add(Download_file_excel);
            Controls.Add(UploadFileToServer);
            Controls.Add(buttonTest);
            Controls.Add(listBox1);
            Controls.Add(checkBoxIsForDrivers);
            Controls.Add(buttonChoosePathToInstruction);
            Controls.Add(label2);
            Controls.Add(InstructionTextBox);
            Controls.Add(label1);
            Controls.Add(datePickerEnd);
            Controls.Add(button1);
            Name = "Form2";
            Text = "Form2";
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
        private ListBox listBox1;
        private Button buttonTest;
        private Button UploadFileToServer;
        private Button Download_file_excel;
        private Button syncExcelAndDB;
        private Button SyncNamesWithDB;
        private Label label3;
        private Button button2;
        private ListBox listOfInstructions;
        private Label label4;
        private Label label5;
        private Button buttonSyncManualyInstrWithDB;
    }
}
