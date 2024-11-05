namespace Kotova.Test1.ClientSide
{
    partial class SignUpForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SignUpForm));
            panel1 = new Panel();
            label2 = new Label();
            label1 = new Label();
            pictureBox1 = new PictureBox();
            panel2 = new Panel();
            panel6 = new Panel();
            showPasswordPicture = new PictureBox();
            PasswordTextBox = new TextBox();
            pictureBox6 = new PictureBox();
            panel5 = new Panel();
            pictureBox7 = new PictureBox();
            emailTextBox = new TextBox();
            skipButton = new Button();
            SupportEmail = new LinkLabel();
            label4 = new Label();
            signUpButton = new Button();
            panel3 = new Panel();
            loginTextBox = new TextBox();
            pictureBox2 = new PictureBox();
            panel4 = new Panel();
            showRepeatedPasswordPicture = new PictureBox();
            RepeatPasswordTextBox = new TextBox();
            pictureBox3 = new PictureBox();
            label3 = new Label();
            toolTip1 = new ToolTip(components);
            toolTip2 = new ToolTip(components);
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            panel2.SuspendLayout();
            panel6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)showPasswordPicture).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox6).BeginInit();
            panel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox7).BeginInit();
            panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            panel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)showRepeatedPasswordPicture).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).BeginInit();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = Color.FromArgb(41, 128, 185);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(pictureBox1);
            panel1.Dock = DockStyle.Left;
            panel1.Location = new Point(0, 0);
            panel1.Margin = new Padding(3, 2, 3, 2);
            panel1.Name = "panel1";
            panel1.Size = new Size(307, 589);
            panel1.TabIndex = 0;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Palatino Linotype", 9F);
            label2.ForeColor = Color.White;
            label2.Location = new Point(28, 507);
            label2.Name = "label2";
            label2.Size = new Size(273, 51);
            label2.TabIndex = 1;
            label2.Text = "Разработал Логинов Владимир\r\n(Отдельная благодарность за пример дизайна:\r\nCode Galaxy T e c h)\r\n";
            label2.TextAlign = ContentAlignment.BottomRight;
            label2.UseMnemonic = false;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Palatino Linotype", 16.2F);
            label1.ForeColor = Color.White;
            label1.Location = new Point(32, 186);
            label1.Name = "label1";
            label1.Size = new Size(247, 87);
            label1.TabIndex = 0;
            label1.Text = "Добро пожаловать\r\nВ Систему Управления\r\nИнструктажей";
            label1.TextAlign = ContentAlignment.TopRight;
            label1.UseMnemonic = false;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Properties.Resources.nanotechnology_white;
            pictureBox1.Location = new Point(111, 57);
            pictureBox1.Margin = new Padding(3, 2, 3, 2);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(141, 123);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // panel2
            // 
            panel2.Controls.Add(panel6);
            panel2.Controls.Add(panel5);
            panel2.Controls.Add(skipButton);
            panel2.Controls.Add(SupportEmail);
            panel2.Controls.Add(label4);
            panel2.Controls.Add(signUpButton);
            panel2.Controls.Add(panel3);
            panel2.Controls.Add(panel4);
            panel2.Controls.Add(label3);
            panel2.Dock = DockStyle.Fill;
            panel2.Location = new Point(307, 0);
            panel2.Margin = new Padding(3, 2, 3, 2);
            panel2.Name = "panel2";
            panel2.Size = new Size(614, 589);
            panel2.TabIndex = 1;
            // 
            // panel6
            // 
            panel6.BackColor = SystemColors.Control;
            panel6.Controls.Add(showPasswordPicture);
            panel6.Controls.Add(PasswordTextBox);
            panel6.Controls.Add(pictureBox6);
            panel6.Location = new Point(6, 186);
            panel6.Name = "panel6";
            panel6.Size = new Size(614, 54);
            panel6.TabIndex = 9;
            // 
            // showPasswordPicture
            // 
            showPasswordPicture.Image = Properties.Resources.eye;
            showPasswordPicture.Location = new Point(46, 12);
            showPasswordPicture.Name = "showPasswordPicture";
            showPasswordPicture.Size = new Size(29, 29);
            showPasswordPicture.SizeMode = PictureBoxSizeMode.Zoom;
            showPasswordPicture.TabIndex = 3;
            showPasswordPicture.TabStop = false;
            toolTip1.SetToolTip(showPasswordPicture, "Вы можете посмотреть новый \r\nвведеный пароль кликнув здесь\r\n(на глаз)\r\n\r\n");
            showPasswordPicture.MouseDown += showPasswordPicture_MouseDown;
            showPasswordPicture.MouseUp += showPasswordPicture_MouseUp;
            // 
            // PasswordTextBox
            // 
            PasswordTextBox.BackColor = SystemColors.Control;
            PasswordTextBox.Font = new Font("Palatino Linotype", 12F);
            PasswordTextBox.Location = new Point(82, 12);
            PasswordTextBox.Name = "PasswordTextBox";
            PasswordTextBox.Size = new Size(520, 29);
            PasswordTextBox.TabIndex = 2;
            PasswordTextBox.Text = "Введите новый пароль";
            PasswordTextBox.Click += PasswordTextBox_Click;
            PasswordTextBox.TextChanged += PasswordTextBox_TextChanged;
            PasswordTextBox.DoubleClick += PasswordTextBox_DoubleClick;
            // 
            // pictureBox6
            // 
            pictureBox6.BackColor = SystemColors.Control;
            pictureBox6.Image = Properties.Resources.output_onlinepngtools;
            pictureBox6.InitialImage = null;
            pictureBox6.Location = new Point(5, 10);
            pictureBox6.Name = "pictureBox6";
            pictureBox6.Size = new Size(35, 33);
            pictureBox6.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox6.TabIndex = 1;
            pictureBox6.TabStop = false;
            // 
            // panel5
            // 
            panel5.BackColor = SystemColors.Control;
            panel5.Controls.Add(pictureBox7);
            panel5.Controls.Add(emailTextBox);
            panel5.Location = new Point(6, 306);
            panel5.Name = "panel5";
            panel5.Size = new Size(614, 48);
            panel5.TabIndex = 8;
            // 
            // pictureBox7
            // 
            pictureBox7.BackColor = SystemColors.Control;
            pictureBox7.Image = (Image)resources.GetObject("pictureBox7.Image");
            pictureBox7.InitialImage = null;
            pictureBox7.Location = new Point(6, 6);
            pictureBox7.Name = "pictureBox7";
            pictureBox7.Size = new Size(35, 33);
            pictureBox7.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox7.TabIndex = 3;
            pictureBox7.TabStop = false;
            // 
            // emailTextBox
            // 
            emailTextBox.BackColor = SystemColors.Control;
            emailTextBox.Font = new Font("Palatino Linotype", 12F);
            emailTextBox.Location = new Point(82, 12);
            emailTextBox.Name = "emailTextBox";
            emailTextBox.Size = new Size(520, 29);
            emailTextBox.TabIndex = 2;
            emailTextBox.Text = "Введите почту (Необязательно)";
            emailTextBox.Click += emailTextBox_Click;
            emailTextBox.DoubleClick += emailTextBox_DoubleClick;
            // 
            // skipButton
            // 
            skipButton.BackColor = Color.White;
            skipButton.Font = new Font("Palatino Linotype", 12F);
            skipButton.ForeColor = Color.Black;
            skipButton.Location = new Point(82, 444);
            skipButton.Name = "skipButton";
            skipButton.Size = new Size(520, 44);
            skipButton.TabIndex = 7;
            skipButton.Text = "Пропустить Смену (Не рекомендуется)";
            skipButton.UseVisualStyleBackColor = false;
            skipButton.Click += skipButton_Click;
            // 
            // SupportEmail
            // 
            SupportEmail.AutoSize = true;
            SupportEmail.Location = new Point(6, 560);
            SupportEmail.Name = "SupportEmail";
            SupportEmail.Size = new Size(199, 15);
            SupportEmail.TabIndex = 6;
            SupportEmail.TabStop = true;
            SupportEmail.Text = "loginovvladimirforwork@gmail.com";
            SupportEmail.LinkClicked += SupportEmail_LinkClicked;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Palatino Linotype", 9F);
            label4.ForeColor = SystemColors.ControlDark;
            label4.Location = new Point(6, 507);
            label4.Name = "label4";
            label4.Size = new Size(304, 85);
            label4.TabIndex = 2;
            label4.Text = "Если у вас есть какие-то вопросы по данному\r\nприложению - отправьте свой вопрос/предложение\r\nчерез указанную ниже почту:\r\n\r\n ";
            // 
            // signUpButton
            // 
            signUpButton.BackColor = Color.FromArgb(41, 128, 185);
            signUpButton.Font = new Font("Palatino Linotype", 12F);
            signUpButton.ForeColor = Color.White;
            signUpButton.Location = new Point(82, 380);
            signUpButton.Name = "signUpButton";
            signUpButton.Size = new Size(520, 44);
            signUpButton.TabIndex = 4;
            signUpButton.Text = "Зарегестрировать новый Логин и Пароль";
            signUpButton.UseVisualStyleBackColor = false;
            signUpButton.Click += signUpButton_Click;
            // 
            // panel3
            // 
            panel3.BackColor = SystemColors.Control;
            panel3.Controls.Add(loginTextBox);
            panel3.Controls.Add(pictureBox2);
            panel3.Location = new Point(6, 126);
            panel3.Name = "panel3";
            panel3.Size = new Size(614, 54);
            panel3.TabIndex = 3;
            // 
            // loginTextBox
            // 
            loginTextBox.BackColor = SystemColors.Control;
            loginTextBox.Font = new Font("Palatino Linotype", 12F);
            loginTextBox.ForeColor = Color.FromArgb(0, 0, 0, 6);
            loginTextBox.Location = new Point(82, 12);
            loginTextBox.Name = "loginTextBox";
            loginTextBox.Size = new Size(520, 29);
            loginTextBox.TabIndex = 1;
            loginTextBox.Text = "Введите новый логин";
            loginTextBox.Click += loginTextBox_Click;
            loginTextBox.DoubleClick += loginTextBox_DoubleClick;
            // 
            // pictureBox2
            // 
            pictureBox2.BackColor = SystemColors.Control;
            pictureBox2.Image = Properties.Resources.user;
            pictureBox2.InitialImage = null;
            pictureBox2.Location = new Point(5, 10);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(35, 33);
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.TabIndex = 0;
            pictureBox2.TabStop = false;
            // 
            // panel4
            // 
            panel4.BackColor = SystemColors.Control;
            panel4.Controls.Add(showRepeatedPasswordPicture);
            panel4.Controls.Add(RepeatPasswordTextBox);
            panel4.Controls.Add(pictureBox3);
            panel4.Location = new Point(6, 246);
            panel4.Name = "panel4";
            panel4.Size = new Size(614, 54);
            panel4.TabIndex = 2;
            // 
            // showRepeatedPasswordPicture
            // 
            showRepeatedPasswordPicture.Image = Properties.Resources.eye;
            showRepeatedPasswordPicture.Location = new Point(47, 12);
            showRepeatedPasswordPicture.Name = "showRepeatedPasswordPicture";
            showRepeatedPasswordPicture.Size = new Size(29, 29);
            showRepeatedPasswordPicture.SizeMode = PictureBoxSizeMode.Zoom;
            showRepeatedPasswordPicture.TabIndex = 4;
            showRepeatedPasswordPicture.TabStop = false;
            toolTip2.SetToolTip(showRepeatedPasswordPicture, "Введите пароль ещё раз и кликните на глаз для его просмотра");
            showRepeatedPasswordPicture.MouseDown += showrepeatedPasswordPicture_MouseDown;
            showRepeatedPasswordPicture.MouseUp += showRepeatedPasswordPicture_MouseUp;
            // 
            // RepeatPasswordTextBox
            // 
            RepeatPasswordTextBox.BackColor = SystemColors.Control;
            RepeatPasswordTextBox.Font = new Font("Palatino Linotype", 12F);
            RepeatPasswordTextBox.Location = new Point(82, 12);
            RepeatPasswordTextBox.Name = "RepeatPasswordTextBox";
            RepeatPasswordTextBox.Size = new Size(520, 29);
            RepeatPasswordTextBox.TabIndex = 2;
            RepeatPasswordTextBox.Text = "Повторите новый пароль";
            RepeatPasswordTextBox.Click += RepeatPasswordTextBox_Click;
            RepeatPasswordTextBox.TextChanged += RepeatPassswordTextBox_TextChanged;
            RepeatPasswordTextBox.DoubleClick += RepeatPassswordTextBox_DoubleClick;
            // 
            // pictureBox3
            // 
            pictureBox3.BackColor = SystemColors.Control;
            pictureBox3.Image = Properties.Resources.output_onlinepngtools;
            pictureBox3.InitialImage = null;
            pictureBox3.Location = new Point(5, 10);
            pictureBox3.Name = "pictureBox3";
            pictureBox3.Size = new Size(35, 33);
            pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox3.TabIndex = 1;
            pictureBox3.TabStop = false;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Palatino Linotype", 16.2F);
            label3.ForeColor = Color.FromArgb(41, 128, 185);
            label3.Location = new Point(46, 25);
            label3.Name = "label3";
            label3.Size = new Size(252, 29);
            label3.TabIndex = 0;
            label3.Text = "Смена логина и пароля";
            // 
            // SignUpForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(921, 589);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Margin = new Padding(3, 2, 3, 2);
            Name = "SignUpForm";
            Text = "Login";
            FormClosing += SignUpForm_FormClosing;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            panel6.ResumeLayout(false);
            panel6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)showPasswordPicture).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox6).EndInit();
            panel5.ResumeLayout(false);
            panel5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox7).EndInit();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            panel4.ResumeLayout(false);
            panel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)showRepeatedPasswordPicture).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Panel panel2;
        private PictureBox pictureBox1;
        private Label label1;
        private Label label2;
        private Label label3;
        private Panel panel3;
        private Panel panel4;
        private PictureBox pictureBox2;
        private PictureBox pictureBox3;
        private TextBox loginTextBox;
        private TextBox RepeatPasswordTextBox;
        private Button signUpButton;
        private Label label4;
        private LinkLabel SupportEmail;
        private ToolTip toolTip1;
        private Button skipButton;
        private Panel panel5;
        private TextBox emailTextBox;
        private Panel panel6;
        private PictureBox showPasswordPicture;
        private TextBox PasswordTextBox;
        private PictureBox pictureBox6;
        private PictureBox pictureBox7;
        private PictureBox showRepeatedPasswordPicture;
        private ToolTip toolTip2;
    }
}