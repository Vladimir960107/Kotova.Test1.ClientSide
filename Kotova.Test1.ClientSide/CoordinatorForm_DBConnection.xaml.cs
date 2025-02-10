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

namespace Kotova.Test1.ClientSide
{
    public partial class CoordinatorForm_DBConnection : Window
    {
        private static readonly string DownloadDepartmentsForUserURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/download-list-of-departments";
        private static readonly string DownloadRolesForUsersUrl = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get-roles-for-newcomer";

        // Regular expressions for validation
        private static readonly Regex RussianNameRegex = new Regex(@"^[А-ЯЁа-яё]+([ -][А-ЯЁа-яё]+)*$");
        private static readonly Regex PersonnelNumberRegex = new Regex(@"^\d{10}$");

        private readonly Login_Russian _loginForm;
        private readonly TelpEmployeeDto _employeeData;

        public CoordinatorForm_DBConnection(Login_Russian loginForm, TelpEmployeeDto employeeData = null)
        {
            InitializeComponent();
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
                    DepartmentForNewcomer.Items.Add(dept);
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
                    if (CommonRoleNamesForCoordinatorForms.RoleDisplayNames.TryGetValue(role, out string displayName))
                    {
                        RoleOfNewcomerListBox.Items.Add(displayName);
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
            var deptItem = DepartmentForNewcomer.Items.Cast<string>()
                .FirstOrDefault(d => d == _employeeData.DepartmentName);
            if (deptItem != null)
            {
                DepartmentForNewcomer.SelectedItem = deptItem;
                DepartmentForNewcomer.IsEnabled = false;
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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            uploadNewcommer.IsEnabled = false;

            if (!ValidateAllFields())
            {
                System.Windows.MessageBox.Show("Пожалуйста, проверьте правильность заполнения всех полей",
                              "Ошибка валидации",
                              MessageBoxButton.OK,
                              MessageBoxImage.Warning);
                uploadNewcommer.IsEnabled = true;
                return;
            }

            if (dateOfBirthDateTimePicker.SelectedDate.HasValue)
            {
                var age = DateTime.Today.Year - dateOfBirthDateTimePicker.SelectedDate.Value.Year;
                if (dateOfBirthDateTimePicker.SelectedDate.Value > DateTime.Today.AddYears(-age)) age--;

                if (age < 18)
                {
                    System.Windows.MessageBox.Show("Сотрудник должен быть старше 18 лет",
                                  "Ошибка валидации",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                    uploadNewcommer.IsEnabled = true;
                    return;
                }
            }

            System.Windows.MessageBox.Show("Данные сотрудника успешно сохранены",
                          "Успех",
                          MessageBoxButton.OK,
                          MessageBoxImage.Information);

            loginTextBox.Text = $"User{new Random().Next(1000000, 9999999)}";
            PasswordTextBox.Text = loginTextBox.Text;

            uploadNewcommer.IsEnabled = true;
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
                DepartmentForNewcomer.SelectedItem == null ||
                RoleOfNewcomerListBox.SelectedItem == null ||
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
                DepartmentForNewcomer.SelectedIndex = -1;
                DepartmentForNewcomer.IsEnabled = true;
            }

            employeesPositionTextBox.Clear();
            WorkplaceNumberTextBox.Clear();
            dateOfBirthDateTimePicker.SelectedDate = DateTime.Today;
            RoleOfNewcomerListBox.SelectedIndex = -1;
            AddInitialInstructionToNewcomer.IsChecked = false;
            loginTextBox.Clear();
            PasswordTextBox.Clear();

            ClearError(employeeFullNameTextBox);
            ClearError(personnelNumberTextBox);

            uploadNewcommer.IsEnabled = false;
        }
    }
}