using ClosedXML.Excel;
using Kotova.CommonClasses;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using Windows.UI.WindowManagement;
using Windows.Web.Http;
using HttpClient = System.Net.Http.HttpClient;
using WinForms = System.Windows.Forms;

using WinFormsColor = System.Drawing.Color;
using WpfColor = System.Windows.Media.Color;
using WpfWindow = System.Windows.Window;
using WinFormsForm = System.Windows.Forms.Form;

namespace Kotova.Test1.ClientSide
{
    public partial class ChiefForm : Form
    {
        public static readonly string urlCreateInstruction = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/add-new-instruction-into-db"; //Исправлено
        public static readonly string urlTest = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/greeting";
        public static readonly string urlSyncInstructions = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/sync-instructions-with-db";
        public static readonly string urlSyncNames = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/sync-names-with-db"; //Исправлено
        public static readonly string urlSubmitInstructionToPeople = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/send-instruction-to-names"; //Исправлено
        public static readonly string DownloadInstructionForUserURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get_not_passed_instructions_for_user";
        public static readonly string SendInstructionIsPassedURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/instruction_is_passed_by_user";
        public static readonly string GetDepartmentIdByUserName = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get-department-id-by";
        public static readonly string getNotPassedInstructionURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get-not-passed-instructions-for-chief";
        public static readonly string instructionDataExportURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/instructions-data-export";
        public static readonly string RefreshTaskForChiefUrl = ConfigurationClass.BASE_TASK_URL_DEVELOPMENT + "/get-all-current-tasks-for-chief";
        public static readonly string DownloadExcelFileUrl = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/export";
        public static readonly string SkipTheUnplannedInstructionURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/skip-the-unplanned-instruction";


        private List<InstructionReportItem> cachedInstructions;


        public static readonly string urlTaskTest = ConfigurationClass.BASE_TASK_URL_DEVELOPMENT + "/create-random-task";

        public const string dB_instructionId = "instruction_id"; //ВЫНЕСИ ЭТИ 3 СТРОЧКИ В ОБЩИЙ ФАЙЛ!
        public const string db_filePath = "file_path";
        public const string db_typeOfInstruction = "type_of_instruction";

        private static readonly HttpClient _client = new HttpClient();
        private HubConnection? _hubConnection = null;

        private List<Dictionary<string, object>> listOfInstructions_global;
        private List<Dictionary<string, object>> listsOfPaths_global;
        private List<InstructionForChiefDto> instructionForChiefs_global;
        private List<InstructionForChiefDto> passedInstructionsForChief_global;
        private List<Instruction> unplannedInstructions_global;

        private static List<WpfWindow> _openWpfWindows = new List<WpfWindow>();


        static string? selectedFolderPath = null;
        private Login_Russian? _loginForm;
        string? _userName;
        public SignUpForm _signUpForm;

        public ChiefForm(Login_Russian loginForm, string userName, string fullName, string departmentName)
        {
            _loginForm = loginForm;
            _userName = userName;
            SignUpForm signUpForm = new SignUpForm(loginForm, this);
            _signUpForm = signUpForm;
            InitializeComponent();
            usernameLabel.Text = userName;
            ChiefTabControl_SelectedIndexChanged(null, EventArgs.Empty);

            InitializeSignalRConnection();

        }

        private async void InitializeSignalRConnection()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(ConfigurationClass.BASE_SIGNALR_CONNECTION_URL_DEVELOPMENT, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(_loginForm._jwtToken);
                })
            .Build();

            _hubConnection.On<string, string>("Получено сообщение", (user, message) =>
            {
                // Handle incoming messages from the SignalR hub
                MessageBox.Show($"{user}: {message}", "Message from Hub");
            });

            _hubConnection.On<string>("ReceiveAlert", message =>
            {
                Notifications.ShowWindowsNotification("Alert", message);
            });

