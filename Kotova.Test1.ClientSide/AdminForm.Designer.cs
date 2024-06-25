namespace Kotova.Test1.ClientSide
{
    partial class AdminForm
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
            panel2 = new Panel();
            button9 = new Button();
            button10 = new Button();
            DepartmentsComboBox1 = new ComboBox();
            label1 = new Label();
            ListOfInstructions = new ListBox();
            buttonSyncManualyInstrWithDB = new Button();
            label5 = new Label();
            label3 = new Label();
            ListBoxNamesOfPeople = new ListBox();
            SyncNamesWithDB = new Button();
            label4 = new Label();
            checkBoxIsForDrivers = new CheckBox();
            button1 = new Button();
            label2 = new Label();
            Download_file_excel = new Button();
            label6 = new Label();
            label7 = new Label();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            tabPage2 = new TabPage();
            panel2.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            SuspendLayout();
            // 
            // panel2
            // 
            panel2.Controls.Add(label7);
            panel2.Controls.Add(label6);
            panel2.Controls.Add(Download_file_excel);
            panel2.Controls.Add(label2);
            panel2.Controls.Add(button9);
            panel2.Controls.Add(button10);
            panel2.Location = new Point(41, 292);
            panel2.Name = "panel2";
            panel2.Size = new Size(766, 143);
            panel2.TabIndex = 25;
            // 
            // button9
            // 
            button9.Location = new Point(17, 75);
            button9.Name = "button9";
            button9.Size = new Size(192, 53);
            button9.TabIndex = 12;
            button9.Text = "Синхронизация последнего файла с сервером";
            button9.UseVisualStyleBackColor = true;
            // 
            // button10
            // 
            button10.Location = new Point(17, 34);
            button10.Name = "button10";
            button10.Size = new Size(192, 35);
            button10.TabIndex = 11;
            button10.Text = "Загрузка excel на сервер";
            button10.UseVisualStyleBackColor = true;
            // 
            // DepartmentsComboBox1
            // 
            DepartmentsComboBox1.FormattingEnabled = true;
            DepartmentsComboBox1.Location = new Point(40, 61);
            DepartmentsComboBox1.Name = "DepartmentsComboBox1";
            DepartmentsComboBox1.Size = new Size(219, 23);
            DepartmentsComboBox1.TabIndex = 26;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(40, 27);
            label1.Name = "label1";
            label1.Size = new Size(100, 15);
            label1.TabIndex = 27;
            label1.Text = "Выберите Отдел:";
            // 
            // ListOfInstructions
            // 
            ListOfInstructions.FormattingEnabled = true;
            ListOfInstructions.ItemHeight = 15;
            ListOfInstructions.Location = new Point(41, 149);
            ListOfInstructions.Name = "ListOfInstructions";
            ListOfInstructions.Size = new Size(232, 79);
            ListOfInstructions.TabIndex = 42;
            // 
            // buttonSyncManualyInstrWithDB
            // 
            buttonSyncManualyInstrWithDB.Location = new Point(41, 113);
            buttonSyncManualyInstrWithDB.Margin = new Padding(3, 2, 3, 2);
            buttonSyncManualyInstrWithDB.Name = "buttonSyncManualyInstrWithDB";
            buttonSyncManualyInstrWithDB.Size = new Size(216, 22);
            buttonSyncManualyInstrWithDB.TabIndex = 41;
            buttonSyncManualyInstrWithDB.Text = "Синхр. инструктажи с Базой Данных";
            buttonSyncManualyInstrWithDB.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(171, 234);
            label5.Name = "label5";
            label5.Size = new Size(74, 15);
            label5.TabIndex = 40;
            label5.Text = "Не выбрано";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(312, 234);
            label3.Name = "label3";
            label3.Size = new Size(310, 15);
            label3.TabIndex = 38;
            label3.Text = "Чтобы отменить выбор - ctrl+ЛКМ(левый клик мыши)";
            // 
            // ListBoxNamesOfPeople
            // 
            ListBoxNamesOfPeople.FormattingEnabled = true;
            ListBoxNamesOfPeople.ItemHeight = 15;
            ListBoxNamesOfPeople.Items.AddRange(new object[] { "Список ФИО" });
            ListBoxNamesOfPeople.Location = new Point(312, 149);
            ListBoxNamesOfPeople.Margin = new Padding(3, 2, 3, 2);
            ListBoxNamesOfPeople.Name = "ListBoxNamesOfPeople";
            ListBoxNamesOfPeople.SelectionMode = SelectionMode.MultiExtended;
            ListBoxNamesOfPeople.Size = new Size(346, 79);
            ListBoxNamesOfPeople.TabIndex = 35;
            // 
            // SyncNamesWithDB
            // 
            SyncNamesWithDB.Location = new Point(371, 123);
            SyncNamesWithDB.Margin = new Padding(3, 2, 3, 2);
            SyncNamesWithDB.Name = "SyncNamesWithDB";
            SyncNamesWithDB.Size = new Size(216, 22);
            SyncNamesWithDB.TabIndex = 37;
            SyncNamesWithDB.Text = "Синхр. ФИО с Базой Данных";
            SyncNamesWithDB.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(41, 234);
            label4.Name = "label4";
            label4.Size = new Size(124, 15);
            label4.TabIndex = 39;
            label4.Text = "Дата инструктажа до:";
            // 
            // checkBoxIsForDrivers
            // 
            checkBoxIsForDrivers.AutoSize = true;
            checkBoxIsForDrivers.Location = new Point(396, 97);
            checkBoxIsForDrivers.Margin = new Padding(3, 2, 3, 2);
            checkBoxIsForDrivers.Name = "checkBoxIsForDrivers";
            checkBoxIsForDrivers.Size = new Size(152, 19);
            checkBoxIsForDrivers.TabIndex = 34;
            checkBoxIsForDrivers.Text = "Только для водителей?";
            checkBoxIsForDrivers.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            button1.Location = new Point(675, 41);
            button1.Name = "button1";
            button1.Size = new Size(120, 43);
            button1.TabIndex = 43;
            button1.Text = "Выйти из учётной записи";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(345, 9);
            label2.Name = "label2";
            label2.Size = new Size(66, 15);
            label2.TabIndex = 13;
            label2.Text = "Excel файл";
            // 
            // Download_file_excel
            // 
            Download_file_excel.Location = new Point(538, 34);
            Download_file_excel.Margin = new Padding(3, 2, 3, 2);
            Download_file_excel.Name = "Download_file_excel";
            Download_file_excel.Size = new Size(216, 94);
            Download_file_excel.TabIndex = 25;
            Download_file_excel.Text = "Скачать Excel выписку для отдела";
            Download_file_excel.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(51, 9);
            label6.Name = "label6";
            label6.Size = new Size(126, 15);
            label6.TabIndex = 26;
            label6.Text = "Загрузить (на сервер)";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(617, 9);
            label7.Name = "label7";
            label7.Size = new Size(51, 15);
            label7.TabIndex = 27;
            label7.Text = "Скачать";
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Location = new Point(25, 23);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(881, 496);
            tabControl1.TabIndex = 44;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(button1);
            tabPage1.Controls.Add(panel2);
            tabPage1.Controls.Add(ListOfInstructions);
            tabPage1.Controls.Add(DepartmentsComboBox1);
            tabPage1.Controls.Add(buttonSyncManualyInstrWithDB);
            tabPage1.Controls.Add(label1);
            tabPage1.Controls.Add(label5);
            tabPage1.Controls.Add(checkBoxIsForDrivers);
            tabPage1.Controls.Add(label3);
            tabPage1.Controls.Add(label4);
            tabPage1.Controls.Add(ListBoxNamesOfPeople);
            tabPage1.Controls.Add(SyncNamesWithDB);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(873, 468);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "tabPage1";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(873, 468);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "tabPage2";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // AdminForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(950, 554);
            Controls.Add(tabControl1);
            Name = "AdminForm";
            Text = "AdminForm";
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private Panel panel2;
        private Button button9;
        private Button button10;
        private ComboBox DepartmentsComboBox1;
        private Label label1;
        private ListBox ListOfInstructions;
        private Button buttonSyncManualyInstrWithDB;
        private Label label5;
        private Label label3;
        private ListBox ListBoxNamesOfPeople;
        private Button SyncNamesWithDB;
        private Label label4;
        private CheckBox checkBoxIsForDrivers;
        private Button button1;
        private Label label2;
        private Button Download_file_excel;
        private Label label7;
        private Label label6;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
    }
}