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
using Microsoft.IdentityModel.Tokens;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Diagnostics;


namespace Kotova.Test1.ClientSide
{
    public partial class CoordinatorForm : Form
    {
        private const string DownloadDepartmentsForUserURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/download-list-of-departments";
        private const string InsertNewEmployeeURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/insert-new-employee";
        private const string GetLoginPasswordUrl = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get-login-and-password-for-newcommer";
        private const string DownloadInstructionForUserURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get_instructions_for_user";
        private const string DownloadRolesForUsersUrl = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get-roles-for-newcomer";
        private const string DownloadNamesForInitialInstrUrl = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get-list-of-people-init-instructions";
        const string SendInstructionIsPassedURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/instruction_is_passed_by_user";
        private Login_Russian? _loginForm;
        private string? _userName;
        private string? selectedFolderPath;
        List<InstructionDto>? namesOfUsersOfInitInstr = new List<InstructionDto>();
        private List<Dictionary<string, object>> listOfInstructions_global;
        private List<Dictionary<string, object>> listsOfPaths_global;


        public const string dB_instructionId = "instruction_id"; //ВЫНЕСИ ЭТИ 2 СТРОЧКИ В ОБЩИЙ ФАЙЛ!
        public const string db_filePath = "file_path";


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

                    // Parse the JSON response to extract the $values array
                    var jsonDocument = JsonDocument.Parse(jsonResponse);
                    var valuesElement = jsonDocument.RootElement.GetProperty("$values");
                    List<string> result = System.Text.Json.JsonSerializer.Deserialize<List<string>>(valuesElement.GetRawText());

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

        private async Task<bool> refreshRolesFromDB(ListBox listBoxRoles)
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

                    // Parse the JSON response to extract the $values array
                    var jsonDocument = JsonDocument.Parse(jsonResponse);
                    var valuesElement = jsonDocument.RootElement.GetProperty("$values");
                    List<string>? rolesOfUsers = RolesDBToRoleNames(System.Text.Json.JsonSerializer.Deserialize<List<string>>(valuesElement.GetRawText()));

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

