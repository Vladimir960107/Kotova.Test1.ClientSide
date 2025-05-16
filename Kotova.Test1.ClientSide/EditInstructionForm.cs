using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kotova.CommonClasses;
using Newtonsoft.Json;

namespace Kotova.Test1.ClientSide
{
    public class EditInstructionForm : Form
    {
        // Properties to store form controls
        private Label lblCause;
        private TextBox txtCause;
        private Label lblEndDate;
        private DateTimePicker dtpEndDate;
        private Label lblType;
        private ComboBox cmbType;
        private Button btnSave;
        private Button btnCancel;

        // Property to return the edited instruction
        public Instruction InstructionResult { get; private set; }

        // The original instruction being edited
        private Instruction _originalInstruction;

        // JWT token for authorization
        private string _jwtToken;

        public EditInstructionForm(Instruction instruction, string jwtToken = null)
        {
            _originalInstruction = instruction;
            _jwtToken = jwtToken;

            InitializeComponent();

            // Pre-populate fields with the instruction data
            Load += EditInstructionForm_Load;
        }

        private void InitializeComponent()
        {
            this.lblCause = new Label();
            this.txtCause = new TextBox();
            this.lblEndDate = new Label();
            this.dtpEndDate = new DateTimePicker();
            this.lblType = new Label();
            this.cmbType = new ComboBox();
            this.btnSave = new Button();
            this.btnCancel = new Button();
            this.SuspendLayout();

            // lblCause
            this.lblCause.AutoSize = true;
            this.lblCause.Location = new System.Drawing.Point(12, 28);
            this.lblCause.Name = "lblCause";
            this.lblCause.Size = new System.Drawing.Size(133, 15);
            this.lblCause.TabIndex = 0;
            this.lblCause.Text = "Причина инструктажа:";

            // txtCause
            this.txtCause.Location = new System.Drawing.Point(12, 46);
            this.txtCause.Multiline = true;
            this.txtCause.Name = "txtCause";
            this.txtCause.Size = new System.Drawing.Size(320, 50);
            this.txtCause.TabIndex = 1;

            // lblEndDate
            this.lblEndDate.AutoSize = true;
            this.lblEndDate.Location = new System.Drawing.Point(12, 99);
            this.lblEndDate.Name = "lblEndDate";
            this.lblEndDate.Size = new System.Drawing.Size(183, 15);
            this.lblEndDate.TabIndex = 2;
            this.lblEndDate.Text = "До какой даты? (включительно)";

            // dtpEndDate
            this.dtpEndDate.Location = new System.Drawing.Point(12, 117);
            this.dtpEndDate.Name = "dtpEndDate";
            this.dtpEndDate.Size = new System.Drawing.Size(320, 23);
            this.dtpEndDate.TabIndex = 3;

            // lblType
            this.lblType.AutoSize = true;
            this.lblType.Location = new System.Drawing.Point(12, 143);
            this.lblType.Name = "lblType";
            this.lblType.Size = new System.Drawing.Size(148, 15);
            this.lblType.TabIndex = 4;
            this.lblType.Text = "Выбор типа инструктажа:";

            // cmbType
            this.cmbType.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbType.FormattingEnabled = true;
            this.cmbType.Location = new System.Drawing.Point(12, 161);
            this.cmbType.Name = "cmbType";
            this.cmbType.Size = new System.Drawing.Size(320, 23);
            this.cmbType.TabIndex = 5;

            // Populate instruction types
            this.cmbType.Items.Add("Первичный");
            this.cmbType.Items.Add("Повторный");
            this.cmbType.Items.Add("Повторный (для водителей)");
            this.cmbType.Items.Add("Целевой");

            // btnSave
            this.btnSave.Location = new System.Drawing.Point(86, 200);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(120, 30);
            this.btnSave.TabIndex = 6;
            this.btnSave.Text = "Сохранить";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new EventHandler(this.btnSave_Click);

            // btnCancel
            this.btnCancel.DialogResult = DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(212, 200);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(120, 30);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new EventHandler(this.btnCancel_Click);

            // EditInstructionForm
            this.AcceptButton = this.btnSave;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(344, 241);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.cmbType);
            this.Controls.Add(this.lblType);
            this.Controls.Add(this.dtpEndDate);
            this.Controls.Add(this.lblEndDate);
            this.Controls.Add(this.txtCause);
            this.Controls.Add(this.lblCause);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EditInstructionForm";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "Редактирование инструктажа";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private void EditInstructionForm_Load(object sender, EventArgs e)
        {
            if (_originalInstruction != null)
            {
                txtCause.Text = _originalInstruction.cause_of_instruction;
                dtpEndDate.Value = _originalInstruction.end_date;

                // Determine the index in the combo box based on the instruction type
                // Subtract 2 because the types start at 2 for these regular instructions
                int typeIndex = _originalInstruction.type_of_instruction - 2;
                if (typeIndex >= 0 && typeIndex < cmbType.Items.Count)
                {
                    cmbType.SelectedIndex = typeIndex;
                }
            }
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            btnSave.Enabled = false;

            try
            {
                // Basic validation
                if (string.IsNullOrWhiteSpace(txtCause.Text))
                {
                    MessageBox.Show("Причина инструктажа пуста. Исправьте это пожалуйста.",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    btnSave.Enabled = true;
                    return;
                }

                DateTime endDate = dtpEndDate.Value.Date;
                if (endDate <= DateTime.Now)
                {
                    MessageBox.Show("До какой даты должно быть больше текущего времени!",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    btnSave.Enabled = true;
                    return;
                }

                if (cmbType.SelectedIndex == -1)
                {
                    MessageBox.Show("Не выбран тип инструктажа!",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    btnSave.Enabled = true;
                    return;
                }

                // Map selected index to type byte (adding 2 because types start at 2 for these)
                byte instructionType = (byte)(cmbType.SelectedIndex + 2);

                // Create a new instruction with updated values
                InstructionResult = new Instruction(
                    txtCause.Text,
                    _originalInstruction.begin_date, // Keep the original begin date
                    endDate,
                    null, // No path to instruction
                    instructionType
                );

                // Set the ID to match the original instruction
                InstructionResult.instruction_id = _originalInstruction.instruction_id;

                // Preserve other properties that shouldn't change
                InstructionResult.is_assigned_to_people = _originalInstruction.is_assigned_to_people;
                InstructionResult.is_passed_by_everyone = _originalInstruction.is_passed_by_everyone;

                if (_jwtToken != null)
                {
                    // Save to server if token is provided
                    bool saveResult = await UpdateInstructionOnServer(InstructionResult);
                    if (!saveResult)
                    {
                        btnSave.Enabled = true;
                        return;
                    }
                }

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении инструктажа: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"Ошибка при обновлении инструктажа: {ex.Message}");
                btnSave.Enabled = true;
            }
        }

        private async Task<bool> UpdateInstructionOnServer(Instruction instruction)
        {
            using (var httpClient = new HttpClient())
            {
                string jwtToken = _jwtToken;
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                // Create a DTO that matches the simplified server model
                var instructionDto = new InstructionUpdateDto
                {
                    CauseOfInstruction = instruction.cause_of_instruction,
                    EndDate = instruction.end_date,
                    TypeOfInstruction = instruction.type_of_instruction
                };

                string json = JsonConvert.SerializeObject(instructionDto);
                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PutAsync(
                    ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + $"/update-instruction/{instruction.instruction_id}",
                    content);

                if (!response.IsSuccessStatusCode)
                {
                    string errorMessage = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to update instruction. Status code: {response.StatusCode}. Error: {errorMessage}");
                }
                return true;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