            try
            {
                await _hubConnection.StartAsync();
                //MessageBox.Show("Подключено к SignalR hub.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось подключиться к SignalR hub: {ex.Message}");
            }
        }



        private async void testButton_Click(object sender, EventArgs e)
        {
            string url = urlTest;
            await Test.connectionToUrlGet(url, _loginForm._jwtToken);
        }

        private async void testButtonForTask_Click(object sender, EventArgs e)
        {
            string url = urlTaskTest;
            await Test.connectionToUrlGet(url, _loginForm._jwtToken);
        }

        private async void buttonCreateInstruction_Click(object sender, EventArgs e)
        {
            buttonCreateInstruction.Enabled = false;
            try
            {
                // Basic validation
                if (string.IsNullOrWhiteSpace(InstructionTextBox.Text))
                {
                    MessageBox.Show("Причина инструктажа пуста. Исправьте это пожалуйста.");
                    buttonCreateInstruction.Enabled = true;
                    return;
                }

                DateTime startTime = DateTime.Now;
                DateTime endDate = datePickerEnd.Value.Date;
                if (endDate <= startTime)
                {
                    MessageBox.Show("До какой даты должно быть больше текущего времени!");
                    buttonCreateInstruction.Enabled = true;
                    return;
                }

                if (typeOfInstructionListBox.SelectedIndex == -1)
                {
                    MessageBox.Show("Не выбран тип инструктажа!");
                    buttonCreateInstruction.Enabled = true;
                    return;
                }

                // Create the instruction without folder path
                string causeOfInstruction = InstructionTextBox.Text;
                Byte typeOfInstruction = (Byte)(typeOfInstructionListBox.SelectedIndex + 2);

                // Create the instruction object without path
                Instruction instruction = new Instruction(
                    causeOfInstruction,
                    startTime,
                    endDate,
                    null, // No path to instruction
                    typeOfInstruction
                );

                // Create empty path list since we no longer need file paths
                List<string> paths = new List<string>();

                FullCustomInstruction fullCustomInstr = new FullCustomInstruction(instruction, paths);

                // Create request object
                var requestObject = new
                {
                    Instruction = new
                    {
                        CauseOfInstruction = instruction.cause_of_instruction,
                        EndDate = instruction.end_date,
                        PathToInstruction = instruction.path_to_instruction,
                        TypeOfInstruction = instruction.type_of_instruction
                    },
                    Paths = paths
                };

                string json = JsonConvert.SerializeObject(requestObject);
                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                await Test.connectionToUrlPost(urlCreateInstruction, content, $"Инструктаж '{causeOfInstruction}' успешно добавлен в базу данных.", _loginForm._jwtToken);

                // Reset form
                InstructionTextBox.Text = "";
                typeOfInstructionListBox.SelectedIndex = -1;

                // Instead of immediately assigning, just inform the user to use the assignment button
                MessageBox.Show("Инструктаж успешно создан. Используйте кнопку 'Назначить инструктаж сотрудникам' для назначения сотрудникам и выбора нормативных инструкций.");

                // Refresh the instructions list
                await SyncManuallyInstrWithDBInternal();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании инструктажа: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                buttonCreateInstruction.Enabled = true;
            }
        }

        // Initialize the tab with a tree view for instruction selection
        private void InitializeComplianceReportTab()
        {
            // Clear the existing controls
            tabPageEmployeeInstructionLog.Controls.Clear();

            // Create a label for instructions
            WinForms.Label instructionLabel = new WinForms.Label
            {
                Text = "Выберите тип и причину инструктажа:",
                Location = new Point(20, 20),
                Size = new Size(300, 20),
                AutoSize = true
            };
            tabPageEmployeeInstructionLog.Controls.Add(instructionLabel);

            // Create a tree view for instruction selection
            instructionReportTreeView = new WinForms.TreeView
            {
                Location = new Point(20, 50),
                Size = new Size(600, 400),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                HideSelection = false,
                FullRowSelect = true
            };
            tabPageEmployeeInstructionLog.Controls.Add(instructionReportTreeView);

            // Create export button
            exportReportButton = new WinForms.Button
            {
                Text = "Экспортировать данные о прохождении",
                Location = new Point(20, 460),
                Size = new Size(250, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                Enabled = false
            };
            exportReportButton.Click += ExportComplianceReport_Click;
            tabPageEmployeeInstructionLog.Controls.Add(exportReportButton);

            // Create refresh button
            refreshReportButton = new WinForms.Button
            {
                Text = "Обновить список инструктажей",
                Location = new Point(290, 460),
                Size = new Size(200, 30),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            refreshReportButton.Click += RefreshInstructionTree_Click;
            tabPageEmployeeInstructionLog.Controls.Add(refreshReportButton);

            // Add a help label
            WinForms.Label helpLabel = new WinForms.Label
            {
                Text = "Выберите инструктаж и нажмите 'Экспортировать' для создания отчета о прохождении",
                Location = new Point(20, 500),
                Size = new Size(600, 20),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                ForeColor = Color.DarkBlue
            };
            tabPageEmployeeInstructionLog.Controls.Add(helpLabel);

            // Attach selection event handler
            instructionReportTreeView.AfterSelect += InstructionReportTreeView_AfterSelect;

            // Load the instruction data
            LoadInstructionTreeData();
        }

        // Event handler for tree node selection
        private void InstructionReportTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // Only enable export if an instruction (not a type) is selected
            if (e.Node?.Tag != null && !e.Node.Tag.ToString().StartsWith("TYPE_"))
            {
                exportReportButton.Enabled = true;
            }
            else
            {
                exportReportButton.Enabled = false;
            }
        }

        // Event handler for the refresh button
        private async void RefreshInstructionTree_Click(object sender, EventArgs e)
        {
            await LoadInstructionTreeData();
        }

        // Load instruction data and populate the tree view
        private async Task LoadInstructionTreeData()
        {
            try
            {
                // Show loading indicator
                instructionReportTreeView.Nodes.Clear();
                TreeNode loadingNode = new TreeNode("Загрузка данных...");
                instructionReportTreeView.Nodes.Add(loadingNode);

                // Disable buttons during loading
                exportReportButton.Enabled = false;
                refreshReportButton.Enabled = false;

                // Fetch data from the server
                cachedInstructions = await GetInstructionsWithComplianceData();

                // Check if we got any data
                if (cachedInstructions == null || cachedInstructions.Count == 0)
                {
                    instructionReportTreeView.Nodes.Clear();
                    instructionReportTreeView.Nodes.Add(new TreeNode("Нет доступных инструктажей"));
                    refreshReportButton.Enabled = true;
                    return;
                }

                // Populate the tree view
                instructionReportTreeView.Nodes.Clear();

                // Group instructions by type
                var instructionsByType = cachedInstructions
                    .GroupBy(i => i.TypeOfInstruction)
                    .OrderBy(g => g.Key);

                // Create nodes for each type
                foreach (var typeGroup in instructionsByType)
                {
                    // Create node for this type
                    TreeNode typeNode = new TreeNode(GetInstructionTypeName(typeGroup.Key));
                    typeNode.Tag = $"TYPE_{typeGroup.Key}";

                    // Add child nodes for each instruction in this type
                    foreach (var instruction in typeGroup.OrderByDescending(i => i.BeginDate))
                    {
                        // Format text to include completion information
                        int totalEmployees = instruction.EmployeeData.Count;
                        int passedCount = instruction.EmployeeData.Count(e => e.HasPassed);
                        string completionStatus = totalEmployees > 0
                            ? $" [{passedCount}/{totalEmployees} ({(passedCount * 100 / Math.Max(totalEmployees, 1))}%)]"
                            : " [Нет сотрудников]";

                        string nodeText = $"{instruction.CauseOfInstruction} (от {instruction.BeginDate:dd.MM.yyyy}){completionStatus}";

                        // Create node for this instruction
                        TreeNode instructionNode = new TreeNode(nodeText);
                        instructionNode.Tag = instruction.InstructionId;

                        // Set node color based on completion status
                        if (totalEmployees > 0)
                        {
                            if (passedCount == totalEmployees)
                            {
                                instructionNode.ForeColor = Color.Green; // Fully completed
                            }
                            else if (passedCount >= totalEmployees / 2)
                            {
                                instructionNode.ForeColor = Color.DarkGreen; // More than half
                            }
                            else if (passedCount > 0)
                            {
                                instructionNode.ForeColor = Color.DarkOrange; // Some completed
                            }
                            else
                            {
                                instructionNode.ForeColor = Color.Red; // None completed
                            }
                        }

                        typeNode.Nodes.Add(instructionNode);
                    }

                    instructionReportTreeView.Nodes.Add(typeNode);
                }

                // Expand all nodes
                instructionReportTreeView.ExpandAll();

                // Re-enable refresh button
                refreshReportButton.Enabled = true;
            }
            catch (Exception ex)
            {
                // Show error
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Clear tree and show error node
                instructionReportTreeView.Nodes.Clear();
                instructionReportTreeView.Nodes.Add(new TreeNode("Ошибка загрузки данных"));

                // Re-enable refresh button
                refreshReportButton.Enabled = true;
            }
        }

        // Event handler for the export button
        private async void ExportComplianceReport_Click(object sender, EventArgs e)
        {
            if (instructionReportTreeView.SelectedNode == null ||
                instructionReportTreeView.SelectedNode.Tag == null ||
                instructionReportTreeView.SelectedNode.Tag.ToString().StartsWith("TYPE_"))
            {
                MessageBox.Show("Пожалуйста, выберите конкретный инструктаж для экспорта.",
                    "Выбор инструктажа", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                // Get selected instruction ID
                int instructionId = Convert.ToInt32(instructionReportTreeView.SelectedNode.Tag);

                // Get report data from server
                var report = await GetInstructionComplianceReport(instructionId);

                // Export to Excel
                ExportToExcel(report);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте отчета: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Method to get instruction type name
        private string GetInstructionTypeName(byte typeCode)
        {
            return typeCode switch
            {
                0 => "Вводный",
                1 => "Внеплановый",
                2 => "Первичный",
                3 => "Повторный",
                4 => "Повторный (для водителей)",
                5 => "Целевой",
                _ => $"Неизвестный тип ({typeCode})"
            };
        }

        // Method to fetch instruction data from the server
        private async Task<List<InstructionReportItem>> GetInstructionsWithComplianceData()
        {
            using (var httpClient = new HttpClient())
            {
                string jwtToken = _loginForm._jwtToken;
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                var response = await httpClient.GetAsync(
                    ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get-instructions-with-compliance-data");

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<InstructionReportItem>>(responseBody);
                }
                else
                {
                    throw new Exception($"Ошибка сервера: {response.StatusCode}");
                }
            }
        }

        // Method to fetch compliance report for a specific instruction
        private async Task<InstructionReportItem> GetInstructionComplianceReport(int instructionId)
        {
            using (var httpClient = new HttpClient())
            {
                string jwtToken = _loginForm._jwtToken;
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                var response = await httpClient.GetAsync(
                    ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + $"/get-instruction-compliance-report/{instructionId}");

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<InstructionReportItem>(responseBody);
                }
                else
                {
                    throw new Exception($"Ошибка сервера: {response.StatusCode}");
                }
            }
        }

        // Method to export data to Excel
        private void ExportToExcel(InstructionReportItem report)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx";
                saveFileDialog.Title = "Сохранить отчет";
                saveFileDialog.FileName = $"Отчет_инструктаж_{report.InstructionId}_{DateTime.Now:yyyyMMdd}.xlsx";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var workbook = new XLWorkbook())
                        {
                            var worksheet = workbook.Worksheets.Add("Отчет");

                            // Add title with instruction information
                            worksheet.Cell("A1").Value = "ОТЧЕТ О ПРОХОЖДЕНИИ ИНСТРУКТАЖА";
                            worksheet.Range("A1:G1").Merge();
                            worksheet.Cell("A1").Style.Font.Bold = true;
                            worksheet.Cell("A1").Style.Font.FontSize = 14;
                            worksheet.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            // Add instruction details
                            worksheet.Cell("A3").Value = "Причина инструктажа:";
                            worksheet.Cell("B3").Value = report.CauseOfInstruction;
                            worksheet.Range("B3:G3").Merge();

                            worksheet.Cell("A4").Value = "Тип инструктажа:";
                            worksheet.Cell("B4").Value = report.TypeName;
                            worksheet.Range("B4:G4").Merge();

                            worksheet.Cell("A5").Value = "Период:";
                            worksheet.Cell("B5").Value = $"{report.BeginDate:dd.MM.yyyy} - {report.EndDate:dd.MM.yyyy}";
                            worksheet.Range("B5:G5").Merge();

                            // Add statistics
                            worksheet.Cell("A7").Value = "Сотрудников всего:";
                            worksheet.Cell("B7").Value = report.EmployeeData.Count;

                            worksheet.Cell("D7").Value = "Прошли инструктаж:";
                            int passedCount = report.EmployeeData.Count(e => e.HasPassed);
                            worksheet.Cell("E7").Value = passedCount;

                            worksheet.Cell("F7").Value = "Не прошли:";
                            worksheet.Cell("G7").Value = report.EmployeeData.Count - passedCount;

                            // Percentage
                            worksheet.Cell("A8").Value = "Процент прохождения:";
                            double percentage = report.EmployeeData.Count > 0
                                ? (double)passedCount / report.EmployeeData.Count * 100
                                : 0;
                            worksheet.Cell("B8").Value = $"{percentage:F1}%";

                            // Style for header cells
                            var headerStyle = workbook.Style;
                            headerStyle.Font.Bold = true;
                            headerStyle.Fill.BackgroundColor = XLColor.LightGray;
                            headerStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            headerStyle.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            // Add column headers at row 10
                            worksheet.Cell("A10").Value = "№";
                            worksheet.Cell("B10").Value = "ФИО сотрудника";
                            worksheet.Cell("C10").Value = "Должность";
                            worksheet.Cell("D10").Value = "Дата рождения";
                            worksheet.Cell("E10").Value = "Дата прохождения";
                            worksheet.Cell("F10").Value = "Статус";
                            worksheet.Cell("G10").Value = "Назначил инструктаж";
                            worksheet.Cell("H10").Value = "Нормативные документы";

                            // Apply style to headers
                            worksheet.Range("A10:H10").Style = headerStyle;

                            // Add employee data starting at row 11
                            int rowIndex = 11;

                            // Sort employees: passed first (chronologically), then not passed
                            var sortedEmployees = report.EmployeeData
                                .OrderBy(e => e.HasPassed ? 0 : 1) // Passed first, not passed after
                                .ThenBy(e => e.DatePassed) // Sort by date (earliest first)
                                .ThenBy(e => e.FullName) // Then by name
                                .ToList();

                            for (int i = 0; i < sortedEmployees.Count; i++)
                            {
                                var employee = sortedEmployees[i];

                                // Row number
                                worksheet.Cell($"A{rowIndex}").Value = i + 1;

                                // Employee information
                                worksheet.Cell($"B{rowIndex}").Value = employee.FullName;
                                worksheet.Cell($"C{rowIndex}").Value = employee.Position;
                                worksheet.Cell($"D{rowIndex}").Value = employee.BirthDate.ToString("dd.MM.yyyy");

                                // Date passed
                                worksheet.Cell($"E{rowIndex}").Value = employee.HasPassed && employee.DatePassed.HasValue
                                    ? employee.DatePassed.Value.ToString("dd.MM.yyyy HH:mm")
                                    : "-";

                                // Status
                                worksheet.Cell($"F{rowIndex}").Value = employee.HasPassed ? "Пройден" : "Не пройден";

                                // Who assigned
                                worksheet.Cell($"G{rowIndex}").Value = employee.AssignedBy;

                                // Normative documents
                                worksheet.Cell($"H{rowIndex}").Value = string.Join(", ", employee.NormativeDocuments);

                                // Color coding based on status
                                if (employee.HasPassed)
                                {
                                    worksheet.Range($"A{rowIndex}:H{rowIndex}").Style.Fill.BackgroundColor = XLColor.LightGreen;
                                }
                                else
                                {
                                    worksheet.Range($"A{rowIndex}:H{rowIndex}").Style.Fill.BackgroundColor = XLColor.LightSalmon;
                                }

                                rowIndex++;
                            }

                            // Adjust column widths
                            worksheet.Column(1).Width = 5;  // №
                            worksheet.Column(2).Width = 30; // ФИО
                            worksheet.Column(3).Width = 25; // Должность
                            worksheet.Column(4).Width = 15; // Дата рождения
                            worksheet.Column(5).Width = 20; // Дата прохождения
                            worksheet.Column(6).Width = 10; // Статус
                            worksheet.Column(7).Width = 35; // Назначил
                            worksheet.Column(8).Width = 40; // Нормативные документы

                            // Add borders to data
                            worksheet.Range($"A10:H{rowIndex - 1}").Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                            worksheet.Range($"A10:H{rowIndex - 1}").Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                            // Add timestamp
                            worksheet.Cell($"A{rowIndex + 2}").Value = $"Отчет сформирован: {DateTime.Now:dd.MM.yyyy HH:mm:ss}";
                            worksheet.Range($"A{rowIndex + 2}:H{rowIndex + 2}").Merge();

                            // Save workbook
                            workbook.SaveAs(saveFileDialog.FileName);

                            // Show success message
                            MessageBox.Show($"Отчет успешно сохранен в файл: {saveFileDialog.FileName}",
                                "Экспорт завершен", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Try to open the file
                            try
                            {
                                Process.Start(new ProcessStartInfo
                                {
                                    FileName = saveFileDialog.FileName,
                                    UseShellExecute = true
                                });
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Не удалось автоматически открыть файл: {ex.Message}",
                                    "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при создании Excel-файла: {ex.Message}",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }






        private async void buttonSyncManualyInstrWithDB_Click(object sender, EventArgs e)
        {
            await SyncManuallyInstrWithDBInternal();
        }
        private async Task SyncManuallyInstrWithDBInternal()
        {
            try
            {
                buttonSyncManualyInstrWithDB.Enabled = false;
                ListOfUnplannedInstructions.Items.Clear();

                using (var httpClient = new HttpClient())
                {
                    string jwtToken = _loginForm._jwtToken;
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                    var response = await httpClient.GetAsync(urlSyncInstructions);
                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        if (string.IsNullOrWhiteSpace(responseBody))
                        {
                            throw new Exception("responseBody пуст"); //throw here better something
                        }
                        List<Instruction> result = JsonConvert.DeserializeObject<List<Instruction>>(responseBody); //checked that is not null before! so warning maybe suppressed
                        unplannedInstructions_global = result;
                        string[] resultArray = result.Select(n => n.cause_of_instruction).ToArray(); //check that they are not null;
                        ListOfUnplannedInstructions.Items.AddRange(resultArray);
                        //MessageBox.Show("Имена успешно синхронизированы с базой данных.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        string errorMessage = await response.Content.ReadAsStringAsync();
                        // The call was not successful, handle errors or retry logic
                        //MessageBox.Show($"Failed to sync names with DB. Status code: {response.StatusCode}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        MessageBox.Show($"Не удалось синхронизировать имена с базой данных. Status code: {response.StatusCode} {errorMessage}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                // Exception handling for networking errors, etc.
                //MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                buttonSyncManualyInstrWithDB.Enabled = true;
            }
        }




        private async void SyncNamesWithDB_Click(object sender, EventArgs e)
        {
            await SyncNamesWithDBInternal();
        }
        private async Task SyncNamesWithDBInternal()
        {
            try
            {
                SyncNamesWithDB.Enabled = false; // Assuming this is a button, disable it to prevent multiple clicks
                checkedListBoxNamesOfPeople.Items.Clear();

                using (var httpClient = new HttpClient())
                {
                    string jwtToken = _loginForm._jwtToken;
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                    var response = await httpClient.GetAsync(urlSyncNames);

                    if (response.IsSuccessStatusCode)
                    {

                        string responseBody = await response.Content.ReadAsStringAsync();
                        List<EmployeeData> result = JsonConvert.DeserializeObject<List<EmployeeData>>(responseBody);
                        if (result is null)
                        {
                            throw new Exception("тело ответа пусто");
                        }
                        string[] resultArray = result.Select(e => $"{e.FullName} ({e.BirthDate})").ToArray();

                        //ListBoxNamesOfPeople.Items.AddRange(resultArray);
                        checkedListBoxNamesOfPeople.Items.AddRange(resultArray);
                        // Successfully called the ImportIntoDB endpoint, handle accordingly
                        //MessageBox.Show("Имена успешно синхронизированы с базой данных.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        string errorMessage = await response.Content.ReadAsStringAsync();
                        //MessageBox.Show($"Failed to sync names with DB. Status code: {response.StatusCode}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        MessageBox.Show($"Не удалось синхронизировать имена с базой данных. Status code: {response.StatusCode} {errorMessage}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                // Exception handling for networking errors, etc.
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SyncNamesWithDB.Enabled = true; // Re-enable the button after the operation completes
            }
        }


        private async void submitInstructionToPeople_Click(object sender, EventArgs e)
        {
            submitInstructionToPeople.Enabled = false;
            try
            {
                var listOfNames = checkedListBoxNamesOfPeople.CheckedItems;

                List<Tuple<string, string>> listOfNamesAndBirthDateString = new List<Tuple<string, string>>();
                if (listOfNames.Count == 0)
                {
                    MessageBox.Show("Люди не выбраны!");
                    submitInstructionToPeople.Enabled = true;
                    return;
                }

                var selectedInstruction = ListOfUnplannedInstructions.SelectedItem;
                if (selectedInstruction is null)
                {
                    MessageBox.Show("Инструкция не выбрана!");
                    submitInstructionToPeople.Enabled = true;
                    return;
                }

                // Check if at least one normative instruction is selected
                var selectedNormativeInstructions = new List<int>();
                foreach (ListBoxItem item in ListOfNormativeInstrNames.SelectedItems)
                {
                    selectedNormativeInstructions.Add(item.Value);
                }

                if (selectedNormativeInstructions.Count == 0)
                {
                    MessageBox.Show("Выберите хотя бы одну нормативную инструкцию!");
                    submitInstructionToPeople.Enabled = true;
                    return;
                }

                try
                {
                    foreach (var item in listOfNames)
                    {
                        listOfNamesAndBirthDateString.Add(DeconstructNameAndBirthDate(item.ToString()));
                    }

                    string instructionNameString = selectedInstruction.ToString();

                    // Include the selected normative instruction IDs in the package
                    InstructionPackage package = new InstructionPackage(
                        listOfNamesAndBirthDateString,
                        instructionNameString,
                        selectedNormativeInstructions
                    );

                    string jsonData = JsonConvert.SerializeObject(package);
                    string encryptedJsonData = Encryption_Kotova.EncryptString(jsonData);

                    try
                    {
                        using (var httpClient = new HttpClient())
                        {
                            string jwtToken = _loginForm._jwtToken;
                            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                            // Set the URI of your server endpoint
                            var uri = new Uri(urlSubmitInstructionToPeople);

                            // Prepare the content to send
                            var content = new StringContent(encryptedJsonData, Encoding.UTF8, "application/json");

                            // Send a POST request with the serialized JSON content
                            var response = await httpClient.PostAsync(uri, content);

                            if (response.IsSuccessStatusCode)
                            {
                                MessageBox.Show("Данные успешно отправлены на сервер и инструктаж назначен людям.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                var errorMessage = await response.Content.ReadAsStringAsync();
                                MessageBox.Show($"Не получилось отправить данные не сервер. Status code: {response.StatusCode},Error: {errorMessage} ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка произошла при отправке данных: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        submitInstructionToPeople.Enabled = true;
                        await SyncManuallyInstrWithDBInternal();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Какая-то ошибка произошла при назначении инструктажа:{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    submitInstructionToPeople.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Неожиданная ошибка: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Tuple<string, string> DeconstructNameAndBirthDate(string? nameWithBirthDate)
        {
            string pattern = @"^(.+?)\s\((\d{4}-\d{2}-\d{2})\)$"; //Эта строка соответсвует birthDate_format в Server side
            if (nameWithBirthDate == null) { throw new ArgumentException("nameWithBirthDate is null! in DeconstructNameAndBirthDate"); }
            Regex regex = new Regex(pattern);
            Match match = regex.Match(nameWithBirthDate);

            if (match.Success)
            {
                string fullName = match.Groups[1].Value;  // ФИО
                string birthDate = match.Groups[2].Value; // BirthDate (Дата рождения)
                return Tuple.Create(fullName, birthDate);
            }
            else
            {
                throw new ArgumentException("nameWithBirthDate doesn't match the pattern! in DeconstructNameAndBirthDate");
            }
        }

        private async void LogOutForm_Click(object sender, EventArgs e)
        {

            LogOutForm_Click_Internal();
        }
        public async void LogOutForm_Click_Internal()
        {
            if (_signUpForm != null)
            {
                _signUpForm.Dispose();
            }
            Decryption_stuff.DeleteJWTToken();
            this.Dispose(true);
            _loginForm.activeForm = _loginForm;
            _loginForm.Show();
        }


        private void ChiefForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            this.ShowInTaskbar = false;

        }



        //ВОТ ЭТО ТРЕТЬЯ ВКЛАДКА! ПОДЕЛИ КАК СЧИТАЕШЬ НУЖНЫМ!



        private async void ChiefTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ChiefTabControl.SelectedTab == tabPageForPassingInstruction)
            {
                ShowWpfInstructionViewer();

                // Optionally switch to another tab to avoid empty tab content
                // This prevents user confusion when they see an empty tab
                if (ChiefTabControl.TabPages.Count > 0 && ChiefTabControl.SelectedTab == tabPageForPassingInstruction)
                {
                    // Find any tab other than tabPage3
                    for (int i = 0; i < ChiefTabControl.TabPages.Count; i++)
                    {
                        if (ChiefTabControl.TabPages[i] != tabPageForPassingInstruction)
                        {
                            ChiefTabControl.SelectedTab = ChiefTabControl.TabPages[i];
                            break;
                        }
                    }
                }

                // Early return to skip other processing for this tab
                return;
            }
            if (ChiefTabControl.SelectedTab.Text == "Внеплановые инструктажи")
            {
                await SyncManuallyInstrWithDBInternal();
                await SyncNamesWithDBInternal();
            }
            if (ChiefTabControl.SelectedTab == instructionManagementTabPage)
            {
                InitializeComplianceReportTab();
            }

            if (ChiefTabControl.SelectedTab.Text == "Создание инструктажа")
            {
                try
                {
                    SyncNamesWithDB.Enabled = false; // Assuming this is a button, disable it to prevent multiple clicks
                    checkedListBoxNamesOfPeopleCreatingInstr.Items.Clear();

                    // Also sync normative instruction names
                    await SyncNormativeInstructionNamesWithDBInternal();

                    using (var httpClient = new HttpClient())
                    {
                        string jwtToken = _loginForm._jwtToken;
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                        var response = await httpClient.GetAsync(urlSyncNames);

                        if (response.IsSuccessStatusCode)
                        {
                            string responseBody = await response.Content.ReadAsStringAsync();
                            List<EmployeeData> result = JsonConvert.DeserializeObject<List<EmployeeData>>(responseBody);
                            if (result is null)
                            {
                                throw new Exception("тело ответа пусто");
                            }
                            string[] resultArray = result.Select(e => $"{e.FullName} ({e.BirthDate})").ToArray();

                            checkedListBoxNamesOfPeopleCreatingInstr.Items.AddRange(resultArray);
                            MessageBox.Show("Имена успешно синхронизированы с базой данных.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            string errorMessage = await response.Content.ReadAsStringAsync();
                            MessageBox.Show($"Не получилось синхронизировать имена с базой данных. Status code: {response.StatusCode} {errorMessage}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    SyncNamesWithDB.Enabled = true; // Re-enable the button after the operation completes
                }
            }
            if (ChiefTabControl.SelectedTab == tabPageTrainingCompliance)
            {
                bool success = await FetchNotPassedInstructionsForChief();

                // Optionally fetch passed instructions if needed
                if (success)
                {
                    await FetchPassedInstructionsForChief();
                }
            }
        }


        /// <summary>
        /// Opens the WPF instruction viewer window for the chief
        /// </summary>
        private void ShowWpfInstructionViewer()
        {
            try
            {
                // Check if window is already open
                var existingWindow = _openWpfWindows.OfType<InstructionViewerWindow>().FirstOrDefault();
                if (existingWindow != null && existingWindow.IsLoaded)
                {
                    // Bring existing window to front
                    existingWindow.Activate();
                    existingWindow.WindowState = System.Windows.WindowState.Normal;
                    return;
                }

                // Create new WPF window with chief mode enabled
                var wpfWindow = new InstructionViewerWindow(_loginForm._jwtToken, _userName, isChief: true);
                wpfWindow.Title = $"Просмотр инструктажей - {_userName} (Руководитель)";

                // Center over parent form
                CenterWpfWindowOverWinForm(wpfWindow, this);

                // Apply theme to match parent
                ApplyWinFormThemeToWpf(wpfWindow, this.BackColor);

                // Set icon safely
                SetWpfWindowIconSafe(wpfWindow, this);

                // Show window
                wpfWindow.Show();

                // Track the window
                TrackWpfWindow(wpfWindow);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии окна просмотра инструктажей: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Centers WPF window over Windows Forms parent
        /// </summary>
        private static void CenterWpfWindowOverWinForm(WpfWindow wpfWindow, WinFormsForm parentForm)
        {
            if (parentForm != null && wpfWindow != null)
            {
                // Ensure WPF window size is loaded
                wpfWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;

                // Calculate center position
                var parentLocation = parentForm.Location;
                var parentSize = parentForm.Size;

                wpfWindow.Left = parentLocation.X + (parentSize.Width - wpfWindow.Width) / 2;
                wpfWindow.Top = parentLocation.Y + (parentSize.Height - wpfWindow.Height) / 2;

                // Ensure window is visible on screen
                var screen = Screen.FromControl(parentForm);
                if (wpfWindow.Left < screen.WorkingArea.Left)
                    wpfWindow.Left = screen.WorkingArea.Left;
                if (wpfWindow.Top < screen.WorkingArea.Top)
                    wpfWindow.Top = screen.WorkingArea.Top;
                if (wpfWindow.Left + wpfWindow.Width > screen.WorkingArea.Right)
                    wpfWindow.Left = screen.WorkingArea.Right - wpfWindow.Width;
                if (wpfWindow.Top + wpfWindow.Height > screen.WorkingArea.Bottom)
                    wpfWindow.Top = screen.WorkingArea.Bottom - wpfWindow.Height;
            }
        }

        /// <summary>
        /// Applies Windows Forms theme colors to WPF window
        /// </summary>
        private static void ApplyWinFormThemeToWpf(WpfWindow wpfWindow, WinFormsColor formsBackColor)
        {
            try
            {
                // Convert Windows Forms color to WPF color
                var wpfColor = WpfColor.FromArgb(
                    formsBackColor.A,
                    formsBackColor.R,
                    formsBackColor.G,
                    formsBackColor.B);

                wpfWindow.Background = new System.Windows.Media.SolidColorBrush(wpfColor);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not apply theme: {ex.Message}");
            }
        }

        /// <summary>
        /// Safely sets icon for WPF window
        /// </summary>
        private static void SetWpfWindowIconSafe(WpfWindow wpfWindow, WinFormsForm parentForm)
        {
            try
            {
                // Try to get icon from parent form
                if (parentForm?.Icon != null)
                {
                    var bitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                        parentForm.Icon.Handle,
                        System.Windows.Int32Rect.Empty,
                        System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                    wpfWindow.Icon = bitmap;
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not set icon from parent: {ex.Message}");
            }

            // Fallback: Create simple programmatic icon
            try
            {
                CreateSimpleWpfIcon(wpfWindow);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not create fallback icon: {ex.Message}");
                // No icon is fine too
            }
        }

        /// <summary>
        /// Creates a simple blue square icon programmatically
        /// </summary>
        private static void CreateSimpleWpfIcon(WpfWindow wpfWindow)
        {
            var drawingGroup = new System.Windows.Media.DrawingGroup();

            // Create a simple blue square icon
            var geometryDrawing = new System.Windows.Media.GeometryDrawing(
                new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.DodgerBlue),
                new System.Windows.Media.Pen(new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Navy), 1),
                new System.Windows.Media.RectangleGeometry(new System.Windows.Rect(0, 0, 16, 16))
            );

            drawingGroup.Children.Add(geometryDrawing);
            var drawingImage = new System.Windows.Media.DrawingImage(drawingGroup);
            wpfWindow.Icon = drawingImage;
        }

        /// <summary>
        /// Tracks WPF windows to prevent duplicates and manage cleanup
        /// </summary>
        private static void TrackWpfWindow(WpfWindow window)
        {
            // Clean up closed windows first
            _openWpfWindows.RemoveAll(w => w == null || !w.IsLoaded);

            // Add new window to tracking
            _openWpfWindows.Add(window);

            // Remove from tracking when window is closed
            window.Closed += (s, e) => _openWpfWindows.Remove(window);
        }

        /// <summary>
        /// Closes all tracked WPF windows
        /// </summary>
        private static void CloseAllWpfWindows()
        {
            foreach (var window in _openWpfWindows.ToList())
            {
                try
                {
                    if (window != null && window.IsLoaded)
                    {
                        window.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error closing WPF window: {ex.Message}");
                }
            }
            _openWpfWindows.Clear();
        }


        public class EmployeeData
        {
            public string FullName { get; set; }
            public string BirthDate { get; set; }
        }




        private async Task<bool> DownloadInstructionsForUserFromServer(string? userName) // по факту эта функция должна быть вместе с в User.cs в совершенно отдельном файле.
        {
            if (userName is null)
            {
                throw new ArgumentNullException(nameof(userName));
            }
            string url = DownloadInstructionForUserURL;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string jwtToken = _loginForm._jwtToken;
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                    System.Net.Http.HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();


                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var result = System.Text.Json.JsonSerializer.Deserialize<QueryResult>(jsonResponse);
                    if (result.Result1.Count == 0)
                    {
                        return true;
                    }

                    listOfInstructions_global = result.Result1;
                    listsOfPaths_global = result.Result2;
                    foreach (Dictionary<string, object> temp in result.Result1)
                    {
                        ListOfInstructionsForUser.Items.Add(temp[DataBaseNames.tableName_sql_INSTRUCTIONS_cause]);

                    }
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                // Handle any exceptions here
                MessageBox.Show($"Ошибка: {ex.Message}");
                Console.WriteLine($"Couldn't download instructions for user(Chief) from server: {ex.Message}"); //TODO: Нужно чтобы здесь не выбрасывалось резко из приложения, если что. По идее, наверное?
                return false;
            }
        }

        private void InstructionsToPass_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilesOfInstructionCheckedListBox.Items.Clear();
            HyperLinkForInstructionsFolder.Enabled = true;
            if (ListOfInstructionsForUser.SelectedItem == null)
            {
                MessageBox.Show("Вы не выбрали инструктаж.");
                PassInstruction.Enabled = false;
                HyperLinkForInstructionsFolder.Enabled = false;
                return;
            }
            Dictionary<string, object> selectedDict = GetDictFromSelectedInstruction(ListOfInstructionsForUser.SelectedItem.ToString());
            int instructionId = Convert.ToInt32(selectedDict[dB_instructionId].ToString());

            List<string> listOfPath = new List<string>();
            foreach (var listOfPaths in listsOfPaths_global)
            {
                if (Convert.ToInt32(listOfPaths[dB_instructionId].ToString()) == instructionId)
                {
                    if (listOfPaths[db_filePath] == null)
                    {
                        if (selectedDict[db_typeOfInstruction].ToString() == "0") // Проверка что мы входим в вводный инструктаж только!
                        {
                            PassInstruction.Enabled = true;
                            HyperLinkForInstructionsFolder.Enabled = false;
                            return;
                        }
                        else
                        {
                            MessageBox.Show("Ooops, Что-то пошло не так. Проверь эту строчку на предмет присутствия файлов инструктажа!");
                            PassInstruction.Enabled = false;
                            HyperLinkForInstructionsFolder.Enabled = false;
                            return;
                        }

                    }
                    FilesOfInstructionCheckedListBox.Items.Add(listOfPaths[db_filePath].ToString());
                }
            }
            HyperLinkForInstructionsFolder.Enabled = true;
        }

        private void HyperLinkForInstructionsFolder_Click(object sender, EventArgs e)
        {
            HyperLinkForInstructionsFolder.Enabled = false;
            if (ListOfInstructionsForUser.SelectedItem == null)
            {
                MessageBox.Show("Вы не выбрали инструктаж.");
                PassInstruction.Enabled = false;
                return;
            }
            Dictionary<string, object> selectedDict = GetDictFromSelectedInstruction(ListOfInstructionsForUser.SelectedItem.ToString()); //most likely suppress it, cause its not null.
            string? pathStr = selectedDict[DataBaseNames.tableName_sql_pathToInstruction].ToString();

            if (pathStr is null || pathStr.Length == 0)
            {
                MessageBox.Show("Путь пуст или отсутствует.");
                PassInstruction.Enabled = false;
                return;
            }
            string path = Path.GetFullPath(pathStr);
            OpenFolderInExplorer(path);

        }

        private void OpenFolderInExplorer(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                MessageBox.Show("Указанный путь пуст или отсутствует.");
                PassInstruction.Enabled = false;
                return;
            }

            // Get the full path and check if it exists
            string fullPath = Path.GetFullPath(path);
            if (!Directory.Exists(fullPath))
            {
                MessageBox.Show($"Путь '{fullPath}' не существует.");
                PassInstruction.Enabled = false;
                return;
            }

            // Open the folder in Windows Explorer
            try
            {
                Process.Start("explorer.exe", fullPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не получилось открыть папку: {ex.Message}");
                PassInstruction.Enabled = false;
            }
        }

        private Dictionary<string, object> GetDictFromSelectedInstruction(string selectedItemStr)
        {

            foreach (Dictionary<string, object> tempD in listOfInstructions_global)
            {
                Dictionary<string, object> selectedDictionary = listOfInstructions_global.FirstOrDefault(tempD => tempD[DataBaseNames.tableName_sql_INSTRUCTIONS_cause].ToString() == selectedItemStr);
                if (selectedDictionary != null)
                {
                    return selectedDictionary; // HERE WE DIDN't CHECK  THAT названия инструктажей не повторяется, а просто вернули первое попавшееся. Проверку бы!
                }
            }
            throw new Exception("Corresponding Dictionary not found!");

        }

        private async void PassInstruction_CheckedChanged(object sender, EventArgs e)
        {
            if (!PassInstruction.Checked) { return; }
            if (ConfirmAction("Вы прошли инструктаж?"))
            {
                MessageBox.Show("Вы согласились с прохождением инструктажа.", "Действите подтверждено", MessageBoxButtons.OK, MessageBoxIcon.Information);
                PassInstruction.Enabled = false;
                if (ListOfInstructionsForUser.SelectedItem == null)
                {
                    MessageBox.Show("Вы не выбрали инструктаж.");
                    PassInstruction.Enabled = false;
                    return;
                }
                Dictionary<string, object> selectedDict = GetDictFromSelectedInstruction(ListOfInstructionsForUser.SelectedItem.ToString());
                await SendInstructionIsPassedToDB(selectedDict);
                FilesOfInstructionCheckedListBox.Items.Clear();
            }
            else
            {
                MessageBox.Show("Вы не согласились с прохождением инструктажа.", "Действие отменено", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                PassInstruction.Checked = false;
                return;
            }
        }

        private bool ConfirmAction(string message)
        {
            var result = MessageBox.Show(message, "Подтвердить действие", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private async Task SendInstructionIsPassedToDB(Dictionary<string, object> selectedDict)
        {
            string url = SendInstructionIsPassedURL;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string jwtToken = _loginForm._jwtToken;
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                    string jsonData = System.Text.Json.JsonSerializer.Serialize(selectedDict);

                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    System.Net.Http.HttpResponseMessage response = await client.PostAsync(url, content);
                    response.EnsureSuccessStatusCode();

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Все хорошо, обновляем лист инструктажей.");
                        ListOfInstructionsForUser.Items.Clear();
                        DownloadInstructionsForUserFromServer(_userName);
                    }
                }
            }
            catch (HttpRequestException ex)
            {

                // Handle any exceptions here
                MessageBox.Show($"Error: {ex.Message}");
            }
            finally
            {
                PassInstruction.Checked = false;
            }
        }

        private void PopulateTreeView(string directoryValue, TreeNode parentNode)
        {
            // Processing directories
            string[] directoryArray = Directory.GetDirectories(directoryValue);
            try
            {
                foreach (string directory in directoryArray)
                {
                    string directoryName = Path.GetFileName(directory);
                    TreeNode myNode = new TreeNode(directoryName);
                    parentNode.Nodes.Add(myNode);
                    PopulateTreeView(directory, myNode);
                }
            }
            catch (UnauthorizedAccessException) { }

            // Processing files
            string[] fileArray = Directory.GetFiles(directoryValue);
            foreach (string file in fileArray)
            {
                string fileName = Path.GetFileName(file);
                TreeNode fileNode = new TreeNode(fileName);
                parentNode.Nodes.Add(fileNode);
            }
        }

        private void CheckAllChildNodes(TreeNode treeNode, bool nodeChecked)
        {
            foreach (TreeNode childNode in treeNode.Nodes)
            {
                if (childNode.Checked != nodeChecked)
                {
                    childNode.Checked = nodeChecked;
                    CheckAllChildNodes(childNode, nodeChecked); // Recursive call
                }
            }
        }

        private void UpdateParentNodes(TreeNode treeNode, bool nodeChecked)
        {
            TreeNode currentNode = treeNode;

            while (currentNode.Parent != null)
            {
                if (nodeChecked)
                {
                    // If the current node is checked, ensure the parent is also checked
                    currentNode.Parent.Checked = true;
                }
                else
                {
                    // If the current node is unchecked, ensure the parent is unchecked
                    // only if all its siblings are also unchecked
                    bool allSiblingsUnchecked = true;

                    foreach (TreeNode sibling in currentNode.Parent.Nodes)
                    {
                        if (sibling.Checked)
                        {
                            allSiblingsUnchecked = false;
                            break;
                        }
                    }

                    if (allSiblingsUnchecked)
                    {
                        currentNode.Parent.Checked = false;
                    }
                }

                currentNode = currentNode.Parent;
            }
        }

        public static List<string> GetSelectedFilePaths(System.Windows.Forms.TreeView treeView)
        {
            HashSet<string> uniqueFilePaths = new HashSet<string>();

            foreach (TreeNode node in treeView.Nodes)
            {
                CollectFilePaths(node, uniqueFilePaths);
            }

            // Sort the paths
            List<string> sortedFilePaths = uniqueFilePaths.ToList();
            sortedFilePaths.Sort();

            return sortedFilePaths;
        }

        private static void CollectFilePaths(TreeNode node, HashSet<string> filePaths)
        {
            if (node.Checked)
            {
                string fullPath = node.FullPath;

                // Check if it's a file (assuming leaf nodes are files)
                if (GetInfo(fullPath).type == 0)
                {
                    filePaths.Add(fullPath);
                }
            }

            foreach (TreeNode childNode in node.Nodes)
            {
                CollectFilePaths(childNode, filePaths);
            }
        }

        static Checkpath GetInfo(string path)
        {
            Checkpath checkpath = new Checkpath();
            try
            {
                FileAttributes attr = File.GetAttributes(path);

                //detect whether its a directory or file  
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    checkpath.type = Filetype.Dir;
                else
                    checkpath.type = Filetype.File;
                checkpath.Ifexists = true;
            }
            catch
            {
                bool t = Path.HasExtension(path);
                if (t)
                {
                    checkpath.type = Filetype.File;
                }
                else
                {
                    checkpath.type = Filetype.Dir;
                }

                checkpath.Ifexists = false;
            }
            return checkpath;
        }

        public class Checkpath
        {
            public bool Ifexists { get; set; }

            public Filetype type { get; set; }
        }

        public enum Filetype
        {
            File = 0,
            Dir = 1
        }

        private void FilesOfInstructionCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            this.BeginInvoke((MethodInvoker)delegate
            {
                if (AreAllItemsChecked(FilesOfInstructionCheckedListBox))
                {
                    PassInstruction.Enabled = true;
                }
                else
                {
                    PassInstruction.Enabled = false;
                }

                if (e.NewValue == CheckState.Checked)
                {
                    string selectedPath = FilesOfInstructionCheckedListBox.Items[e.Index].ToString();
                    OpenFile(selectedPath);
                }
            });

        }
        private bool AreAllItemsChecked(CheckedListBox checkedListBox)
        {
            for (int index = 0; index < checkedListBox.Items.Count; index++)
            {
                if (!checkedListBox.GetItemChecked(index))
                {
                    return false;
                }
            }
            return true;
        }

        private void OpenFile(string filePath)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не получилось открыть файл: {ex.Message}");
            }
        }

        private async void TestButtonForInstructions_Click(object sender, EventArgs e)
        {
            bool success = await FetchNotPassedInstructionsForChief();

            // Optionally fetch passed instructions if needed
            if (success)
            {
                await FetchPassedInstructionsForChief();
            }
        }

        /// <summary>
        /// Fetches not passed instructions from the server and populates the TreeView
        /// </summary>
        /// <returns>True if data was successfully fetched, false otherwise</returns>
        private async Task<bool> FetchNotPassedInstructionsForChief()
        {
            try
            {
                // Clear previous data
                dataGridViewPeopleThatNotPassedInstr.Rows.Clear();
                treeViewInstructions.Nodes.Clear(); // Using TreeView instead of ListBox

                // Show loading indicator
                Cursor = Cursors.WaitCursor;

                using (var httpClient = new HttpClient())
                {
                    // Get the JWT token from the login form
                    string jwtToken = _loginForm._jwtToken;
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                    // Make the API call to get not passed instructions
                    var response = await httpClient.GetAsync(ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get-not-passed-instructions-for-chief");

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();

                        // Deserialize the response to the new DTO format
                        var result = System.Text.Json.JsonSerializer.Deserialize<List<InstructionForChiefDto>>(
                            jsonResponse,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                        );

                        // Check for null result
                        if (result is null)
                        {
                            MessageBox.Show("Произошла ошибка при получении данных с сервера!",
                                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }

                        // Check for empty result
                        if (result.Count == 0)
                        {
                            MessageBox.Show("Похоже все инструктажи всеми пройдены!",
                                "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return true; // Consider empty result as success
                        }

                        // Store the results globally
                        instructionForChiefs_global = result;

                        // Group instructions by type
                        var instructionsByType = result.GroupBy(i => i.TypeOfInstruction ?? "Неизвестный тип");

                        // Populate the TreeView with instruction types and causes
                        foreach (var typeGroup in instructionsByType)
                        {
                            // Create a parent node for each instruction type
                            TreeNode typeNode = new TreeNode(typeGroup.Key);
                            typeNode.Tag = "Type"; // Mark as a type node

                            // Add child nodes for each instruction in this type
                            foreach (var instruction in typeGroup)
                            {
                                TreeNode causeNode = new TreeNode($"[{instruction.InstructionId}]: {instruction.CauseOfInstruction}");
                                causeNode.Tag = instruction.InstructionId; // Store instruction ID in the Tag property
                                typeNode.Nodes.Add(causeNode);
                            }

                            // Add the type node to the TreeView
                            treeViewInstructions.Nodes.Add(typeNode);
                        }

                        // Expand all nodes for better visibility
                        treeViewInstructions.ExpandAll();

                        // Optional: Select the first instruction if available
                        if (treeViewInstructions.Nodes.Count > 0 && treeViewInstructions.Nodes[0].Nodes.Count > 0)
                        {
                            treeViewInstructions.SelectedNode = treeViewInstructions.Nodes[0].Nodes[0];
                        }

                        return true;
                    }
                    else
                    {
                        // Handle error response
                        string errorMessage = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Не удалось получить данные. Код статуса: {response.StatusCode}\nОшибка: {errorMessage}",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show($"Произошла ошибка при получении непройденных инструктажей: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            finally
            {
                // Restore cursor
                Cursor = Cursors.Default;
            }
        }

        private void treeViewInstructions_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                // Clear the current data grid
                dataGridViewPeopleThatNotPassedInstr.Rows.Clear();

                // Only process selection of instruction nodes (child nodes), not type nodes (parent nodes)
                TreeNode selectedNode = e.Node;

                // Skip if it's a type node or null
                if (selectedNode == null || selectedNode.Tag is string tag && tag == "Type")
                {
                    return;
                }

                // Get the instruction ID from the node's Tag
                if (selectedNode.Tag is int instructionId)
                {
                    // Find the matching instruction
                    var matchingInstruction = instructionForChiefs_global.FirstOrDefault(i => i.InstructionId == instructionId);

                    if (matchingInstruction != null)
                    {
                        // Display instruction details in a label if needed
                        // instructionDetailsLabel.Text = $"Инструктаж: {matchingInstruction.CauseOfInstruction} (Срок до: {matchingInstruction.EndDate.ToShortDateString()})";

                        // Populate the data grid with person statuses
                        foreach (var person in matchingInstruction.Persons)
                        {
                            int rowIndex = dataGridViewPeopleThatNotPassedInstr.Rows.Add(
                                person.PersonName,
                                person.Passed ? "Да" : "Нет"
                            );

                            // Color the row based on passed status
                            dataGridViewPeopleThatNotPassedInstr.Rows[rowIndex].DefaultCellStyle.BackColor =
                                person.Passed ? Color.LightGreen : Color.LightCoral;
                        }

                        // Optionally add a status summary
                        // statusLabel.Text = $"Прошли: {matchingInstruction.Persons.Count(p => p.Passed)} из {matchingInstruction.Persons.Count}";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отображении данных инструктажа: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task FetchPassedInstructionsForChief()
        {
            try
            {
                // Clear previous data
                dataGridViewPassedInstructions.Rows.Clear();
                treeViewPassedInstructions.Nodes.Clear();

                // Show loading indicator
                Cursor = Cursors.WaitCursor;

                using (var httpClient = new HttpClient())
                {
                    // Get the JWT token from the login form
                    string jwtToken = _loginForm._jwtToken;
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                    // Make the API call to get passed instructions
                    var response = await httpClient.GetAsync(ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get-passed-instructions-for-chief");

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();

                        // Deserialize the response to the new DTO format
                        var result = System.Text.Json.JsonSerializer.Deserialize<List<InstructionForChiefDto>>(
                            jsonResponse,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                        );

                        // Check for null result
                        if (result is null)
                        {
                            MessageBox.Show("Произошла ошибка при получении данных с сервера!",
                                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        // Check for empty result
                        if (result.Count == 0)
                        {
                            MessageBox.Show("Нет пройденных инструктажей!",
                                "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                        // Store the results globally
                        passedInstructionsForChief_global = result;

                        // Group instructions by type
                        var instructionsByType = result.GroupBy(i => i.TypeOfInstruction ?? "Неизвестный тип");

                        // Populate the TreeView with instruction types and causes
                        foreach (var typeGroup in instructionsByType)
                        {
                            // Create a parent node for each instruction type
                            TreeNode typeNode = new TreeNode(typeGroup.Key);
                            typeNode.Tag = "Type"; // Mark as a type node

                            // Add child nodes for each instruction in this type, sorted by PassedPercentage in descending order
                            foreach (var instruction in typeGroup.OrderByDescending(i => i.PassedPercentage))
                            {
                                string completionStatus = instruction.IsPassedByEveryone ? "[ЗАВЕРШЕН]" : $"[{instruction.PassedPercentage:F0}%]";
                                TreeNode causeNode = new TreeNode($"{completionStatus} [{instruction.InstructionId}]: {instruction.CauseOfInstruction}");
                                causeNode.Tag = instruction.InstructionId; // Store instruction ID in the Tag property

                                // Set node color based on completion status
                                if (instruction.IsPassedByEveryone)
                                {
                                    causeNode.ForeColor = Color.Green;
                                }
                                else if (instruction.PassedPercentage >= 75)
                                {
                                    causeNode.ForeColor = Color.DarkGreen;
                                }
                                else if (instruction.PassedPercentage >= 50)
                                {
                                    causeNode.ForeColor = Color.Orange;
                                }
                                else
                                {
                                    causeNode.ForeColor = Color.DarkOrange;
                                }

                                typeNode.Nodes.Add(causeNode);
                            }

                            // Add the type node to the TreeView
                            treeViewPassedInstructions.Nodes.Add(typeNode);
                        }

                        // Expand all nodes for better visibility
                        treeViewPassedInstructions.ExpandAll();

                        // Optional: Select the first instruction if available
                        if (treeViewPassedInstructions.Nodes.Count > 0 && treeViewPassedInstructions.Nodes[0].Nodes.Count > 0)
                        {
                            treeViewPassedInstructions.SelectedNode = treeViewPassedInstructions.Nodes[0].Nodes[0];
                        }
                    }
                    else
                    {
                        // Handle error response
                        string errorMessage = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Не удалось получить данные. Код статуса: {response.StatusCode}\nОшибка: {errorMessage}",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                MessageBox.Show($"Произошла ошибка: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Restore cursor
                Cursor = Cursors.Default;
            }
        }

        private void treeViewPassedInstructions_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                // Clear the current data grid
                dataGridViewPassedInstructions.Rows.Clear();

                // Only process selection of instruction nodes (child nodes), not type nodes (parent nodes)
                TreeNode selectedNode = e.Node;

                // Skip if it's a type node or null
                if (selectedNode == null || selectedNode.Tag is string tag && tag == "Type")
                {
                    return;
                }

                // Get the instruction ID from the node's Tag
                if (selectedNode.Tag is int instructionId)
                {
                    // Find the matching instruction
                    var matchingInstruction = passedInstructionsForChief_global.FirstOrDefault(i => i.InstructionId == instructionId);

                    if (matchingInstruction != null)
                    {
                        // Display instruction details in labels if needed
                        // lblInstructionCause.Text = matchingInstruction.CauseOfInstruction;
                        // lblDateRange.Text = $"Дата: {matchingInstruction.BeginDate.ToShortDateString()} - {matchingInstruction.EndDate.ToShortDateString()}";
                        // lblCompletionStatus.Text = matchingInstruction.IsPassedByEveryone ? "Статус: Завершен" : $"Статус: В процессе ({matchingInstruction.PassedPercentage:F0}%)";

                        // Sort people by passed status and date
                        var sortedPersons = matchingInstruction.Persons
                            .OrderByDescending(p => p.Passed)
                            .ThenByDescending(p => p.DatePassed);

                        // Populate the data grid with person statuses
                        foreach (var person in sortedPersons)
                        {
                            string passedDate = person.Passed && person.DatePassed.HasValue
                                ? person.DatePassed.Value.ToString("dd.MM.yyyy HH:mm")
                                : "-";

                            int rowIndex = dataGridViewPassedInstructions.Rows.Add(
                                person.PersonName,
                                person.Passed ? "Да" : "Нет",  // IsPassed column
                                passedDate
                            );

                            // Color the row based on passed status
                            dataGridViewPassedInstructions.Rows[rowIndex].DefaultCellStyle.BackColor =
                                person.Passed ? Color.LightGreen : Color.LightCoral;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отображении данных инструктажа: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private async void RefreshTasksButton_Click(object sender, EventArgs e)
        {
            string url = RefreshTaskForChiefUrl;
            TrayOfTasksList.Items.Clear();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string jwtToken = _loginForm._jwtToken;
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                    System.Net.Http.HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var jsonResponse = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(jsonResponse))
                    {
                        MessageBox.Show("Новых заданий нет.");
                        return;
                    }

                    var tasks = JsonConvert.DeserializeObject<List<TaskDto>>(jsonResponse);

                    // Check for null or empty list
                    if (tasks != null && tasks.Count > 0)
                    {

                        // Add tasks to the ListView
                        foreach (var task in tasks)
                        {
                            TrayOfTasksList.Items.Add(task.Description);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Новых заданий нет!");
                    }

                }
            }
            catch (HttpRequestException ex)
            {

                // Handle any exceptions here
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

       

        void ExportToExcelWithSaveDialog(List<InstructionExportInstance> data)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx"; // Restrict to Excel files
                saveFileDialog.Title = "Save Excel File";               // Title of the dialog box
                saveFileDialog.FileName = "data.xlsx";                 // Default file name

                // Show the save dialog
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // This is the path where the file will be saved, chosen by the user
                    string filePath = saveFileDialog.FileName;

                    // Call a method to save the data to the chosen file path
                    ExportToExcel(data, filePath);
                }
            }
        }

        void ExportToExcel(List<InstructionExportInstance> data, string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Data");

                // Add column headers
                worksheet.Cell(1, 1).Value = "Дата проведения инструктажа по охране труда";
                worksheet.Cell(1, 2).Value = "Фамилия, имя, отчество (при наличии) работника, прошедшего инструктаж по охране труда";
                worksheet.Cell(1, 3).Value = "Профессия (должность) работника, прошедшего инструктаж по охране труда";
                worksheet.Cell(1, 4).Value = "Число, месяц, год рождения работника, прошедшего инструктаж по охране труда";
                worksheet.Cell(1, 5).Value = "Вид инструктажа по охране труда";
                worksheet.Cell(1, 6).Value = "Причина проведения инструктажа по охране труда (для внепланового или целевого инструктажа по охране труда)";
                worksheet.Cell(1, 7).Value = "Фамилия, имя отчество (при наличии), профессия (должность) работника, проводившего инструктаж по охране труда";
                worksheet.Cell(1, 8).Value = "Наименование локального акта (локальных актов), в объеме требований которого проведён инструктаж по охране труда";
                worksheet.Cell(1, 9).Value = "Подпись работника, проводившего инструктаж по охране труда";
                worksheet.Cell(1, 10).Value = "Подпись работника, прошедшего инструктаж по охране труда";


                // Enable wrapping for headers (row 1)
                for (int col = 1; col <= 10; col++)
                {
                    worksheet.Cell(1, col).Style.Alignment.WrapText = true;
                }

                // Add sample data (for debugging)
                worksheet.Cell(2, 1).Value = 1;
                worksheet.Cell(2, 2).Value = 2;
                worksheet.Cell(2, 3).Value = 3;
                worksheet.Cell(2, 4).Value = 4;
                worksheet.Cell(2, 5).Value = 5;
                worksheet.Cell(2, 6).Value = 6;
                worksheet.Cell(2, 7).Value = 7;
                worksheet.Cell(2, 8).Value = 8;
                worksheet.Cell(2, 9).Value = 9;
                worksheet.Cell(2, 10).Value = 10;


                // Add data to cells
                for (int i = 0; i < data.Count; i++)
                {
                    worksheet.Cell(i + 3, 1).Value = data[i].DateWhenPassedByEmployee;
                    worksheet.Cell(i + 3, 2).Value = data[i].FullNameOfEmployee;
                    worksheet.Cell(i + 3, 3).Value = data[i].PositionOfEmployee;
                    worksheet.Cell(i + 3, 4).Value = data[i].BirthDateOfEmployee;
                    string? instructionTypeName = InstructionTypeToName(data[i].InstructionType);
                    worksheet.Cell(i + 3, 5).Value = instructionTypeName ?? "неизвестный тип инструктажа!";

                    if (data[i].InstructionType == 1 || data[i].InstructionType == 5)
                    {
                        worksheet.Cell(i + 3, 6).Value = data[i].CauseOfInstruction;
                    }
                    else
                    {
                        worksheet.Cell(i + 3, 6).Value = "";
                    }
                    worksheet.Cell(i + 3, 7).Value = data[i].FullNameOfEmployeeWhoConductedInstruction;
                    worksheet.Cell(i + 3, 8).Value = data[i].FileNamesOfInstructionInOneString;

                    // Enable text wrapping for each row of data
                    worksheet.Row(i + 3).Style.Alignment.WrapText = true;
                }

                // Adjust font size
                worksheet.Style.Font.FontSize = 10;  // Reduced font size to fit more data

                // Auto-fit columns based on content
                worksheet.Columns().AdjustToContents();

                // Set specific column width for long text headers (if needed)
                worksheet.Column(1).Width = 10;
                worksheet.Column(2).Width = 30;
                worksheet.Column(3).Width = 25;
                worksheet.Column(4).Width = 10;
                worksheet.Column(5).Width = 15;
                worksheet.Column(6).Width = 30;// Опциональная, можно сжимать/разжимать
                worksheet.Column(7).Width = 30;
                worksheet.Column(8).Width = 20;
                worksheet.Column(9).Width = 10;
                worksheet.Column(10).Width = 10;


                // Set paper size to A4
                worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;

                // Set print settings to fit width on one page and height
                worksheet.PageSetup.PagesWide = 1; // Fit all columns to one page width
                worksheet.PageSetup.PagesTall = 1; // Fit all rows to one page height

                // Optional: Set the page orientation to landscape for wider content
                worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;

                /*// Set margins to fit more data on the page
                worksheet.PageSetup.Margins.Top = 0.5;
                worksheet.PageSetup.Margins.Bottom = 0.5;
                worksheet.PageSetup.Margins.Left = 0.5;
                worksheet.PageSetup.Margins.Right = 0.5;*/

                // Optional: Scale down the content if needed
                //worksheet.PageSetup.Scale = 85;  // Scale down the content slightly to fit more on the page

                // Save the workbook
                workbook.SaveAs(filePath);

                Console.WriteLine("Excel file created and saved.");
            }
        }


        private string? InstructionTypeToName(byte instructionType) //TODO: Убери этот хардкод и скачай с базы данных данные.
        {
            string? result = instructionType switch
            {
                0 => $"Вводный",
                1 => $"Внеплановый",
                2 => $"Первичный",
                3 => "Повторный",
                4 => $"Повторный (для водителей)",
                5 => $"Целевой",
                _ => null
            };
            return result;
        }

        private void SelectAllThePeopleInListBoxButton_Click(object sender, EventArgs e)
        {
            // Check if all people are selected
            bool allSelected = true;
            for (int i = 0; i < checkedListBoxNamesOfPeople.Items.Count; i++)
            {
                if (!checkedListBoxNamesOfPeople.GetItemChecked(i))
                {
                    allSelected = false;
                    break;
                }
            }

            // Toggle the selection
            for (int i = 0; i < checkedListBoxNamesOfPeople.Items.Count; i++)
            {
                checkedListBoxNamesOfPeople.SetItemChecked(i, !allSelected);
            }

            // Change button text based on selection state
            SelectAllThePeopleInListBoxButton.Text = allSelected ? "Выбрать всех людей" : "Не выбрать никого";
        }


        private async void SkipTheAssignmentOfInstrCheckedBox_CheckedChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.CheckBox checkBox = sender as System.Windows.Forms.CheckBox;


            if (checkBox != null && checkBox.Checked)
            {
                var selectedInstruction = ListOfUnplannedInstructions.SelectedItem;
                if (selectedInstruction is null)
                {
                    MessageBox.Show("Инструкция не выбрана!");
                    checkBox.Checked = false;
                    return;
                }
                if (ConfirmAction("Вы уверены, что хотите не назначить людей для данного инструктажа?"))
                {
                    await SkipTheAssignmentOfInstrInternal();
                }
                else
                {
                    MessageBox.Show("Вы не подтвердили действие");
                }
                checkBox.Checked = false;
            }
            else
            {
                return;
            }
        }

        private async Task SkipTheAssignmentOfInstrInternal()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    string url = SkipTheUnplannedInstructionURL;
                    string jwtToken = _loginForm._jwtToken;
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                    var selectedInstruction = ListOfUnplannedInstructions.SelectedItem;

                    Instruction instructionToSkip = unplannedInstructions_global
        .FirstOrDefault(ins => ins.cause_of_instruction == selectedInstruction.ToString());
                    string jsonData = JsonConvert.SerializeObject(instructionToSkip);

                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    // Send PUT request to the server
                    System.Net.Http.HttpResponseMessage response = await httpClient.PatchAsync(url, content);

                    if (response.IsSuccessStatusCode)
                    {

                        MessageBox.Show("Инструктаж успешно пропущен!");
                        await SyncManuallyInstrWithDBInternal();
                    }
                    else
                    {
                        MessageBox.Show($"Ошибка при пропуске инструктажа: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}");
            }
        }
        private async Task<bool> SyncNormativeInstructionNamesWithDBInternal()
        {
            try
            {
                ListOfNormativeInstrNames.Items.Clear();

                using (var httpClient = new HttpClient())
                {
                    string jwtToken = _loginForm._jwtToken;
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                    // Use the existing endpoint
                    var response = await httpClient.GetAsync(ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/normative-instructions");

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        if (string.IsNullOrWhiteSpace(responseBody))
                        {
                            throw new Exception("responseBody пуст");
                        }

                        // Deserialize the response - note the property names match what your endpoint returns
                        var result = JsonConvert.DeserializeObject<List<NormativeInstructionDto>>(responseBody);

                        if (result == null)
                        {
                            throw new Exception("Не удалось десериализовать ответ");
                        }

                        // Add items to the listbox
                        foreach (var instruction in result)
                        {
                            ListOfNormativeInstrNames.Items.Add(new ListBoxItem
                            {
                                Text = instruction.Name,
                                Value = instruction.Id
                            });
                        }

                        return true;
                    }
                    else
                    {
                        string errorMessage = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Не удалось синхронизировать нормативные инструкции. Status code: {response.StatusCode} {errorMessage}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // DTO to match the endpoint response
        public class NormativeInstructionDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Url { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        // Helper class to store both text and value in ListBox items
        public class ListBoxItem
        {
            public string Text { get; set; }
            public int Value { get; set; }

            public override string ToString()
            {
                return Text;
            }
        }

        private void ShowInstructionAssignmentManager(string instructionName)
        {
            var assignmentManager = new InstructionAssignmentManager(
                instructionName,
                ConvertItemsToEmployeeList(checkedListBoxNamesOfPeople.Items),
                ConvertItemsToNormativeInstructionsList(ListOfNormativeInstrNames.Items),
                _loginForm._jwtToken,
                urlSubmitInstructionToPeople
            );

            assignmentManager.ShowDialog();
        }

        private List<EmployeeInfo> ConvertItemsToEmployeeList(CheckedListBox.ObjectCollection items)
        {
            var result = new List<EmployeeInfo>();

            foreach (var item in items)
            {
                var tuple = DeconstructNameAndBirthDate(item.ToString());
                result.Add(new EmployeeInfo
                {
                    FullName = tuple.Item1,
                    BirthDate = tuple.Item2
                });
            }

            return result;
        }

        private List<NormativeInstructionInfo> ConvertItemsToNormativeInstructionsList(System.Windows.Forms.ListBox.ObjectCollection items)
        {
            var result = new List<NormativeInstructionInfo>();

            foreach (ListBoxItem item in items)
            {
                result.Add(new NormativeInstructionInfo
                {
                    Id = item.Value,
                    Name = item.Text
                });
            }

            return result;
        }

        #region Вкладка "Управление инструктажами"

        private async void assignInstructionToGroupsButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if an instruction is selected
                if (instructionsListView.SelectedItems.Count == 0)
                {
                    WinForms.MessageBox.Show("Пожалуйста, выберите инструктаж для назначения.");
                    return;
                }

                var selectedItem = instructionsListView.SelectedItems[0];
                string selectedInstructionName = selectedItem.SubItems[1].Text; // Cause column

                // Fetch employee data
                await SyncNamesWithDBInternal();

                // Fetch normative instruction names
                await SyncNormativeInstructionNamesWithDBInternal();

                // Show the assignment manager
                ShowInstructionAssignmentManager(selectedInstructionName);
            }
            catch (Exception ex)
            {
                WinForms.MessageBox.Show($"Ошибка: {ex.Message}", "Error", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Error);
            }
        }

        private async void btnAddInstruction_Click(object sender, EventArgs e)
        {
            // Open a form or dialog to collect instruction details
            var addInstructionForm = new AddInstructionForm(_loginForm._jwtToken);
            if (addInstructionForm.ShowDialog() == WinForms.DialogResult.OK)
            {
                var newInstruction = addInstructionForm.InstructionResult;
                await CreateInstructionOnServer(newInstruction);
                await RefreshInstructionsListView();
            }
        }

        private async void btnEditInstruction_Click(object sender, EventArgs e)
        {
            if (instructionsListView.SelectedItems.Count == 0)
            {
                WinForms.MessageBox.Show("Выберите инструктаж для редактирования.");
                return;
            }

            var selectedItem = instructionsListView.SelectedItems[0];
            var instructionId = Convert.ToInt32(selectedItem.SubItems[0].Text);
            var instruction = await GetInstructionById(instructionId);

            var editInstructionForm = new EditInstructionForm(instruction, _loginForm._jwtToken);
            if (editInstructionForm.ShowDialog() == WinForms.DialogResult.OK)
            {
                var updatedInstruction = editInstructionForm.InstructionResult;
                await UpdateInstructionOnServer(updatedInstruction);
                await RefreshInstructionsListView();
            }
        }

        private async void btnDeleteInstruction_Click(object sender, EventArgs e)
        {
            if (instructionsListView.SelectedItems.Count == 0)
            {
                WinForms.MessageBox.Show("Выберите инструктаж для удаления.");
                return;
            }

            var selectedItem = instructionsListView.SelectedItems[0];
            var instructionId = Convert.ToInt32(selectedItem.SubItems[0].Text);

            if (WinForms.MessageBox.Show(
                "Вы уверены, что хотите удалить этот инструктаж?",
                "Подтверждение удаления",
                WinForms.MessageBoxButtons.YesNo,
                WinForms.MessageBoxIcon.Warning) == WinForms.DialogResult.Yes)
            {
                await DeleteInstructionOnServer(instructionId);
                await RefreshInstructionsListView();
            }
        }

        private async Task RefreshInstructionsListView()
        {
            instructionsListView.Items.Clear();

            // Get all instructions from server (excluding unplanned ones)
            var instructions = await GetAllInstructionsFromServer();

            foreach (var instruction in instructions)
            {
                var item = new WinForms.ListViewItem(instruction.instruction_id.ToString());
                item.SubItems.Add(instruction.cause_of_instruction);
                item.SubItems.Add(GetInstructionTypeName(instruction.type_of_instruction));
                item.SubItems.Add(instruction.begin_date.ToString("dd.MM.yyyy"));
                item.SubItems.Add(instruction.end_date.ToString("dd.MM.yyyy"));
                item.SubItems.Add(instruction.is_assigned_to_people ? "Назначен" : "Не назначен");

                instructionsListView.Items.Add(item);
            }
        }
        #endregion





        #region API Calls For CRUD Operations for tab "Управление Инструктажами"

        private async Task<List<Instruction>> GetAllInstructionsFromServer()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    string jwtToken = _loginForm._jwtToken;
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                    // Create a new endpoint for getting all non-unplanned instructions
                    var response = await httpClient.GetAsync(ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get-all-instructions");

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        var instructions = JsonConvert.DeserializeObject<List<Instruction>>(responseBody);

                        // Filter out unplanned instructions (type 1)
                        return instructions.Where(i => i.type_of_instruction != 1).ToList();
                    }
                    else
                    {
                        string errorMessage = await response.Content.ReadAsStringAsync();
                        WinForms.MessageBox.Show($"Не удалось получить инструктажи. Status code: {response.StatusCode} {errorMessage}",
                            "Error", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Error);
                        return new List<Instruction>();
                    }
                }
            }
            catch (Exception ex)
            {
                WinForms.MessageBox.Show($"Произошла ошибка: {ex.Message}",
                    "Error", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Error);
                return new List<Instruction>();
            }
        }

        private async Task<Instruction> GetInstructionById(int instructionId)
        {
            using (var httpClient = new HttpClient())
            {
                string jwtToken = _loginForm._jwtToken;
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                var response = await httpClient.GetAsync(ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + $"/get-instruction/{instructionId}");

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Instruction>(responseBody);
                }

                throw new Exception($"Failed to get instruction. Status code: {response.StatusCode}");
            }
        }

        private async Task CreateInstructionOnServer(Instruction instruction)
        {
            // Reuse your existing code for creating instructions
            // Similar to buttonCreateInstruction_Click but using the instruction parameter
        }

        private async Task UpdateInstructionOnServer(Instruction instruction)
        {
            using (var httpClient = new HttpClient())
            {
                string jwtToken = _loginForm._jwtToken;
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                // Create an appropriate format that matches what the server expects
                var instructionDto = new
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
            }
        }

        private async Task DeleteInstructionOnServer(int instructionId)
        {
            using (var httpClient = new HttpClient())
            {
                string jwtToken = _loginForm._jwtToken;
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                var response = await httpClient.DeleteAsync(
                    ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + $"/delete-instruction/{instructionId}");

                if (!response.IsSuccessStatusCode)
                {
                    string errorMessage = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to delete instruction. Status code: {response.StatusCode}. Error: {errorMessage}");
                }
            }
        }

        #endregion


        private async void btnRefreshInstructions_Click(object sender, EventArgs e)
        {
            await RefreshInstructionsListView();
        }

        
    }
}
