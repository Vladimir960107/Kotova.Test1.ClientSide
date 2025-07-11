using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using MessageBox = System.Windows.MessageBox;

namespace Kotova.Test1.ClientSide.ChiefWPF
{
    /// <summary>
    /// Interaction logic for ChiefWindow.xaml
    /// </summary>
    public partial class ChiefWindow : Window
    {
        // Fields from original ChiefForm.cs
        private Login_Russian _loginForm;
        private HubConnection _connection;

        // URLs from original form
        private readonly string urlTest = "https://localhost:7048/api/test/TestEndpoint";
        private readonly string urlTaskTest = "https://localhost:7048/api/TestTasks/test-task";
        private readonly string urlCreateInstruction = "https://localhost:7048/api/instruction/create-custom-instruction";
        private readonly string urlGetInstructionsByDepartment = "https://localhost:7048/api/instruction/get-instructions-by-department";
        private readonly string urlDownloadInstructionsForUser = "https://localhost:7048/api/instruction/download-instructions-for-user";
        private readonly string urlMarkInstructionAsPassed = "https://localhost:7048/api/instruction/mark-instruction-as-passed-by-personnel";
        private readonly string urlGetAllUnplannedInstructions = "https://localhost:7048/api/instruction/get-all-unplanned-instructions";
        private readonly string urlGetInstructionById = "https://localhost:7048/api/instruction/get-instruction-by-id";
        private readonly string urlSkipUnplannedInstruction = "https://localhost:7048/api/instruction/skip-unplanned-instruction-for-personnel";

        // Collections for data binding
        public ObservableCollection<InstructionViewModel> Instructions { get; set; }
        public ObservableCollection<TaskViewModel> Tasks { get; set; }
        public ObservableCollection<UnplannedInstructionViewModel> UnplannedInstructions { get; set; }

        public ChiefWindow()
        {
            InitializeComponent();
            InitializeCollections();
            InitializeSignalR();

            // Set default end date to tomorrow
            datePickerEnd_Wpf.SelectedDate = DateTime.Now.AddDays(1);
        }

        public ChiefWindow(Login_Russian loginForm) : this()
        {
            _loginForm = loginForm;
            usernameLabel_Wpf.Text = loginForm.UsernameTextBox.Text;
        }

        private void InitializeCollections()
        {
            Instructions = new ObservableCollection<InstructionViewModel>();
            Tasks = new ObservableCollection<TaskViewModel>();
            UnplannedInstructions = new ObservableCollection<UnplannedInstructionViewModel>();

            // Set data context for binding
            instructionsListView_Wpf.ItemsSource = Instructions;
            TrayOfTasksList_Wpf.ItemsSource = Tasks;
            ListOfUnplannedInstructions_Wpf.ItemsSource = UnplannedInstructions;
        }

