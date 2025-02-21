using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Newtonsoft.Json;
using System.Linq;
using Kotova.CommonClasses;
using System.Text;
using MessageBox = System.Windows.MessageBox;

namespace Kotova.Test1.ClientSide
{
    public partial class CoordinatorForm_DBConnection : Window
    {
        private static readonly string DownloadDepartmentsForUserURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/download-list-of-departments";
        private static readonly string DownloadRolesForUsersUrl = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get-roles-for-newcomer";
        private static readonly string InsertNewEmployeeURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/insert-new-employee";
        private static readonly string GetLoginPasswordUrl = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get-login-and-password-for-newcommer";

        // Regular expressions for validation
        private static readonly Regex RussianNameRegex = new Regex(@"^[А-ЯЁа-яё]+([ -][А-ЯЁа-яё]+)*$");
        private static readonly Regex PersonnelNumberRegex = new Regex(@"^\d{10}$");

        private readonly Login_Russian _loginForm;
        private readonly TelpEmployeeDto _employeeData;

        public CoordinatorForm_DBConnection(Login_Russian loginForm, TelpEmployeeDto employeeData = null)
        {
            InitializeComponent();
            DepartmentForNewcomerComboBox.PreventMouseWheelScroll();
            RoleOfNewcomerComboBox.PreventMouseWheelScroll();
            _loginForm = loginForm;
            _employeeData = employeeData;

            // Load initial data asynchronously
            InitializeFormAsync();
            

        }

        private async void InitializeFormAsync()
        {
            try
            {
                // Download departments and roles
                await LoadDepartmentsAsync();
                await LoadRolesAsync();

                // Set up validation events
                employeeFullNameTextBox.LostFocus += ValidateFullName;
                personnelNumberTextBox.LostFocus += ValidatePersonnelNumber;
                //Removed: uploadNewcommer.Click += SaveButton_Click;

                // Make login and password read-only
                loginTextBox.IsReadOnly = true;
                PasswordTextBox.IsReadOnly = true;

                // Set default date
                dateOfBirthDateTimePicker.SelectedDate = DateTime.Today;

                // If we have employee data, populate and lock appropriate fields
                if (_employeeData != null)
                {
                    PopulateEmployeeData();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка при инициализации формы: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        


        private async Task LoadDepartmentsAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _loginForm._jwtToken);

                var response = await client.GetAsync(DownloadDepartmentsForUserURL);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var departments = JsonConvert.DeserializeObject<List<string>>(jsonResponse);

                foreach (var dept in departments)
                {
                    DepartmentForNewcomerComboBox.Items.Add(dept);
                }
            }
        }

        private async Task LoadRolesAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _loginForm._jwtToken);

                var response = await client.GetAsync(DownloadRolesForUsersUrl);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                var roles = JsonConvert.DeserializeObject<List<string>>(jsonResponse);

