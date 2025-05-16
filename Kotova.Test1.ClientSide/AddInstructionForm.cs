using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinForms = System.Windows.Forms;
using Kotova.CommonClasses;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http;

namespace Kotova.Test1.ClientSide
{
    public class AddInstructionForm : WinForms.Form
    {
        private WinForms.Label lblCause;
        private WinForms.TextBox txtCause;
        private WinForms.Label lblEndDate;
        private WinForms.DateTimePicker dtpEndDate;
        private WinForms.Label lblType;
        private WinForms.ComboBox cmbType;
        private WinForms.Button btnSave;
        private WinForms.Button btnCancel;
        private string _jwtToken;

        public Instruction InstructionResult { get; private set; }

        public AddInstructionForm(string jwtToken = null)
        {
            _jwtToken = jwtToken;
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
            this.lblCause = new WinForms.Label();
            this.txtCause = new WinForms.TextBox();
            this.lblEndDate = new WinForms.Label();
            this.dtpEndDate = new WinForms.DateTimePicker();
            this.lblType = new WinForms.Label();
            this.cmbType = new WinForms.ComboBox();
            this.btnSave = new WinForms.Button();
            this.btnCancel = new WinForms.Button();
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
            this.cmbType.DropDownStyle = WinForms.ComboBoxStyle.DropDownList;
            this.cmbType.FormattingEnabled = true;
            this.cmbType.Location = new System.Drawing.Point(12, 161);
            this.cmbType.Name = "cmbType";
            this.cmbType.Size = new System.Drawing.Size(320, 23);
            this.cmbType.TabIndex = 5;

            // btnSave
            this.btnSave.Location = new System.Drawing.Point(86, 200);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(120, 30);
            this.btnSave.TabIndex = 6;
            this.btnSave.Text = "Сохранить";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);

            // btnCancel
            this.btnCancel.DialogResult = WinForms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(212, 200);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(120, 30);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);

            // AddInstructionForm
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
            this.FormBorderStyle = WinForms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddInstructionForm";
            this.StartPosition = WinForms.FormStartPosition.CenterParent;
            this.Text = "Добавление инструктажа";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            btnSave.Enabled = false;

            try
            {
                // Basic validation
                if (string.IsNullOrWhiteSpace(txtCause.Text))
                {
                    WinForms.MessageBox.Show("Причина инструктажа пуста. Исправьте это пожалуйста.",
                        "Ошибка", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Warning);
                    btnSave.Enabled = true;
                    return;
                }

                DateTime startTime = DateTime.Now;
                DateTime endDate = dtpEndDate.Value.Date;
                if (endDate <= startTime)
                {
                    WinForms.MessageBox.Show("До какой даты должно быть больше текущего времени!",
                        "Ошибка", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Warning);
                    btnSave.Enabled = true;
                    return;
                }

                if (cmbType.SelectedIndex == -1)
                {
                    WinForms.MessageBox.Show("Не выбран тип инструктажа!",
                        "Ошибка", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Warning);
                    btnSave.Enabled = true;
                    return;
                }

                // Get values from form
                string causeOfInstruction = txtCause.Text;
                // Map selected index to type byte (adding 2 because types start at 2 for these)
                Byte typeOfInstruction = (Byte)(cmbType.SelectedIndex + 2);

                // Create the instruction object for local use
                Instruction instruction = new Instruction(
                    causeOfInstruction,
                    startTime,
                    endDate,
                    null, // No path to instruction
                    typeOfInstruction
                );

                if (_jwtToken != null)
                {
                    // Save to server if token is provided
                    bool saveResult = await SaveInstructionToServer(instruction);
                    if (!saveResult)
                    {
                        btnSave.Enabled = true;
                        return;
                    }
                }

                // Set the result and close the form
                InstructionResult = instruction;
                DialogResult = WinForms.DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                WinForms.MessageBox.Show($"Ошибка при создании инструктажа: {ex.Message}",
                    "Ошибка", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Error);
                Console.WriteLine($"Ошибка при создании инструктажа: {ex.Message}");
                btnSave.Enabled = true;
            }
        }

        private async Task<bool> SaveInstructionToServer(Instruction instruction)
        {
            try
            {
                // Create a new DTO that matches the server's expected format
                var instructionDto = new InstructionCreateDto
                {
                    CauseOfInstruction = instruction.cause_of_instruction,
                    EndDate = instruction.end_date,
                    TypeOfInstruction = instruction.type_of_instruction
                };

                // Serialize the DTO directly - no nested structure anymore
                string json = JsonConvert.SerializeObject(instructionDto);

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);

                    HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(
                        ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/add-new-instruction-into-db",
                        content);

                    if (response.IsSuccessStatusCode)
                    {
                        WinForms.MessageBox.Show($"Инструктаж '{instruction.cause_of_instruction}' успешно добавлен в базу данных.",
                            "Успех", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Information);
                        return true;
                    }
                    else
                    {
                        string errorMessage = await response.Content.ReadAsStringAsync();
                        WinForms.MessageBox.Show($"Ошибка при сохранении инструктажа: {errorMessage}",
                            "Ошибка", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                WinForms.MessageBox.Show($"Ошибка при сохранении инструктажа: {ex.Message}",
                    "Ошибка", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Error);
                return false;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = WinForms.DialogResult.Cancel;
            Close();
        }
    }

    // Similar EditInstructionForm with pre-populated fields
}
