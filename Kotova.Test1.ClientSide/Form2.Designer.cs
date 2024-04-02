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
            checkBoxIsForDrivers.Location = new Point(479, 55);
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
            listBox1.Location = new Point(479, 109);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(284, 104);
            listBox1.TabIndex = 8;
            // 
            // Form2
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
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
    }
}
