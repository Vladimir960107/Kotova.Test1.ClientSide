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

using System.Linq;
using System.Collections.Generic;
using WpfBinding = System.Windows.Data.Binding;


namespace Kotova.Test1.ClientSide
{
    public partial class CoordinatorWindow : Window, INotifyPropertyChanged
    {
        #region Fields
        private Login_Russian? _loginForm;
        private string? _userName;
        private string? _jwtToken;

        private GridViewColumnHeader _lastHeaderClicked = null;
        private ListSortDirection _lastDirection = ListSortDirection.Ascending;
        private List<GridViewColumnHeader> _allHeaders = new List<GridViewColumnHeader>();

        private static readonly string GetEmployeesWithDifferencesUrl =
    ConfigurationClass.BASE_URL_DEVELOPMENT + "/api/DatabaseComparison/employees-with-differences";

        private static readonly string InsertNewEmployeeURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/insert-new-employee";
        private static readonly string GetLoginPasswordUrl = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get-login-and-password-for-newcommer";

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

        }
        private void SetupDataContext()
        {
            DataContext = this;
        }
        #endregion

        // Add these methods to your CoordinatorWindow.xaml.cs file

        #region Tab Selection Event Handler

        /// <summary>
        /// Handle tab selection changes to auto-load data when needed
        /// </summary>
        private async void CoordinatorTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.Source is System.Windows.Controls.TabControl tabControl && tabControl.SelectedItem is TabItem selectedTab)
                {
                    // Check if the "Данные сотрудника" tab is selected
                    if (selectedTab.Header.ToString().Contains("Данные сотрудника"))
                    {
                        await LoadDepartmentsAndRolesForEmployeeTabAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in tab selection: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads departments and roles data specifically for the employee tab
        /// </summary>
        private async Task LoadDepartmentsAndRolesForEmployeeTabAsync()
        {
            try
            {
                // Only load if collections are empty or need refresh
                if (Departments.Count == 0 || Roles.Count == 0 || ShouldRefreshData())
                {
                    Console.WriteLine("Loading departments and roles for employee tab...");

                    bool success = await LoadDepartmentsAndRolesAsync();

                    if (success)
                    {
                        Console.WriteLine($"Successfully loaded {Departments.Count} departments and {Roles.Count} roles");
                    }
                    else
                    {
                        Console.WriteLine("Failed to load departments and roles data");
                    }
                }
                else
                {
                    Console.WriteLine("Data already loaded, skipping refresh");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data for employee tab: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if data should be refreshed (optional logic)
        /// </summary>
        private bool ShouldRefreshData()
        {
            // Refresh data if it's older than 5 minutes (optional)
            // You can remove this check if you want to load only once
            return false; // For now, only load if collections are empty
        }

        #endregion

        #region Data Download Methods (Simplified)

        /// <summary>
        /// Downloads departments and roles data from the server (silent operation)
        /// </summary>
        /// <returns>True if successful, false otherwise</returns>
        private async Task<bool> LoadDepartmentsAndRolesAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    // Set authorization header
                    client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", _jwtToken);
                    client.Timeout = TimeSpan.FromSeconds(15); // Shorter timeout for auto-load

                    // Call the departments-and-roles endpoint
                    string url = ConfigurationClass.BASE_URL_DEVELOPMENT + "/api/datadownload/departments-and-roles";

                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonContent = await response.Content.ReadAsStringAsync();
                        var responseData = JsonConvert.DeserializeObject<DepartmentsAndRolesResponse>(jsonContent);

                        if (responseData?.Success == true)
                        {
                            // Clear existing collections
                            Departments.Clear();
                            Roles.Clear();

                            // Populate departments
                            if (responseData.Departments?.Data != null)
                            {
                                foreach (var dept in responseData.Departments.Data)
                                {
                                    Departments.Add(dept.DepartmentName);
                                }
                            }

                            // Populate roles with Russian names
                            if (responseData.Roles?.Data != null)
                            {
                                foreach (var role in responseData.Roles.Data)
                                {
                                    Roles.Add(role.RoleNameRussian);
                                }
                            }

                            return true;
                        }
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading departments and roles: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region ColumnHeaderCLickedHandler stuff
        private void GridViewColumnHeaderClickedHandler(object sender, RoutedEventArgs e)
        {
            var headerClicked = e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if (headerClicked != null)
            {
                if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                {
                    // Store this header in our collection if not already there
                    if (!_allHeaders.Contains(headerClicked))
                    {
                        _allHeaders.Add(headerClicked);
                    }

                    if (headerClicked != _lastHeaderClicked)
                    {
                        direction = ListSortDirection.Ascending;
                    }
                    else
                    {
                        if (_lastDirection == ListSortDirection.Ascending)
                        {
                            direction = ListSortDirection.Descending;
                        }
                        else
                        {
                            direction = ListSortDirection.Ascending;
                        }
                    }

                    // Get the property name to sort by
                    string sortBy;
                    var columnBinding = headerClicked.Column.DisplayMemberBinding as WpfBinding;

                    if (columnBinding != null)
                    {
                        // For columns with DisplayMemberBinding
                        sortBy = columnBinding.Path.Path;
                    }
                    else
                    {
                        // For columns with CellTemplate (like Status column)
                        string headerText = headerClicked.Column.Header as string;
                        sortBy = GetPropertyNameFromHeader(headerText);
                    }

                    Sort(sortBy, direction);

                    // Update visual indicators
                    UpdateColumnHeaders(headerClicked, direction);

                    _lastHeaderClicked = headerClicked;
                    _lastDirection = direction;
                }
            }
        }

        // Add this sorting method
        private void Sort(string sortBy, ListSortDirection direction)
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(TelpEmployeesListView.ItemsSource);

            if (dataView != null)
            {
                dataView.SortDescriptions.Clear();
                SortDescription sd = new SortDescription(sortBy, direction);
                dataView.SortDescriptions.Add(sd);
                dataView.Refresh();
            }
        }

        // Updated method to properly clear all sort indicators
        private void UpdateColumnHeaders(GridViewColumnHeader clickedHeader, ListSortDirection direction)
        {
            // Clear indicators from ALL headers that we've tracked
            foreach (var header in _allHeaders)
            {
                if (header != clickedHeader)
                {
                    string originalText = GetOriginalHeaderText(header);
                    header.Content = originalText;
                }
            }

            // Add sort indicator to clicked header
            if (clickedHeader != null)
            {
                string originalText = GetOriginalHeaderText(clickedHeader);
                string indicator = direction == ListSortDirection.Ascending ? " ▲" : " ▼";
                clickedHeader.Content = originalText + indicator;
            }
        }

        // Method to clear all sort indicators from all column headers - simplified version
        private void ClearAllSortIndicators()
        {
            foreach (var header in _allHeaders)
            {
                string originalText = GetOriginalHeaderText(header);
                header.Content = originalText;
            }
        }

        // Helper method to get original header text without sort indicators
        private string GetOriginalHeaderText(GridViewColumnHeader header)
        {
            string content = header.Content?.ToString() ?? "";

            // Remove existing sort indicators
            if (content.EndsWith(" ▲") || content.EndsWith(" ▼"))
            {
                content = content.Substring(0, content.Length - 2);
            }

            return content;
        }

        // Helper method to get all GridView columns
        private IEnumerable<GridViewColumn> GetGridViewColumns()
        {
            var gridView = TelpEmployeesListView.View as GridView;
            return gridView?.Columns ?? Enumerable.Empty<GridViewColumn>();
        }

        // Helper method to get column header
        private GridViewColumnHeader GetColumnHeader(GridViewColumn column)
        {
            // This is a simplified approach - in practice, you might need to traverse the visual tree
            return null; // You might need to implement visual tree traversal here if needed
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

        private string GetPropertyNameFromHeader(string headerText)
        {
            // Remove sort indicators if present
            string cleanHeader = headerText?.Replace(" ▲", "").Replace(" ▼", "") ?? "";

            return cleanHeader switch
            {
                "Статус" => "StatusText",      // Maps to the StatusText property
                "ФИО" => "FullName",
                "Отдел" => "DepartmentName",
                "Должность" => "PositionName",
                "Email" => "Email",
                "Табельный номер" => "PersonnelNumber",
                _ => cleanHeader  // Fallback to the header text itself
            };
        }

        #endregion

        #region Tab 3: Employee Data Events (Not Implemented)

        private async void UploadNewcommer_Click(object sender, RoutedEventArgs e)
        {
            // Disable button during processing
            UploadNewcommer.IsEnabled = false;

            try
            {
                // Validate required fields
                if (!ValidateEmployeeData())
                {
                    UploadNewcommer.IsEnabled = true;
                    return;
                }

                // Create Employee object from form data
                var newEmployee = CreateEmployeeFromForm();

                string token = _loginForm?._jwtToken ?? _jwtToken;
                if (string.IsNullOrEmpty(token))
                {
                    MessageBox.Show("Токен авторизации отсутствует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    UploadNewcommer.IsEnabled = true;
                    return;
                }

                // Step 1: Insert new employee (server will handle initial instruction internally)
                var insertResponse = await InsertNewEmployeeAsync(newEmployee, token, NewEmployee.AddInitialInstruction);

                if (!insertResponse.IsSuccessStatusCode)
                {
                    string errorText = await insertResponse.Content.ReadAsStringAsync();
                    MessageBox.Show($"Ошибка при добавлении сотрудника: {errorText}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    UploadNewcommer.IsEnabled = true;
                    return;
                }

                MessageBox.Show("Сотрудник успешно добавлен в базу данных", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Step 2: Get login and password
                string roleName = RoleMappings.GetRoleDisplayName(NewEmployee.Role);
                if (string.IsNullOrEmpty(roleName))
                {
                    MessageBox.Show("Выбрана недопустимая роль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    UploadNewcommer.IsEnabled = true;
                    return;
                }

                var loginPasswordResponse = await GetLoginPassword(
                    new List<string> {
                NewEmployee.PersonnelNumber,
                NewEmployee.Department,
                NewEmployee.WorkplaceNumber ?? "1", // Default workplace number
                roleName,
                NewEmployee.AddInitialInstruction.ToString()
                    },
                    token);

                if (loginPasswordResponse.IsSuccessStatusCode)
                {
                    var jsonResponse = await loginPasswordResponse.Content.ReadAsStringAsync();
                    var loginAndPassword = JsonConvert.DeserializeObject<Tuple<string, string>>(jsonResponse);

                    // Update NewEmployee properties with generated credentials
                    NewEmployee.Login = loginAndPassword.Item1;
                    NewEmployee.Password = loginAndPassword.Item2;

                    string successMessage = $"Учетные данные созданы:\nЛогин: {NewEmployee.Login}\nПароль: {NewEmployee.Password}";

                    if (NewEmployee.AddInitialInstruction)
                    {
                        successMessage += "\n\nВводный инструктаж автоматически назначен сотруднику.";
                    }

                    MessageBox.Show(successMessage, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Clear form after successful creation
                    ClearEmployeeForm();
                }
                else
                {
                    string errorText = await loginPasswordResponse.Content.ReadAsStringAsync();
                    MessageBox.Show($"Ошибка при получении логина/пароля: {errorText}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                UploadNewcommer.IsEnabled = true;
            }
        }

        // Helper method for inserting employee
        private async Task<HttpResponseMessage> InsertNewEmployeeAsync(Employee employee, string token, bool addInitialInstruction)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var json = JsonConvert.SerializeObject(employee);
                var data = new StringContent(json, Encoding.UTF8, "application/json");

                // Add query parameter for initial instruction
                string url = $"{InsertNewEmployeeURL}?addInitialInstruction={addInitialInstruction}";
                return await client.PostAsync(url, data);
            }
        }

        // Helper method for getting login/password (same as in other forms)
        private async Task<HttpResponseMessage> GetLoginPassword(List<string> dataAboutUser, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var json = JsonConvert.SerializeObject(dataAboutUser);
                var data = new StringContent(json, Encoding.UTF8, "application/json");
                return await client.PostAsync(GetLoginPasswordUrl, data);
            }
        }

        // Keep the existing validation and helper methods...
        private bool ValidateEmployeeData()
        {
            if (string.IsNullOrWhiteSpace(NewEmployee.FullName))
            {
                MessageBox.Show("Введите ФИО сотрудника", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(NewEmployee.Position))
            {
                MessageBox.Show("Введите должность сотрудника", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(NewEmployee.PersonnelNumber))
            {
                MessageBox.Show("Введите табельный номер", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (NewEmployee.BirthDate == null)
            {
                MessageBox.Show("Выберите дату рождения", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            else if (IsAtLeast18YearsOld(NewEmployee.BirthDate))


            if (string.IsNullOrWhiteSpace(NewEmployee.Department))
            {
                MessageBox.Show("Выберите отдел", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(NewEmployee.Role))
            {
                MessageBox.Show("Выберите роль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        public bool IsAtLeast18YearsOld(DateTime? birthDate_nullable)
        {
            if (birthDate_nullable is null)
            {
                return false;
            }
            DateTime birthDate = birthDate_nullable.Value;

            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;

            // Adjust if the birthday has not occurred yet this year
            if (birthDate.Date > today.AddYears(-age)) age--;

            return age >= 18;
        }

        private Employee CreateEmployeeFromForm()
        {
            return new Employee
            {
                full_name = NewEmployee.FullName,
                job_position = NewEmployee.Position,
                personnel_number = NewEmployee.PersonnelNumber,
                department = NewEmployee.Department,
                birth_date = NewEmployee.BirthDate ?? DateTime.Now,
                gender = 0, // Default value
                is_driver = false, // Default value
                is_working_in_department = true, // Default for new employees
                group = null // Optional field
            };
        }

        private void ClearEmployeeForm()
        {
            NewEmployee.FullName = string.Empty;
            NewEmployee.Position = string.Empty;
            NewEmployee.PersonnelNumber = string.Empty;
            NewEmployee.WorkplaceNumber = string.Empty;
            NewEmployee.BirthDate = null;
            NewEmployee.Department = string.Empty;
            NewEmployee.Role = string.Empty;
            NewEmployee.AddInitialInstruction = false;
            NewEmployee.Login = string.Empty;
            NewEmployee.Password = string.Empty;
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