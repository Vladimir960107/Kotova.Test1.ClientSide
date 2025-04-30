using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kotova.CommonClasses;

namespace Kotova.Test1.ClientSide
{
    public partial class EditInstructionForm : Form
    {
        // Properties to store form controls that you'll add manually
        private TextBox txtCause;
        private DateTimePicker dtpEndDate;
        private ComboBox cmbType;
        private Button btnSave;
        private Button btnCancel;

        // Property to return the edited instruction
        public Instruction InstructionResult { get; private set; }

        // The original instruction being edited
        private Instruction _originalInstruction;

        public EditInstructionForm(Instruction instruction)
        {
            _originalInstruction = instruction;

            // Set up the form properties
            Text = "Редактирование инструктажа";
            Size = new System.Drawing.Size(500, 300);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;

            // You'll manually add the controls in the designer

            // Pre-populate fields with the instruction data
            Load += EditInstructionForm_Load;
        }

        private void EditInstructionForm_Load(object sender, EventArgs e)
        {
            // This assumes you'll add these controls manually in the designer
            if (_originalInstruction != null)
            {
                txtCause.Text = _originalInstruction.cause_of_instruction;
                dtpEndDate.Value = _originalInstruction.end_date;

                // Determine the index in the combo box based on the instruction type
                // Add 2 because your types start at 2 for these regular instructions
                int typeIndex = _originalInstruction.type_of_instruction - 2;
                if (typeIndex >= 0 && typeIndex < cmbType.Items.Count)
                {
                    cmbType.SelectedIndex = typeIndex;
                }
            }
        }

        // This method will be called when the Save button is clicked
        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCause.Text))
            {
                MessageBox.Show("Введите причину инструктажа");
                return;
            }

            if (cmbType.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите тип инструктажа");
                return;
            }

            // Map selected index to type byte (adding 2 because types start at 2 for these)
            byte instructionType = (byte)(cmbType.SelectedIndex + 2);

            // Create a new instruction with updated values
            InstructionResult = new Instruction(
                txtCause.Text,
                _originalInstruction.begin_date, // Keep the original begin date
                dtpEndDate.Value,
                _originalInstruction.path_to_instruction, // Keep the original path
                instructionType
            );

            // Set the ID to match the original instruction
            InstructionResult.instruction_id = _originalInstruction.instruction_id;

            // Preserve other properties that shouldn't change
            InstructionResult.is_assigned_to_people = _originalInstruction.is_assigned_to_people;
            InstructionResult.is_passed_by_everyone = _originalInstruction.is_passed_by_everyone;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
