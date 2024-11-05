namespace Kotova.Test1.ClientSide
{
    partial class UpdateProgressForm
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
            progressBar = new ProgressBar();
            labelMessage = new Label();
            SuspendLayout();
            // 
            // progressBar
            // 
            progressBar.Location = new Point(148, 250);
            progressBar.Name = "progressBar";
            progressBar.Size = new Size(514, 23);
            progressBar.TabIndex = 0;
            // 
            // labelMessage
            // 
            labelMessage.AutoSize = true;
            labelMessage.Location = new Point(148, 170);
            labelMessage.Name = "labelMessage";
            labelMessage.Size = new Size(121, 15);
            labelMessage.TabIndex = 1;
            labelMessage.Text = "Preparing to update...";
            // 
            // UpdateProgressForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(labelMessage);
            Controls.Add(progressBar);
            Name = "UpdateProgressForm";
            Text = "Обновление программы LynKS";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ProgressBar progressBar;
        private Label labelMessage;
    }
}