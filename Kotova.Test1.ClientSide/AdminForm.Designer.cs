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
            label7 = new Label();
            label6 = new Label();
            Download_file_excel = new Button();
            label2 = new Label();
            button9 = new Button();
            button10 = new Button();
            DepartmentsForUsersComboBox = new ComboBox();
            label1 = new Label();
            button1 = new Button();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            tabPage2 = new TabPage();
            MessageTextBox = new TextBox();
            SendMessageToEveryoneButton = new Button();
            tabPage3 = new TabPage();
            label10 = new Label();
            label9 = new Label();
            textBox1 = new TextBox();
            DueDateTimePicker = new DateTimePicker();
            label8 = new Label();
            label25 = new Label();
            RoleOfNewcomerListBox = new ListBox();
            label17 = new Label();
            DepartmentForNewcomer = new ListBox();
            CustomTaskDescriptionTextBox = new TextBox();
            PostCustomTaskButton = new Button();
            panel2.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            tabPage3.SuspendLayout();
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
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(617, 9);
            label7.Name = "label7";
            label7.Size = new Size(51, 15);
            label7.TabIndex = 27;
            label7.Text = "Скачать";
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
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(345, 9);
            label2.Name = "label2";
            label2.Size = new Size(66, 15);
            label2.TabIndex = 13;
            label2.Text = "Excel файл";
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
            // DepartmentsForUsersComboBox
            // 
            DepartmentsForUsersComboBox.FormattingEnabled = true;
            DepartmentsForUsersComboBox.Location = new Point(41, 61);
            DepartmentsForUsersComboBox.Name = "DepartmentsForUsersComboBox";
            DepartmentsForUsersComboBox.Size = new Size(219, 23);
            DepartmentsForUsersComboBox.TabIndex = 26;
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
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Controls.Add(tabPage3);
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
            tabPage1.Controls.Add(DepartmentsForUsersComboBox);
            tabPage1.Controls.Add(label1);
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
            tabPage2.Controls.Add(MessageTextBox);
            tabPage2.Controls.Add(SendMessageToEveryoneButton);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(873, 468);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "tabPage2";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // MessageTextBox
            // 
            MessageTextBox.Location = new Point(20, 29);
            MessageTextBox.Multiline = true;
            MessageTextBox.Name = "MessageTextBox";
            MessageTextBox.Size = new Size(503, 118);
            MessageTextBox.TabIndex = 1;
            // 
            // SendMessageToEveryoneButton
            // 
            SendMessageToEveryoneButton.Location = new Point(688, 51);
            SendMessageToEveryoneButton.Name = "SendMessageToEveryoneButton";
            SendMessageToEveryoneButton.Size = new Size(125, 63);
            SendMessageToEveryoneButton.TabIndex = 0;
            SendMessageToEveryoneButton.Text = "Отправить всем сообщение";
            SendMessageToEveryoneButton.UseVisualStyleBackColor = true;
            SendMessageToEveryoneButton.Click += SendMessageToEveryoneButton_Click;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(label10);
            tabPage3.Controls.Add(label9);
            tabPage3.Controls.Add(textBox1);
            tabPage3.Controls.Add(DueDateTimePicker);
            tabPage3.Controls.Add(label8);
            tabPage3.Controls.Add(label25);
            tabPage3.Controls.Add(RoleOfNewcomerListBox);
            tabPage3.Controls.Add(label17);
            tabPage3.Controls.Add(DepartmentForNewcomer);
            tabPage3.Controls.Add(CustomTaskDescriptionTextBox);
            tabPage3.Controls.Add(PostCustomTaskButton);
            tabPage3.Location = new Point(4, 24);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new Padding(3);
            tabPage3.Size = new Size(873, 468);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Создание кастомных заданий";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(430, 220);
            label10.Name = "label10";
            label10.Size = new Size(42, 15);
            label10.TabIndex = 36;
            label10.Text = "Status:";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(430, 108);
            label9.Name = "label9";
            label9.Size = new Size(58, 15);
            label9.TabIndex = 35;
            label9.Text = "Due Date:";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(430, 252);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(309, 23);
            textBox1.TabIndex = 34;
            // 
            // DueDateTimePicker
            // 
            DueDateTimePicker.Location = new Point(430, 147);
            DueDateTimePicker.Name = "DueDateTimePicker";
            DueDateTimePicker.Size = new Size(160, 23);
            DueDateTimePicker.TabIndex = 33;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(24, 23);
            label8.Name = "label8";
            label8.Size = new Size(111, 15);
            label8.TabIndex = 32;
            label8.Text = "Описание задания:";
            // 
            // label25
            // 
            label25.AutoSize = true;
            label25.Location = new Point(24, 297);
            label25.Name = "label25";
            label25.Size = new Size(153, 15);
            label25.TabIndex = 31;
            label25.Text = "Выбрать роль сотрудника:";
            // 
            // RoleOfNewcomerListBox
            // 
            RoleOfNewcomerListBox.FormattingEnabled = true;
            RoleOfNewcomerListBox.ItemHeight = 15;
            RoleOfNewcomerListBox.Location = new Point(24, 315);
            RoleOfNewcomerListBox.Name = "RoleOfNewcomerListBox";
            RoleOfNewcomerListBox.Size = new Size(281, 64);
            RoleOfNewcomerListBox.TabIndex = 30;
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Location = new Point(24, 183);
            label17.Name = "label17";
            label17.Size = new Size(43, 15);
            label17.TabIndex = 16;
            label17.Text = "Отдел:";
            // 
            // DepartmentForNewcomer
            // 
            DepartmentForNewcomer.FormattingEnabled = true;
            DepartmentForNewcomer.ItemHeight = 15;
            DepartmentForNewcomer.Location = new Point(24, 201);
            DepartmentForNewcomer.Name = "DepartmentForNewcomer";
            DepartmentForNewcomer.Size = new Size(209, 64);
            DepartmentForNewcomer.TabIndex = 15;
            // 
            // CustomTaskDescriptionTextBox
            // 
            CustomTaskDescriptionTextBox.Location = new Point(24, 52);
            CustomTaskDescriptionTextBox.Multiline = true;
            CustomTaskDescriptionTextBox.Name = "CustomTaskDescriptionTextBox";
            CustomTaskDescriptionTextBox.Size = new Size(306, 118);
            CustomTaskDescriptionTextBox.TabIndex = 2;
            // 
            // PostCustomTaskButton
            // 
            PostCustomTaskButton.Location = new Point(627, 339);
            PostCustomTaskButton.Name = "PostCustomTaskButton";
            PostCustomTaskButton.Size = new Size(168, 53);
            PostCustomTaskButton.TabIndex = 0;
            PostCustomTaskButton.Text = "Опубликовать кастомное задание";
            PostCustomTaskButton.UseVisualStyleBackColor = true;
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
            tabPage2.ResumeLayout(false);
            tabPage2.PerformLayout();
            tabPage3.ResumeLayout(false);
            tabPage3.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private Panel panel2;
        private Button button9;
        private Button button10;
        private ComboBox DepartmentsForUsersComboBox;
        private Label label1;
        private Button button1;
        private Label label2;
        private Button Download_file_excel;
        private Label label7;
        private Label label6;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private TextBox MessageTextBox;
        private Button SendMessageToEveryoneButton;
        private TabPage tabPage3;
        private Button PostCustomTaskButton;
        private TextBox CustomTaskDescriptionTextBox;
        private ListBox DepartmentForNewcomer;
        private Label label17;
        private Label label8;
        private Label label25;
        private ListBox RoleOfNewcomerListBox;
        private DateTimePicker DueDateTimePicker;
        private Label label10;
        private Label label9;
        private TextBox textBox1;
    }
}