namespace Kotova.Test1.ClientSide
{
    partial class Form1
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
            checkBoxIsForDrivers = new RadioButton();
            datePickerEnd = new DateTimePicker();
            label1 = new Label();
            textBox1 = new TextBox();
            label2 = new Label();
            buttonChoosePathToInstruction = new Button();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(34, 347);
            button1.Name = "button1";
            button1.Size = new Size(250, 54);
            button1.TabIndex = 0;
            button1.Text = "Внести новый инструткаж";
            button1.UseVisualStyleBackColor = true;
            button1.Click += buttonCreateNotification_Click;
            // 
            // checkBoxIsForDrivers
            // 
            checkBoxIsForDrivers.AutoSize = true;
            checkBoxIsForDrivers.Location = new Point(597, 93);
            checkBoxIsForDrivers.Name = "checkBoxIsForDrivers";
            checkBoxIsForDrivers.Size = new Size(191, 24);
            checkBoxIsForDrivers.TabIndex = 1;
            checkBoxIsForDrivers.TabStop = true;
            checkBoxIsForDrivers.Text = "Только для водителей?";
            checkBoxIsForDrivers.UseVisualStyleBackColor = true;
            // 
            // datePickerEnd
            // 
            datePickerEnd.Location = new Point(34, 90);
            datePickerEnd.Name = "datePickerEnd";
            datePickerEnd.Size = new Size(250, 27);
            datePickerEnd.TabIndex = 2;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(34, 57);
            label1.Name = "label1";
            label1.Size = new Size(220, 20);
            label1.TabIndex = 3;
            label1.Text = "До какой даты включительно?";
            // 
            // textBox1
            // 
            textBox1.Location = new Point(538, 201);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(250, 67);
            textBox1.TabIndex = 4;
            textBox1.Text = "Работа в зоне железнодрожных путей СТО-357";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(606, 154);
            label2.Name = "label2";
            label2.Size = new Size(165, 20);
            label2.TabIndex = 5;
            label2.Text = "Название инструкции:";
            // 
            // buttonChoosePathToInstruction
            // 
            buttonChoosePathToInstruction.Location = new Point(87, 181);
            buttonChoosePathToInstruction.Name = "buttonChoosePathToInstruction";
            buttonChoosePathToInstruction.Size = new Size(138, 67);
            buttonChoosePathToInstruction.TabIndex = 6;
            buttonChoosePathToInstruction.Text = "Указать путь для инструктажа";
            buttonChoosePathToInstruction.UseVisualStyleBackColor = true;
            buttonChoosePathToInstruction.Click += buttonChoosePathToInstruction_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(buttonChoosePathToInstruction);
            Controls.Add(label2);
            Controls.Add(textBox1);
            Controls.Add(label1);
            Controls.Add(datePickerEnd);
            Controls.Add(checkBoxIsForDrivers);
            Controls.Add(button1);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private RadioButton checkBoxIsForDrivers;
        private DateTimePicker datePickerEnd;
        private Label label1;
        private TextBox textBox1;
        private Label label2;
        private Button buttonChoosePathToInstruction;
    }
}
