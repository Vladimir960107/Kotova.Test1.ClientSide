using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using Kotova.CommonClasses;
using MessageBox = System.Windows.MessageBox;

namespace Kotova.Test1.ClientSide.ChiefWPF
{
    /// <summary>
    /// Interaction logic for ChiefWindowFresh.xaml
    /// </summary>
    public partial class ChiefWindowFresh : Window
    {
        // Fields from original ChiefForm.cs
        private Login_Russian _loginForm;
        private HubConnection _connection;

        // URLs from original form
        private readonly string urlTest = "https://localhost:7048/api/test/TestEndpoint";
        private readonly string urlTaskTest = "https://localhost:7048/api/TestTasks/test-task";
        private readonly string urlCreateInstruction = "https://localhost:7048/api/instruction/create-custom-instruction";
        private readonly string urlGetInstructionsByDepartment = "https://localhost:7048/api/instruction/get-instructions-by-department";
        private readonly string urlGetAllUnplannedInstructions = "https://localhost:7048/api/instruction/get-all-unplanned-instructions";
        private readonly string urlSkipUnplannedInstruction = "https://localhost:7048/api/instruction/skip-unplanned-instruction-for-personnel";

        // Collections for data binding
        public ObservableCollection<InstructionViewModel> Instructions { get; set; }

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
            usernameLabel_Wpf.Text = loginForm.LoginTextBox.Text;
            InitializeSignalR();
        }

        private void InitializeCollections()
        {
            Instructions = new ObservableCollection<InstructionViewModel>();

            // Set data context for binding
            instructionsListView_Wpf.ItemsSource = Instructions;
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
                await LoadInstructionsAsync();
            }
        }

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
                await LoadInstructionsAsync();
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
            await LoadInstructionsAsync();
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
                    // Create empty collections for the constructor parameters
                    var employees = new List<EmployeeInfo>();
                    var normativeInstructions = new List<NormativeInstructionInfo>();

                    // Open the InstructionAssignmentManager window with required parameters
                    var assignmentWindow = new InstructionAssignmentManager(
                        selectedInstruction.Cause,
                        employees,
                        normativeInstructions,
                        _loginForm._jwtToken,
                        "assignment-endpoint",
                        1 // instruction type
                    );
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

        private async void RefreshButton_Wpf_Click(object sender, RoutedEventArgs e)
        {
            await LoadInstructionsAsync();
        }

        #endregion

        #region Helper Methods

        private async Task LoadInstructionsAsync()
        {
            try
            {
                // Clear existing instructions
                Instructions.Clear();

                // Load instructions by department
                if (_loginForm?.GetDepartmentIdFromToken(_loginForm._jwtToken) != -1)
                {
                    int departmentId = _loginForm.GetDepartmentIdFromToken(_loginForm._jwtToken);
                    string departmentUrl = $"{urlGetInstructionsByDepartment}?departmentId={departmentId}";
                    var instructionsResponse = await ConnectionToUrlGetWithReturn(departmentUrl, _loginForm._jwtToken);

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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке инструктажей: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int GetPersonnelIdFromToken(string jwtToken)
        {
            if (string.IsNullOrEmpty(jwtToken))
                return -1;

            try
            {
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(jwtToken) as System.IdentityModel.Tokens.Jwt.JwtSecurityToken;

                if (jsonToken == null)
                    return -1;

                // Look for personnel ID claim
                var personnelIdClaim = jsonToken.Claims.FirstOrDefault(claim =>
                    claim.Type == "PersonnelId" ||
                    claim.Type == "personnel_id");

                if (personnelIdClaim != null && int.TryParse(personnelIdClaim.Value, out int personnelId))
                {
                    return personnelId;
                }

                return -1;
            }
            catch (Exception)
            {
                return -1; // Return -1 for any parsing errors
            }
        }

        // Helper method to replace Test.connectionToUrlGetWithReturn
        private async Task<string> ConnectionToUrlGetWithReturn(string url, string token)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsStringAsync();
                    }
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
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

    #endregion
}