        private List<string>? RolesDBToRoleNames(List<string>? list)
        {
            var roles = new List<string>();
            if (list.IsNullOrEmpty()) { return list; }
            {
                foreach (var role in list)
                {
                    switch (role)
                    {
                        case "user":
                            roles.Add("Сотрудник");
                            break;
                        case "chief of department":
                            roles.Add("Руководство ОТДЕЛА");
                            break;
                        case "coordinator":
                            roles.Add("Охрана труда");
                            break;
                        case "management":
                            roles.Add("Руководство ФИЛИАЛА");
                            break;
                        default:
                            break;
                    }
                }
            }
            return roles;
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

            string? roleName = RoleNameToRoleDB(RoleOfNewcomerListBox.SelectedItem.ToString());

            string isEmployeeRequireInitInstr = AddInitialInstructionToNewcomer.Checked.ToString();
            try
            {
                string token = _loginForm._jwtToken;
                var response = await InsertNewEmployeeAsync(newEmployee, token);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Employee inserted into DB", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    try
                    {
                        response = await GetLoginPassword(new List<string> { newEmployee.personnel_number, newEmployee.department, WorkplaceNumberTextBox.Text, roleName, isEmployeeRequireInitInstr }, token);
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

        private string? RoleNameToRoleDB(string? v)
        {
            switch (v)
            {
                case "Обычный сотрудник":
                    return "user";
                case "Руководство ОТДЕЛА":
                    return("chief of department");
                case "Охрана труда":
                    return("coordinator");
                case "Руководство ФИЛИАЛА":
                    return("management");
                default:
                    return null;
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
                string url = InsertNewEmployeeURL;
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var json = JsonConvert.SerializeObject(employee);
                var data = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, data); // TODO: Это надо переделать, так как сервер возвращает что всё хорошо даже когда это не так :/
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
            Decryption_stuff.DeleteJWTToken();
            _loginForm.Show();
            this.Dispose(true);
        }

        private async void buttonSyncNamesForInitialInstrWithDB_Click(object sender, EventArgs e)
        {
            string url = DownloadNamesForInitialInstrUrl;
            NamesOfPeopleForInitialInstr.Items.Clear();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string jwtToken = _loginForm._jwtToken;
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var jsonResponse = await response.Content.ReadAsStringAsync();

                    // Log the JSON response
                    Console.WriteLine("JSON Response: " + jsonResponse);

                    // Parse the JSON response
                    var jsonDocument = JsonDocument.Parse(jsonResponse);
                    var root = jsonDocument.RootElement;

                    // Extract the "$values" array
                    if (root.TryGetProperty("$values", out JsonElement valuesElement))
                    {
                        // Deserialize the "$values" array to a list of InstructionDto
                        var options = new JsonSerializerOptions
                        {
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            WriteIndented = true
                        };

                        var result = System.Text.Json.JsonSerializer.Deserialize<List<InstructionDto>>(valuesElement.GetRawText(), options);
                        namesOfUsersOfInitInstr = result;
                        // Check and display the results
                        if (result == null || result.Count == 0)
                        {
                            MessageBox.Show("Все люди прошли вводные инструктажи!");
                        }
                        else
                        {
                            var itemsToAdd = result.Select(e => $"{e.Name}").ToList();
                            foreach (var item in itemsToAdd)
                            {
                                NamesOfPeopleForInitialInstr.Items.Add(item);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("JSON does not contain '$values' key");
                        MessageBox.Show("Ошибка: JSON не содержит ключа '$values'");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                // Handle any exceptions here
                MessageBox.Show($"Error: {ex.Message}");
            }
            catch (System.Text.Json.JsonException jsonEx)
            {
                // Handle JSON exceptions
                MessageBox.Show($"JSON Error: {jsonEx.Message}");
            }
        }

        private void NamesOfPeopleForInitialInstr_SelectedIndexChanged(object sender, EventArgs e)
        {
            //НЕ РЕАЛИЗОВАНО!
        }








        //ВКЛАДКА С "Прохождение инструктажей"




        private async void CoordinatorTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CoordinatorTabControl.SelectedTab.Text == "Прохождение инструктажей")
            {
                ListOfInstructionsForUser.Items.Clear();
                bool IsEmpty = await DownloadInstructionsForUserFromServer(_userName);
                if (IsEmpty == true)
                {
                    MessageBox.Show("All the instructions passed!");
                }
            }
            if (CoordinatorTabControl.SelectedTab.Text == "Данные сотрудника")
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

        private async Task<bool> DownloadInstructionsForUserFromServer(string? userName) // по факту эта функция должна быть вместе с в User.cs в совершенно отдельном файле.
        {
            if (userName is null)
            {
                throw new ArgumentNullException(nameof(userName));
            }
            string url = DownloadInstructionForUserURL;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string jwtToken = _loginForm._jwtToken;
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();


                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var result = System.Text.Json.JsonSerializer.Deserialize<QueryResult>(jsonResponse);
                    if (result.Result1.Count == 0)
                    {
                        return true;
                    }

                    listOfInstructions_global = result.Result1;
                    listsOfPaths_global = result.Result2;
                    foreach (Dictionary<string, object> temp in result.Result1)
                    {
                        ListOfInstructionsForUser.Items.Add(temp[DataBaseNames.tableName_sql_INSTRUCTIONS_cause]);

                    }
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                // Handle any exceptions here
                MessageBox.Show($"Error: {ex.Message}");
                throw ex;
            }
        }

        private void InstructionsToPass_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilesOfInstructionCheckedListBox.Items.Clear();
            HyperLinkForInstructionsFolder.Enabled = true;
            if (ListOfInstructionsForUser.SelectedItem == null)
            {
                MessageBox.Show("You haven't select the Instruction.");
                PassInstructionAsUser.Enabled = false;
                HyperLinkForInstructionsFolder.Enabled = false;
                return;
            }
            Dictionary<string, object> selectedDict = GetDictFromSelectedInstruction(ListOfInstructionsForUser.SelectedItem.ToString());
            int instructionId = Convert.ToInt32(selectedDict[dB_instructionId].ToString());

            List<string> listOfPath = new List<string>();
            foreach (var listOfPaths in listsOfPaths_global)
            {
                if (Convert.ToInt32(listOfPaths[dB_instructionId].ToString()) == instructionId)
                {
                    FilesOfInstructionCheckedListBox.Items.Add(listOfPaths[db_filePath].ToString());
                }
            }
            HyperLinkForInstructionsFolder.Enabled = true;
        }

        private void HyperLinkForInstructionsFolder_Click(object sender, EventArgs e)
        {
            HyperLinkForInstructionsFolder.Enabled = false;
            if (ListOfInstructionsForUser.SelectedItem == null)
            {
                MessageBox.Show("You haven't select the Instruction.");
                PassInstructionAsUser.Enabled = false;
                return;
            }
            Dictionary<string, object> selectedDict = GetDictFromSelectedInstruction(ListOfInstructionsForUser.SelectedItem.ToString()); //most likely suppress it, cause its not null.
            string? pathStr = selectedDict[DataBaseNames.tableName_sql_pathToInstruction].ToString();

            if (pathStr is null || pathStr.Length == 0)
            {
                MessageBox.Show("Path is null or empty.");
                PassInstructionAsUser.Enabled = false;
                return;
            }
            string path = Path.GetFullPath(pathStr);
            OpenFolderInExplorer(path);

        }

        private void OpenFolderInExplorer(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                MessageBox.Show("Provided path is null or empty.");
                PassInstructionAsUser.Enabled = false;
                return;
            }

            // Get the full path and check if it exists
            string fullPath = Path.GetFullPath(path);
            if (!Directory.Exists(fullPath))
            {
                MessageBox.Show($"The path '{fullPath}' does not exist.");
                PassInstructionAsUser.Enabled = false;
                return;
            }

            // Open the folder in Windows Explorer
            try
            {
                Process.Start("explorer.exe", fullPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open the folder: {ex.Message}");
                PassInstructionAsUser.Enabled = false;
            }
        }

        private Dictionary<string, object> GetDictFromSelectedInstruction(string selectedItemStr)
        {

            foreach (Dictionary<string, object> tempD in listOfInstructions_global)
            {
                Dictionary<string, object> selectedDictionary = listOfInstructions_global.FirstOrDefault(tempD => tempD[DataBaseNames.tableName_sql_INSTRUCTIONS_cause].ToString() == selectedItemStr);
                if (selectedDictionary != null)
                {
                    return selectedDictionary; // HERE WE DIDN't CHECK  THAT названия инструктажей не повторяется, а просто вернули первое попавшееся. Проверку бы!
                }
            }
            throw new Exception("Corresponding Dictionary not found!");

        }

        private async void PassInstruction_CheckedChanged(object sender, EventArgs e)
        {
            if (!PassInstructionAsUser.Checked) { return; }
            if (ConfirmAction())
            {
                MessageBox.Show("You agreed with the action.", "Action Confirmed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                PassInstructionAsUser.Enabled = false;
                if (ListOfInstructionsForUser.SelectedItem == null)
                {
                    MessageBox.Show("You haven't select the Instruction.");
                    PassInstructionAsUser.Enabled = false;
                    return;
                }
                Dictionary<string, object> selectedDict = GetDictFromSelectedInstruction(ListOfInstructionsForUser.SelectedItem.ToString());
                await SendInstructionIsPassedToDB(selectedDict);
                FilesOfInstructionCheckedListBox.Items.Clear();
            }
            else
            {
                MessageBox.Show("You did not agree with the action.", "Action Canceled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                PassInstructionAsUser.Checked = false;
                return;
            }
        }

        private bool ConfirmAction()
        {
            var result = MessageBox.Show("Do you agree with the action?", "Confirm Action", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private async Task SendInstructionIsPassedToDB(Dictionary<string, object> selectedDict)
        {
            string url = SendInstructionIsPassedURL;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string jwtToken = _loginForm._jwtToken;
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                    string jsonData = System.Text.Json.JsonSerializer.Serialize(selectedDict);

                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(url, content);
                    response.EnsureSuccessStatusCode();

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Everyting is fine, updating the listbox of instructions");
                        ListOfInstructionsForUser.Items.Clear();
                        await DownloadInstructionsForUserFromServer(_userName);
                    }
                }
            }
            catch (HttpRequestException ex)
            {

                // Handle any exceptions here
                MessageBox.Show($"Error: {ex.Message}");
            }
            finally
            {
                PassInstructionAsUser.Checked = false;
            }
        }

        private void PopulateTreeView(string directoryValue, TreeNode parentNode)
        {
            // Processing directories
            string[] directoryArray = Directory.GetDirectories(directoryValue);
            try
            {
                foreach (string directory in directoryArray)
                {
                    string directoryName = Path.GetFileName(directory);
                    TreeNode myNode = new TreeNode(directoryName);
                    parentNode.Nodes.Add(myNode);
                    PopulateTreeView(directory, myNode);
                }
            }
            catch (UnauthorizedAccessException) { }

            // Processing files
            string[] fileArray = Directory.GetFiles(directoryValue);
            foreach (string file in fileArray)
            {
                string fileName = Path.GetFileName(file);
                TreeNode fileNode = new TreeNode(fileName);
                parentNode.Nodes.Add(fileNode);
            }
        }

        private void CheckAllChildNodes(TreeNode treeNode, bool nodeChecked)
        {
            foreach (TreeNode childNode in treeNode.Nodes)
            {
                if (childNode.Checked != nodeChecked)
                {
                    childNode.Checked = nodeChecked;
                    CheckAllChildNodes(childNode, nodeChecked); // Recursive call
                }
            }
        }

        private static void CollectFilePaths(TreeNode node, HashSet<string> filePaths)
        {
            if (node.Checked)
            {
                string fullPath = node.FullPath;

                // Check if it's a file (assuming leaf nodes are files)
                if (GetInfo(fullPath).type == 0)
                {
                    filePaths.Add(fullPath);
                }
            }

            foreach (TreeNode childNode in node.Nodes)
            {
                CollectFilePaths(childNode, filePaths);
            }
        }

        static Checkpath GetInfo(string path)
        {
            Checkpath checkpath = new Checkpath();
            try
            {
                FileAttributes attr = File.GetAttributes(path);

                //detect whether its a directory or file  
                if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    checkpath.type = Filetype.Dir;
                else
                    checkpath.type = Filetype.File;
                checkpath.Ifexists = true;
            }
            catch
            {
                bool t = Path.HasExtension(path);
                if (t)
                {
                    checkpath.type = Filetype.File;
                }
                else
                {
                    checkpath.type = Filetype.Dir;
                }

                checkpath.Ifexists = false;
            }
            return checkpath;
        }

        public class Checkpath
        {
            public bool Ifexists { get; set; }

            public Filetype type { get; set; }
        }

        public enum Filetype
        {
            File = 0,
            Dir = 1
        }

        private void FilesOfInstructionCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            this.BeginInvoke((MethodInvoker)delegate
            {
                if (AreAllItemsChecked(FilesOfInstructionCheckedListBox))
                {
                    PassInstructionAsUser.Enabled = true;
                }
                else
                {
                    PassInstructionAsUser.Enabled = false;
                }

                if (e.NewValue == CheckState.Checked)
                {
                    string selectedPath = FilesOfInstructionCheckedListBox.Items[e.Index].ToString();
                    OpenFile(selectedPath);
                }
            });

        }
        private bool AreAllItemsChecked(CheckedListBox checkedListBox)
        {
            for (int index = 0; index < checkedListBox.Items.Count; index++)
            {
                if (!checkedListBox.GetItemChecked(index))
                {
                    return false;
                }
            }
            return true;
        }

        private void OpenFile(string filePath)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open file: {ex.Message}");
            }
        }

    }
}
