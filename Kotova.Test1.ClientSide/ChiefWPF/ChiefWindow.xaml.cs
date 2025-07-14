using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using Kotova.CommonClasses;
using MessageBox = System.Windows.MessageBox;
using Button = System.Windows.Controls.Button;

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

        private void btnAddInstruction_Wpf_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Clear the form for new instruction
                InstructionTextBox_Wpf.Text = "";
                typeOfInstructionListBox_Wpf.SelectedIndex = -1;
                datePickerEnd_Wpf.SelectedDate = DateTime.Now.AddDays(1);

                // Focus on the instruction text box
                InstructionTextBox_Wpf.Focus();

                MessageBox.Show("Форма очищена для создания нового инструктажа.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подготовке формы: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
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

                // Fill the form with selected instruction data for editing
                InstructionTextBox_Wpf.Text = selectedInstruction.Cause;

                // Parse the type and set the listbox selection
                var typeText = selectedInstruction.Type;
                int typeIndex = typeText switch
                {
                    "Первичный" => 0,
                    "Повторный" => 1,
                    "Повторный (для водителей)" => 2,
                    "Целевой" => 3,
                    _ => -1
                };
                typeOfInstructionListBox_Wpf.SelectedIndex = typeIndex;

                // Parse and set the end date
                if (DateTime.TryParse(selectedInstruction.EndDate, out DateTime endDate))
                {
                    datePickerEnd_Wpf.SelectedDate = endDate;
                }

                MessageBox.Show($"Данные инструктажа '{selectedInstruction.Cause}' загружены в форму для редактирования. Внесите изменения и нажмите 'Внести новый инструктаж'.",
                    "Редактирование", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных для редактирования: {ex.Message}", "Ошибка",
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
                    MessageBox.Show("Выберите инструктаж для назначения сотрудникам.", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // TODO: Open assignment dialog or navigate to assignment tab
                MessageBox.Show($"Назначение инструктажа '{selectedInstruction.Cause}' сотрудникам будет реализовано позже.",
                    "В разработке", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при назначении инструктажа: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

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

                    var response = await httpClient.GetAsync(urlSyncInstructions);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseContent = await response.Content.ReadAsStringAsync();

                        // Debug: Show what we received
                        MessageBox.Show($"Server response length: {responseContent.Length} characters\nFirst 500 chars: {responseContent.Substring(0, Math.Min(500, responseContent.Length))}",
                            "Debug: Server Response", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Try to deserialize as the expected format first
                        try
                        {
                            var result = JsonConvert.DeserializeObject<List<Instruction>>(responseContent);

                            MessageBox.Show($"Deserialized {result?.Count ?? 0} instructions as Instruction objects",
                                "Debug: Deserialization", MessageBoxButton.OK, MessageBoxImage.Information);

                            if (result != null && result.Count > 0)
                            {
                                unplannedInstructions_global = result;

                                // Update UI
                                Instructions.Clear();
                                foreach (var instruction in result)
                                {
                                    Instructions.Add(new InstructionViewModel
                                    {
                                        Id = instruction.instruction_id,
                                        Cause = instruction.cause_of_instruction ?? "No Cause",
                                        Type = InstructionTypeToName(instruction.type_of_instruction),
                                        StartDate = instruction.begin_date.ToString("dd.MM.yyyy") ?? "Не указано",
                                        EndDate = instruction.end_date.ToString("dd.MM.yyyy") ?? "Не указано",
                                        AssignedStatus = "Готов к назначению",
                                        CompletedStatus = "Ожидает назначения"
                                    });
                                }

                                MessageBox.Show($"Added {Instructions.Count} items to ObservableCollection",
                                    "Debug: UI Update", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show("Server returned empty list or null",
                                    "Debug: Empty Result", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                        catch (JsonException jsonEx)
                        {
                            // If it fails, try deserializing as dynamic to see the actual structure
                            try
                            {
                                var dynamicResult = JsonConvert.DeserializeObject(responseContent);
                                MessageBox.Show($"JSON deserialization failed for Instruction type. Raw structure type: {dynamicResult?.GetType().Name}\nError: {jsonEx.Message}",
                                    "Debug: JSON Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            catch
                            {
                                MessageBox.Show($"Complete JSON parsing failure: {jsonEx.Message}\nResponse: {responseContent}",
                                    "Debug: JSON Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                    else
                    {
                        string errorMessage = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Server error: Status {response.StatusCode}\nError: {errorMessage}",
                            "Debug: Server Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception in LoadInstructionsFromDatabase: {ex.Message}\nStack: {ex.StackTrace}",
                    "Debug: Exception", MessageBoxButton.OK, MessageBoxImage.Error);
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
        private string InstructionTypeToName(byte instructionType)
        {
            return instructionType switch
            {
                0 => "Вводный",
                1 => "Внеплановый",
                2 => "Первичный",
                3 => "Повторный",
                4 => "Повторный (для водителей)",
                5 => "Целевой",
                _ => "Неизвестный"
            };
        }
        #endregion

        #region Window Events
        protected override async void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (_connection != null)
                {
                    await _connection.StopAsync();
                    await _connection.DisposeAsync();
                    _connection = null;
                }
            }
            catch (Exception ex)
            {
                // Log error but don't prevent closing
                Console.WriteLine($"Error closing SignalR connection: {ex.Message}");
            }

            base.OnClosing(e);
        }
        #endregion
    }

    #region View Models
    public class InstructionViewModel
    {
        public int Id { get; set; }
        public string Cause { get; set; }
        public string Type { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string AssignedStatus { get; set; }
        public string CompletedStatus { get; set; }
    }

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
}