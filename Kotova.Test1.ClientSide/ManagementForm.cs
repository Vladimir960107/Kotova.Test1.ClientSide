﻿using ClosedXML.Excel;
using Kotova.CommonClasses;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Kotova.Test1.ClientSide
{
    public partial class ManagementForm : Form
    {

        private static readonly string DownloadDepartmentsForUserURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/download-list-of-departments";
        private static readonly string DownloadDepartmentsAndEmployeesURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/download-list-of-all-departments-and-employees";
        private static readonly string InsertNewEmployeeURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/insert-new-employee";
        private static readonly string GetLoginPasswordUrl = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get-login-and-password-for-newcommer";
        private static readonly string DownloadInstructionForUserURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get_not_passed_instructions_for_user";
        private static readonly string DownloadRolesForUsersUrl = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get-roles-for-newcomer";
        private static readonly string DownloadNamesForInitialInstrUrl = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get-list-of-people-init-instructions";
        private static readonly string SubmitUnplannedInstructionURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/send-unplanned-instruction-to-chiefs";
        private static readonly string instructionDataExportForManagementURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/instructions-data-export-for-management";
        private static readonly string SendInstructionIsPassedURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/instruction_is_passed_by_user";


        private Login_Russian? _loginForm;
        private string? _userName;
        public SignUpForm _signUpForm;
        private string? selectedFolderPath;
        List<InstructionDto>? namesOfUsersOfInitInstr = new List<InstructionDto>();
        private List<Dictionary<string, object>> listOfInstructions_global;
        private List<Dictionary<string, object>> listsOfPaths_global;

        private HubConnection? _hubConnection = null;


        public const string dB_instructionId = "instruction_id"; //ВЫНЕСИ ЭТИ 3 СТРОЧКИ В ОБЩИЙ ФАЙЛ!
        public const string db_filePath = "file_path";
        public const string db_typeOfInstruction = "type_of_instruction";

        public ManagementForm(Login_Russian loginForm, string userName)
        {
            InitializeComponent();
            _loginForm = loginForm;
            _userName = userName;
            ManagementLabel.Text = _userName;
            PassInstructionAsUser.Enabled = false;
            DepartmentsCheckedListBox.Enabled = false;
            PeopleAndDepartmentsTreeView.Enabled = false;
            refreshDepartmentsAndnPeopleFromDBInternal();

            InitializeSignalRConnection();

            SignUpForm signUpForm = new SignUpForm(loginForm, this);
            _signUpForm = signUpForm;
        }

        public ManagementForm()
        {
            InitializeComponent();
        }

        private async void InitializeSignalRConnection()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(ConfigurationClass.BASE_SIGNALR_CONNECTION_URL_DEVELOPMENT, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(_loginForm._jwtToken);
                })
            .Build();

            _hubConnection.On<string, string>("Получено сообщение", (user, message) =>
            {
                // Handle incoming messages from the SignalR hub
                MessageBox.Show($"{user}: {message}", "Message from Hub");
            });

            _hubConnection.On<string>("ReceiveAlert", message =>
            {
                Notifications.ShowWindowsNotification("Alert", message);
            });

            try
            {
                await _hubConnection.StartAsync();
                //MessageBox.Show("Подключено к SignalR hub.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось подключиться к SignalR hub: {ex.Message}");
            }
        }



        private async void ManagementTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ManagementTabControl.SelectedTab.Text == "Прохождение инструктажей")
            {
                ListOfInstructionsForUser.Items.Clear();
                bool IsEmpty = await DownloadInstructionsForUserFromServer(_userName);
                if (IsEmpty == true)
                {
                    MessageBox.Show("Все инструктажи пройдены!");
                }
            }
            if (ManagementTabControl.SelectedTab.Text == "Создание инструктажей")
            {
                refreshDepartmentsAndnPeopleFromDBInternal();
            }
        }

        private async void refreshDepartmentsAndnPeopleFromDBInternal()
        {
            if (await refreshDepartmentsFromDB(DepartmentsCheckedListBox))
            {
                if (await refreshDepartmentsAndPeopleFromDB(PeopleAndDepartmentsTreeView))
                {
                    //MessageBox.Show("Отделы и люди обновились успешно.");
                    return;
                }
                else
                {
                    MessageBox.Show("Не обновились люди, но обновились отделы. Потеряно соединение с сервером?");
                    return;
                }
            }
            else
            {
                MessageBox.Show("Не обновились данные (скорее всего отсутствует соединение с сервером)");
                return;
            }
        }


        #region Вкладка создания инструктажей для отдела/конкретных людей


        private void buttonChoosePathToInstruction_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                // Optionally set the initial directory
                // folderBrowserDialog.SelectedPath = @"C:\Initial\Folder\Path";

                DialogResult result = folderBrowserDialog.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
                {
                    // Store the selected folder path in a variable
                    selectedFolderPath = folderBrowserDialog.SelectedPath;

                    PathToFolderOfInstruction.Text = selectedFolderPath;

                    buttonCreateInstruction.Enabled = true;

                    treeView1.Nodes.Clear();  // Clear the existing items in the TreeView
                    TreeNode rootNode = new TreeNode(selectedFolderPath);
                    treeView1.Nodes.Add(rootNode);  // Add a root node with the selected folder
                    PopulateTreeView(selectedFolderPath, rootNode);  // Populate the TreeView
                    //rootNode.Expand();  // Optionally expand the root node, enable if want to all the rootNode be collapsed (чтобы было видно все вложенные файлы сразу, не нажимая плюсик :))

                    MessageBox.Show($"Selected Folder: {selectedFolderPath}");


                }
            }
        }

        private async Task<bool> refreshDepartmentsFromDB(ListBox departmentForNewcomer) // Эта функция повторяется в CoordinatorForm.cs, поэтому попробуй их объединить в один файл мб.
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

        private async Task<bool> refreshDepartmentsAndPeopleFromDB(System.Windows.Forms.TreeView peopleAndDepartmentsTreeView) // Эта функция повторяется в CoordinatorForm.cs, поэтому попробуй их объединить в один файл мб.
        {
            string url = DownloadDepartmentsAndEmployeesURL;
            peopleAndDepartmentsTreeView.Nodes.Clear();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string jwtToken = _loginForm._jwtToken;
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    if (jsonResponse is null)
                    {
                        MessageBox.Show("Не обновились отделы с людьми!");
                        return false;
                    }
                    // Parse the JSON response to extract the $values array
                    List<Dept> result = System.Text.Json.JsonSerializer.Deserialize<List<Dept>>(jsonResponse);
                    PopulateTreeView(peopleAndDepartmentsTreeView, result);
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

        private void PopulateTreeView(System.Windows.Forms.TreeView treeView, List<Dept> departments)
        {
            treeView.Nodes.Clear(); // Clear existing nodes
            foreach (var department in departments)
            {
                TreeNode departmentNode = new TreeNode(department.Name) { Tag = department };
                AddUserNodes(departmentNode, department.Employees);
                treeView.Nodes.Add(departmentNode);
            }
        }

        private void AddUserNodes(TreeNode treeNode, List<Employee> users)
        {
            if (users == null) return;

            foreach (var user in users)
            {
                TreeNode userNode = new TreeNode(user.full_name) { Tag = user };
                treeNode.Nodes.Add(userNode);
            }
        }

        private void peopleAndDepartmentsTreeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Action != TreeViewAction.Unknown) // Ensure the change was triggered by user interaction
            {
                PeopleAndDepartmentsTreeView.Enabled = false;

                try
                {
                    // Perform the checking/unchecking synchronously
                    CheckAllChildNodes(e.Node, e.Node.Checked);
                    UpdateParentNodes(e.Node, e.Node.Checked);
                }
                finally
                {
                    PeopleAndDepartmentsTreeView.Enabled = true;
                }
            }
        }

        private void UpdateParentNodes(TreeNode treeNode, bool nodeChecked)
        {
            TreeNode currentNode = treeNode;

            while (currentNode.Parent != null)
            {
                if (nodeChecked)
                {
                    // If the current node is checked, ensure the parent is also checked
                    currentNode.Parent.Checked = true;
                }
                else
                {
                    // If the current node is unchecked, ensure the parent is unchecked
                    // only if all its siblings are also unchecked
                    bool allSiblingsUnchecked = true;

                    foreach (TreeNode sibling in currentNode.Parent.Nodes)
                    {
                        if (sibling.Checked)
                        {
                            allSiblingsUnchecked = false;
                            break;
                        }
                    }

                    if (allSiblingsUnchecked)
                    {
                        currentNode.Parent.Checked = false;
                    }
                }

                currentNode = currentNode.Parent;
            }
        }

        private void typeOfInstructionListBox_SelectedValueChanged(object sender, EventArgs e)
        {
            if (typeOfInstructionListBox.SelectedItem.ToString() == "Внеплановый;")
            {
                DepartmentsCheckedListBox.Enabled = true;
                PeopleAndDepartmentsTreeView.Enabled = false;
            }
            else
            {
                DepartmentsCheckedListBox.Enabled = false;
                PeopleAndDepartmentsTreeView.Enabled = true;
            }

        }


        #endregion

        #region Вкладка прохождения инструктажей самим сотрудником

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
                MessageBox.Show("Вы не выбрали инструктаж.");
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
                    if (listOfPaths[db_filePath] == null)
                    {
                        if (selectedDict[db_typeOfInstruction].ToString() == "0") // Проверка что мы входим в вводный инструктаж только!
                        {
                            PassInstructionAsUser.Enabled = true;
                            HyperLinkForInstructionsFolder.Enabled = false;
                            return;
                        }
                        else
                        {
                            MessageBox.Show("Ooops, Что-то пошло не так. Проверь эту строчку на предмет присутствия файлов инструктажа!");
                            PassInstructionAsUser.Enabled = true;
                            HyperLinkForInstructionsFolder.Enabled = false;
                            return;
                        }

                    }
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
                MessageBox.Show("Вы не выбрали инструктаж.");
                PassInstructionAsUser.Enabled = false;
                return;
            }
            Dictionary<string, object> selectedDict = GetDictFromSelectedInstruction(ListOfInstructionsForUser.SelectedItem.ToString()); //most likely suppress it, cause its not null.
            string? pathStr = selectedDict[DataBaseNames.tableName_sql_pathToInstruction].ToString();

            if (pathStr is null || pathStr.Length == 0)
            {
                MessageBox.Show("Путь пуст или отсутствует.");
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
                MessageBox.Show("Указанный путь пуст или отсутствует.");
                PassInstructionAsUser.Enabled = false;
                return;
            }

            // Get the full path and check if it exists
            string fullPath = Path.GetFullPath(path);
            if (!Directory.Exists(fullPath))
            {
                MessageBox.Show($"Путь '{fullPath}' не существует.");
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
                MessageBox.Show($"Не удалось открыть папку: {ex.Message}");
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
            throw new Exception("Соответствующий словарь не найден!");

        }

        private async void PassInstruction_CheckedChanged(object sender, EventArgs e)
        {
            if (!PassInstructionAsUser.Checked) { return; }
            if (ConfirmAction())
            {
                MessageBox.Show("Вы согласились, что прошли инструктаж", "Действие подтверждено", MessageBoxButtons.OK, MessageBoxIcon.Information);
                PassInstructionAsUser.Enabled = false;
                if (ListOfInstructionsForUser.SelectedItem == null)
                {
                    MessageBox.Show("Вы не выбрали инструктаж.");
                    PassInstructionAsUser.Enabled = false;
                    return;
                }
                Dictionary<string, object> selectedDict = GetDictFromSelectedInstruction(ListOfInstructionsForUser.SelectedItem.ToString());
                await SendInstructionIsPassedToDB(selectedDict);
                FilesOfInstructionCheckedListBox.Items.Clear();
            }
            else
            {
                MessageBox.Show("Согласие о прохождении инструктажа не подтверждено.", "Действите отменено", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                PassInstructionAsUser.Checked = false;
                return;
            }
        }

        private bool ConfirmAction()
        {
            var result = MessageBox.Show("Пройден ли инструктаж?", "Подтверждение действия", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

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
                        MessageBox.Show("Обновляем список инструктажей...");
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
                MessageBox.Show($"Не смогли открыть файл: {ex.Message}");
            }
        }
        #endregion

        #region Общие кнопки
        private void signUpButton_Click(object sender, EventArgs e)
        {
            _signUpForm.Show();
        }

        private void LogOutButton_Click(object sender, EventArgs e)
        {
            LogOutForm_Click_Internal();
        }

        public async void LogOutForm_Click_Internal()
        {
            if (_signUpForm != null)
            {
                _signUpForm.Dispose();
            }
            Decryption_stuff.DeleteJWTToken();
            this.Dispose(true);
            _loginForm.activeForm = _loginForm;
            _loginForm.Show();
        }


        #endregion



        private async void buttonCreateInstruction_Click(object sender, EventArgs e)
        {
            buttonCreateInstruction.Enabled = false;

            if (selectedFolderPath is null && string.IsNullOrWhiteSpace(UrlInDocsVisionTextBox.Text))
            {
                MessageBox.Show("Путь до инструктажа не выбран!");
                buttonCreateInstruction.Enabled = true;
                return;
            }

            DateTime startDateTime = DateTime.Now;
            DateTime endDateTime = datePickerEnd.Value.Date;
            if (endDateTime <= startDateTime)
            {
                MessageBox.Show("До какой даты должно быть больше текущего времени!");
                buttonCreateInstruction.Enabled = true;
                return;
            }
            List<string>? paths = GetPathsFromFolderOrURLDocsVision();
            if (paths is null || paths.Count == 0)
            {
                MessageBox.Show("Не выбраны файлы для инструктажа");
                buttonCreateInstruction.Enabled = true;
                return;
            }
            string causeOfInstruction = InstructionTextBox.Text;
            if (string.IsNullOrWhiteSpace(causeOfInstruction))
            {
                MessageBox.Show("Причина инструктажа пуста. Исправьте это пожалуйста.");
                buttonCreateInstruction.Enabled = true;
                return;
            }
            var checkedDepartmentsItemCollection = DepartmentsCheckedListBox.CheckedItems;
            if (checkedDepartmentsItemCollection.Count == 0)
            {
                MessageBox.Show("Отделы для внепланнового инструктажа не выбраны");
                buttonCreateInstruction.Enabled = true;
                return;
            }

            var departmentsList = new List<string>();
            foreach (var item in checkedDepartmentsItemCollection)
            {
                departmentsList.Add(item.ToString());
            }

            try
            {

                string causeOfInstructionString = causeOfInstruction.ToString();

                Instruction instruction = new Instruction(causeOfInstructionString, startDateTime, endDateTime, selectedFolderPath, 1);
                FullCustomInstruction fullInstruction = new FullCustomInstruction(instruction, paths);
                UnplannedInstructionPackage package = new UnplannedInstructionPackage(departmentsList, fullInstruction);

                string jsonData = JsonConvert.SerializeObject(package);
                string encryptedJsonData = Encryption_Kotova.EncryptString(jsonData);

                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        string jwtToken = _loginForm._jwtToken;
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                        // Set the URI of your server endpoint
                        var uri = new Uri(SubmitUnplannedInstructionURL);

                        // Prepare the content to send
                        var content = new StringContent(encryptedJsonData, Encoding.UTF8, "application/json");
                        // Send a POST request with the serialized JSON content
                        var response = await httpClient.PostAsync(uri, content);


                        if (response.IsSuccessStatusCode)
                        {
                            MessageBox.Show("Данные успешно отправлены на сервер и инструктаж назначен начальникам.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            var errorMessage = await response.Content.ReadAsStringAsync();
                            MessageBox.Show($"Не удалось отправить данные на сервер. Status code: {response.StatusCode},Error: {errorMessage} ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка произошла при отправке данных: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    buttonCreateInstruction.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошли ошибка, проверь эту строчку:{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                //Обнуляем все данные по инструткажу на странице внеплановых инструктажей для следующего инструктажа.
                InstructionTextBox.Text = "";
                treeView1.Nodes.Clear();
            }

        }

        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Action != TreeViewAction.Unknown) // Ensure the change was triggered by user interaction
            {
                treeView1.Enabled = false;

                try
                {
                    // Perform the checking/unchecking synchronously
                    CheckAllChildNodes(e.Node, e.Node.Checked);
                    UpdateParentNodes(e.Node, e.Node.Checked);
                }
                finally
                {
                    treeView1.Enabled = true;
                }
            }
        }
        public static List<string> GetSelectedFilePaths(System.Windows.Forms.TreeView treeView)
        {
            HashSet<string> uniqueFilePaths = new HashSet<string>();

            foreach (TreeNode node in treeView.Nodes)
            {
                CollectFilePaths(node, uniqueFilePaths);
            }

            // Sort the paths
            List<string> sortedFilePaths = uniqueFilePaths.ToList();
            sortedFilePaths.Sort();

            return sortedFilePaths;
        }

        private void ManagementForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            this.ShowInTaskbar = false;
        }

        private List<string>? GetPathsFromFolderOrURLDocsVision()
        {
            List<string>? paths = GetSelectedFilePaths(treeView1);
            if (paths != null && paths.Any())
            {
                return paths;
            }
            else if (string.IsNullOrWhiteSpace(UrlInDocsVisionTextBox.Text))
            {
                return null;
            }
            return new List<string> { UrlInDocsVisionTextBox.Text };
        }

        private async void ExportInstructionRequestButton_Click(object sender, EventArgs e)
        {
            DateTime startDate = startDateInstructionExportRequest.Value.Date;
            DateTime endDate = endDateInstructionExportRequest.Value.Date;
            List<int> selectedIndices = checkedListBoxTypesOfInstruction.CheckedIndices.Cast<int>().ToList();
            List<byte> shiftedIndices = selectedIndices.Select(index => (byte)(index + 1)).ToList();

            try
            {
                InstructionExportRequest instructionExportRequest = new InstructionExportRequest(startDate, endDate, shiftedIndices);



                using (var httpClient = new HttpClient())
                {
                    string jwtToken = _loginForm._jwtToken;
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                    string jsonData = JsonConvert.SerializeObject(instructionExportRequest);

                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    var uri = new Uri(instructionDataExportForManagementURL);
                    var response = await httpClient.PostAsync(uri, content);
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Данные успешно отправлены на сервер и инструктажи скачаны.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        var errorMessage = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Не удалось отправить данные на сервер. Status code: {response.StatusCode},Error: {errorMessage} ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<List<InstructionExportInstance>>(jsonResponse);

                    if (result == null || !result.Any())
                    {
                        MessageBox.Show("Нет инструктажей за этот период.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    result = result.OrderBy(i => i.DateWhenPassedByEmployee).ToList();
                    foreach (var instance in result)
                    {
                        instance.DateWhenPassedByEmployee = instance.DateWhenPassedByEmployee.Date;
                    }

                    ExportToExcelWithSaveDialog(result);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

        }

        void ExportToExcelWithSaveDialog(List<InstructionExportInstance> data)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx"; // Restrict to Excel files
                saveFileDialog.Title = "Save Excel File";               // Title of the dialog box
                saveFileDialog.FileName = "data.xlsx";                 // Default file name

                // Show the save dialog
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // This is the path where the file will be saved, chosen by the user
                    string filePath = saveFileDialog.FileName;

                    // Call a method to save the data to the chosen file path
                    ExportToExcel(data, filePath);
                }
            }
        }

        void ExportToExcel(List<InstructionExportInstance> data, string filePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Data");

                // Add column headers
                worksheet.Cell(1, 1).Value = "Дата проведения инструктажа по охране труда";
                worksheet.Cell(1, 2).Value = "Фамилия, имя, отчество (при наличии) работника, прошедшего инструктаж по охране труда";
                worksheet.Cell(1, 3).Value = "Профессия (должность) работника, прошедшего инструктаж по охране труда";
                worksheet.Cell(1, 4).Value = "Число, месяц, год рождения работника, прошедшего инструктаж по охране труда";
                worksheet.Cell(1, 5).Value = "Вид инструктажа по охране труда";
                worksheet.Cell(1, 6).Value = "Причина проведения инструктажа по охране труда (для внепланового или целевого инструктажа по охране труда)";
                worksheet.Cell(1, 7).Value = "Фамилия, имя отчество (при наличии), профессия (должность) работника, проводившего инструктаж по охране труда";
                worksheet.Cell(1, 8).Value = "Наименование локального акта (локальных актов), в объеме требований которого проведён инструктаж по охране труда";

                // Enable wrapping for headers (row 1)
                for (int col = 1; col <= 8; col++)
                {
                    worksheet.Cell(1, col).Style.Alignment.WrapText = true;
                }

                // Add sample data (for debugging)
                worksheet.Cell(2, 1).Value = 1;
                worksheet.Cell(2, 2).Value = 2;
                worksheet.Cell(2, 3).Value = 3;
                worksheet.Cell(2, 4).Value = 4;
                worksheet.Cell(2, 5).Value = 5;
                worksheet.Cell(2, 6).Value = 6;
                worksheet.Cell(2, 7).Value = 7;
                worksheet.Cell(2, 8).Value = 8;

                // Add data to cells
                for (int i = 0; i < data.Count; i++)
                {
                    worksheet.Cell(i + 3, 1).Value = data[i].DateWhenPassedByEmployee;
                    worksheet.Cell(i + 3, 2).Value = data[i].FullNameOfEmployee;
                    worksheet.Cell(i + 3, 3).Value = data[i].PositionOfEmployee;
                    worksheet.Cell(i + 3, 4).Value = data[i].BirthDateOfEmployee;
                    string? instructionTypeName = InstructionTypeToName(data[i].InstructionType);
                    worksheet.Cell(i + 3, 5).Value = instructionTypeName ?? "неизвестный тип инструктажа!";

                    if (data[i].InstructionType == 1 || data[i].InstructionType == 5)
                    {
                        worksheet.Cell(i + 3, 6).Value = data[i].CauseOfInstruction;
                    }
                    else
                    {
                        worksheet.Cell(i + 3, 6).Value = "";
                    }
                    worksheet.Cell(i + 3, 7).Value = data[i].FullNameOfEmployeeWhoConductedInstruction;
                    worksheet.Cell(i + 3, 8).Value = data[i].FileNamesOfInstructionInOneString;

                    // Enable text wrapping for each row of data
                    worksheet.Row(i + 3).Style.Alignment.WrapText = true;
                }

                // Adjust font size
                worksheet.Style.Font.FontSize = 12;

                // Auto-fit columns based on content
                worksheet.Columns().AdjustToContents();

                // Set specific column width for long text headers (if needed)
                worksheet.Column(2).Width = 50; // Adjust column 2 (Name) width to fit long text
                worksheet.Column(3).Width = 40; // Adjust column 3 (Job) width to fit long text

                // Set print settings to fit width on one page, but allow multiple pages for height
                worksheet.PageSetup.PagesWide = 1; // Fit all columns to one page width
                worksheet.PageSetup.PagesTall = 0; // Allow multiple pages for row height

                // Optional: Set the page orientation to landscape for wider content
                worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;

                // Save the workbook
                workbook.SaveAs(filePath);

                Console.WriteLine("Excel file created and saved.");
            }
        }
        private string? InstructionTypeToName(byte instructionType)
        {
            string? result = instructionType switch
            {
                0 => $"Вводный",
                1 => $"Внеплановый",
                2 => $"Первичный",
                3 => "Повторный",
                4 => $"Повторный (для водителей)",
                5 => $"Целевой",
                _ => null
            };
            return result;
        }
    }

}
