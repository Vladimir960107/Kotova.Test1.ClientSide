using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Irony.Parsing;
using Kotova.CommonClasses;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using Windows.UI.WindowManagement;
using Windows.Web.Http;
using Color = System.Drawing.Color;
using HttpClient = System.Net.Http.HttpClient;
using WinForms = System.Windows.Forms;
using WinFormsColor = System.Drawing.Color;
using WinFormsForm = System.Windows.Forms.Form;
using WpfColor = System.Windows.Media.Color;
using WpfWindow = System.Windows.Window;

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

        private void ExportToExcel(InstructionReportItem report)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx";
                saveFileDialog.Title = "Save Excel File";
                saveFileDialog.FileName = $"Отчет_инструктаж_{report.InstructionId}_{DateTime.Now:yyyyMMdd}.xlsx";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = saveFileDialog.FileName;

                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Data");

                        // Установка шрифта Times New Roman 12 для всего листа
                        worksheet.Style.Font.FontName = "Times New Roman";
                        worksheet.Style.Font.FontSize = 12;



                        // Add column headers - exactly as in the other method
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

                        // Устанавливаем выравнивание для заголовков (верхнее выравнивание и по центру)
                        for (int col = 1; col <= 10; col++)
                        {
                            var cell = worksheet.Cell(1, col);
                            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            cell.Style.Alignment.WrapText = true;
                            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }

                        // Добавление нумерации столбцов во вторую строку (среднее выравнивание и по центру)
                        for (int col = 1; col <= 10; col++)
                        {
                            var cell = worksheet.Cell(2, col);
                            cell.Value = col;
                            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }

                        // Сортировка сотрудников
                        var sortedEmployees = report.EmployeeData
                            .OrderBy(e => e.HasPassed ? 0 : 1)
                            .ThenBy(e => e.DatePassed)
                            .ThenBy(e => e.FullName)
                            .ToList();

                        // Добавление данных, начиная с третьей строки
                        for (int i = 0; i < sortedEmployees.Count; i++)
                        {
                            var employee = sortedEmployees[i];
                            int rowIndex = i + 3; // Начинаем с 3 строки

                            // Колонка 1: Дата проведения инструктажа - пустая для непройденных инструктажей
                            worksheet.Cell(rowIndex, 1).Value = employee.HasPassed && employee.DatePassed.HasValue
                                ? employee.DatePassed.Value.ToString("dd.MM.yyyy")
                                : ""; // Пустое значение вместо текущей даты для непройденных инструктажей

                            // Колонка 2: ФИО работника (с Alt+Enter между словами)
                            string[] nameParts = employee.FullName.Split(' ');
                            worksheet.Cell(rowIndex, 2).Value = string.Join("\n", nameParts);

                            // Колонка 3: Профессия (должность) (с Alt+Enter между словами)
                            string[] positionParts = employee.Position.Split(' ');
                            worksheet.Cell(rowIndex, 3).Value = string.Join("\n", positionParts);

                            // Колонка 4: Дата рождения
                            worksheet.Cell(rowIndex, 4).Value = employee.BirthDate.ToString("dd.MM.yyyy");

                            // Колонка 5: Вид инструктажа
                            string typeName = report.TypeName;
                            if (typeName == "Повторный (Для водителей)")
                            {
                                typeName = "Повторный";
                            }
                            worksheet.Cell(rowIndex, 5).Value = typeName;

                            // Колонка 6: Причина проведения (только для внепланового или целевого)
                            if (report.TypeOfInstruction == 1 || report.TypeOfInstruction == 5)
                            {
                                worksheet.Cell(rowIndex, 6).Value = report.CauseOfInstruction;
                            }
                            else
                            {
                                worksheet.Cell(rowIndex, 6).Value = "";
                            }

                            // Колонка 7: Проводивший инструктаж
                            worksheet.Cell(rowIndex, 7).Value = employee.AssignedBy;

                            // Колонка 8: Локальные акты
                            worksheet.Cell(rowIndex, 8).Value = string.Join("; ", employee.NormativeDocuments);

                            // Колонка 9-10: Подписи (пусто)
                            worksheet.Cell(rowIndex, 9).Value = "";
                            worksheet.Cell(rowIndex, 10).Value = "";

                            // Устанавливаем выравнивание по верхнему краю для всех ячеек в строке
                            for (int col = 1; col <= 10; col++)
                            {
                                worksheet.Cell(rowIndex, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                                worksheet.Cell(rowIndex, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                worksheet.Cell(rowIndex, col).Style.Alignment.WrapText = true;
                            }
                        }

                        // Настройка полей страницы - в ДЮЙМАХ (0.5 см ≈ 0.197 дюйма)
                        worksheet.PageSetup.Margins.Left = 0.197;  // 0.5 см в дюймах
                        worksheet.PageSetup.Margins.Right = 0.197; // 0.5 см в дюймах
                        worksheet.PageSetup.Margins.Top = 0.394;   // 1 см в дюймах
                        worksheet.PageSetup.Margins.Bottom = 0.394; // 1 см в дюймах

                        // Настройка ширины столбцов в соответствии с изображением и с учетом формата A4
                        worksheet.Column(1).Width = 12;  // Дата проведения
                        worksheet.Column(2).Width = 20;  // ФИО - немного уменьшил
                        worksheet.Column(3).Width = 12;  // Профессия (должность)
                        worksheet.Column(4).Width = 12;  // Число, месяц, год рождения - уменьшил
                        worksheet.Column(5).Width = 14;  // Вид инструктажа - уменьшил
                        worksheet.Column(6).Width = 14;  // Причина проведения
                        worksheet.Column(7).Width = 20;  // Фамилия, имя отчество проводящего - уменьшил
                        worksheet.Column(8).Width = 28;  // Наименование локального акта - уменьшил
                        worksheet.Column(9).Width = 11;   // Подпись работника, проводившего - уменьшил
                        worksheet.Column(10).Width = 11;  // Подпись работника, прошедшего - уменьшил

                        // Настройка параметров страницы
                        worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                        worksheet.PageSetup.FitToPages(1, 1); // Уместить на 1 страницу по ширине и 1 по высоте
                        worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape; // Альбомная ориентация
                        worksheet.PageSetup.ScaleHFWithDocument = true; // Масштабировать колонтитулы вместе с документом

                        // Сохранение и открытие файла
                        workbook.SaveAs(filePath);

                        MessageBox.Show($"Отчет успешно сохранен в файл: {filePath}",
                            "Экспорт завершен", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        try
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = filePath,
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
                SyncNamesWithDB.Enabled = false;
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

                        // DEBUG: Show the first few employees' exact data
                        for (int i = 0; i < Math.Min(3, result.Count); i++)
                        {
                            var emp = result[i];
                            string formatted = $"{emp.FullName} ({emp.BirthDate})";
                            MessageBox.Show($"Employee {i + 1}:\nName: '{emp.FullName}'\nBirthDate: '{emp.BirthDate}'\nFormatted: '{formatted}'",
                                "Debug Employee Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }

                        string[] resultArray = result.Select(e => $"{e.FullName} ({e.BirthDate})").ToArray();
                        checkedListBoxNamesOfPeople.Items.AddRange(resultArray);
                    }
                    else
                    {
                        string errorMessage = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Не удалось синхронизировать имена с базой данных. Status code: {response.StatusCode} {errorMessage}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SyncNamesWithDB.Enabled = true;
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
                        string itemText = item.ToString();

                        // Parse the format: "FullName (BirthDate) - Role"
                        var parts = itemText.Split(" - ");
                        string nameAndBirthDate;

                        if (parts.Length >= 2)
                        {
                            nameAndBirthDate = parts[0]; // Remove the role part
                        }
                        else
                        {
                            nameAndBirthDate = itemText; // Use as-is if no role
                        }

                        listOfNamesAndBirthDateString.Add(DeconstructNameAndBirthDate(nameAndBirthDate));
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

        /*private async void submitInstructionToPeople_Click(object sender, EventArgs e)
        {
            Console.WriteLine("=== submitInstructionToPeople_Click STARTED ===");

            submitInstructionToPeople.Enabled = false;
            try
            {
                var listOfNames = checkedListBoxNamesOfPeople.CheckedItems;
                Console.WriteLine($"Number of checked items: {listOfNames.Count}");

                List<Tuple<string, string>> listOfNamesAndBirthDateString = new List<Tuple<string, string>>();
                if (listOfNames.Count == 0)
                {
                    Console.WriteLine("ERROR: No people selected");
                    MessageBox.Show("Люди не выбраны!");
                    submitInstructionToPeople.Enabled = true;
                    return;
                }

                var selectedInstruction = ListOfUnplannedInstructions.SelectedItem;
                Console.WriteLine($"Selected instruction: {selectedInstruction?.ToString() ?? "NULL"}");

                if (selectedInstruction is null)
                {
                    Console.WriteLine("ERROR: No instruction selected");
                    MessageBox.Show("Инструкция не выбрана!");
                    submitInstructionToPeople.Enabled = true;
                    return;
                }

                // Check if at least one normative instruction is selected
                var selectedNormativeInstructions = new List<int>();
                Console.WriteLine($"Checking normative instructions. Count: {ListOfNormativeInstrNames.SelectedItems.Count}");

                foreach (ListBoxItem item in ListOfNormativeInstrNames.SelectedItems)
                {
                    Console.WriteLine($"Adding normative instruction ID: {item.Value}");
                    selectedNormativeInstructions.Add(item.Value);
                }

                if (selectedNormativeInstructions.Count == 0)
                {
                    Console.WriteLine("ERROR: No normative instructions selected");
                    MessageBox.Show("Выберите хотя бы одну нормативную инструкцию!");
                    submitInstructionToPeople.Enabled = true;
                    return;
                }

                try
                {
                    Console.WriteLine("=== Processing selected people ===");
                    int itemIndex = 0;

                    foreach (var item in listOfNames)
                    {
                        itemIndex++;
                        string itemText = item.ToString();
                        Console.WriteLine($"Processing item {itemIndex}: '{itemText}'");

                        // Parse the format: "FullName (BirthDate) - Role"
                        var parts = itemText.Split(" - ");
                        Console.WriteLine($"Split into {parts.Length} parts:");
                        for (int i = 0; i < parts.Length; i++)
                        {
                            Console.WriteLine($"  Part {i}: '{parts[i]}'");
                        }

                        string nameAndBirthDate;
                        if (parts.Length >= 2)
                        {
                            nameAndBirthDate = parts[0]; // Remove the role part
                            Console.WriteLine($"Extracted nameAndBirthDate (with role): '{nameAndBirthDate}'");
                        }
                        else
                        {
                            nameAndBirthDate = itemText; // Use as-is if no role
                            Console.WriteLine($"Using full text as nameAndBirthDate (no role): '{nameAndBirthDate}'");
                        }

                        Console.WriteLine($"About to call DeconstructNameAndBirthDate with: '{nameAndBirthDate}'");

                        try
                        {
                            var result = DeconstructNameAndBirthDate(nameAndBirthDate);
                            Console.WriteLine($"DeconstructNameAndBirthDate succeeded. Name: '{result.Item1}', Date: '{result.Item2}'");
                            listOfNamesAndBirthDateString.Add(result);
                        }
                        catch (Exception deconstructEx)
                        {
                            Console.WriteLine($"ERROR in DeconstructNameAndBirthDate: {deconstructEx.Message}");
                            Console.WriteLine($"Input was: '{nameAndBirthDate}'");
                            throw; // Re-throw to be caught by outer catch
                        }
                    }

                    Console.WriteLine($"Successfully processed {listOfNamesAndBirthDateString.Count} people");

                    string instructionNameString = selectedInstruction.ToString();
                    Console.WriteLine($"Instruction name: '{instructionNameString}'");

                    // Include the selected normative instruction IDs in the package
                    InstructionPackage package = new InstructionPackage(
                        listOfNamesAndBirthDateString,
                        instructionNameString,
                        selectedNormativeInstructions
                    );

                    Console.WriteLine("Creating package and serializing...");
                    string jsonData = JsonConvert.SerializeObject(package);
                    Console.WriteLine($"JSON data length: {jsonData.Length}");

                    string encryptedJsonData = Encryption_Kotova.EncryptString(jsonData);
                    Console.WriteLine($"Encrypted data length: {encryptedJsonData.Length}");

                    try
                    {
                        Console.WriteLine("Sending HTTP request...");
                        using (var httpClient = new HttpClient())
                        {
                            string jwtToken = _loginForm._jwtToken;
                            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                            var uri = new Uri(urlSubmitInstructionToPeople);
                            Console.WriteLine($"Target URL: {uri}");

                            var content = new StringContent(encryptedJsonData, Encoding.UTF8, "application/json");
                            var response = await httpClient.PostAsync(uri, content);

                            Console.WriteLine($"Response status code: {response.StatusCode}");

                            if (response.IsSuccessStatusCode)
                            {
                                Console.WriteLine("SUCCESS: Data sent successfully");
                                MessageBox.Show("Данные успешно отправлены на сервер и инструктаж назначен людям.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                var errorMessage = await response.Content.ReadAsStringAsync();
                                Console.WriteLine($"ERROR: Server returned error. Status: {response.StatusCode}, Message: {errorMessage}");
                                MessageBox.Show($"Не получилось отправить данные не сервер. Status code: {response.StatusCode},Error: {errorMessage} ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    catch (Exception httpEx)
                    {
                        Console.WriteLine($"ERROR in HTTP request: {httpEx.Message}");
                        Console.WriteLine($"Stack trace: {httpEx.StackTrace}");
                        MessageBox.Show($"Ошибка произошла при отправке данных: {httpEx.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        Console.WriteLine("HTTP request completed, re-enabling button and syncing...");
                        submitInstructionToPeople.Enabled = true;
                        await SyncManuallyInstrWithDBInternal();
                    }
                }
                catch (Exception processingEx)
                {
                    Console.WriteLine($"ERROR in processing loop: {processingEx.Message}");
                    Console.WriteLine($"Stack trace: {processingEx.StackTrace}");
                    MessageBox.Show($"Какая-то ошибка произошла при назначении инструктажа:{processingEx.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    Console.WriteLine("Processing completed, re-enabling button");
                    submitInstructionToPeople.Enabled = true;
                }
            }
            catch (Exception mainEx)
            {
                Console.WriteLine($"ERROR in main try block: {mainEx.Message}");
                Console.WriteLine($"Stack trace: {mainEx.StackTrace}");
                MessageBox.Show($"Неожиданная ошибка: {mainEx.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            Console.WriteLine("=== submitInstructionToPeople_Click FINISHED ===");
        }*/



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

                // Add a label to the empty tab to inform the user
                if (tabPageForPassingInstruction.Controls.Count == 0)
                {
                    var infoLabel = new System.Windows.Forms.Label
                    {
                        Text = "Окно просмотра инструктажей открыто в отдельном окне.\nЕсли окно не видно, проверьте панель задач.",
                        Dock = System.Windows.Forms.DockStyle.Fill,
                        TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                        Font = new System.Drawing.Font(this.Font.FontFamily, 12, System.Drawing.FontStyle.Regular),
                        ForeColor = System.Drawing.Color.DarkBlue
                    };
                    tabPageForPassingInstruction.Controls.Add(infoLabel);
                }

                // Don't switch tabs - let user stay on this tab
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

                // Allow completion for Chiefs, DeputyChiefs, Management, and Admins
                var role = GetCurrentUserRole();
                bool allowCompletion = IsRoleAllowedToCompleteInstructions(role);

                // Create new WPF window with appropriate settings
                var wpfWindow = new InstructionViewerWindow(
                    _loginForm._jwtToken,
                    _userName,
                    isChief: true,
                    allowCompletion: allowCompletion);

                wpfWindow.Title = $"Просмотр инструктажей - {_userName} ({TranslateRole(role)})";

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
        /// Determines if a role is allowed to complete instructions
        /// </summary>
        /// <param name="role">The user's role</param>
        /// <returns>True if the role can complete instructions, false otherwise</returns>
        private bool IsRoleAllowedToCompleteInstructions(string role)
        {
            if (string.IsNullOrEmpty(role))
                return false;

            // Roles that are allowed to complete instructions (from highest to lowest authority)
            var allowedRoles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "ChiefOfDepartment",
        "DeputyChief",
    };

            return allowedRoles.Contains(role);
        }

        // Helper method to get current user's role
        private string GetCurrentUserRole()
        {
            // Parse JWT token to extract role or get it from login form
            // This is just a placeholder - implement based on your authentication system
            return GetRoleFromToken(_loginForm._jwtToken) ?? "Unknown";
        }

        public string GetRoleFromToken(string jwtToken)
        {
            if (string.IsNullOrEmpty(jwtToken))
                return string.Empty;

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(jwtToken) as JwtSecurityToken;

                if (jsonToken == null)
                    return string.Empty;

                // Try standard role claim first
                var roleClaim = jsonToken.Claims.FirstOrDefault(claim =>
                     claim.Type == ClaimTypes.Role ||  // Full URI format
                     claim.Type == "role" ||           // Short format common in newer JWT implementations
                     claim.Type == "roles" ||          // Plural version sometimes used
                     claim.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"); // Explicit URI

                return roleClaim?.Value ?? string.Empty;
            }
            catch (Exception)
            {
                return string.Empty; // Return empty string for any parsing errors
            }
        }

        // Helper method to translate role names to Russian
        private string TranslateRole(string role)
        {
            return role switch
            {
                "ChiefOfDepartment" => "Руководитель",
                "DeputyChief" => "Заместитель",
                "Administrator" => "Администратор",
                "User" => "Пользователь",
                _ => role
            };
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
        /*private List<EmployeeInfo> ConvertItemsToEmployeeList(CheckedListBox.ObjectCollection items)
        {
            Console.WriteLine("*** ConvertItemsToEmployeeList called ***");
            var result = new List<EmployeeInfo>();

            foreach (var item in items)
            {
                Console.WriteLine($"ConvertItemsToEmployeeList processing: '{item}'");
                var tuple = DeconstructNameAndBirthDate(item.ToString());
                result.Add(new EmployeeInfo
                {
                    FullName = tuple.Item1,
                    BirthDate = tuple.Item2
                });
            }

            return result;
        }*/

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
                if (instructionsListView.SelectedItems.Count == 0)
                {
                    WinForms.MessageBox.Show("Выберите инструктаж для назначения.");
                    return;
                }

                var selectedItem = instructionsListView.SelectedItems[0];
                string selectedInstructionName = selectedItem.SubItems[2].Text; // Cause column
                byte instructionType = GetInstructionTypeFromSelectedItem(selectedItem);

                // **NEW: Check if this is an unplanned instruction and if chief has passed it**
                if (instructionType == 1) // Внеплановый (unplanned)
                {
                    // Get the instruction details to check is_passed_by_chief_unplanned_instr
                    int instructionId = Convert.ToInt32(selectedItem.SubItems[0].Text);
                    var instruction = await GetInstructionById(instructionId);

                    if (instruction == null)
                    {
                        WinForms.MessageBox.Show("Не удалось получить данные инструктажа.");
                        return;
                    }

                    if (!instruction.is_passed_by_chief_unplanned_instr)
                    {
                        WinForms.MessageBox.Show(
                            "Данный внеплановый инструктаж может быть назначен сотрудникам только после того, как начальник или заместитель отдела пройдет его.",
                            "Ограничение доступа",
                            WinForms.MessageBoxButtons.OK,
                            WinForms.MessageBoxIcon.Warning);
                        return;
                    }
                }

                // Fetch employee data with roles
                await SyncEmployeesWithRolesAsync();

                // Fetch normative instruction names
                await SyncNormativeInstructionNamesWithDBInternal();

                // Show the assignment manager with the instruction type
                ShowInstructionAssignmentManager(selectedInstructionName, instructionType);
            }
            catch (Exception ex)
            {
                WinForms.MessageBox.Show($"Ошибка: {ex.Message}", "Error", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Error);
            }
        }

        private async Task SyncEmployeesWithRolesAsync()
        {
            try
            {
                checkedListBoxNamesOfPeople.Items.Clear();

                using (var httpClient = new HttpClient())
                {
                    string jwtToken = _loginForm._jwtToken;
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                    // IMPORTANT: Set Accept-Charset header
                    httpClient.DefaultRequestHeaders.AcceptCharset.Add(new System.Net.Http.Headers.StringWithQualityHeaderValue("utf-8"));

                    var response = await httpClient.GetAsync(ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get-employees-with-roles");

                    if (response.IsSuccessStatusCode)
                    {
                        // IMPORTANT: Explicitly specify UTF-8 encoding when reading response
                        var responseBytes = await response.Content.ReadAsByteArrayAsync();
                        string responseBody = System.Text.Encoding.UTF8.GetString(responseBytes);

                        //Console.WriteLine($"Raw response body: {responseBody}");

                        var employees = JsonConvert.DeserializeObject<List<dynamic>>(responseBody);

                        if (employees == null)
                        {
                            throw new Exception("тело ответа пусто");
                        }

                        // Convert to the format expected by the UI
                        var employeeInfoList = employees
                            .Select(e => new
                            {
                                FullName = e.FullName?.ToString() ?? "",
                                BirthDate = e.BirthDate?.ToString() ?? "",
                                Role = e.Role?.ToString() ?? "User"
                            })
                            .Where(e => !string.IsNullOrWhiteSpace(e.FullName) && !string.IsNullOrWhiteSpace(e.BirthDate))
                            .ToList();

                        //Console.WriteLine($"Parsed {employeeInfoList.Count} valid employees");

                        string[] resultArray = employeeInfoList.Select(e => $"{e.FullName} ({e.BirthDate}) - {e.Role}").ToArray();

                        /*foreach (var item in resultArray)
                        {
                            Console.WriteLine($"Adding to list: '{item}'");
                        }*/

                        checkedListBoxNamesOfPeople.Items.AddRange(resultArray);
                    }
                    else
                    {
                        string errorMessage = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Не получилось синхронизировать данные сотрудников. Status code: {response.StatusCode} {errorMessage}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in SyncEmployeesWithRolesAsync: {ex}");
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Update the existing ShowInstructionAssignmentManager method
        private void ShowInstructionAssignmentManager(string instructionName, byte instructionType)
        {
            var assignmentManager = new InstructionAssignmentManager(
                instructionName,
                ConvertItemsToEmployeeList(checkedListBoxNamesOfPeople.Items),
                ConvertItemsToNormativeInstructionsList(ListOfNormativeInstrNames.Items),
                _loginForm._jwtToken,
                urlSubmitInstructionToPeople,
                instructionType  // Pass the instruction type
            );

            assignmentManager.ShowDialog();
        }


        /*private List<EmployeeInfo> ConvertItemsToEmployeeListWithRoles(CheckedListBox.ObjectCollection items)
        {
            Console.WriteLine("*** ConvertItemsToEmployeeListWithRoles called ***");
            var result = new List<EmployeeInfo>();

            foreach (var item in items)
            {
                string itemText = item.ToString();
                Console.WriteLine($"ConvertItemsToEmployeeListWithRoles processing: '{itemText}'");

                // Parse the format: "FullName (BirthDate) - Role"
                var parts = itemText.Split(" - ");
                if (parts.Length >= 2)
                {
                    string nameAndBirthDate = parts[0]; // "John Doe (2023-01-15)"
                    string role = parts[1]; // "User"

                    Console.WriteLine($"With role - nameAndBirthDate: '{nameAndBirthDate}', role: '{role}'");

                    var tuple = DeconstructNameAndBirthDate(nameAndBirthDate);
                    result.Add(new EmployeeInfo
                    {
                        FullName = tuple.Item1,
                        BirthDate = tuple.Item2,
                        Role = role
                    });
                }
                else
                {
                    Console.WriteLine($"No role found, processing directly: '{itemText}'");
                    var tuple = DeconstructNameAndBirthDate(itemText);
                    result.Add(new EmployeeInfo
                    {
                        FullName = tuple.Item1,
                        BirthDate = tuple.Item2,
                        Role = "User" // Default role
                    });
                }
            }

            return result;
        }*/
        private List<EmployeeInfo> ConvertItemsToEmployeeListWithRoles(CheckedListBox.ObjectCollection items)
        {
            var result = new List<EmployeeInfo>();

            foreach (var item in items)
            {
                string itemText = item.ToString();

                // Parse the format: "FullName (BirthDate) - Role"
                var parts = itemText.Split(" - ");
                if (parts.Length >= 2)
                {
                    string nameAndBirthDate = parts[0]; // "John Doe (2023-01-15)"
                    string role = parts[1]; // "User"

                    // Now use the original DeconstructNameAndBirthDate method on the clean nameAndBirthDate
                    var tuple = DeconstructNameAndBirthDate(nameAndBirthDate);
                    result.Add(new EmployeeInfo
                    {
                        FullName = tuple.Item1,
                        BirthDate = tuple.Item2,
                        Role = role
                    });
                }
                else
                {
                    // Fallback for old format without role
                    var tuple = DeconstructNameAndBirthDate(itemText);
                    result.Add(new EmployeeInfo
                    {
                        FullName = tuple.Item1,
                        BirthDate = tuple.Item2,
                        Role = "User" // Default role
                    });
                }
            }

            return result;
        }

        private Tuple<string, string> DeconstructNameAndBirthDate(string? nameWithBirthDate)
        {
            if (string.IsNullOrWhiteSpace(nameWithBirthDate))
            {
                throw new ArgumentException("nameWithBirthDate is null or empty! in DeconstructNameAndBirthDate");
            }

            // Check for obviously malformed input
            if (nameWithBirthDate.Trim() == "()" || nameWithBirthDate.Trim().StartsWith("()"))
            {
                throw new ArgumentException($"nameWithBirthDate '{nameWithBirthDate}' appears to have missing name or birth date data!");
            }

            string pattern = @"^(.+?)\s\((\d{4}-\d{2}-\d{2})\)$";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(nameWithBirthDate);

            if (match.Success)
            {
                string fullName = match.Groups[1].Value.Trim();
                string birthDate = match.Groups[2].Value;

                // Additional validation
                if (string.IsNullOrWhiteSpace(fullName))
                {
                    throw new ArgumentException($"Extracted name is empty from '{nameWithBirthDate}'");
                }

                return System.Tuple.Create(fullName, birthDate);
            }
            else
            {
                throw new ArgumentException($"nameWithBirthDate '{nameWithBirthDate}' doesn't match the expected pattern 'Name (YYYY-MM-DD)'!");
            }
        }

        /*private Tuple<string, string> DeconstructNameAndBirthDate(string? nameWithBirthDate)
        {
            // This should ALWAYS show when the method is called
            Console.WriteLine($"*** DeconstructNameAndBirthDate called with: '{nameWithBirthDate ?? "NULL"}' ***");
            System.Diagnostics.Debug.WriteLine($"*** DeconstructNameAndBirthDate called with: '{nameWithBirthDate ?? "NULL"}' ***");

            if (nameWithBirthDate == null)
            {
                Console.WriteLine("ERROR: Input is null");
                throw new ArgumentException("nameWithBirthDate is null! in DeconstructNameAndBirthDate");
            }

            // Check for obviously malformed input
            if (nameWithBirthDate.Trim() == "()" || nameWithBirthDate.Trim().StartsWith("()"))
            {
                Console.WriteLine($"ERROR: Input appears malformed: '{nameWithBirthDate}'");
                throw new ArgumentException($"nameWithBirthDate '{nameWithBirthDate}' appears to have missing name or birth date data!");
            }

            string pattern = @"^(.+?)\s\((\d{4}-\d{2}-\d{2})\)$";
            Regex regex = new Regex(pattern);
            Match match = regex.Match(nameWithBirthDate);

            if (match.Success)
            {
                string fullName = match.Groups[1].Value;
                string birthDate = match.Groups[2].Value;
                Console.WriteLine($"SUCCESS: Name: '{fullName}', Date: '{birthDate}'");
                return System.Tuple.Create(fullName, birthDate);
            }
            else
            {
                Console.WriteLine($"ERROR: Pattern mismatch. Input: '{nameWithBirthDate}'");
                throw new ArgumentException($"nameWithBirthDate '{nameWithBirthDate}' doesn't match the pattern! Expected format: 'Name (YYYY-MM-DD)'");
            }
        }*/


        // Helper method to get instruction type from selected item
        private byte GetInstructionTypeFromSelectedItem(WinForms.ListViewItem item)
        {
            // Now the columns are in the correct order:
            // 0: ID, 1: Type, 2: Cause, 3: Start Date, 4: End Date, 5: Assigned, 6: Completed
            string typeText = item.SubItems[1].Text; // Type column (now correct)

            return typeText switch
            {
                "Вводный" => 0,
                "Внеплановый" => 1,
                "Первичный" => 2,
                "Повторный" => 3,
                "Повторный (для водителей)" => 4,
                "Целевой" => 5,
                _ => 255 // Default value
            };
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

            // Get all instructions from server
            var instructions = await GetAllInstructionsFromServer();

            foreach (var instruction in instructions)
            {
                var item = new WinForms.ListViewItem(instruction.instruction_id.ToString());

                // Add items in the correct order to match column headers
                item.SubItems.Add(GetInstructionTypeName(instruction.type_of_instruction)); // Column 1: Type
                item.SubItems.Add(instruction.cause_of_instruction);  // Column 2: Cause
                item.SubItems.Add(instruction.begin_date.ToString("dd.MM.yyyy")); // Column 3: Start Date
                item.SubItems.Add(instruction.end_date.ToString("dd.MM.yyyy")); // Column 4: End Date
                item.SubItems.Add(instruction.is_assigned_to_people ? "Назначен" : "Не назначен"); // Column 5: Assigned

                // Column 6: Completed - with special handling for unplanned instructions
                string completedText;
                if (instruction.type_of_instruction == 1) // Unplanned instruction
                {
                    if (!instruction.is_passed_by_chief_unplanned_instr)
                    {
                        completedText = "Не пройден начальником";
                        item.BackColor = Color.LightCoral; // Red background for unpassable instructions
                        item.ForeColor = Color.DarkRed;
                    }
                    else if (!instruction.is_assigned_to_people)
                    {
                        completedText = "Готов к назначению";
                        item.BackColor = Color.LightGreen; // Green background for assignable instructions
                        item.ForeColor = Color.DarkGreen;
                    }
                    else
                    {
                        completedText = instruction.is_passed_by_everyone ? "Завершен всеми" : "В процессе";
                        item.BackColor = instruction.is_passed_by_everyone ? Color.LightBlue : Color.LightYellow;
                    }
                }
                else
                {
                    completedText = instruction.is_passed_by_everyone ? "Завершен" : "В процессе";
                }

                item.SubItems.Add(completedText);

                instructionsListView.Items.Add(item);
            }
        }
        #endregion

        public async Task<List<EmployeeInfo>> GetEmployeesByRoleAsync(string roleFilter = null)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    string jwtToken = _loginForm._jwtToken;
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                    var response = await httpClient.GetAsync(ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get-employees-with-roles");

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        var employees = JsonConvert.DeserializeObject<List<EmployeeInfo>>(responseBody);

                        // Filter by role if specified
                        if (!string.IsNullOrEmpty(roleFilter))
                        {
                            employees = employees.Where(e => e.Role == roleFilter).ToList();
                        }

                        return employees;
                    }
                    else
                    {
                        MessageBox.Show($"Failed to get employees: {response.StatusCode}");
                        return new List<EmployeeInfo>();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                return new List<EmployeeInfo>();
            }
        }





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
    }




    #region OLD PARTS OF CODE, THAT ARE NOT IN USE ANYMORE


    #region Passing instructions as User by Chief
    /*private async Task<bool> DownloadInstructionsForUserFromServer(string? userName) // по факту эта функция должна быть вместе с в User.cs в совершенно отдельном файле.
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
        }*/

    #endregion

    #endregion
}
