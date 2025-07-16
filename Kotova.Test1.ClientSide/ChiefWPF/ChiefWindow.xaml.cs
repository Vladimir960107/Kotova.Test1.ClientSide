using ClosedXML.Excel;
using Kotova.CommonClasses;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Button = System.Windows.Controls.Button;
using Color = System.Windows.Media.Color;
using MessageBox = System.Windows.MessageBox;
using Microsoft.Win32;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace Kotova.Test1.ClientSide.ChiefWPF
{
    /// <summary>
    /// Interaction logic for ChiefWindowFresh.xaml
    /// </summary>
    public partial class ChiefWindowFresh : Window
    {
        #region Fields and Properties
        // Fields from original ChiefForm.cs
        private Login_Russian _loginForm;
        private HubConnection _connection;
        private string _userName;

        private bool _isEditMode = false;
        private InstructionViewModel _editingInstruction = null;

        private List<EmployeeInfo> _cachedEmployees = new List<EmployeeInfo>();
        private List<NormativeInstructionInfo> _cachedNormativeInstructions = new List<NormativeInstructionInfo>();

        public ObservableCollection<EmployeeComplianceViewModel> NotPassedEmployees { get; set; }
        public ObservableCollection<EmployeeComplianceViewModel> PassedEmployees { get; set; }

        private List<InstructionReportItem> _cachedInstructions = new List<InstructionReportItem>();

        // Global storage for instruction data (from ChiefForm.cs)
        private List<InstructionForChiefDto> instructionForChiefs_global = new List<InstructionForChiefDto>();
        private List<InstructionForChiefDto> passedInstructionsForChief_global = new List<InstructionForChiefDto>();

        // URLs using ConfigurationClass instead of hardcoded values
        private readonly string urlTest = ConfigurationClass.BASE_URL_DEVELOPMENT + "/api/test/TestEndpoint";
        private readonly string urlTaskTest = ConfigurationClass.BASE_TASK_URL_DEVELOPMENT + "/test-task";
        private readonly string urlCreateInstruction = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/add-new-instruction-into-db";
        private readonly string urlGetInstructionsByDepartment = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get-instructions-by-department";
        private readonly string urlGetAllUnplannedInstructions = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get-all-unplanned-instructions";
        private readonly string urlSkipUnplannedInstruction = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/skip-unplanned-instruction-for-personnel";
        private readonly string urlSyncNames = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/sync-names-with-db";
        private readonly string urlSubmitInstructionToPeople = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/send-instruction-to-names";
        private readonly string urlSyncInstructions = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/sync-instructions-with-db";
        private readonly string urlGetAllInstructions = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get-all-instructions";
        private readonly string urleditInstruction = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/update-instruction";

        // Collections for data binding
        public ObservableCollection<InstructionViewModel> Instructions { get; set; }

        // Data storage
        private List<Instruction> unplannedInstructions_global;
        private List<Dictionary<string, object>> listOfInstructions_global;
        private List<Dictionary<string, object>> listsOfPaths_global;
        #endregion

        #region Constructors and Initialization
        public ChiefWindowFresh()
        {
            InitializeComponent();
            InitializeCollections();

            // Set default end date to tomorrow
            datePickerEnd_Wpf.SelectedDate = DateTime.Now.AddDays(1);
        }

        public ChiefWindowFresh(Login_Russian loginForm) : this()
        {
            _loginForm = loginForm;
            _userName = loginForm.LoginTextBox.Text;
            usernameLabel_Wpf.Text = _userName;
            InitializeSignalR();

            // Load data when window opens
            Loaded += ChiefWindowFresh_Loaded;

            NotPassedEmployees = new ObservableCollection<EmployeeComplianceViewModel>();
            PassedEmployees = new ObservableCollection<EmployeeComplianceViewModel>();
        }

        private async void ChiefWindowFresh_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Load initial data
                await LoadInstructionsFromDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке начальных данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeCollections()
        {
            Instructions = new ObservableCollection<InstructionViewModel>();

            // Set data context for binding
            instructionsListView_Wpf.ItemsSource = Instructions;

            // Initially disable management buttons
            btnEditInstruction_Wpf.IsEnabled = false;
            btnDeleteInstruction_Wpf.IsEnabled = false;
            assignInstructionToGroupsButton_Wpf.IsEnabled = false;
        }

        private async void InitializeSignalR()
        {
            try
            {
                _connection = new HubConnectionBuilder()
                    .WithUrl(ConfigurationClass.BASE_SIGNALR_CONNECTION_URL_DEVELOPMENT, options =>
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
                    });
                });

                _connection.On<string, string>("ReceiveMessage", (user, message) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"{user}: {message}", "Сообщение от Hub",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    });
                });

                _connection.On<string>("ReceiveAlert", message =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        Notifications.ShowWindowsNotification("Alert", message);
                    });
                });

                await _connection.StartAsync();
                //MessageBox.Show("Подключено к SignalR hub.", "Подключение", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось подключиться к SignalR hub: {ex.Message}", "Ошибка подключения",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Event Handlers - Header Buttons
        private async void testButton_Wpf_Click(object sender, RoutedEventArgs e)
        {
            await Test.connectionToUrlGet(urlTest, _loginForm._jwtToken);
        }

        private void LogOutButton_Wpf_Click(object sender, RoutedEventArgs e)
        {
            LogOutInternal();
        }

        private async void LogOutInternal()
        {
            try
            {
                if (_connection != null)
                {
                    await _connection.StopAsync();
                    await _connection.DisposeAsync();
                    _connection = null;
                }

                Decryption_stuff.DeleteJWTToken();
                this.Close();
                _loginForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выходе: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Event Handlers - Tab Control
        private async void ChiefTabControl_Wpf_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source != ChiefTabControl_Wpf) return;

            var selectedTab = ChiefTabControl_Wpf.SelectedItem as TabItem;
            if (selectedTab == null) return;

            // Handle tab-specific loading
            switch (selectedTab.Header.ToString())
            {
                case "📝 Создание инструктажа":
                    await LoadInstructionCreationTab();
                    break;
                case "📋 Назначение инструктажей":
                    await LoadInstructionAssignmentTab();
                    break;
                    // Add other tabs as needed
            }
        }

        private async Task LoadInstructionCreationTab()
        {
            // Instruction types are already in XAML as ListBoxItems
            // Just refresh the instructions list
            await LoadInstructionsFromDatabase();
        }

        private async Task LoadInstructionAssignmentTab()
        {
            // This would be implemented when you add the second tab
            // For now, just load instructions
            await LoadInstructionsFromDatabase();
        }
        #endregion

        #region Event Handlers - Instruction Creation Tab
        private async void buttonCreateInstruction_Wpf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(InstructionTextBox_Wpf.Text))
                {
                    MessageBox.Show("Введите причину инструктажа.", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (typeOfInstructionListBox_Wpf.SelectedIndex == -1)
                {
                    MessageBox.Show("Выберите тип инструктажа.", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (datePickerEnd_Wpf.SelectedDate == null)
                {
                    MessageBox.Show("Выберите дату окончания инструктажа.", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Disable button during processing
                buttonCreateInstruction_Wpf.IsEnabled = false;

                if (_isEditMode)
                {
                    // Update existing instruction
                    buttonCreateInstruction_Wpf.Content = "⏳ Обновление...";
                    await UpdateInstruction();
                }
                else
                {
                    // Create new instruction
                    buttonCreateInstruction_Wpf.Content = "⏳ Создание...";
                    await CreateNewInstruction();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                buttonCreateInstruction_Wpf.IsEnabled = true;
                ResetButtonStates();
            }
        }

        private async Task UpdateInstruction()
        {
            try
            {
                // Create the DTO that matches the server's expected InstructionUpdateDto
                var instructionDto = new InstructionUpdateDto
                {
                    CauseOfInstruction = InstructionTextBox_Wpf.Text,
                    EndDate = datePickerEnd_Wpf.SelectedDate.Value,
                    TypeOfInstruction = (byte)(typeOfInstructionListBox_Wpf.SelectedIndex + 2)
                };

                var json = JsonConvert.SerializeObject(instructionDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _loginForm._jwtToken);

                    // Fixed URL to match the server endpoint
                    var response = await client.PutAsync($"{ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT}/update-instruction/{_editingInstruction.Id}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Инструктаж успешно обновлен!", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        // Reset edit mode
                        _isEditMode = false;
                        _editingInstruction = null;
                        // Clear form and refresh list
                        ClearForm();
                        await LoadInstructionsFromDatabase();
                    }
                    else
                    {
                        string errorMessage = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Ошибка при обновлении инструктажа. Status: {response.StatusCode}. Error: {errorMessage}",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при обновлении инструктажа: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetButtonStates()
        {
            // Reset button states to default
            buttonCreateInstruction_Wpf.Content = "➕ Внести новый инструктаж";
            buttonCreateInstruction_Wpf.ToolTip = "Создать новый инструктаж";

            btnClearForm_Wpf.Content = "🧹 Очистить форму создания инструктажа";
            btnClearForm_Wpf.ToolTip = "Очистить все поля формы";
        }

        private void ClearForm()
        {
            // Clear all form fields
            InstructionTextBox_Wpf.Text = "";
            typeOfInstructionListBox_Wpf.SelectedIndex = -1;
            datePickerEnd_Wpf.SelectedDate = DateTime.Now.AddDays(1);

            // Focus on the instruction text box
            InstructionTextBox_Wpf.Focus();
        }

        private async Task CreateNewInstruction()
        {
            buttonCreateInstruction_Wpf.IsEnabled = false;
            try
            {
                // Basic validation
                if (string.IsNullOrWhiteSpace(InstructionTextBox_Wpf.Text))
                {
                    MessageBox.Show("Причина инструктажа пуста. Исправьте это пожалуйста.", "Ошибка валидации",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DateTime startTime = DateTime.Now;
                DateTime endDate = datePickerEnd_Wpf.SelectedDate ?? DateTime.Now.AddDays(1);

                if (endDate <= startTime)
                {
                    MessageBox.Show("До какой даты должно быть больше текущего времени!", "Ошибка валидации",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (typeOfInstructionListBox_Wpf.SelectedIndex == -1)
                {
                    MessageBox.Show("Не выбран тип инструктажа!", "Ошибка валидации",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Create the instruction DTO that matches server expectations
                string causeOfInstruction = InstructionTextBox_Wpf.Text;
                Byte typeOfInstruction = (Byte)(typeOfInstructionListBox_Wpf.SelectedIndex + 2);

                // Server expects InstructionCreateDto format
                var instructionDto = new
                {
                    CauseOfInstruction = causeOfInstruction,
                    EndDate = endDate,
                    TypeOfInstruction = typeOfInstruction
                };

                string json = JsonConvert.SerializeObject(instructionDto);
                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                // Use HttpClient for proper error handling
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _loginForm._jwtToken);

                    var response = await httpClient.PostAsync(urlCreateInstruction, content);

                    if (response.IsSuccessStatusCode)
                    {
                        // Reset form
                        InstructionTextBox_Wpf.Text = "";
                        typeOfInstructionListBox_Wpf.SelectedIndex = -1;
                        datePickerEnd_Wpf.SelectedDate = DateTime.Now.AddDays(1);

                        MessageBox.Show($"Инструктаж '{causeOfInstruction}' успешно добавлен в базу данных.", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);

                        // Refresh the instructions list
                        await LoadInstructionsFromDatabase();
                    }
                    else
                    {
                        string errorMessage = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Ошибка при создании инструктажа. Status: {response.StatusCode}. Error: {errorMessage}",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при создании инструктажа: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                buttonCreateInstruction_Wpf.IsEnabled = true;
            }
        }
        #endregion

        #region Event Handlers - Instructions Management
        private async void btnRefreshInstructions_Wpf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                if (button != null)
                {
                    button.IsEnabled = false;
                    button.Content = "⏳ Загрузка...";
                }

                await LoadInstructionsFromDatabase();

                if (button != null)
                {
                    button.Content = "🔄 Обновить";
                    button.IsEnabled = true;
                }

                MessageBox.Show("Список инструктажей обновлен.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                var button = sender as Button;
                if (button != null)
                {
                    button.Content = "🔄 Обновить";
                    button.IsEnabled = true;
                }
            }
        }

        private async void btnEditInstruction_Wpf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedInstruction = instructionsListView_Wpf.SelectedItem as InstructionViewModel;
                if (selectedInstruction == null)
                {
                    MessageBox.Show("Выберите инструктаж для редактирования.", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                if (selectedInstruction.Type == "Внеплановый")
                {
                    MessageBox.Show("Внеплановые инструктажи не могут быть редактированы!", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                // Fill the form with selected instruction data for editing
                InstructionTextBox_Wpf.Text = selectedInstruction.Cause;

                // Parse the type and set the listbox selection
                var typeText = selectedInstruction.Type;
                int typeIndex = typeText switch
                {
                    "Первичный" => 0,
                    "Повторный" => 1,
                    "Повторный (Для водителей)" => 2,
                    "Целевой" => 3,
                    _ => -1
                };
                typeOfInstructionListBox_Wpf.SelectedIndex = typeIndex;

                // Parse and set the end date
                if (DateTime.TryParse(selectedInstruction.EndDate, out DateTime endDate))
                {
                    datePickerEnd_Wpf.SelectedDate = endDate;
                }

                // Set edit mode and change button states
                _isEditMode = true;
                _editingInstruction = selectedInstruction;

                // Change button appearance and behavior
                buttonCreateInstruction_Wpf.Content = "💾 Сохранить изменения";
                buttonCreateInstruction_Wpf.ToolTip = "Сохранить изменения в инструктаже";

                btnClearForm_Wpf.Content = "↩️ Отменить редактирование";
                btnClearForm_Wpf.ToolTip = "Отменить редактирование и очистить форму";

                MessageBox.Show($"Данные инструктажа '{selectedInstruction.Cause}' загружены в форму для редактирования. Внесите изменения и нажмите 'Сохранить изменения'.",
                    "Редактирование", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных для редактирования: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnClearForm_Wpf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_isEditMode)
                {
                    // This is now "Undo Edit" functionality
                    var result = MessageBox.Show("Вы уверены, что хотите отменить редактирование? Все несохраненные изменения будут потеряны.",
                        "Отмена редактирования", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        // Reset edit mode
                        _isEditMode = false;
                        _editingInstruction = null;

                        // Reset button states
                        ResetButtonStates();

                        // Clear the form
                        ClearForm();
                    }
                }
                else
                {
                    // Normal "Add New" functionality - clear form for new instruction
                    // If we're in edit mode, reset it first
                    if (_isEditMode)
                    {
                        _isEditMode = false;
                        _editingInstruction = null;
                        ResetButtonStates();
                    }

                    // Clear the form for new instruction
                    ClearForm();

                    MessageBox.Show("Форма очищена для создания нового инструктажа.", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подготовке формы: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnDeleteInstruction_Wpf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedInstruction = instructionsListView_Wpf.SelectedItem as InstructionViewModel;
                if (selectedInstruction == null)
                {
                    MessageBox.Show("Выберите инструктаж для удаления.", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var result = MessageBox.Show($"Вы уверены, что хотите удалить инструктаж '{selectedInstruction.Cause}'?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var button = sender as Button;
                    if (button != null)
                    {
                        button.IsEnabled = false;
                        button.Content = "⏳ Удаление...";
                    }

                    await DeleteInstructionFromServer(selectedInstruction.Id);
                    await LoadInstructionsFromDatabase();

                    MessageBox.Show("Инструктаж успешно удален.", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    if (button != null)
                    {
                        button.Content = "🗑️ Удалить";
                        button.IsEnabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении инструктажа: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                var button = sender as Button;
                if (button != null)
                {
                    button.Content = "🗑️ Удалить";
                    button.IsEnabled = true;
                }
            }
        }

        private void instructionsListView_Wpf_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // Handle instruction selection for details display if needed
                var selectedInstruction = instructionsListView_Wpf.SelectedItem as InstructionViewModel;
                if (selectedInstruction != null)
                {
                    // Update details or enable/disable buttons as needed
                    btnEditInstruction_Wpf.IsEnabled = true;
                    btnDeleteInstruction_Wpf.IsEnabled = true;
                    assignInstructionToGroupsButton_Wpf.IsEnabled = true;
                }
                else
                {
                    btnEditInstruction_Wpf.IsEnabled = false;
                    btnDeleteInstruction_Wpf.IsEnabled = false;
                    assignInstructionToGroupsButton_Wpf.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обработке выбора: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void assignInstructionToGroupsButton_Wpf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedInstruction = instructionsListView_Wpf.SelectedItem as InstructionViewModel;
                if (selectedInstruction == null)
                {
                    MessageBox.Show("Выберите инструктаж для назначения.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string selectedInstructionName = selectedInstruction.Cause;
                byte instructionType = (byte)InstructionTypeMappings.GetInstructionId(selectedInstruction.Type);
                int instructionId = selectedInstruction.Id;

                // Enhanced validation for unplanned instructions
                if (instructionType == 1) // Внеплановый (unplanned)
                {
                    // Check if this instruction is ready for assignment
                    if (selectedInstruction.Tag?.ToString() == "cannot_assign")
                    {
                        MessageBox.Show(
                            "Данный внеплановый инструктаж может быть назначен сотрудникам только после того, как начальник или заместитель отдела пройдет его.\n\n" +
                            "Статус: Ожидает прохождения начальником",
                            "Инструктаж не готов к назначению",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }

                    // Get the instruction details to double-check the status
                    var instruction = await GetInstructionByIdAsync(instructionId);

                    if (instruction == null)
                    {
                        MessageBox.Show("Не удалось получить данные инструктажа.", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (!instruction.is_passed_by_chief_unplanned_instr)
                    {
                        MessageBox.Show(
                            "Внеплановый инструктаж еще не пройден начальником или заместителем отдела.\n\n" +
                            "Для назначения инструктажа сотрудникам необходимо сначала пройти его самостоятельно.",
                            "Требуется прохождение начальником",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        return;
                    }

                    // For unplanned instructions, use the specialized handler
                    await HandleUnplannedInstructionAssignmentAsync(selectedInstructionName, instructionId);
                    return;
                }

                // For regular instructions, use the existing logic
                Console.WriteLine($"Assigning instruction: {selectedInstructionName} of type: {instructionType}");

                // Fetch employee data with roles
                await SyncEmployeesWithRolesAsync();

                // Fetch normative instruction names - FILTERED for non-unplanned instructions only
                await SyncNormativeInstructionNamesAsync(isUnplannedInstruction: false);

                // Create and show the regular instruction assignment manager
                var assignmentManager = new InstructionAssignmentManager(
                    selectedInstructionName,
                    await ConvertToEmployeeListWithRolesAsync(),
                    await ConvertToNormativeInstructionsListAsync(),
                    _loginForm._jwtToken,
                    urlSubmitInstructionToPeople,
                    instructionType);

                var result = assignmentManager.ShowDialog();

                if (result == true) // WPF DialogResult.True
                {
                    // Refresh the list to show updated assignment status
                    await LoadInstructionsFromDatabase();
                    MessageBox.Show("Инструктаж успешно назначен сотрудникам!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error assigning instruction: {ex.Message}");
                MessageBox.Show($"Произошла ошибка при назначении инструктажа: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Helper Methods for Assignment

        // Enhanced ExportToExcel method adapted from ChiefForm.cs - using ClosedXML instead of OpenXML
        private void ExportToExcel(InstructionReportItem report, string filePath)
        {
            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Data");

                    // Установка шрифта Times New Roman 12 для всего листа
                    worksheet.Style.Font.FontName = "Times New Roman";
                    worksheet.Style.Font.FontSize = 12;

                    // Add column headers - exactly as in the ChiefForm method
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

                    // Добавление нумерации столбцов во вторую строку
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

                        // Колонка 1: Дата проведения инструктажа
                        worksheet.Cell(rowIndex, 1).Value = employee.HasPassed && employee.DatePassed.HasValue
                            ? employee.DatePassed.Value.ToString("dd.MM.yyyy")
                            : "";

                        // Колонка 2: ФИО работника
                        worksheet.Cell(rowIndex, 2).Value = employee.FullName;

                        // Колонка 3: Должность
                        worksheet.Cell(rowIndex, 3).Value = employee.Position;

                        // Колонка 4: Дата рождения
                        worksheet.Cell(rowIndex, 4).Value = employee.BirthDate.ToString("dd.MM.yyyy") ?? "";

                        // Колонка 5: Вид инструктажа
                        worksheet.Cell(rowIndex, 5).Value = report.TypeName ?? InstructionTypeMappings.GetInstructionName(report.TypeOfInstruction);

                        // Колонка 6: Причина (только для внепланового и целевого)
                        worksheet.Cell(rowIndex, 6).Value = (report.TypeOfInstruction == 1 || report.TypeOfInstruction == 5)
                            ? report.CauseOfInstruction
                            : "";

                        // Колонка 7: Проводивший инструктаж
                        worksheet.Cell(rowIndex, 7).Value = employee.AssignedBy;

                        // Колонка 8: Локальные акты - ENHANCED PROCESSING FOR UNPLANNED INSTRUCTIONS
                        string normativeDocumentsText = ProcessNormativeDocuments(employee.NormativeDocuments, report.TypeOfInstruction);
                        worksheet.Cell(rowIndex, 8).Value = normativeDocumentsText;

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

                    // Page setup settings...
                    worksheet.PageSetup.Margins.Left = 0.197;
                    worksheet.PageSetup.Margins.Right = 0.197;
                    worksheet.PageSetup.Margins.Top = 0.394;
                    worksheet.PageSetup.Margins.Bottom = 0.394;

                    // Column width settings...
                    worksheet.Column(1).Width = 12;
                    worksheet.Column(2).Width = 25;
                    worksheet.Column(3).Width = 20;
                    worksheet.Column(4).Width = 12;
                    worksheet.Column(5).Width = 15;
                    worksheet.Column(6).Width = 30;
                    worksheet.Column(7).Width = 25;
                    worksheet.Column(8).Width = 35; // Increased for normative documents
                    worksheet.Column(9).Width = 10;
                    worksheet.Column(10).Width = 10;

                    worksheet.PageSetup.PaperSize = XLPaperSize.A4Paper;
                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;

                    workbook.SaveAs(filePath);
                }

                MessageBox.Show($"Отчет успешно экспортирован: {filePath}", "Экспорт завершен",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при создании Excel файла: {ex.Message}");
            }
        }

        private async Task<InstructionReportItem> GetInstructionComplianceReport(int instructionId)
        {
            try
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
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при получении данных: {ex.Message}");
            }
        }

        private int GetInstructionIdFromSelectedItem(object selectedItem)
        {
            if (selectedItem is InstructionTreeItem treeItem &&
                !treeItem.IsTypeNode &&
                treeItem.InstructionId.HasValue)
            {
                return treeItem.InstructionId.Value;
            }

            return 0;
        }

        /// <summary>
        /// Processes normative documents for Excel export with special handling for unplanned instructions
        /// </summary>
        /// <param name="normativeDocuments">List of normative document names/URLs</param>
        /// <param name="instructionType">Type of instruction (1 = Внеплановый)</param>
        /// <returns>Formatted string for Excel cell</returns>
        private string ProcessNormativeDocuments(List<string> normativeDocuments, byte instructionType)
        {
            if (normativeDocuments == null || !normativeDocuments.Any())
            {
                return "";
            }

            // For unplanned instructions (Внеплановый), apply special processing
            if (instructionType == 1)
            {
                var processedDocuments = new List<string>();

                foreach (var document in normativeDocuments)
                {
                    if (string.IsNullOrWhiteSpace(document))
                        continue;

                    // Check if this document contains the "Нормативная база:" prefix and "|" separators
                    if (document.Contains("Нормативная база:") && document.Contains("|"))
                    {
                        // Remove the "Нормативная база:" prefix
                        string cleanDocument = document.Replace("Нормативная база:", "").Trim();

                        // Split by "|" and clean each part
                        var parts = cleanDocument.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(part => part.Trim())
                            .Where(part => !string.IsNullOrWhiteSpace(part))
                            .ToList();

                        processedDocuments.AddRange(parts);
                    }
                    else
                    {
                        // Regular document, add as-is
                        processedDocuments.Add(document.Trim());
                    }
                }

                // Join with semicolon and newline for Excel cell line breaks
                return string.Join(";\n", processedDocuments.Where(d => !string.IsNullOrWhiteSpace(d)));
            }
            else
            {
                // For other instruction types, use standard processing
                return string.Join("; ", normativeDocuments.Where(d => !string.IsNullOrWhiteSpace(d)));
            }
        }

        // Alternative method if you want to use Alt+Enter line breaks in Excel
        private string ProcessNormativeDocumentsWithLineBreaks(List<string> normativeDocuments, byte instructionType)
        {
            if (normativeDocuments == null || !normativeDocuments.Any())
            {
                return "";
            }

            if (instructionType == 1) // Внеплановый
            {
                var processedDocuments = new List<string>();

                foreach (var document in normativeDocuments)
                {
                    if (string.IsNullOrWhiteSpace(document))
                        continue;

                    if (document.Contains("Нормативная база:") && document.Contains("|"))
                    {
                        string cleanDocument = document.Replace("Нормативная база:", "").Trim();
                        var parts = cleanDocument.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(part => part.Trim())
                            .Where(part => !string.IsNullOrWhiteSpace(part))
                            .Select(part => $"{part};") // Add semicolon to each part
                            .ToList();

                        processedDocuments.AddRange(parts);
                    }
                    else
                    {
                        processedDocuments.Add($"{document.Trim()};");
                    }
                }

                // Use \n for line breaks in Excel (equivalent to Alt+Enter)
                return string.Join("\n", processedDocuments.Where(d => !string.IsNullOrWhiteSpace(d)));
            }
            else
            {
                return string.Join("; ", normativeDocuments.Where(d => !string.IsNullOrWhiteSpace(d)));
            }
        }

        private async Task<dynamic> GetInstructionByIdAsync(int instructionId)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    string jwtToken = _loginForm._jwtToken;
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                    var response = await httpClient.GetAsync($"{ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT}/{instructionId}");

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject(responseContent);
                    }
                    else
                    {
                        Console.WriteLine($"Failed to get instruction by ID: {response.StatusCode}");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting instruction by ID: {ex.Message}");
                return null;
            }
        }

        private async Task HandleUnplannedInstructionAssignmentAsync(string instructionName, int instructionId)
        {
            try
            {
                // Fetch employee data with roles
                await SyncEmployeesWithRolesAsync();

                // Get predetermined normative instructions for this unplanned instruction
                var predeterminedNormativeInstructions = await GetPredeterminedNormativeInstructionsForUnplannedInstructionAsync(instructionId);

                // Create and show the unplanned instruction assignment manager
                var unplannedAssignmentManager = new InstructionAssignmentManagerUnplanned(
                    instructionName,
                    instructionId,
                    await ConvertToEmployeeListWithRolesAsync(),
                    predeterminedNormativeInstructions,
                    _loginForm._jwtToken,
                    $"{ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT}/assign-unplanned-instruction-to-employees");

                var result = unplannedAssignmentManager.ShowDialog();

                if (result == true) // WPF DialogResult.True
                {
                    // Refresh the list to show updated assignment status
                    await LoadInstructionsFromDatabase();
                    MessageBox.Show("Внеплановый инструктаж успешно назначен сотрудникам!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling unplanned instruction assignment: {ex.Message}");
                MessageBox.Show($"Произошла ошибка при назначении внепланового инструктажа: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SyncEmployeesWithRolesAsync()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    string jwtToken = _loginForm._jwtToken;
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                    var response = await httpClient.GetAsync($"{ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT}/get-employees-with-roles");

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        var employees = JsonConvert.DeserializeObject<List<EmployeeInfo>>(responseContent);

                        _cachedEmployees = employees.Where(e => !string.IsNullOrWhiteSpace(e.FullName) &&
                                                               !string.IsNullOrWhiteSpace(e.BirthDate)).ToList();
                    }
                    else
                    {
                        string errorMessage = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Не получилось синхронизировать данные сотрудников. Status code: {response.StatusCode} {errorMessage}",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in SyncEmployeesWithRolesAsync: {ex}");
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SyncNormativeInstructionNamesAsync(bool isUnplannedInstruction = false)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    string jwtToken = _loginForm._jwtToken;
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                    string url = $"{ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT}/normative-instructions?isUnplannedInstruction={isUnplannedInstruction}";
                    var response = await httpClient.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        var normativeInstructions = JsonConvert.DeserializeObject<List<NormativeInstructionInfo>>(responseContent);

                        _cachedNormativeInstructions = normativeInstructions ?? new List<NormativeInstructionInfo>();
                    }
                    else
                    {
                        string errorMessage = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Не получилось синхронизировать нормативные инструкции. Status code: {response.StatusCode} {errorMessage}",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in SyncNormativeInstructionNamesAsync: {ex}");
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task<List<EmployeeInfo>> ConvertToEmployeeListWithRolesAsync()
        {
            return _cachedEmployees.ToList();
        }

        private async Task<List<NormativeInstructionInfo>> ConvertToNormativeInstructionsListAsync()
        {
            return _cachedNormativeInstructions.ToList();
        }

        private async Task<List<NormativeInstructionInfo>> GetPredeterminedNormativeInstructionsForUnplannedInstructionAsync(int instructionId)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    string jwtToken = _loginForm._jwtToken;
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                    var response = await httpClient.GetAsync($"{ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT}/get-predetermined-normative-instructions/{instructionId}");

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<List<NormativeInstructionInfo>>(responseContent) ?? new List<NormativeInstructionInfo>();
                    }
                    else
                    {
                        Console.WriteLine($"Failed to get predetermined normative instructions: {response.StatusCode}");
                        return new List<NormativeInstructionInfo>();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting predetermined normative instructions: {ex.Message}");
                return new List<NormativeInstructionInfo>();
            }
        }

        #endregion

        private async void RefreshButton_Wpf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await LoadInstructionsFromDatabase();
                LabelTray_Wpf.Text = "📋 Статус: Обновлено";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                LabelTray_Wpf.Text = "📋 Статус: Ошибка обновления";
            }
        }
        #endregion

        #region Data Loading Methods
        private async Task LoadInstructionsFromDatabase()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    string jwtToken = _loginForm._jwtToken;
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                    var response = await httpClient.GetAsync(urlGetAllInstructions);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();

                        // Check if response is empty
                        if (string.IsNullOrWhiteSpace(responseContent))
                        {
                            Instructions.Clear();
                            instructionsListView_Wpf.ItemsSource = Instructions;
                            MessageBox.Show("Нет доступных инструктажей для отображения.", "Информация",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }

                        try
                        {
                            // Deserialize as the full instruction objects from get-all-instructions endpoint
                            var serverResponse = JsonConvert.DeserializeObject<List<dynamic>>(responseContent);

                            // Clear existing instructions
                            Instructions.Clear();

                            // Convert server response to InstructionViewModel objects
                            if (serverResponse != null && serverResponse.Count > 0)
                            {
                                foreach (var item in serverResponse)
                                {
                                    var instruction = new InstructionViewModel
                                    {
                                        Id = (int)item.instruction_id,
                                        Type = InstructionTypeMappings.GetInstructionName((byte)item.type_of_instruction),
                                        Cause = item.cause_of_instruction?.ToString() ?? "N/A",
                                        StartDate = item.begin_date != null ?
                                            DateTime.Parse(item.begin_date.ToString()).ToString("dd.MM.yyyy") : "N/A",
                                        EndDate = item.end_date != null ?
                                            DateTime.Parse(item.end_date.ToString()).ToString("dd.MM.yyyy") : "N/A",
                                        AssignedStatus = GetAssignedStatusText(item),
                                        CompletedStatus = GetCompletedStatusText(item)
                                    };

                                    // Apply styling and tags based on ChiefForm.cs logic
                                    ApplyInstructionStyling(instruction, item);
                                    if (instruction.Tag == "can_assign")
                                    {
                                        Instructions.Add(instruction);
                                    }
                                }

                                // Update the ListView
                                instructionsListView_Wpf.ItemsSource = Instructions;
                            }
                            else
                            {
                                instructionsListView_Wpf.ItemsSource = Instructions;
                                MessageBox.Show("В базе данных нет неназначенных инструктажей для вашего отдела.", "Информация",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            
                        }
                        catch (JsonException jsonEx)
                        {
                            MessageBox.Show($"Ошибка при разборе ответа сервера: {jsonEx.Message}\n\nОтвет сервера: {responseContent.Substring(0, Math.Min(200, responseContent.Length))}",
                                "Ошибка JSON", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Ошибка при загрузке инструктажей. Код статуса: {response.StatusCode}\nСообщение: {errorContent}",
                            "Ошибка сервера", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (HttpRequestException httpEx)
            {
                MessageBox.Show($"Ошибка сетевого подключения: {httpEx.Message}", "Ошибка сети",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла неожиданная ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteInstructionFromServer(int instructionId)
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

        #region Helper Methods
        private string GetAssignedStatusText(dynamic item)
        {
            bool isAssigned = item.is_assigned_to_people != null && (bool)item.is_assigned_to_people;
            return isAssigned ? "Назначен" : "Не назначен";
        }

        // Helper method to get completed status text with EXACT logic from ChiefForm.cs
        private string GetCompletedStatusText(dynamic item)
        {
            bool isAssigned = item.is_assigned_to_people != null && (bool)item.is_assigned_to_people;
            bool isPassedByEveryone = item.is_passed_by_everyone != null && (bool)item.is_passed_by_everyone;
            byte instructionType = (byte)item.type_of_instruction;
            bool isPassedByChief = item.is_passed_by_chief_unplanned_instr != null && (bool)item.is_passed_by_chief_unplanned_instr;

            string completedText;

            if (instructionType == 1) // Unplanned instruction
            {
                if (!isPassedByChief)
                {
                    completedText = "Ожидает прохождения начальником";
                    // Note: In WPF, you would set background colors differently than WinForms
                    // You could store additional properties in the ViewModel for styling
                }
                else if (!isAssigned)
                {
                    completedText = "Готов к назначению сотрудникам";
                }
                else
                {
                    completedText = isPassedByEveryone ? "Завершен всеми" : "В процессе выполнения";
                }
            }
            else if (instructionType != 0) // Regular planned instruction that are not initial
            {
                if (!isAssigned)
                {
                    completedText = "Готов к назначению";
                }
                else
                {
                    completedText = isPassedByEveryone ? "Завершен всеми" : "В процессе";
                }
            }
            else // Initial instructions (type 0)
            {
                // Handle initial instructions if needed
                if (!isAssigned)
                {
                    completedText = "Готов к назначению";
                }
                else
                {
                    completedText = isPassedByEveryone ? "Завершен всеми" : "В процессе";
                }
            }

            return completedText;
        }

        // Helper method to apply styling and tags based on ChiefForm.cs logic
        private void ApplyInstructionStyling(InstructionViewModel instruction, dynamic item)
        {
            bool isAssigned = item.is_assigned_to_people != null && (bool)item.is_assigned_to_people;
            bool isPassedByEveryone = item.is_passed_by_everyone != null && (bool)item.is_passed_by_everyone;
            byte instructionType = (byte)item.type_of_instruction;
            bool isPassedByChief = item.is_passed_by_chief_unplanned_instr != null && (bool)item.is_passed_by_chief_unplanned_instr;

            if (instructionType == 1) // Unplanned instruction
            {
                if (!isPassedByChief)
                {
                    instruction.BackgroundColor = new SolidColorBrush(Color.FromRgb(240, 128, 128)); // LightCoral
                    instruction.ForegroundColor = new SolidColorBrush(Color.FromRgb(139, 0, 0)); // DarkRed
                    instruction.Tag = "cannot_assign";
                }
                else if (!isAssigned)
                {
                    // Note: Commented out in original ChiefForm.cs
                    // instruction.BackgroundColor = new SolidColorBrush(Color.FromRgb(144, 238, 144)); // LightGreen
                    // instruction.ForegroundColor = new SolidColorBrush(Color.FromRgb(0, 100, 0)); // DarkGreen
                    instruction.Tag = "can_assign";
                }
                else
                {
                    if (isPassedByEveryone)
                    {
                        instruction.BackgroundColor = new SolidColorBrush(Color.FromRgb(173, 216, 230)); // LightBlue
                        instruction.ForegroundColor = new SolidColorBrush(Color.FromRgb(0, 0, 139)); // DarkBlue
                    }
                    else
                    {
                        instruction.BackgroundColor = new SolidColorBrush(Color.FromRgb(255, 255, 224)); // LightYellow
                        instruction.ForegroundColor = new SolidColorBrush(Color.FromRgb(255, 140, 0)); // DarkOrange
                    }
                    instruction.Tag = "in_progress";
                }
            }
            else if (instructionType != 0) // Regular planned instruction that are not initial
            {
                if (!isAssigned)
                {
                    // Note: Commented out in original ChiefForm.cs
                    // instruction.BackgroundColor = new SolidColorBrush(Color.FromRgb(144, 238, 144)); // LightGreen
                    // instruction.ForegroundColor = new SolidColorBrush(Color.FromRgb(0, 100, 0)); // DarkGreen
                    instruction.Tag = "can_assign";
                }
                else
                {
                    if (isPassedByEveryone)
                    {
                        instruction.BackgroundColor = new SolidColorBrush(Color.FromRgb(173, 216, 230)); // LightBlue
                        instruction.ForegroundColor = new SolidColorBrush(Color.FromRgb(0, 0, 139)); // DarkBlue
                    }
                    else
                    {
                        instruction.BackgroundColor = new SolidColorBrush(Color.FromRgb(255, 255, 224)); // LightYellow
                        instruction.ForegroundColor = new SolidColorBrush(Color.FromRgb(255, 140, 0)); // DarkOrange
                    }
                    instruction.Tag = "in_progress";
                }
            }
            else // Initial instructions (type 0) - handle if needed
            {
                instruction.Tag = "initial_instruction";
            }
        }

        // Helper method to get instruction type text
        private string GetInstructionTypeText(byte typeId)
        {
            return typeId switch
            {
                0 => "Вводный",
                1 => "Внеплановый",
                2 => "Первичный",
                3 => "Повторный",
                4 => "Повторный (Для водителей)",
                5 => "Целевой",
                _ => "Неизвестный"
            };
        }

        #region Control Tab - Data Loading Methods (New)

        /// <summary>
        /// Fetches not passed instructions from the server and populates the TreeView
        /// Based on FetchNotPassedInstructionsForChief() from ChiefForm.cs
        /// </summary>
        private async Task<bool> FetchNotPassedInstructionsForChief()
        {
            try
            {
                // Clear previous data
                NotPassedEmployees.Clear();
                treeViewInstructions_Wpf.Items.Clear();

                using (var httpClient = new HttpClient())
                {
                    // Get the JWT token from the login form
                    string jwtToken = _loginForm._jwtToken;
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

                    // Make the API call to get not passed instructions
                    var response = await httpClient.GetAsync(ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get-not-passed-instructions-for-chief");

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();

                        // Deserialize the response to the DTO format from ChiefForm.cs
                        var result = System.Text.Json.JsonSerializer.Deserialize<List<InstructionForChiefDto>>(
                            jsonResponse,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                        );

                        if (result is null)
                        {
                            MessageBox.Show("Произошла ошибка при получении данных с сервера!",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return false;
                        }

                        if (result.Count == 0)
                        {
                            MessageBox.Show("Похоже все инструктажи всеми пройдены!",
                                "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                            return true;
                        }

                        // Store globally for later use
                        instructionForChiefs_global = result;

                        // Populate the TreeView with not passed instructions
                        PopulateNotPassedInstructionsTreeView(result);

                        return true;
                    }
                    else
                    {
                        MessageBox.Show($"Ошибка при получении данных: {response.StatusCode}",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// Fetches passed instructions from the server and populates the TreeView
        /// Based on FetchPassedInstructionsForChief() from ChiefForm.cs
        /// </summary>
        private async Task FetchPassedInstructionsForChief()
        {
            try
            {
                // Clear previous data
                PassedEmployees.Clear();
                treeViewPassedInstructions_Wpf.Items.Clear();

                using (var httpClient = new HttpClient())
                {
                    // Get the JWT token from the login form
                    string jwtToken = _loginForm._jwtToken;
                    httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);

                    // Make the API call to get passed instructions
                    var response = await httpClient.GetAsync(ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get-passed-instructions-for-chief");

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();

                        // Deserialize the response to the DTO format from ChiefForm.cs
                        var result = System.Text.Json.JsonSerializer.Deserialize<List<InstructionForChiefDto>>(
                            jsonResponse,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                        );

                        if (result is null)
                        {
                            MessageBox.Show("Произошла ошибка при получении данных с сервера!",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        if (result.Count == 0)
                        {
                            MessageBox.Show("Нет пройденных инструктажей!",
                                "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                            return;
                        }

                        // Store globally for later use
                        passedInstructionsForChief_global = result;

                        // Populate the TreeView with passed instructions
                        PopulatePassedInstructionsTreeView(result);
                    }
                    else
                    {
                        MessageBox.Show($"Ошибка при получении данных: {response.StatusCode}",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Populates the not passed instructions TreeView based on ChiefForm.cs logic
        /// </summary>
        private void PopulateNotPassedInstructionsTreeView(List<InstructionForChiefDto> instructions)
        {
            treeViewInstructions_Wpf.Items.Clear();

            // Group instructions by type (based on ChiefForm.cs logic)
            var instructionsByType = instructions
                .GroupBy(i => InstructionTypeMappings.GetInstructionName(i.TypeOfInstruction))
                .OrderBy(g => g.Key);

            foreach (var typeGroup in instructionsByType)
            {
                var typeNode = new TreeViewItem
                {
                    Header = $"{typeGroup.Key} ({typeGroup.Count()})",
                    IsExpanded = true,
                    Tag = $"TYPE_{typeGroup.Key}"
                };

                foreach (var instruction in typeGroup.OrderBy(i => i.CauseOfInstruction))
                {
                    var instructionNode = new TreeViewItem
                    {
                        Header = $"[{instruction.InstructionId}] {instruction.CauseOfInstruction}",
                        Tag = instruction
                    };

                    // Add person nodes for this instruction
                    if (instruction.Persons != null && instruction.Persons.Any())
                    {
                        foreach (var person in instruction.Persons.Where(p => !p.Passed))
                        {
                            var personNode = new TreeViewItem
                            {
                                Header = $"❌ {person.PersonName}",
                                Tag = person
                            };
                            instructionNode.Items.Add(personNode);
                        }
                    }

                    typeNode.Items.Add(instructionNode);
                }

                treeViewInstructions_Wpf.Items.Add(typeNode);
            }
        }

        /// <summary>
        /// Populates the passed instructions TreeView based on ChiefForm.cs logic
        /// </summary>
        private void PopulatePassedInstructionsTreeView(List<InstructionForChiefDto> instructions)
        {
            treeViewPassedInstructions_Wpf.Items.Clear();

            // Group instructions by type (based on ChiefForm.cs logic)
            var instructionsByType = instructions
                .GroupBy(i => InstructionTypeMappings.GetInstructionName(i.TypeOfInstruction))
                .OrderBy(g => g.Key);

            foreach (var typeGroup in instructionsByType)
            {
                var typeNode = new TreeViewItem
                {
                    Header = $"{typeGroup.Key} ({typeGroup.Count()})",
                    IsExpanded = true,
                    Tag = $"TYPE_{typeGroup.Key}"
                };

                foreach (var instruction in typeGroup.OrderBy(i => i.CauseOfInstruction))
                {
                    var passedCount = instruction.Persons?.Count(p => p.Passed) ?? 0;
                    var totalCount = instruction.Persons?.Count ?? 0;

                    var instructionNode = new TreeViewItem
                    {
                        Header = $"[{instruction.InstructionId}] {instruction.CauseOfInstruction} ({passedCount}/{totalCount})",
                        Tag = instruction
                    };

                    // Add person nodes for this instruction
                    if (instruction.Persons != null && instruction.Persons.Any())
                    {
                        foreach (var person in instruction.Persons.Where(p => p.Passed))
                        {
                            var personNode = new TreeViewItem
                            {
                                Header = $"✅ {person.PersonName} ({person.DatePassed?.ToString("dd.MM.yyyy") ?? "N/A"})",
                                Tag = person
                            };
                            instructionNode.Items.Add(personNode);
                        }
                    }

                    typeNode.Items.Add(instructionNode);
                }

                treeViewPassedInstructions_Wpf.Items.Add(typeNode);
            }
        }

        /// <summary>
        /// Updates the not passed employees DataGrid based on selected instruction
        /// Based on DisplayInstructionData() from ChiefForm.cs
        /// </summary>
        private void UpdateNotPassedEmployeesDataGrid(InstructionForChiefDto selectedInstruction)
        {
            try
            {
                NotPassedEmployees.Clear();

                if (selectedInstruction?.Persons == null) return;

                // Find people who haven't passed this instruction
                var notPassedPersons = selectedInstruction.Persons.Where(p => !p.Passed);

                foreach (var person in notPassedPersons)
                {
                    NotPassedEmployees.Add(new EmployeeComplianceViewModel
                    {
                        FullName = person.PersonName,
                        Status = "Не пройден"
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отображении данных инструктажа: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Updates the passed employees DataGrid based on selected instruction
        /// Based on ChiefForm.cs logic
        /// </summary>
        private void UpdatePassedEmployeesDataGrid(InstructionForChiefDto selectedInstruction)
        {
            try
            {
                PassedEmployees.Clear();

                if (selectedInstruction?.Persons == null) return;

                // Find people who have passed this instruction
                var passedPersons = selectedInstruction.Persons.Where(p => p.Passed);

                foreach (var person in passedPersons)
                {
                    PassedEmployees.Add(new EmployeeComplianceViewModel
                    {
                        FullName = person.PersonName,
                        IsPassed = "Да",
                        DatePassed = person.DatePassed?.ToString("dd.MM.yyyy") ?? "N/A"
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отображении данных инструктажа: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Missing Event Handlers for Control Tab

        private void treeViewInstructions_Wpf_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var selectedItem = e.NewValue as TreeViewItem;
            if (selectedItem?.Tag is InstructionForChiefDto instruction)
            {
                UpdateNotPassedEmployeesDataGrid(instruction);
            }
        }

        private void treeViewPassedInstructions_Wpf_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var selectedItem = e.NewValue as TreeViewItem;
            if (selectedItem?.Tag is InstructionForChiefDto instruction)
            {
                UpdatePassedEmployeesDataGrid(instruction);
            }
        }

        private async void refreshDataButton_Wpf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await LoadControlTab();
                MessageBox.Show("Данные обновлены.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления данных: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void exportButton_Wpf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check if an instruction is selected in the treeview
                if (treeViewInstructionReports_Wpf?.SelectedItem == null)
                {
                    MessageBox.Show("Пожалуйста, выберите конкретный инструктаж для экспорта.",
                        "Выбор инструктажа", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Get the selected instruction - you'll need to adapt this based on your TreeView item structure
                var selectedItem = treeViewInstructionReports_Wpf.SelectedItem;
                int instructionId = GetInstructionIdFromSelectedItem(selectedItem);

                if (instructionId <= 0)
                {
                    MessageBox.Show("Невозможно определить ID выбранного инструктажа.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Disable button during export
                var button = sender as Button;
                if (button != null)
                {
                    button.Content = "🔄 Экспортирование...";
                    button.IsEnabled = false;
                }

                // Get compliance report for the selected instruction
                var report = await GetInstructionComplianceReport(instructionId);

                if (report == null)
                {
                    MessageBox.Show("Не удалось получить данные о прохождении инструктажа.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Show save dialog
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx",
                    Title = "Сохранить отчет о прохождении инструктажа",
                    FileName = $"Отчет_инструктаж_{report.InstructionId}_{DateTime.Now:yyyyMMdd}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    // Export to Excel
                    ExportToExcel(report, saveFileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Re-enable button
                var button = sender as Button;
                if (button != null)
                {
                    button.Content = "📊 Экспортировать отчет";
                    button.IsEnabled = true;
                }
            }
        }

        #endregion

        private async void refreshInstructionReports_Wpf_Click(object sender, RoutedEventArgs e)
        {
            await LoadInstructionReportsData();
        }

        private void treeViewInstructionReports_Wpf_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                var selectedItem = e.NewValue as InstructionTreeItem;

                if (selectedItem != null && !selectedItem.IsTypeNode && selectedItem.InstructionId.HasValue)
                {
                    // Enable export button for instruction nodes
                    exportButton_Wpf.IsEnabled = true;

                    // Update instruction details
                    UpdateInstructionDetails(selectedItem.InstructionId.Value);
                }
                else
                {
                    // Disable export button for type nodes or invalid selections
                    exportButton_Wpf.IsEnabled = false;

                    // Clear instruction details
                    ClearInstructionDetails();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обработке выбора: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadInstructionReportsData()
        {
            try
            {
                // Clear existing data
                treeViewInstructionReports_Wpf.Items.Clear();
                exportButton_Wpf.IsEnabled = false;
                ClearInstructionDetails();

                // Create loading indicator
                var loadingItem = new InstructionTreeItem
                {
                    DisplayName = "🔄 Загрузка данных...",
                    IsTypeNode = true
                };
                treeViewInstructionReports_Wpf.Items.Add(loadingItem);

                // Disable refresh button during loading
                refreshInstructionReports_Wpf.IsEnabled = false;

                // Fetch data from server
                _cachedInstructions = await GetInstructionsWithComplianceData();

                // Clear loading indicator
                treeViewInstructionReports_Wpf.Items.Clear();

                if (_cachedInstructions == null || _cachedInstructions.Count == 0)
                {
                    var noDataItem = new InstructionTreeItem
                    {
                        DisplayName = "📭 Нет доступных инструктажей",
                        IsTypeNode = true,
                        TextColor = Brushes.Gray
                    };
                    treeViewInstructionReports_Wpf.Items.Add(noDataItem);
                    return;
                }

                // Group instructions by type
                var instructionsByType = _cachedInstructions
                    .GroupBy(i => i.TypeOfInstruction)
                    .OrderBy(g => g.Key);

                // Create tree structure
                foreach (var typeGroup in instructionsByType)
                {
                    // Create type node
                    var typeNode = new InstructionTreeItem
                    {
                        DisplayName = InstructionTypeMappings.GetInstructionName(typeGroup.Key),
                        IsTypeNode = true,
                        FontWeight = FontWeights.Bold,
                        TextColor = Brushes.DarkBlue
                    };

                    // Add instruction nodes
                    foreach (var instruction in typeGroup.OrderByDescending(i => i.BeginDate))
                    {
                        int totalEmployees = instruction.EmployeeData?.Count ?? 0;
                        int passedCount = instruction.EmployeeData?.Count(e => e.HasPassed) ?? 0;

                        string completionStatus = totalEmployees > 0
                            ? $" [{passedCount}/{totalEmployees} ({(passedCount * 100 / Math.Max(totalEmployees, 1))}%)]"
                            : " [Нет сотрудников]";

                        string nodeText = $"{instruction.CauseOfInstruction} (от {instruction.BeginDate:dd.MM.yyyy}){completionStatus}";

                        var instructionNode = new InstructionTreeItem
                        {
                            DisplayName = nodeText,
                            InstructionId = instruction.InstructionId,
                            IsTypeNode = false
                        };

                        // Set color based on completion status
                        if (totalEmployees > 0)
                        {
                            if (passedCount == totalEmployees)
                            {
                                instructionNode.TextColor = Brushes.Green; // Fully completed
                            }
                            else if (passedCount >= totalEmployees / 2)
                            {
                                instructionNode.TextColor = Brushes.DarkGreen; // More than half
                            }
                            else if (passedCount > 0)
                            {
                                instructionNode.TextColor = Brushes.DarkOrange; // Some completed
                            }
                            else
                            {
                                instructionNode.TextColor = Brushes.Red; // None completed
                            }
                        }

                        typeNode.Children.Add(instructionNode);
                    }

                    treeViewInstructionReports_Wpf.Items.Add(typeNode);
                }

                // Re-enable refresh button
                refreshInstructionReports_Wpf.IsEnabled = true;
            }
            catch (Exception ex)
            {
                // Clear tree and show error
                treeViewInstructionReports_Wpf.Items.Clear();
                var errorItem = new InstructionTreeItem
                {
                    DisplayName = "❌ Ошибка загрузки данных",
                    IsTypeNode = true,
                    TextColor = Brushes.Red
                };
                treeViewInstructionReports_Wpf.Items.Add(errorItem);

                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

                // Re-enable refresh button
                refreshInstructionReports_Wpf.IsEnabled = true;
            }
        }

        private void UpdateInstructionDetails(int instructionId)
        {
            try
            {
                var instruction = _cachedInstructions?.FirstOrDefault(i => i.InstructionId == instructionId);
                if (instruction == null)
                {
                    ClearInstructionDetails();
                    return;
                }

                // Update basic information
                instructionIdDetail_Wpf.Text = instruction.InstructionId.ToString();
                instructionTypeDetail_Wpf.Text = InstructionTypeMappings.GetInstructionName(instruction.TypeOfInstruction);
                instructionCauseDetail_Wpf.Text = instruction.CauseOfInstruction ?? "Не указана";
                instructionStartDateDetail_Wpf.Text = instruction.BeginDate.ToString("dd.MM.yyyy");
                instructionEndDateDetail_Wpf.Text = instruction.EndDate.ToString("dd.MM.yyyy") ?? "Не указана";

                // Update progress information
                int totalEmployees = instruction.EmployeeData?.Count ?? 0;
                int passedCount = instruction.EmployeeData?.Count(e => e.HasPassed) ?? 0;
                double completionPercentage = totalEmployees > 0 ? (double)passedCount / totalEmployees * 100 : 0;

                totalEmployeesDetail_Wpf.Text = totalEmployees.ToString();
                passedEmployeesDetail_Wpf.Text = passedCount.ToString();
                completionPercentageDetail_Wpf.Text = $"{completionPercentage:F1}%";

                // Update progress bar
                completionProgressBar_Wpf.Value = completionPercentage;

                // Set progress bar color based on completion
                if (completionPercentage == 100)
                {
                    completionProgressBar_Wpf.Foreground = Brushes.Green;
                }
                else if (completionPercentage >= 75)
                {
                    completionProgressBar_Wpf.Foreground = Brushes.LightGreen;
                }
                else if (completionPercentage >= 50)
                {
                    completionProgressBar_Wpf.Foreground = Brushes.Orange;
                }
                else
                {
                    completionProgressBar_Wpf.Foreground = Brushes.Red;
                }

                // Update employee list
                var employeeViewModels = instruction.EmployeeData?.Select(e => new EmployeeDetailViewModel
                {
                    FullName = e.FullName ?? "Не указано",
                    Position = e.Position ?? "Не указано",
                    HasPassed = e.HasPassed,
                    StatusText = e.HasPassed ? "✅ Пройден" : "❌ Не пройден",
                    DatePassedText = e.HasPassed && e.DatePassed.HasValue
                        ? e.DatePassed.Value.ToString("dd.MM.yyyy HH:mm")
                        : "-"
                }).ToList() ?? new List<EmployeeDetailViewModel>();

                employeeDetailsListView_Wpf.ItemsSource = employeeViewModels;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении деталей инструктажа: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                ClearInstructionDetails();
            }
        }

        // Clear instruction details panel
        private void ClearInstructionDetails()
        {
            instructionIdDetail_Wpf.Text = "-";
            instructionTypeDetail_Wpf.Text = "-";
            instructionCauseDetail_Wpf.Text = "-";
            instructionStartDateDetail_Wpf.Text = "-";
            instructionEndDateDetail_Wpf.Text = "-";

            totalEmployeesDetail_Wpf.Text = "-";
            passedEmployeesDetail_Wpf.Text = "-";
            completionPercentageDetail_Wpf.Text = "-";

            completionProgressBar_Wpf.Value = 0;
            completionProgressBar_Wpf.Foreground = Brushes.LightGray;

            employeeDetailsListView_Wpf.ItemsSource = null;
        }

        // Method to fetch instructions with compliance data from server
        private async Task<List<InstructionReportItem>> GetInstructionsWithComplianceData()
        {
            try
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
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при получении данных с сервера: {ex.Message}");
            }
        }

        // Add this method to your ChiefWindow constructor or initialization
        private async void InitializeInstructionReportsTab()
        {
            try
            {
                // Load data when the window is initialized
                await LoadInstructionReportsData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при инициализации вкладки отчетов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        #region Control Tab LoadControl

        /// <summary>
        /// Updated Control Tab loading method to use real data from ChiefForm.cs APIs
        /// </summary>
        private async Task LoadControlTab()
        {
            try
            {
                // Fetch real data from server using ChiefForm.cs methods
                bool notPassedSuccess = await FetchNotPassedInstructionsForChief();

                // Optionally fetch passed instructions if not passed were successful
                if (notPassedSuccess)
                {
                    await FetchPassedInstructionsForChief();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки вкладки контроля: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion


        #region View Models
        public class InstructionViewModel : INotifyPropertyChanged
        {
            private int _id;
            private string _type;
            private string _cause;
            private string _startDate;
            private string _endDate;
            private string _assignedStatus;
            private string _completedStatus;
            private Brush _backgroundColor;
            private Brush _foregroundColor;
            private string _tag;

            public int Id
            {
                get => _id;
                set
                {
                    _id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }

            public string Type
            {
                get => _type;
                set
                {
                    _type = value;
                    OnPropertyChanged(nameof(Type));
                }
            }

            public string Cause
            {
                get => _cause;
                set
                {
                    _cause = value;
                    OnPropertyChanged(nameof(Cause));
                }
            }

            public string StartDate
            {
                get => _startDate;
                set
                {
                    _startDate = value;
                    OnPropertyChanged(nameof(StartDate));
                }
            }

            public string EndDate
            {
                get => _endDate;
                set
                {
                    _endDate = value;
                    OnPropertyChanged(nameof(EndDate));
                }
            }

            public string AssignedStatus
            {
                get => _assignedStatus;
                set
                {
                    _assignedStatus = value;
                    OnPropertyChanged(nameof(AssignedStatus));
                }
            }

            public string CompletedStatus
            {
                get => _completedStatus;
                set
                {
                    _completedStatus = value;
                    OnPropertyChanged(nameof(CompletedStatus));
                }
            }

            // New properties for styling (similar to ChiefForm.cs BackColor/ForeColor)
            public Brush BackgroundColor
            {
                get => _backgroundColor ?? Brushes.Transparent;
                set
                {
                    _backgroundColor = value;
                    OnPropertyChanged(nameof(BackgroundColor));
                }
            }

            public Brush ForegroundColor
            {
                get => _foregroundColor ?? Brushes.Black;
                set
                {
                    _foregroundColor = value;
                    OnPropertyChanged(nameof(ForegroundColor));
                }
            }

            // Tag property for business logic (like "cannot_assign", "can_assign", etc.)
            public string Tag
            {
                get => _tag;
                set
                {
                    _tag = value;
                    OnPropertyChanged(nameof(Tag));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        public class PersonViewModel : System.ComponentModel.INotifyPropertyChanged
        {
            private bool _isSelected;

            public string FullName { get; set; }
            public string PersonnelNumber { get; set; }
            public string Department { get; set; }

            public bool IsSelected
            {
                get => _isSelected;
                set
                {
                    _isSelected = value;
                    PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(IsSelected)));
                }
            }

            public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        }

        public class NormativeInstructionViewModel : System.ComponentModel.INotifyPropertyChanged
        {
            private bool _isSelected;

            public int Id { get; set; }
            public string Name { get; set; }
            public string Url { get; set; }

            public bool IsSelected
            {
                get => _isSelected;
                set
                {
                    _isSelected = value;
                    PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(IsSelected)));
                }
            }

            public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        }
        #endregion

        public class EmployeeComplianceViewModel : INotifyPropertyChanged
        {
            private string _fullName;
            private string _status;
            private string _isPassed;
            private string _datePassed;

            public string FullName
            {
                get => _fullName;
                set { _fullName = value; OnPropertyChanged(nameof(FullName)); }
            }

            public string Status
            {
                get => _status;
                set { _status = value; OnPropertyChanged(nameof(Status)); }
            }

            public string IsPassed
            {
                get => _isPassed;
                set { _isPassed = value; OnPropertyChanged(nameof(IsPassed)); }
            }

            public string DatePassed
            {
                get => _datePassed;
                set { _datePassed = value; OnPropertyChanged(nameof(DatePassed)); }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        // TreeView item class for passed instructions
        public class InstructionTreeItem
        {
            public string DisplayName { get; set; }
            public List<InstructionTreeItem> Children { get; set; } = new List<InstructionTreeItem>();
            public int? InstructionId { get; set; }
            public bool IsTypeNode { get; set; }
            public Brush TextColor { get; set; } = Brushes.Black;
            public FontWeight FontWeight { get; set; } = FontWeights.Normal;
        }

        // Employee detail class for display
        public class EmployeeDetailViewModel
        {
            public string FullName { get; set; }
            public string Position { get; set; }
            public string StatusText { get; set; }
            public string DatePassedText { get; set; }
            public bool HasPassed { get; set; }
        }
    }
}