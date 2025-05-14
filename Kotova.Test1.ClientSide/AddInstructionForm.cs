using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinForms = System.Windows.Forms;
using Kotova.CommonClasses;

namespace Kotova.Test1.ClientSide
{
    public class AddInstructionForm : WinForms.Form
    {
        private WinForms.TextBox txtCause;
        private WinForms.DateTimePicker dtpEndDate;
        private WinForms.ComboBox cmbType;
        private WinForms.Button btnSave;
        private Button btnCancel;

        public Instruction InstructionResult { get; private set; }

        public AddInstructionForm()
        {
            InitializeComponent();

            // Populate instruction types
            cmbType.Items.Add("Первичный");
            cmbType.Items.Add("Повторный");
            cmbType.Items.Add("Повторный (для водителей)");
            cmbType.Items.Add("Целевой");

            // Set default values
            dtpEndDate.Value = DateTime.Now.AddMonths(1);
        }

        private void InitializeComponent()
        {
            txtCause = new TextBox();
            dtpEndDate = new DateTimePicker();
            cmbType = new ComboBox();
            btnSave = new Button();
            btnCancel = new Button();
            SuspendLayout();
            // 
            // txtCause
            // 
            txtCause.Location = new Point(62, 46);
            txtCause.Name = "txtCause";
            txtCause.Size = new Size(223, 23);
            txtCause.TabIndex = 0;
            // 
            // dtpEndDate
            // 
            dtpEndDate.Location = new Point(62, 115);
            dtpEndDate.Name = "dtpEndDate";
            dtpEndDate.Size = new Size(223, 23);
            dtpEndDate.TabIndex = 1;
            // 
            // cmbType
            // 
            cmbType.FormattingEnabled = true;
            cmbType.Location = new Point(62, 196);
            cmbType.Name = "cmbType";
            cmbType.Size = new Size(223, 23);
            cmbType.TabIndex = 2;
            // 
            // btnSave
            // 
            btnSave.Location = new Point(62, 271);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(75, 23);
            btnSave.TabIndex = 3;
            btnSave.Text = "Сохранить";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(210, 271);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 4;
            btnCancel.Text = "btnCancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // AddInstructionForm
            // 
            ClientSize = new Size(804, 363);
            Controls.Add(btnCancel);
            Controls.Add(btnSave);
            Controls.Add(cmbType);
            Controls.Add(dtpEndDate);
            Controls.Add(txtCause);
            Name = "AddInstructionForm";
            ResumeLayout(false);
            PerformLayout();
            // Designer code
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCause.Text))
            {
                WinForms.MessageBox.Show("Введите причину инструктажа");
                return;
            }

            if (cmbType.SelectedIndex == -1)
            {
                WinForms.MessageBox.Show("Выберите тип инструктажа");
                return;
            }

            // Map selected index to type byte (adding 2 because types start at 2 for these)
            byte instructionType = (byte)(cmbType.SelectedIndex + 2);

            InstructionResult = new Instruction(
                txtCause.Text,
                DateTime.Now,
                dtpEndDate.Value,
                null, // No path
                instructionType
            );

            DialogResult = WinForms.DialogResult.OK;
            Close();
        }
    }

    // Similar EditInstructionForm with pre-populated fields
}
