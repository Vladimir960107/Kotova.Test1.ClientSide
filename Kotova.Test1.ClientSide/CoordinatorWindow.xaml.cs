using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
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
using Kotova.CommonClasses;
using Newtonsoft.Json;
using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Cursors = System.Windows.Input.Cursors;
using MessageBox = System.Windows.MessageBox;


namespace Kotova.Test1.ClientSide
{
    public partial class CoordinatorWindow : Window, INotifyPropertyChanged
    {
        #region Fields
        private Login_Russian? _loginForm;
        private string? _userName;
        private string? _jwtToken;

        private static readonly string GetEmployeesWithDifferencesUrl =
    ConfigurationClass.BASE_URL_DEVELOPMENT + "/api/DatabaseComparison/employees-with-differences";

        // Data collections for binding (using common DTOs)
        private ObservableCollection<InitialInstructionPersonDto> _initialInstructionPeople;
        private ObservableCollection<TelpEmployeeDtoEnhanced> _telpEmployees;
        private ObservableCollection<string> _departments;
        private ObservableCollection<string> _roles;
        private ObservableCollection<UserInstructionDto> _userInstructions;
        private ObservableCollection<string> _instructionTypes;
        private ObservableCollection<string> _additionalFilters;
        private ObservableCollection<NormativeInstructionDtoWithNotification> _normativeInstructions;

        // Selected items
        private InitialInstructionPersonDto? _selectedPerson;
        private UserInstructionDto? _selectedInstruction;
        private string? _selectedInstructionType;
        private string? _selectedDepartment;
        private string? _selectedAdditionalFilter;
        private NormativeInstructionDtoWithNotification? _selectedNormativeInstruction;

        // New employee data (using common DTO)
        private NewEmployeeDto _newEmployee;

        // Report dates
        private DateTime? _reportStartDate;
        private DateTime? _reportEndDate;
        #endregion

        #region Properties
        public ObservableCollection<InitialInstructionPersonDto> InitialInstructionPeople
        {
            get => _initialInstructionPeople;
            set { _initialInstructionPeople = value; OnPropertyChanged(); }
        }

