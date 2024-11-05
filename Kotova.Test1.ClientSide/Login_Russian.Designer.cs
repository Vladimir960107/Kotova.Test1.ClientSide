namespace Kotova.Test1.ClientSide
{
    partial class Login_Russian
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Login_Russian));
            panel1 = new Panel();
            label2 = new Label();
            label1 = new Label();
            pictureBox1 = new PictureBox();
            toolTip1 = new ToolTip(components);
            pictureBox4 = new PictureBox();
            label3 = new Label();
            panel4 = new Panel();
            PasswordTextBox = new TextBox();
            pictureBox3 = new PictureBox();
            panel3 = new Panel();
            pictureBox2 = new PictureBox();
            LoginTextBox = new TextBox();
            LogInButton = new Button();
            ForgotPasswordLabel = new LinkLabel();
            label4 = new Label();
            SupportEmail = new LinkLabel();
            RememberCredentialsCheckBox = new CheckBox();
            panel2 = new Panel();
            versionLabel = new Label();
            pictureBox5 = new PictureBox();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox4).BeginInit();
            panel4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).BeginInit();
            panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox5).BeginInit();
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
            label1.Location = new Point(44, 182);
            label1.Name = "label1";
            label1.Size = new Size(228, 174);
            label1.TabIndex = 0;
            label1.Text = "Добро пожаловать в\r LynKS \r\n(ЛинКС)\r\n- \r\nСистему Управления\r\nИнструктажей";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            label1.UseMnemonic = false;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = Properties.Resources.nanotechnology_white;
            pictureBox1.Location = new Point(88, 30);
            pictureBox1.Margin = new Padding(3, 2, 3, 2);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(141, 123);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // pictureBox4
            // 
            pictureBox4.Image = Properties.Resources.eye;
            pictureBox4.Location = new Point(46, 12);
            pictureBox4.Name = "pictureBox4";
            pictureBox4.Size = new Size(29, 29);
            pictureBox4.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox4.TabIndex = 3;
            pictureBox4.TabStop = false;
            toolTip1.SetToolTip(pictureBox4, "Вы можете посмотреть\r\nвведеный пароль кликнув здесь\r\n(на глаз)\r\n\r\n");
            pictureBox4.MouseDown += lookPassword_MouseDown;
            pictureBox4.MouseUp += lookPassword_MouseUp;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Palatino Linotype", 16.2F);
            label3.ForeColor = Color.FromArgb(41, 128, 185);
            label3.Location = new Point(42, 151);
            label3.Name = "label3";
            label3.Size = new Size(248, 29);
            label3.TabIndex = 0;
            label3.Text = "Войдите в свой аккаунт";
            // 
            // panel4
            // 
            panel4.BackColor = SystemColors.Control;
            panel4.Controls.Add(PasswordTextBox);
            panel4.Controls.Add(pictureBox4);
            panel4.Controls.Add(pictureBox3);
            panel4.Location = new Point(0, 271);
            panel4.Name = "panel4";
            panel4.Size = new Size(614, 54);
            panel4.TabIndex = 1;
            // 
            // PasswordTextBox
            // 
            PasswordTextBox.BackColor = SystemColors.Control;
            PasswordTextBox.Font = new Font("Palatino Linotype", 12F);
            PasswordTextBox.Location = new Point(82, 10);
            PasswordTextBox.Name = "PasswordTextBox";
            PasswordTextBox.Size = new Size(520, 29);
            PasswordTextBox.TabIndex = 1;
            PasswordTextBox.UseSystemPasswordChar = true;
            PasswordTextBox.Click += textBox2_Click;
            PasswordTextBox.KeyPress += textBox2_KeyPress;
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
            // panel3
            // 
            panel3.BackColor = SystemColors.Control;
            panel3.Controls.Add(pictureBox2);
            panel3.Controls.Add(LoginTextBox);
            panel3.Location = new Point(0, 211);
            panel3.Name = "panel3";
            panel3.Size = new Size(614, 54);
            panel3.TabIndex = 0;
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
            // LoginTextBox
            // 
            LoginTextBox.BackColor = SystemColors.Control;
            LoginTextBox.Font = new Font("Palatino Linotype", 12F);
            LoginTextBox.Location = new Point(82, 10);
            LoginTextBox.Name = "LoginTextBox";
            LoginTextBox.Size = new Size(520, 29);
            LoginTextBox.TabIndex = 0;
            LoginTextBox.Click += textBox1_Click;
            // 
            // LogInButton
            // 
            LogInButton.BackColor = Color.FromArgb(41, 128, 185);
            LogInButton.Font = new Font("Palatino Linotype", 12F);
            LogInButton.ForeColor = Color.White;
            LogInButton.Location = new Point(135, 344);
            LogInButton.Name = "LogInButton";
            LogInButton.Size = new Size(466, 44);
            LogInButton.TabIndex = 3;
            LogInButton.Text = "Войти";
            LogInButton.UseVisualStyleBackColor = false;
            LogInButton.Click += LogInButton_Click;
            // 
            // ForgotPasswordLabel
            // 
            ForgotPasswordLabel.AutoSize = true;
            ForgotPasswordLabel.Font = new Font("Palatino Linotype", 12F);
            ForgotPasswordLabel.LinkColor = Color.FromArgb(41, 128, 185);
            ForgotPasswordLabel.Location = new Point(412, 503);
            ForgotPasswordLabel.Name = "ForgotPasswordLabel";
            ForgotPasswordLabel.Size = new Size(126, 22);
            ForgotPasswordLabel.TabIndex = 4;
            ForgotPasswordLabel.TabStop = true;
            ForgotPasswordLabel.Text = "Забыли пароль?";
            ForgotPasswordLabel.LinkClicked += ForgotPasswordLabel_LinkClicked;
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
            // SupportEmail
            // 
            SupportEmail.AutoSize = true;
            SupportEmail.Location = new Point(6, 560);
            SupportEmail.Name = "SupportEmail";
            SupportEmail.Size = new Size(111, 15);
            SupportEmail.TabIndex = 5;
            SupportEmail.TabStop = true;
            SupportEmail.Text = "LoginovVS@rzdp.ru";
            SupportEmail.LinkClicked += SupportEmail_LinkClicked;
            // 
            // RememberCredentialsCheckBox
            // 
            RememberCredentialsCheckBox.AutoSize = true;
            RememberCredentialsCheckBox.Location = new Point(21, 359);
            RememberCredentialsCheckBox.Name = "RememberCredentialsCheckBox";
            RememberCredentialsCheckBox.Size = new Size(92, 19);
            RememberCredentialsCheckBox.TabIndex = 2;
            RememberCredentialsCheckBox.Text = "Запомнить?";
            RememberCredentialsCheckBox.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            panel2.Controls.Add(versionLabel);
            panel2.Controls.Add(pictureBox5);
            panel2.Controls.Add(RememberCredentialsCheckBox);
            panel2.Controls.Add(SupportEmail);
            panel2.Controls.Add(label4);
            panel2.Controls.Add(ForgotPasswordLabel);
            panel2.Controls.Add(LogInButton);
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
            // versionLabel
            // 
            versionLabel.AutoSize = true;
            versionLabel.Location = new Point(503, 560);
            versionLabel.Name = "versionLabel";
            versionLabel.Size = new Size(0, 15);
            versionLabel.TabIndex = 7;
            // 
            // pictureBox5
            // 
            pictureBox5.Image = Properties.Resources.setting_line_icon;
            pictureBox5.Location = new Point(549, 30);
            pictureBox5.Name = "pictureBox5";
            pictureBox5.Size = new Size(34, 36);
            pictureBox5.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox5.TabIndex = 6;
            pictureBox5.TabStop = false;
            pictureBox5.Click += pictureBox5_Click;
            // 
            // Login_Russian
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(921, 589);
            Controls.Add(panel2);
            Controls.Add(panel1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(3, 2, 3, 2);
            Name = "Login_Russian";
            Text = "Login";
            FormClosing += Login_Russian_FormClosing;
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox4).EndInit();
            panel4.ResumeLayout(false);
            panel4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).EndInit();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox5).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private PictureBox pictureBox1;
        private Label label1;
        private Label label2;
        private ToolTip toolTip1;
        private Label label3;
        private Panel panel4;
        private TextBox PasswordTextBox;
        private PictureBox pictureBox4;
        private PictureBox pictureBox3;
        private Panel panel3;
        private PictureBox pictureBox2;
        private TextBox LoginTextBox;
        private Button LogInButton;
        private LinkLabel ForgotPasswordLabel;
        private Label label4;
        private LinkLabel SupportEmail;
        private CheckBox RememberCredentialsCheckBox;
        private Panel panel2;
        private PictureBox pictureBox5;
        private Label versionLabel;
    }
}