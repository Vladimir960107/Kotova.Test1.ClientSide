using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.Json;
using System.Text.RegularExpressions;
using Kotova.CommonClasses;
using Newtonsoft.Json;


namespace Kotova.Test1.ClientSide
{
    public partial class CoordinatorForm : Form
    {
        private const string DownloadDepartmentsForUserURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/download-list-of-departments";
        private const string InsertNewEmployeeURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/insert-new-employee";
        private Form? _loginForm;
        private string? _userName;
        public CoordinatorForm()
        {
            InitializeComponent();
        }

        public CoordinatorForm(Form loginForm, string userName)
        {
            _userName = userName;
            _loginForm = loginForm;
            InitializeComponent();
        }

        private async void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 3)
            {
                DepartmentForNewcomer.Items.Clear();
                if (await refreshDepartmentsFromDB(DepartmentForNewcomer))
                {
                    MessageBox.Show("Отделы обновились успешно.");
                    return;

                }
                else
                {
                    MessageBox.Show("Не обновились данные (скорее всего отсутствует соединение с сервером)");
                }

            }
        }

        private async Task<bool> refreshDepartmentsFromDB(ListBox departmentForNewcomer)
        {
            string url = DownloadDepartmentsForUserURL;
            DepartmentForNewcomer.Items.Clear();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string jwtToken = Decryption_stuff.DecryptedJWTToken();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                        
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    List<string> result = System.Text.Json.JsonSerializer.Deserialize<List<string>>(jsonResponse); //If you want - check that returned not empty List
                    DepartmentForNewcomer.Items.AddRange(result.ToArray());
                    return true;
                }
            }
            catch (HttpRequestException ex)
            {
                // Handle any exceptions here
                MessageBox.Show($"Error: {ex.Message}");
                return false;
            }
        }

        private async void uploadNewcommer_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(employeeFullNameTextBox.Text))
            {
                MessageBox.Show("ФИО не заплнено.");
                return;
            }
            if (!IsValidRussianFullName(employeeFullNameTextBox.Text))
            {
                MessageBox.Show("Недействительные ФИО. Пожалуйста, вводите только русские буквы и дефисы.");
                return;
            }
            if (string.IsNullOrWhiteSpace(employeesPositionTextBox.Text))
            {
                MessageBox.Show("Должность не заполнена.");
                return;
            }
            if (DepartmentForNewcomer.SelectedIndex == -1)
            {
                MessageBox.Show("Отдел не выбран.");
                return;
            }
            if (string.IsNullOrWhiteSpace(personnelNumberTextBox.Text))
            {
                MessageBox.Show("Табельный номер не заполнен.");
                return;
            }
            if (!IsValidPersonnelNumber(personnelNumberTextBox.Text))
            {
                MessageBox.Show("Неверный табельный номер. Он должен состоять ровно из 10 цифр.");
                return;
            }
            if (string.IsNullOrWhiteSpace(WorkplaceNumberTextBox.Text))
            {
                MessageBox.Show("Номер рабочего места не заполнен.");
                return;
            }
            if (!IsValidDateOfBirth(dateOfBirthDateTimePicker.Text))
            {
                MessageBox.Show("Неверная дата рождения. Введите действительную дату и убедитесь, что возрастной критерий(>=18 лет) соответствует.");
                return;
            }
            Employee newEmployee = new Employee();
            newEmployee.personnel_number = personnelNumberTextBox.Text;
            newEmployee.full_name = employeeFullNameTextBox.Text;
            newEmployee.job_position = employeesPositionTextBox.Text;
            newEmployee.department = DepartmentForNewcomer.SelectedItem.ToString(); //Здесь уже осуществлена проверка на выбор отдела
            newEmployee.group = null;
            newEmployee.birth_date = dateOfBirthDateTimePicker.Value;
            newEmployee.gender = 3;

            try
            {
                string token = Decryption_stuff.DecryptedJWTToken(); 
                var response = await InsertNewEmployeeAsync(newEmployee, token);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Employee inserted into DB", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    string errorText = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Error inserting employee: {errorText}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        private async Task<HttpResponseMessage> InsertNewEmployeeAsync(Employee employee, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                string url = InsertNewEmployeeURL; // Replace with your actual API URL
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var json = JsonConvert.SerializeObject(employee);
                var data = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, data);
                return response;
            }
                
        }

        private bool IsValidDateOfBirth(string dobText)
        {
            DateTime dob;
            // Attempt to parse the date
            if (DateTime.TryParse(dobText, out dob))
            {
                // Check if date is within a reasonable range, e.g., at least 18 years ago
                if (dob <= DateTime.Now.AddYears(-18))
                {
                    return true;
                }
            }
            return false;
        }
        private static bool IsValidRussianFullName(string name)
        {
            return Regex.IsMatch(name, @"^[А-ЯЁа-яё]+(-[А-ЯЁа-яё]+)*$");
        }
        private static bool IsValidPersonnelNumber(string number)
        {
            return Regex.IsMatch(number, @"^\d{10}$");
        }
    }
}