        private async void InitializeSignalR()
        {
            try
            {
                _connection = new HubConnectionBuilder()
                    .WithUrl("https://localhost:7048/taskshub", options =>
                    {
                        options.AccessTokenProvider = () => Task.FromResult(_loginForm?._jwtToken);
                    })
                    .Build();

                _connection.On<string>("ReceiveTaskNotification", (message) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"Новая задача: {message}", "Уведомление о задаче",
                            MessageBoxButton.OK, MessageBoxImage.Information);

                        // Refresh tasks list
                        _ = RefreshTasksAsync();
                    });
                });

                await _connection.StartAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось подключиться к SignalR hub: {ex.Message}",
                    "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        #region Event Handlers

        private async void testButton_Wpf_Click(object sender, RoutedEventArgs e)
        {
            await Test.connectionToUrlGet(urlTest, _loginForm._jwtToken);
        }

        private void LogOutButton_Wpf_Click(object sender, RoutedEventArgs e)
        {
            _loginForm.Show();
            this.Close();
        }

        private async void ChiefTabControl_Wpf_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ChiefTabControl_Wpf.SelectedIndex == 0) // Создание инструктажей tab
            {
                await SyncManuallyInstrWithDBInternal();
            }
            else if (ChiefTabControl_Wpf.SelectedIndex == 3) // Прохождение инструктажей tab
            {
                await SyncManuallyInstrWithDBInternal();
            }
        }

        #endregion

        #region Tab 1: Создание инструктажей - Event Handlers

        private async void buttonCreateInstruction_Wpf_Click(object sender, RoutedEventArgs e)
        {
            buttonCreateInstruction_Wpf.IsEnabled = false;

            try
            {
                // Basic validation
                if (string.IsNullOrWhiteSpace(InstructionTextBox_Wpf.Text))
                {
                    MessageBox.Show("Причина инструктажа пуста. Исправьте это пожалуйста.",
                        "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DateTime startTime = DateTime.Now;
                DateTime endDate = datePickerEnd_Wpf.SelectedDate ?? DateTime.Now.AddDays(1);

                if (endDate <= startTime)
                {
                    MessageBox.Show("До какой даты должно быть больше текущего времени!",
                        "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (typeOfInstructionListBox_Wpf.SelectedIndex == -1)
                {
                    MessageBox.Show("Не выбран тип инструктажа!",
                        "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Create the instruction
                string causeOfInstruction = InstructionTextBox_Wpf.Text;
                byte typeOfInstruction = (byte)(typeOfInstructionListBox_Wpf.SelectedIndex + 2);

                var instruction = new
                {
                    CauseOfInstruction = causeOfInstruction,
                    EndDate = endDate,
                    PathToInstruction = (string)null,
                    TypeOfInstruction = typeOfInstruction
                };

                var requestObject = new
                {
                    Instruction = instruction,
                    Paths = new List<string>()
                };

                string json = JsonConvert.SerializeObject(requestObject);
                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                await Test.connectionToUrlPost(urlCreateInstruction, content,
                    $"Инструктаж '{causeOfInstruction}' успешно добавлен в базу данных.", _loginForm._jwtToken);

                // Reset form
                InstructionTextBox_Wpf.Text = "";
                typeOfInstructionListBox_Wpf.SelectedIndex = -1;
                datePickerEnd_Wpf.SelectedDate = DateTime.Now.AddDays(1);

                MessageBox.Show("Инструктаж успешно создан. Используйте кнопку 'Назначить инструктаж сотрудникам' для назначения сотрудникам и выбора нормативных инструкций.",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Refresh the instructions list
                await SyncManuallyInstrWithDBInternal();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании инструктажа: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                buttonCreateInstruction_Wpf.IsEnabled = true;
            }
        }

        private async void btnRefreshInstructions_Wpf_Click(object sender, RoutedEventArgs e)
        {
            await SyncManuallyInstrWithDBInternal();
        }

        private void btnAddInstruction_Wpf_Click(object sender, RoutedEventArgs e)
        {
            // Focus on instruction creation form
            InstructionTextBox_Wpf.Focus();
        }

        private void btnEditInstruction_Wpf_Click(object sender, RoutedEventArgs e)
        {
            if (instructionsListView_Wpf.SelectedItem is InstructionViewModel selectedInstruction)
            {
                // Pre-fill form with selected instruction data
                InstructionTextBox_Wpf.Text = selectedInstruction.Cause;

                // Set instruction type based on the type
                var typeText = selectedInstruction.Type.ToLower();
                if (typeText.Contains("первичный")) typeOfInstructionListBox_Wpf.SelectedIndex = 0;
                else if (typeText.Contains("повторный") && typeText.Contains("водител")) typeOfInstructionListBox_Wpf.SelectedIndex = 2;
                else if (typeText.Contains("повторный")) typeOfInstructionListBox_Wpf.SelectedIndex = 1;
                else if (typeText.Contains("целевой")) typeOfInstructionListBox_Wpf.SelectedIndex = 3;

                if (DateTime.TryParse(selectedInstruction.EndDate, out DateTime endDate))
                {
                    datePickerEnd_Wpf.SelectedDate = endDate;
                }

                MessageBox.Show("Данные инструктажа загружены в форму. Внесите изменения и нажмите 'Внести новый инструктаж'.",
                    "Редактирование", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Выберите инструктаж для редактирования.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void btnDeleteInstruction_Wpf_Click(object sender, RoutedEventArgs e)
        {
            if (instructionsListView_Wpf.SelectedItem is InstructionViewModel selectedInstruction)
            {
                var result = MessageBox.Show($"Вы уверены, что хотите удалить инструктаж '{selectedInstruction.Cause}'?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Implementation for delete instruction API call would go here
                    MessageBox.Show("Функция удаления будет реализована в следующей версии.",
                        "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Выберите инструктаж для удаления.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void assignInstructionToGroupsButton_Wpf_Click(object sender, RoutedEventArgs e)
        {
            if (instructionsListView_Wpf.SelectedItem is InstructionViewModel selectedInstruction)
            {
                try
                {
                    // Open the InstructionAssignmentManager window
                    var assignmentWindow = new InstructionAssignmentManager(_loginForm._jwtToken, _loginForm.GetCurrentUserRole());
                    assignmentWindow.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при открытии окна назначения: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Выберите инструктаж для назначения сотрудникам.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void instructionsListView_Wpf_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Enable/disable buttons based on selection
            bool hasSelection = instructionsListView_Wpf.SelectedItem != null;
            btnEditInstruction_Wpf.IsEnabled = hasSelection;
            btnDeleteInstruction_Wpf.IsEnabled = hasSelection;
            assignInstructionToGroupsButton_Wpf.IsEnabled = hasSelection;
        }

        #endregion

        #region Tab 2: Соответствие обучения - Event Handlers

        private async void refreshReportButton_Wpf_Click(object sender, RoutedEventArgs e)
        {
            await RefreshComplianceReport();
        }

        private void exportReportButton_Wpf_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция экспорта в Excel будет реализована в следующей версии.",
                "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region Tab 3: Журнал инструктажей - Event Handlers

        private void instructionReportTreeView_Wpf_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Handle tree view selection for instruction report
            if (e.NewValue is TreeViewItem selectedItem)
            {
                MessageBox.Show($"Выбран элемент: {selectedItem.Header}",
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        #endregion

        #region Tab 4: Прохождение инструктажей - Event Handlers

        private async void buttonSyncManualyInstrWithDB_Wpf_Click(object sender, RoutedEventArgs e)
        {
            await SyncManuallyInstrWithDBInternal();
        }

        private async void ListOfUnplannedInstructions_Wpf_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListOfUnplannedInstructions_Wpf.SelectedItem is UnplannedInstructionViewModel selectedInstruction)
            {
                // Show instruction details or handle selection
                var result = MessageBox.Show($"Хотите пропустить инструктаж '{selectedInstruction.DisplayText}'?",
                    "Прохождение инструктажа", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    await SkipUnplannedInstruction(selectedInstruction.Id);
                }
            }
        }

        #endregion

        #region Footer Tasks - Event Handlers

        private async void RefreshTasksButton_Wpf_Click(object sender, RoutedEventArgs e)
        {
            await RefreshTasksAsync();
        }

        private async void button1_Wpf_Click(object sender, RoutedEventArgs e)
        {
            if (TrayOfTasksList_Wpf.SelectedItem is TaskViewModel selectedTask)
            {
                var result = MessageBox.Show($"Отметить задачу '{selectedTask.Description}' как выполненную?",
                    "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    await CompleteTask(selectedTask.Id);
                }
            }
            else
            {
                MessageBox.Show("Выберите задачу для отметки как выполненная.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        #endregion

        #region Helper Methods

        private async Task SyncManuallyInstrWithDBInternal()
        {
            try
            {
                // Clear existing instructions
                Instructions.Clear();
                UnplannedInstructions.Clear();

                // Load instructions by department
                if (_loginForm?.GetCurrentUserDepartmentId() != null)
                {
                    string departmentUrl = $"{urlGetInstructionsByDepartment}?departmentId={_loginForm.GetCurrentUserDepartmentId()}";
                    var instructionsResponse = await Test.connectionToUrlGetWithReturn(departmentUrl, _loginForm._jwtToken);

                    if (!string.IsNullOrEmpty(instructionsResponse))
                    {
                        var instructions = JsonConvert.DeserializeObject<List<dynamic>>(instructionsResponse);
                        foreach (var instr in instructions)
                        {
                            Instructions.Add(new InstructionViewModel
                            {
                                Id = instr.instruction_id,
                                Type = GetInstructionTypeName((int)instr.type_of_instruction),
                                Cause = instr.cause_of_instruction,
                                StartDate = DateTime.Parse(instr.creation_date.ToString()).ToString("dd.MM.yyyy"),
                                EndDate = DateTime.Parse(instr.end_date.ToString()).ToString("dd.MM.yyyy"),
                                AssignedStatus = "Не назначен", // This would come from API
                                CompletedStatus = "Не выполнен" // This would come from API
                            });
                        }
                    }
                }

                // Load unplanned instructions
                var unplannedResponse = await Test.connectionToUrlGetWithReturn(urlGetAllUnplannedInstructions, _loginForm._jwtToken);
                if (!string.IsNullOrEmpty(unplannedResponse))
                {
                    var unplannedInstructions = JsonConvert.DeserializeObject<List<dynamic>>(unplannedResponse);
                    foreach (var instr in unplannedInstructions)
                    {
                        UnplannedInstructions.Add(new UnplannedInstructionViewModel
                        {
                            Id = instr.instruction_id,
                            DisplayText = $"ID: {instr.instruction_id} - {instr.cause_of_instruction}"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при синхронизации с базой данных: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task RefreshComplianceReport()
        {
            try
            {
                // This would load compliance data from the API
                // For now, show placeholder message
                MessageBox.Show("Функция отчета о соответствии обучения будет реализована в следующей версии.",
                    "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении отчета: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task RefreshTasksAsync()
        {
            try
            {
                Tasks.Clear();

                var tasksResponse = await Test.connectionToUrlGetWithReturn(urlTaskTest, _loginForm._jwtToken);
                if (!string.IsNullOrEmpty(tasksResponse))
                {
                    var tasks = JsonConvert.DeserializeObject<List<dynamic>>(tasksResponse);
                    foreach (var task in tasks)
                    {
                        Tasks.Add(new TaskViewModel
                        {
                            Id = task.id ?? 0,
                            Description = task.description ?? "Без описания",
                            Status = task.status ?? "Новая"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении задач: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CompleteTask(int taskId)
        {
            try
            {
                // Implementation for completing task would go here
                MessageBox.Show($"Задача {taskId} отмечена как выполненная.",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                await RefreshTasksAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выполнении задачи: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SkipUnplannedInstruction(int instructionId)
        {
            try
            {
                string url = $"{urlSkipUnplannedInstruction}?instructionId={instructionId}&personnelId={_loginForm.GetCurrentUserPersonnelId()}";
                var response = await Test.connectionToUrlPostWithReturn(url, null, _loginForm._jwtToken);

                MessageBox.Show("Инструктаж успешно пропущен.",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                await SyncManuallyInstrWithDBInternal();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при пропуске инструктажа: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetInstructionTypeName(int typeId)
        {
            return typeId switch
            {
                2 => "Первичный",
                3 => "Повторный",
                4 => "Повторный (для водителей)",
                5 => "Целевой",
                1 => "Внеплановый",
                _ => "Неизвестный"
            };
        }

        #endregion

        #region Window Lifecycle

        protected override void OnClosed(EventArgs e)
        {
            _connection?.DisposeAsync();
            base.OnClosed(e);
        }

        #endregion
    }

    #region ViewModels for Data Binding

    public class InstructionViewModel
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Cause { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string AssignedStatus { get; set; }
        public string CompletedStatus { get; set; }
    }

    public class TaskViewModel
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
    }

    public class UnplannedInstructionViewModel
    {
        public int Id { get; set; }
        public string DisplayText { get; set; }
    }

    public class ComplianceReportViewModel
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public string DatePassed { get; set; }
        public string RequiredBy { get; set; }
    }

    #endregion
}