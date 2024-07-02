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
using Newtonsoft.Json.Linq;


namespace Kotova.Test1.ClientSide
{
    public partial class CoordinatorForm : Form
    {
        private const string DownloadDepartmentsForUserURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/download-list-of-departments";
        private const string InsertNewEmployeeURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/insert-new-employee";
        private const string GetLoginPasswordUrl = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get-login-and-password-for-newcommer";
        private const string DownloadRolesForUsersUrl = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get-roles-for-newcomer";
        private Login_Russian? _loginForm;
        private string? _userName;
        private string? selectedFolderPath;
        List<string>? rolesOfUsers = new List<string>();
        public CoordinatorForm()
        {
            InitializeComponent();
        }

        public CoordinatorForm(Login_Russian loginForm, string userName)
        {
            _userName = userName;
            _loginForm = loginForm;
            InitializeComponent();
        }

        private async void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 3)
            {
                if (await refreshDepartmentsFromDB(DepartmentForNewcomer))
                {
                    if (await refreshRolesFromDB(RoleOfNewcomerListBox))
                    {
                        MessageBox.Show("Отделы и роли обновились успешно.");
                        return;

                    }
                    else
                    {
                        MessageBox.Show($"Отделы обновились, но роли нет! Что-то не так. Проверь tabControl1_SelectedIndexChanged");
                    }
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
            departmentForNewcomer.Items.Clear();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string jwtToken = _loginForm._jwtToken;
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();


                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    List<string> result = System.Text.Json.JsonSerializer.Deserialize<List<string>>(jsonResponse); //If you want - check that returned not empty List
                    departmentForNewcomer.Items.AddRange(result.ToArray());
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

        private async Task<bool> refreshRolesFromDB(ListBox listBoxRoles) // THE SAME FUNCTION AS refreshDepartmentsFromDB. just url changed and listbox, that's all. Oh, and List<string> have different names additionally.
        {
            string url = DownloadRolesForUsersUrl;
            listBoxRoles.Items.Clear();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string jwtToken = _loginForm._jwtToken;
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();


                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    rolesOfUsers = System.Text.Json.JsonSerializer.Deserialize<List<string>>(jsonResponse); //If you want - check that returned not empty List
                    if (rolesOfUsers == null || rolesOfUsers.Count == 0)
                    {
                        return false;
                    }
                    listBoxRoles.Items.AddRange(rolesOfUsers.ToArray());
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
            if (!IsValidRole(RoleOfNewcomerListBox.SelectedIndex))
            {
                MessageBox.Show("Не выбрана или выбрана неверная роль сотрудника.");
            }
            Employee newEmployee = new Employee();
            newEmployee.personnel_number = personnelNumberTextBox.Text;
            newEmployee.full_name = employeeFullNameTextBox.Text;
            newEmployee.job_position = employeesPositionTextBox.Text;
            newEmployee.department = DepartmentForNewcomer.SelectedItem.ToString(); //Здесь уже осуществлена проверка на выбор отдела
            newEmployee.group = null;
            newEmployee.birth_date = dateOfBirthDateTimePicker.Value;
            newEmployee.gender = 3;

            string roleName = RoleOfNewcomerListBox.SelectedItem.ToString();

            try
            {
                string token = _loginForm._jwtToken;
                var response = await InsertNewEmployeeAsync(newEmployee, token);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Employee inserted into DB", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    try
                    {
                        response = await GetLoginPassword(new List<string> { newEmployee.personnel_number, newEmployee.department, WorkplaceNumberTextBox.Text, roleName }, token);
                        if (response.IsSuccessStatusCode)
                        {

                            var jsonResponse = await response.Content.ReadAsStringAsync();
                            var login_and_password = System.Text.Json.JsonSerializer.Deserialize<Tuple<string, string>>(jsonResponse);
                            loginTextBox.Text = login_and_password.Item1;
                            PasswordTextBox.Text = login_and_password.Item2;
                        }
                        else
                        {
                            string errorText = await response.Content.ReadAsStringAsync();
                            MessageBox.Show($"Error getting login/password: {errorText}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"some error occured while getting login/password: {ex}");
                    }
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

        private bool IsValidRole(int selectedIndex)
        {
            List<int> validRoles = new List<int> { 0, 1, 3 };
            if (validRoles.Contains(selectedIndex))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private async Task<HttpResponseMessage> GetLoginPassword(List<string> dataAboutUser, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                string url = GetLoginPasswordUrl;
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var json = JsonConvert.SerializeObject(dataAboutUser);
                var data = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, data);
                return response;
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
        //TODO: is valid number
        private static bool IsValidRussianFullName(string name)
        {
            return Regex.IsMatch(name, @"^[А-ЯЁа-яё]+([ -][А-ЯЁа-яё]+)*$");
        }
        private static bool IsValidPersonnelNumber(string number)
        {
            return Regex.IsMatch(number, @"^\d{10}$");
        }

        private void InitialInstructionButton_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.SelectedPath = ConfigurationClass.DEFAULT_PATH_TO_INITIAL_INSTRUCTIONS;

                DialogResult result = folderBrowserDialog.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
                {
                    selectedFolderPath = folderBrowserDialog.SelectedPath;

                    InitialInstructionPathLabel.Text = selectedFolderPath;


                    MessageBox.Show($"Selected Folder: {selectedFolderPath}");

                }
            }
            uploadNewcommer.Enabled = true;
            // Добавить последующий код в функцию! ОНО ВООБЩЕ ДОЛЖНО БЫТЬ ЗДЕСЬ?
            /*buttonCreateInstruction.Enabled = false;
            if (selectedFolderPath is null)
            {
                MessageBox.Show("Путь до инструктажа не выбран!");
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
                buttonCreateInstruction.Enabled = true;
                MessageBox.Show("Не выбран тип инструктажа!");
                return;
            }

            string causeOfInstruction = "Вводный инструктаж";
            Byte typeOfInstruction = (Byte)(typeOfInstructionListBox.SelectedIndex + 2); //ЗДЕСЬ ПОДРАЗУМЕВАЕТСЯ, ЧТО ТИПОВ ИНСТРУКТАЖЕЙ НЕ БОЛЬШЕ 6 в listBox!
            Instruction instruction = new Instruction(causeOfInstruction, startTime, endDate, selectedFolderPath, typeOfInstruction);
            string json = JsonConvert.SerializeObject(instruction);
            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
            await Test.connectionToUrlPost(urlCreateInstruction, content, $"Инструктаж '{causeOfInstruction}' успешно добавлен в базу данных.");
            buttonCreateInstruction.Enabled = true;
            InstructionTextBox.Text = "";
            typeOfInstructionListBox.SelectedIndex = -1;
            selectedFolderPath = null;
            PathToFolderOfInstruction.Text = "Путь не выбран";*/

        }

        private void CoordinatorForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_loginForm is not null)
            {
                _loginForm.Dispose();
            }
            this.Dispose(true);
        }

        private void LogOut_Click(object sender, EventArgs e)
        {
            try
            {
                File.Delete(Decryption_stuff.defaultFilePath);
            }
            catch
            {

            }
            _loginForm.Show();
            this.Dispose(true);
        }
    }
}
