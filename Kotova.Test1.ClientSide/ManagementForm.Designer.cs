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
            ManagementLabel = new Label();
            label1 = new Label();
            ManagementTabControl = new TabControl();
            tabPage1 = new TabPage();
            button5 = new Button();
            button2 = new Button();
            listBox22345 = new ListBox();
            label9 = new Label();
            textBox11 = new TextBox();
            label8 = new Label();
            textBox10 = new TextBox();
            label7 = new Label();
            textBox6 = new TextBox();
            label4 = new Label();
            listBox1 = new ListBox();
            label3 = new Label();
            label2 = new Label();
            dateTimePicker2 = new DateTimePicker();
            tabPage2 = new TabPage();
            FilesOfInstructionCheckedListBox = new CheckedListBox();
            PassInstructionAsUser = new CheckBox();
            ListOfInstructionsForUser = new ListBox();
            HyperLinkForInstructionsFolder = new Button();
            label10 = new Label();
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
            ManagementTabControl.Size = new Size(1000, 424);
            ManagementTabControl.TabIndex = 30;
            ManagementTabControl.SelectedIndexChanged += ManagementTabControl_SelectedIndexChanged;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(button5);
            tabPage1.Controls.Add(button2);
            tabPage1.Controls.Add(listBox22345);
            tabPage1.Controls.Add(label9);
            tabPage1.Controls.Add(textBox11);
            tabPage1.Controls.Add(label8);
            tabPage1.Controls.Add(textBox10);
            tabPage1.Controls.Add(label7);
            tabPage1.Controls.Add(textBox6);
            tabPage1.Controls.Add(label4);
            tabPage1.Controls.Add(listBox1);
            tabPage1.Controls.Add(label3);
            tabPage1.Controls.Add(label2);
            tabPage1.Controls.Add(dateTimePicker2);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(992, 396);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Создание инструктажей";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // button5
            // 
            button5.Location = new Point(776, 72);
            button5.Name = "button5";
            button5.Size = new Size(178, 286);
            button5.TabIndex = 27;
            button5.Text = "Сформировать excel форму об инструктажах";
            button5.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            button2.Location = new Point(463, 284);
            button2.Name = "button2";
            button2.Size = new Size(214, 74);
            button2.TabIndex = 26;
            button2.Text = "Отправить инструктаж сотрудникам";
            button2.UseVisualStyleBackColor = true;
            // 
            // listBox22345
            // 
            listBox22345.FormattingEnabled = true;
            listBox22345.ItemHeight = 15;
            listBox22345.Items.AddRange(new object[] { "ФИО1 (дата рождения, должность) отдел1", "ФИО2 (дата рождения, должность) отдел1", "ФИО1 (дата рождения, должность) отдел2", "ФИО2 (дата рождения, должность) отдел2", "Хочешь - возьми что-то ещё, потому что здесь будет 400 человек и 17 отделов." });
            listBox22345.Location = new Point(270, 85);
            listBox22345.Name = "listBox22345";
            listBox22345.Size = new Size(407, 109);
            listBox22345.TabIndex = 25;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(270, 67);
            label9.Name = "label9";
            label9.Size = new Size(292, 15);
            label9.TabIndex = 24;
            label9.Text = "ОТДЕЛЫ И ФИО людей, отвечающих за инструктаж";
            // 
            // textBox11
            // 
            textBox11.Location = new Point(38, 335);
            textBox11.Name = "textBox11";
            textBox11.Size = new Size(213, 23);
            textBox11.TabIndex = 23;
            textBox11.Text = "Выбирается автоматически из БД";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(38, 317);
            label8.Name = "label8";
            label8.Size = new Size(206, 15);
            label8.TabIndex = 22;
            label8.Text = "Должность проводившего(автомат)";
            // 
            // textBox10
            // 
            textBox10.Location = new Point(38, 286);
            textBox10.Name = "textBox10";
            textBox10.Size = new Size(213, 23);
            textBox10.TabIndex = 21;
            textBox10.Text = "Выбирается из списка базы данных";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(38, 268);
            label7.Name = "label7";
            label7.Size = new Size(186, 15);
            label7.TabIndex = 20;
            label7.Text = "ФИО проводившего инструктаж";
            // 
            // textBox6
            // 
            textBox6.Location = new Point(38, 225);
            textBox6.Name = "textBox6";
            textBox6.Size = new Size(213, 23);
            textBox6.TabIndex = 19;
            textBox6.Text = "Причина инструктажа такая-то";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(38, 207);
            label4.Name = "label4";
            label4.Size = new Size(133, 15);
            label4.TabIndex = 18;
            label4.Text = "Причина инструктажа:";
            // 
            // listBox1
            // 
            listBox1.FormattingEnabled = true;
            listBox1.ItemHeight = 15;
            listBox1.Items.AddRange(new object[] { "Тех. отдел", "Общестрой" });
            listBox1.Location = new Point(38, 125);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(213, 49);
            listBox1.TabIndex = 17;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(38, 107);
            label3.Name = "label3";
            label3.Size = new Size(52, 15);
            label3.TabIndex = 16;
            label3.Text = "Отделы:";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(38, 39);
            label2.Name = "label2";
            label2.Size = new Size(239, 15);
            label2.TabIndex = 15;
            label2.Text = "Дата окончания проведения инструктажа:";
            // 
            // dateTimePicker2
            // 
            dateTimePicker2.Location = new Point(38, 72);
            dateTimePicker2.Name = "dateTimePicker2";
            dateTimePicker2.Size = new Size(201, 23);
            dateTimePicker2.TabIndex = 14;
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
            tabPage2.Size = new Size(992, 396);
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
            // ManagementForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1139, 596);
            Controls.Add(ManagementTabControl);
            Controls.Add(ManagementLabel);
            Controls.Add(label1);
            Name = "ManagementForm";
            Text = "ManagementForm";
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
        private Button button2;
        private ListBox listBox22345;
        private Label label9;
        private TextBox textBox11;
        private Label label8;
        private TextBox textBox10;
        private Label label7;
        private TextBox textBox6;
        private Label label4;
        private ListBox listBox1;
        private Label label3;
        private Label label2;
        private DateTimePicker dateTimePicker2;
        private TabPage tabPage2;
        private CheckedListBox FilesOfInstructionCheckedListBox;
        private CheckBox PassInstructionAsUser;
        private ListBox ListOfInstructionsForUser;
        private Button HyperLinkForInstructionsFolder;
        private Label label10;
    }
}