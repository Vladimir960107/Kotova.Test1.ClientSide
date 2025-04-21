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
        private WinForms.Button btnCancel;

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