        public ObservableCollection<TelpEmployeeDtoEnhanced> TelpEmployees
        {
            get => _telpEmployees;
            set { _telpEmployees = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> Departments
        {
            get => _departments;
            set { _departments = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> Roles
        {
            get => _roles;
            set { _roles = value; OnPropertyChanged(); }
        }

        public ObservableCollection<UserInstructionDto> UserInstructions
        {
            get => _userInstructions;
            set { _userInstructions = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> InstructionTypes
        {
            get => _instructionTypes;
            set { _instructionTypes = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> AdditionalFilters
        {
            get => _additionalFilters;
            set { _additionalFilters = value; OnPropertyChanged(); }
        }

        public ObservableCollection<NormativeInstructionDtoWithNotification> NormativeInstructions
        {
            get => _normativeInstructions;
            set { _normativeInstructions = value; OnPropertyChanged(); }
        }

        public InitialInstructionPersonDto? SelectedPerson
        {
            get => _selectedPerson;
            set { _selectedPerson = value; OnPropertyChanged(); }
        }

        public UserInstructionDto? SelectedInstruction
        {
            get => _selectedInstruction;
            set { _selectedInstruction = value; OnPropertyChanged(); }
        }

        public string? SelectedInstructionType
        {
            get => _selectedInstructionType;
            set { _selectedInstructionType = value; OnPropertyChanged(); }
        }

        public string? SelectedDepartment
        {
            get => _selectedDepartment;
            set { _selectedDepartment = value; OnPropertyChanged(); }
        }

        public string? SelectedAdditionalFilter
        {
            get => _selectedAdditionalFilter;
            set { _selectedAdditionalFilter = value; OnPropertyChanged(); }
        }

        public NormativeInstructionDtoWithNotification? SelectedNormativeInstruction
        {
            get => _selectedNormativeInstruction;
            set { _selectedNormativeInstruction = value; OnPropertyChanged(); }
        }

        public NewEmployeeDto NewEmployee
        {
            get => _newEmployee;
            set { _newEmployee = value; OnPropertyChanged(); }
        }

        public DateTime? ReportStartDate
        {
            get => _reportStartDate;
            set { _reportStartDate = value; OnPropertyChanged(); }
        }

        public DateTime? ReportEndDate
        {
            get => _reportEndDate;
            set { _reportEndDate = value; OnPropertyChanged(); }
        }
        #endregion

        #region Constructor
        public CoordinatorWindow(Login_Russian loginForm, string userName)
        {
            InitializeComponent();

            _loginForm = loginForm;
            _userName = userName;
            _jwtToken = loginForm._jwtToken;

            UserLabel.Text = userName;

            InitializeCollections();
            SetupDataContext();

            // Load normative instructions (the only working tab)
            LoadNormativeInstructionsAsync();
        }
        #endregion

        #region Initialization
        private void InitializeCollections()
        {
            InitialInstructionPeople = new ObservableCollection<InitialInstructionPersonDto>();
            TelpEmployees = new ObservableCollection<TelpEmployeeDtoEnhanced>();
            Departments = new ObservableCollection<string>();
            Roles = new ObservableCollection<string>();
            UserInstructions = new ObservableCollection<UserInstructionDto>();
            InstructionTypes = new ObservableCollection<string>();
            AdditionalFilters = new ObservableCollection<string>();
            NormativeInstructions = new ObservableCollection<NormativeInstructionDtoWithNotification>();

            NewEmployee = new NewEmployeeDto();

            // Initialize with sample data for UI testing
            InitializeSampleData();
        }

        private void InitializeSampleData()
        {
            // Sample departments
            Departments.Add("IT отдел");
            Departments.Add("Бухгалтерия");
            Departments.Add("HR отдел");
            Departments.Add("Производство");

            // Sample roles
            Roles.Add("Пользователь");
            Roles.Add("Координатор");
            Roles.Add("Начальник");
            Roles.Add("Администратор");

            // Sample instruction types
            InstructionTypes.Add("Вводный");
            InstructionTypes.Add("Первичный");
            InstructionTypes.Add("Повторный");
            InstructionTypes.Add("Внеплановый");

            // Sample additional filters
            AdditionalFilters.Add("Все сотрудники");
            AdditionalFilters.Add("Только новые");
            AdditionalFilters.Add("С просрочкой");
        }

        private void SetupDataContext()
        {
            DataContext = this;
        }
        #endregion

        #region Tab 1: Initial Instructions Events (Not Implemented)
        private void ButtonSyncInitialInstr_Click(object sender, RoutedEventArgs e)
        {
            ShowNotImplementedMessage("синхронизация вводных инструктажей");
        }

        private void ExcelFormButton_Click(object sender, RoutedEventArgs e)
        {
            ShowNotImplementedMessage("генерация Excel формы");
        }
        #endregion

        #region Tab 2: Database Connection Events (Working Implementation)
        private async void ButtonRefreshTelpDatabase_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;
                ButtonRefreshTelpDatabase.IsEnabled = false;
                ButtonRefreshTelpDatabase.Content = "🔄 Загрузка...";

                using (var client = new HttpClient())
                {
                    if (string.IsNullOrEmpty(_jwtToken))
                    {
                        MessageBox.Show("Ошибка авторизации. Пожалуйста, войдите в систему заново.",
                                      "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);
                    var response = await client.GetAsync(GetEmployeesWithDifferencesUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        var employeesComparison = JsonConvert.DeserializeObject<List<EmployeeComparisonDto>>(jsonResponse);

                        // Clear and populate the TelpEmployees collection
                        TelpEmployees.Clear();

                        foreach (var employeeComparison in employeesComparison)
                        {
                            var enhancedEmployee = new TelpEmployeeDtoEnhanced
                            {
                                FullName = employeeComparison.FullName ?? "",
                                DepartmentName = employeeComparison.DepartmentName ?? "",
                                PositionName = employeeComparison.PositionName ?? "",
                                Email = employeeComparison.Email ?? "",
                                PersonnelNumber = employeeComparison.PersonnelNumber ?? "",
                                HasDifferences = employeeComparison.HasDifferences,
                                DifferenceFields = employeeComparison.DifferenceFields ?? new List<string>(),
                                RowColor = employeeComparison.RowColor ?? "Green" // Use the server-provided color
                            };

                            TelpEmployees.Add(enhancedEmployee);
                        }

                        // Show summary with color breakdown
                        var totalEmployees = employeesComparison.Count;
                        var redCount = employeesComparison.Count(e => e.RowColor == "Red");
                        var greenCount = employeesComparison.Count(e => e.RowColor == "Green");
                        var blueCount = employeesComparison.Count(e => e.RowColor == "Blue");
                        var yellowCount = employeesComparison.Count(e => e.RowColor == "Yellow");

                        MessageBox.Show(
                            $"База данных успешно обновлена!\n\n" +
                            $"Всего сотрудников: {totalEmployees}\n" +
                            $"🟢(Зеленые) Данные совпадают: {greenCount}\n" +
                            $"🔴(Красные) Есть различия: {redCount}\n" +
                            $"🔵(Синие) Только в Lynks: {blueCount}\n" +
                            $"🟡(Желтые) Только в TransElectro: {yellowCount}\n\n" +
                            $"Дважды щелкните на строке для детального просмотра.",
                            "Результат обновления", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Ошибка при обновлении базы данных: {response.StatusCode}\n{errorContent}",
                                      "Ошибка API", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка соединения с сервером: {ex.Message}",
                              "Ошибка сети", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
                ButtonRefreshTelpDatabase.IsEnabled = true;
                ButtonRefreshTelpDatabase.Content = "🔄 Обновить базу данных TELP";
            }
        }
        #endregion

        #region Tab 3: Employee Data Events (Not Implemented)
        private void UploadNewcommer_Click(object sender, RoutedEventArgs e)
        {
            ShowNotImplementedMessage("сохранение нового сотрудника");
        }
        #endregion

        #region Tab 4: Taking Instructions Events (Not Implemented)
        private void HyperLinkForInstructionsFolder_Click(object sender, RoutedEventArgs e)
        {
            ShowNotImplementedMessage("переход к папке с инструктажами");
        }
        #endregion

        #region Tab 5: Reports Events (Not Implemented)
        private void ExcelExportForCoordinatorButton_Click(object sender, RoutedEventArgs e)
        {
            ShowNotImplementedMessage("экспорт отчета в Excel");
        }
        #endregion

        #region Tab 6: Normative Instructions Events (Working Implementation)
        private async Task LoadNormativeInstructionsAsync()
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);

                    var response = await httpClient.GetAsync(
                        ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/normative-instructions?isUnplannedInstruction=false");

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        var instructions = JsonConvert.DeserializeObject<List<NormativeInstructionDto>>(responseBody);

                        NormativeInstructions.Clear();
                        if (instructions != null)
                        {
                            foreach (var instruction in instructions)
                            {
                                NormativeInstructions.Add(
                                    NormativeInstructionDtoWithNotification.FromNormativeInstructionDto(instruction));
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show($"Не удалось загрузить нормативные инструкции. Код ошибки: {response.StatusCode}",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке нормативных инструкций: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnNewNormativeInstruction_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SelectedNormativeInstruction = new NormativeInstructionDtoWithNotification
                {
                    Id = 0,
                    Name = "Новая инструкция",
                    Url = "",
                    CreatedAt = DateTime.Now,
                    IsUnplannedInstruction = false  // Ensure new instructions are NOT unplanned
                };

                TxtNormativeInstructionName.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании новой инструкции: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnSaveNormativeInstruction_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectedNormativeInstruction == null)
                {
                    MessageBox.Show("Выберите инструкцию для сохранения.", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(SelectedNormativeInstruction.Name))
                {
                    MessageBox.Show("Введите название инструкции.", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);

                    // Convert to the DTO that the API expects
                    var apiDto = SelectedNormativeInstruction.ToNormativeInstructionDto();
                    string json = JsonConvert.SerializeObject(apiDto);
                    HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                    string url = SelectedNormativeInstruction.Id == 0
                        ? ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/normative-instructions"
                        : ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + $"/normative-instructions/{SelectedNormativeInstruction.Id}";

                    var response = SelectedNormativeInstruction.Id == 0
                        ? await httpClient.PostAsync(url, content)
                        : await httpClient.PutAsync(url, content);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Инструкция успешно сохранена.", "Успех",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadNormativeInstructionsAsync();
                    }
                    else
                    {
                        string errorMessage = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Не удалось сохранить инструкцию. Код ошибки: {response.StatusCode}\n{errorMessage}",
                            "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении инструкции: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnDeleteNormativeInstruction_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SelectedNormativeInstruction == null || SelectedNormativeInstruction.Id == 0)
                {
                    MessageBox.Show("Выберите инструкцию для удаления.", "Предупреждение",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show($"Вы уверены, что хотите удалить инструкцию '{SelectedNormativeInstruction.Name}'?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    using (var httpClient = new HttpClient())
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);

                        var response = await httpClient.DeleteAsync(
                            ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + $"/normative-instructions/{SelectedNormativeInstruction.Id}");

                        if (response.IsSuccessStatusCode)
                        {
                            MessageBox.Show("Инструкция успешно удалена.", "Успех",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            await LoadNormativeInstructionsAsync();
                            SelectedNormativeInstruction = null;
                        }
                        else
                        {
                            string errorMessage = await response.Content.ReadAsStringAsync();
                            MessageBox.Show($"Не удалось удалить инструкцию. Код ошибки: {response.StatusCode}\n{errorMessage}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении инструкции: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnRefreshNormativeInstructions_Click(object sender, RoutedEventArgs e)
        {
            await LoadNormativeInstructionsAsync();
        }

        private void BtnCancelNormativeInstruction_Click(object sender, RoutedEventArgs e)
        {
            SelectedNormativeInstruction = null;
        }
        #endregion

        #region General Events
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти из учетной записи?",
                "Подтверждение выхода", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _loginForm?.Show();
                this.Close();
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _loginForm?.Show();
        }
        #endregion

        #region Helper Methods
        private void ShowNotImplementedMessage(string feature)
        {
            MessageBox.Show($"Функция '{feature}' еще не реализована.", "Не реализовано",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private void TelpEmployeesListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (TelpEmployeesListView.SelectedItem is TelpEmployeeDtoEnhanced selectedEmployee)
                {
                    if (selectedEmployee.HasDifferences)
                    {
                        if (string.IsNullOrEmpty(selectedEmployee.PersonnelNumber))
                        {
                            MessageBox.Show("Не удалось получить табельный номер сотрудника.",
                                          "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        if (string.IsNullOrEmpty(_jwtToken))
                        {
                            MessageBox.Show("Ошибка авторизации. Пожалуйста, войдите в систему заново.",
                                          "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        // Open the database differences resolver form
                        var differencesForm = new DatabaseDifferencesResolverForm(selectedEmployee.PersonnelNumber, _jwtToken);
                        differencesForm.ShowDialog();

                        // Refresh the list after the form closes to show updated data
                        ButtonRefreshTelpDatabase_Click(sender, new RoutedEventArgs());
                    }
                    else
                    {
                        MessageBox.Show("У данного сотрудника нет различий в базах данных.",
                                      "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии формы разрешения различий: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public class TelpEmployeeDtoEnhanced : TelpEmployeeDto, INotifyPropertyChanged
        {
            private bool _hasDifferences;
            private string _statusText = "";
            private Brush _backgroundBrush = Brushes.White;
            private Brush _foregroundBrush = Brushes.Black;
            private string _rowColor = "Green"; // Default to green

            public bool HasDifferences
            {
                get => _hasDifferences;
                set
                {
                    _hasDifferences = value;
                    OnPropertyChanged();
                    UpdateDisplayProperties();
                }
            }

            public string RowColor
            {
                get => _rowColor;
                set
                {
                    _rowColor = value;
                    OnPropertyChanged();
                    UpdateDisplayProperties();
                }
            }

            public string StatusText
            {
                get => _statusText;
                set { _statusText = value; OnPropertyChanged(); }
            }

            public Brush BackgroundBrush
            {
                get => _backgroundBrush;
                set { _backgroundBrush = value; OnPropertyChanged(); }
            }

            public Brush ForegroundBrush
            {
                get => _foregroundBrush;
                set { _foregroundBrush = value; OnPropertyChanged(); }
            }

            public List<string> DifferenceFields { get; set; } = new List<string>();

            private void UpdateDisplayProperties()
            {
                switch (RowColor?.ToLower())
                {
                    case "red":
                        // Red: Employees exist in both databases but have data differences
                        StatusText = "🔴 Различия";
                        BackgroundBrush = new SolidColorBrush(Color.FromRgb(255, 235, 238)); // Light red
                        ForegroundBrush = new SolidColorBrush(Color.FromRgb(183, 28, 28));   // Dark red
                        break;

                    case "green":
                        // Green: Employees exist in both databases with identical data
                        StatusText = "🟢 Совпадает";
                        BackgroundBrush = new SolidColorBrush(Color.FromRgb(232, 245, 233)); // Light green
                        ForegroundBrush = new SolidColorBrush(Color.FromRgb(27, 94, 32));    // Dark green
                        break;

                    case "blue":
                        // Blue: Employees exist only in Lynks database (not in TransElectro)
                        StatusText = "🔵 Только в Lynks";
                        BackgroundBrush = new SolidColorBrush(Color.FromRgb(227, 242, 253)); // Light blue
                        ForegroundBrush = new SolidColorBrush(Color.FromRgb(13, 71, 161));   // Dark blue
                        break;

                    case "yellow":
                        // Yellow: Employees exist only in TransElectro database (not in Lynks) - "Так не должно было быть"
                        StatusText = "🟡 Только в TransElectro";
                        BackgroundBrush = new SolidColorBrush(Color.FromRgb(255, 248, 225)); // Light yellow
                        ForegroundBrush = new SolidColorBrush(Color.FromRgb(230, 81, 0));    // Dark orange
                        break;

                    default:
                        // Fallback for unknown colors
                        StatusText = "❓ Неизвестно";
                        BackgroundBrush = Brushes.White;
                        ForegroundBrush = Brushes.Black;
                        break;
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}