                foreach (var role in roles)
                {
                    if (RoleMappings.RoleDbToDisplay.TryGetValue(role, out string displayName))
                        {
                        RoleOfNewcomerComboBox.Items.Add(displayName);
                    }
                }
            }
        }

        private void PopulateEmployeeData()
        {
            // Populate and make read-only the fields we have from TelpEmployeeDto
            employeeFullNameTextBox.Text = _employeeData.FullName;
            employeeFullNameTextBox.IsReadOnly = true;

            employeesPositionTextBox.Text = _employeeData.PositionName;
            employeesPositionTextBox.IsReadOnly = true;

            personnelNumberTextBox.Text = _employeeData.PersonnelNumber;
            personnelNumberTextBox.IsReadOnly = true;
            // Select and disable department if it exists in the list
            var deptItem = DepartmentForNewcomerComboBox.Items.Cast<string>()
                .FirstOrDefault(d => string.Equals(d.Trim(), _employeeData.DepartmentName.Trim(), StringComparison.OrdinalIgnoreCase));
            if (deptItem != null)
            {
                DepartmentForNewcomerComboBox.SelectedItem = deptItem;
                DepartmentForNewcomerComboBox.IsEnabled = false;
            }
        }

        private void ValidateFullName(object sender, RoutedEventArgs e)
        {
            var textBox = sender as System.Windows.Controls.TextBox;
            if (textBox == null) return;

            string name = textBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                ShowError(textBox, "ФИО не может быть пустым");
                return;
            }

            if (!RussianNameRegex.IsMatch(name))
            {
                ShowError(textBox, "ФИО должно содержать только русские буквы и дефисы");
                return;
            }

            ClearError(textBox);
        }

        private void ValidatePersonnelNumber(object sender, RoutedEventArgs e)
        {
            var textBox = sender as System.Windows.Controls.TextBox;
            if (textBox == null) return;

            string number = textBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(number))
            {
                ShowError(textBox, "Табельный номер не может быть пустым");
                return;
            }

            if (!PersonnelNumberRegex.IsMatch(number))
            {
                ShowError(textBox, "Табельный номер должен содержать ровно 10 цифр");
                return;
            }

            ClearError(textBox);
        }

        private void ShowError(System.Windows.Controls.TextBox textBox, string message)
        {
            textBox.BorderBrush = new SolidColorBrush(Colors.Red);
            textBox.ToolTip = message;
        }

        private void ClearError(System.Windows.Controls.TextBox textBox)
        {
            textBox.BorderBrush = new SolidColorBrush(Colors.Gray);
            textBox.ToolTip = null;
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            uploadNewcommer.IsEnabled = false;

            try
            {
                // Enhanced validation
                if (!ValidateAllFields())
                {
                    MessageBox.Show("Пожалуйста, проверьте правильность заполнения всех полей",
                                  "Ошибка валидации",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                    uploadNewcommer.IsEnabled = true;
                    return;
                }

                // Create Employee object (similar to CoordinatorForm)
                Employee newEmployee = new Employee
                {
                    personnel_number = personnelNumberTextBox.Text,
                    full_name = employeeFullNameTextBox.Text,
                    job_position = employeesPositionTextBox.Text,
                    department = DepartmentForNewcomerComboBox.SelectedItem?.ToString(),
                    group = null,
                    birth_date = dateOfBirthDateTimePicker.SelectedDate ?? DateTime.Now,
                    gender = 3, // Default as in CoordinatorForm
                    is_working_in_department = true
                };

                // Get role name in DB format
                string? roleName = RoleMappings.GetRoleDisplayName(RoleOfNewcomerComboBox.SelectedItem?.ToString());
                if (string.IsNullOrEmpty(roleName))
                {
                    MessageBox.Show("Выбрана недопустимая роль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    uploadNewcommer.IsEnabled = true;
                    return;
                }

                string isEmployeeRequireInitInstr = AddInitialInstructionToNewcomer.IsChecked?.ToString() ?? "False";

                // Insert employee using the same API endpoint as CoordinatorForm
                using (HttpClient client = new HttpClient())
                {
                    string token = _loginForm._jwtToken;
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                    // First API call - Insert Employee
                    var response = await InsertNewEmployeeAsync(newEmployee, token);

                    if (response.IsSuccessStatusCode)
                    {
                        // Second API call - Get Login/Password
                        var loginPasswordResponse = await GetLoginPassword(
                            new List<string> {
                        newEmployee.personnel_number,
                        newEmployee.department,
                        "1", // Default workplace number since this field was removed from WPF form
                        roleName,
                        isEmployeeRequireInitInstr
                            },
                            token);

                        if (loginPasswordResponse.IsSuccessStatusCode)
                        {
                            var jsonResponse = await loginPasswordResponse.Content.ReadAsStringAsync();
                            var loginAndPassword = JsonConvert.DeserializeObject<Tuple<string, string>>(jsonResponse);

                            // Update UI with generated credentials
                            loginTextBox.Text = loginAndPassword.Item1;
                            PasswordTextBox.Text = loginAndPassword.Item2;

                            MessageBox.Show("Сотрудник успешно добавлен в базу данных",
                                          "Успех",
                                          MessageBoxButton.OK,
                                          MessageBoxImage.Information);
                        }
                        else
                        {
                            string errorText = await loginPasswordResponse.Content.ReadAsStringAsync();
                            MessageBox.Show($"Ошибка получения логина/пароля: {errorText}",
                                          "Ошибка",
                                          MessageBoxButton.OK,
                                          MessageBoxImage.Error);
                            uploadNewcommer.IsEnabled = true;
                        }
                    }
                    else
                    {
                        string errorText = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Ошибка добавления сотрудника: {errorText}",
                                      "Ошибка",
                                      MessageBoxButton.OK,
                                      MessageBoxImage.Error);
                        uploadNewcommer.IsEnabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
                uploadNewcommer.IsEnabled = true;
            }
        }

        // Helper method for HTTP request to insert employee (from CoordinatorForm)
        private async Task<HttpResponseMessage> InsertNewEmployeeAsync(Employee employee, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var json = JsonConvert.SerializeObject(employee);
                var data = new StringContent(json, Encoding.UTF8, "application/json");
                return await client.PostAsync(InsertNewEmployeeURL, data);
            }
        }

        // Helper method for HTTP request to get login/password (from CoordinatorForm)
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

        private bool ValidateAllFields()
        {
            bool isValid = true;

            // Skip validation for fields that are populated from TelpEmployeeDto
            if (_employeeData == null)
            {
                if (string.IsNullOrWhiteSpace(employeeFullNameTextBox.Text) ||
                    !RussianNameRegex.IsMatch(employeeFullNameTextBox.Text))
                {
                    ShowError(employeeFullNameTextBox, "ФИО должно содержать только русские буквы и дефисы");
                    isValid = false;
                }

                if (string.IsNullOrWhiteSpace(personnelNumberTextBox.Text) ||
                    !PersonnelNumberRegex.IsMatch(personnelNumberTextBox.Text))
                {
                    ShowError(personnelNumberTextBox, "Табельный номер должен содержать ровно 10 цифр");
                    isValid = false;
                }
            }

            // Always validate these fields
            if (string.IsNullOrWhiteSpace(employeesPositionTextBox.Text) ||
                DepartmentForNewcomerComboBox.SelectedItem == null ||
                RoleOfNewcomerComboBox.SelectedItem == null ||
                !dateOfBirthDateTimePicker.SelectedDate.HasValue)
            {
                isValid = false;
            }

            return isValid;
        }

        private void DataIsFilledButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Проверьте правильность введенных данных пожалуйста.");
            uploadNewcommer.IsEnabled = true;
        }

        private void ClearForm()
        {
            if (_employeeData == null)
            {
                employeeFullNameTextBox.Clear();
                personnelNumberTextBox.Clear();
                DepartmentForNewcomerComboBox.SelectedIndex = -1;
                DepartmentForNewcomerComboBox.IsEnabled = true;
            }

            employeesPositionTextBox.Clear();
            //WorkplaceNumberTextBox.Clear(); КНОПКА БЫЛА УБРАНА, ПОКА ЧТО.
            dateOfBirthDateTimePicker.SelectedDate = DateTime.Today;
            RoleOfNewcomerComboBox.SelectedIndex = -1;
            AddInitialInstructionToNewcomer.IsChecked = false;
            loginTextBox.Clear();
            PasswordTextBox.Clear();

            ClearError(employeeFullNameTextBox);
            ClearError(personnelNumberTextBox);

            uploadNewcommer.IsEnabled = false;
        }
    }
}