using Kotova.CommonClasses;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Text.Json;
using System.Diagnostics;
using System.Net.Http;
using Microsoft.AspNetCore.SignalR.Client;
using System.IO;
using System.Windows.Controls;
using ClosedXML.Excel;
using System.Collections.Generic;

namespace Kotova.Test1.ClientSide
{
    public partial class ChiefForm : Form
    {
        const string urlCreateInstruction = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/add-new-instruction-into-db";
        const string urlTest = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/greeting";
        const string urlSyncInstructions = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/sync-instructions-with-db";
        const string urlSyncNames = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/sync-names-with-db";
        const string urlSubmitInstructionToPeople = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/send-instruction-to-names";
        const string DownloadInstructionForUserURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get_instructions_for_user";
        const string SendInstructionIsPassedURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/instruction_is_passed_by_user";
        const string GetDepartmentIdByUserName = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get-department-id-by";
        const string getNotPassedInstructionURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get-not-passed-instructions-for-chief";
        const string instructionDataExportURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/instructions-data-export";
        const string RefreshTaskForChiefUrl = ConfigurationClass.BASE_TASK_URL_DEVELOPMENT + "/get-all-current-tasks-for-chief";

        const string urlTaskTest = ConfigurationClass.BASE_TASK_URL_DEVELOPMENT + "/create-random-task";

        public const string dB_instructionId = "instruction_id"; //ВЫНЕСИ ЭТИ 3 СТРОЧКИ В ОБЩИЙ ФАЙЛ!
        public const string db_filePath = "file_path";
        public const string db_typeOfInstruction = "type_of_instruction";

        private static readonly HttpClient _client = new HttpClient();
        private HubConnection? _hubConnection = null;

        private List<Dictionary<string, object>> listOfInstructions_global;
        private List<Dictionary<string, object>> listsOfPaths_global;
        private List<InstructionForChief> instructionForChiefs_global;

        static string? selectedFolderPath = null;
        private Login_Russian? _loginForm;
        string? _userName;
        public SignUpForm _signUpForm;

        public ChiefForm(Login_Russian loginForm, string userName)
        {
            _loginForm = loginForm;
            _userName = userName;
            SignUpForm signUpForm = new SignUpForm(loginForm, this);
            _signUpForm = signUpForm;
            InitializeComponent();
            usernameLabel.Text = userName;
            ChiefTabControl_SelectedIndexChanged(null, EventArgs.Empty);

            InitializeSignalRConnection();

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



        private async void testButton_Click(object sender, EventArgs e)
        {
            string url = urlTest;
            await Test.connectionToUrlGet(url, _loginForm._jwtToken);
        }

        private async void testButtonForTask_Click(object sender, EventArgs e)
        {
            string url = urlTaskTest;
            await Test.connectionToUrlGet(url, _loginForm._jwtToken);
        }

        private async void buttonCreateInstruction_Click(object sender, EventArgs e)
        {
            buttonCreateInstruction.Enabled = false;
            var listOfNames = checkedListBoxNamesOfPeopleCreatingInstr.CheckedItems;

            List<Tuple<string, string>> listOfNamesAndBirthDateString = new List<Tuple<string, string>>();
            if (listOfNames.Count == 0)
            {
                MessageBox.Show("Люди не выбраны!");
                buttonCreateInstruction.Enabled = true;
                return;
            }

            FullCustomInstruction? fullCustomInstruction = await CreateInstructionInternal();



            if (fullCustomInstruction is null)
            {
                return;
            }

            string? causeOfCreatedInstruction = fullCustomInstruction._instruction.cause_of_instruction;
            // Дальше по сути идёт функция назначения выбранному инструктажу людей. 
            buttonCreateInstruction.Enabled = false;

            if (causeOfCreatedInstruction is null)
            {
                MessageBox.Show("Причина инструктажа - null, инструктаж не назначен людям");
                return;
            }

            try
            {
                foreach (var item in listOfNames)
                {
                    listOfNamesAndBirthDateString.Add(DeconstructNameAndBirthDate(item.ToString()));
                }

                string instructionNameString = causeOfCreatedInstruction.ToString();
                InstructionPackage package = new InstructionPackage(listOfNamesAndBirthDateString, instructionNameString);
                string jsonData = JsonConvert.SerializeObject(package);
                string encryptedJsonData = Encryption_Kotova.EncryptString(jsonData);

                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        string jwtToken = _loginForm._jwtToken;
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                        // Set the URI of your server endpoint
                        var uri = new Uri(urlSubmitInstructionToPeople);

                        // Prepare the content to send
                        var content = new StringContent(encryptedJsonData, Encoding.UTF8, "application/json");
                        // Send a POST request with the serialized JSON content
                        var response = await httpClient.PostAsync(uri, content);


                        if (response.IsSuccessStatusCode)
                        {
                            MessageBox.Show("Данные успешно отправлены на сервер и инструктаж назначен пользователям.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                    submitInstructionToPeople.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошли ошибка, проверь эту строчку:{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                submitInstructionToPeople.Enabled = true;
                treeView1.Nodes.Clear();
            }
        }
        private async Task<FullCustomInstruction?> CreateInstructionInternal()
        {
            try
            {
                buttonCreateInstruction.Enabled = false;
                if (selectedFolderPath is null)
                {
                    MessageBox.Show("Путь до инструктажа не выбран!");
                    buttonCreateInstruction.Enabled = true;
                    return null;
                }
                DateTime startTime = DateTime.Now;
                DateTime endDate = datePickerEnd.Value.Date;
                if (endDate <= startTime)
                {
                    MessageBox.Show("До какой даты должно быть больше текущего времени!");
                    buttonCreateInstruction.Enabled = true;
                    return null;
                }
                if (typeOfInstructionListBox.SelectedIndex == -1)
                {
                    buttonCreateInstruction.Enabled = true;
                    MessageBox.Show("Не выбран тип инструктажа!");
                    return null;
                }
                List<string> paths = GetSelectedFilePaths(treeView1);
                if (paths.Count == 0)
                {
                    MessageBox.Show("Не выбраны файлы для инструктажа");
                    buttonCreateInstruction.Enabled = true;
                    return null;
                }

                bool isForDrivers = false;
                int bitValueIsForDrivers = isForDrivers ? 1 : 0;
                string causeOfInstruction = InstructionTextBox.Text;
                if (string.IsNullOrWhiteSpace(causeOfInstruction))
                {
                    MessageBox.Show("Причина инструктажа пуста. Исправьте это пожалуйста.");
                    buttonCreateInstruction.Enabled = true;
                    return null;
                }
                Byte typeOfInstruction = (Byte)(typeOfInstructionListBox.SelectedIndex + 2); //ЗДЕСЬ ПОДРАЗУМЕВАЕТСЯ, ЧТО ТИПОВ ИНСТРУКТАЖЕЙ НЕ БОЛЬШЕ 6 в listBox! 0 - вводный, 1 - внеплановый
                Instruction instruction = new Instruction(causeOfInstruction, startTime, endDate, selectedFolderPath, typeOfInstruction);
                FullCustomInstruction fullCustomInstr = new FullCustomInstruction(instruction, paths);
                string json = JsonConvert.SerializeObject(fullCustomInstr);
                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                await Test.connectionToUrlPost(urlCreateInstruction, content, $"Инструктаж '{causeOfInstruction}' успешно добавлен в базу данных.", _loginForm._jwtToken);
                buttonCreateInstruction.Enabled = true;
                InstructionTextBox.Text = "";
                typeOfInstructionListBox.SelectedIndex = -1;
                selectedFolderPath = null;
                PathToFolderOfInstruction.Text = "Путь не выбран";
                return fullCustomInstr;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }

        }

        private async void buttonSyncManualyInstrWithDB_Click(object sender, EventArgs e)
        {
            await SyncManuallyInstrWithDBInternal();
        }
        private async Task SyncManuallyInstrWithDBInternal()
        {
            try
            {
                buttonSyncManualyInstrWithDB.Enabled = false;
                ListOfInstructions.Items.Clear();

                using (var httpClient = new HttpClient())
                {
                    string jwtToken = _loginForm._jwtToken;
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                    var response = await httpClient.GetAsync(urlSyncInstructions);
                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        if (string.IsNullOrWhiteSpace(responseBody))
                        {
                            throw new Exception("responseBody пуст"); //throw here better something
                        }
                        List<Instruction> result = JsonConvert.DeserializeObject<List<Instruction>>(responseBody); //checked that is not null before! so warning maybe suppressed

                        string[] resultArray = result.Select(n => n.cause_of_instruction).ToArray(); //check that they are not null;
                        ListOfInstructions.Items.AddRange(resultArray);
                        //MessageBox.Show("Имена успешно синхронизированы с базой данных.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        string errorMessage = await response.Content.ReadAsStringAsync();
                        // The call was not successful, handle errors or retry logic
                        //MessageBox.Show($"Failed to sync names with DB. Status code: {response.StatusCode}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        MessageBox.Show($"Не удалось синхронизировать имена с базой данных. Status code: {response.StatusCode} {errorMessage}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                // Exception handling for networking errors, etc.
                //MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                buttonSyncManualyInstrWithDB.Enabled = true;
            }
        }




        private async void SyncNamesWithDB_Click(object sender, EventArgs e)
        {
            await SyncNamesWithDBInternal();
        }
        private async Task SyncNamesWithDBInternal()
        {
            try
            {
                SyncNamesWithDB.Enabled = false; // Assuming this is a button, disable it to prevent multiple clicks
                checkedListBoxNamesOfPeople.Items.Clear();

                using (var httpClient = new HttpClient())
                {
                    string jwtToken = _loginForm._jwtToken;
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                    var response = await httpClient.GetAsync(urlSyncNames);

                    if (response.IsSuccessStatusCode)
                    {

                        string responseBody = await response.Content.ReadAsStringAsync();
                        List<Tuple<string, string>>? result = JsonConvert.DeserializeObject<List<Tuple<string, string>>>(responseBody);
                        if (result is null)
                        {
                            throw new Exception("тело ответа пусто");
                        }
                        string[] resultArray = result.Select(t => $"{t.Item1} ({t.Item2})").ToArray<string>();

                        //ListBoxNamesOfPeople.Items.AddRange(resultArray);
                        checkedListBoxNamesOfPeople.Items.AddRange(resultArray);
                        // Successfully called the ImportIntoDB endpoint, handle accordingly
                        //MessageBox.Show("Имена успешно синхронизированы с базой данных.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        string errorMessage = await response.Content.ReadAsStringAsync();
                        //MessageBox.Show($"Failed to sync names with DB. Status code: {response.StatusCode}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        MessageBox.Show($"Не удалось синхронизировать имена с базой данных. Status code: {response.StatusCode} {errorMessage}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                // Exception handling for networking errors, etc.
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                SyncNamesWithDB.Enabled = true; // Re-enable the button after the operation completes
            }
        }


        private async void submitInstructionToPeople_Click(object sender, EventArgs e)
        {
            submitInstructionToPeople.Enabled = false;
            var listOfNames = checkedListBoxNamesOfPeople.CheckedItems;
            //var listOfNames = ListBoxNamesOfPeople.SelectedItems;

            List<Tuple<string, string>> listOfNamesAndBirthDateString = new List<Tuple<string, string>>();
            if (listOfNames.Count == 0)
            {
                MessageBox.Show("Люди не выбраны!");
                submitInstructionToPeople.Enabled = true;
                return;
            }
            var selectedInstruction = ListOfInstructions.SelectedItem;
            if (selectedInstruction is null)
            {
                MessageBox.Show("Инструкция не выбрана!");
                submitInstructionToPeople.Enabled = true;
                return;
            }
            try
            {
                foreach (var item in listOfNames)
                {
                    listOfNamesAndBirthDateString.Add(DeconstructNameAndBirthDate(item.ToString()));
                }
                string instructionNameString = selectedInstruction.ToString();
                InstructionPackage package = new InstructionPackage(listOfNamesAndBirthDateString, instructionNameString);
                string jsonData = JsonConvert.SerializeObject(package);
                string encryptedJsonData = Encryption_Kotova.EncryptString(jsonData);

                try
                {
                    using (var httpClient = new HttpClient())
                    {
                        string jwtToken = _loginForm._jwtToken;
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                        // Set the URI of your server endpoint
                        var uri = new Uri(urlSubmitInstructionToPeople);

                        // Prepare the content to send
                        var content = new StringContent(encryptedJsonData, Encoding.UTF8, "application/json");

                        // Send a POST request with the serialized JSON content
                        var response = await httpClient.PostAsync(uri, content);

                        if (response.IsSuccessStatusCode)
                        {
                            MessageBox.Show("Данные успешно отправлены на сервер и инструктаж назначен людям.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            var errorMessage = await response.Content.ReadAsStringAsync();
                            MessageBox.Show($"Не получилось отправить данные не сервер. Status code: {response.StatusCode},Error: {errorMessage} ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка произошла при отправке данных: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    submitInstructionToPeople.Enabled = true;
                    await SyncManuallyInstrWithDBInternal();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Какая-то ошибка произошла при назначении инструктажа:{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                submitInstructionToPeople.Enabled = true;
            }
        }

        private Tuple<string, string> DeconstructNameAndBirthDate(string? nameWithBirthDate)
        {
            string pattern = @"^(.+?)\s\((\d{4}-\d{2}-\d{2})\)$"; //Эта строка соответсвует birthDate_format в Server side
            if (nameWithBirthDate == null) { throw new ArgumentException("nameWithBirthDate is null! in DeconstructNameAndBirthDate"); }
            Regex regex = new Regex(pattern);
            Match match = regex.Match(nameWithBirthDate);

            if (match.Success)
            {
                string fullName = match.Groups[1].Value;  // ФИО
                string birthDate = match.Groups[2].Value; // BirthDate (Дата рождения)
                return Tuple.Create(fullName, birthDate);
            }
            else
            {
                throw new ArgumentException("nameWithBirthDate doesn't match the pattern! in DeconstructNameAndBirthDate");
            }
        }

        private async void LogOutForm_Click(object sender, EventArgs e)
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


        private void ChiefForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            this.ShowInTaskbar = false;

        }



        //ВОТ ЭТО ТРЕТЬЯ ВКЛАДКА! ПОДЕЛИ КАК СЧИТАЕШЬ НУЖНЫМ!



        private async void ChiefTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ChiefTabControl.SelectedTab.Text == "Прохождение инструктажей")
            {
                ListOfInstructions.Items.Clear();
                ListOfInstructionsForUser.Items.Clear();
                bool IsEmpty = await DownloadInstructionsForUserFromServer(_userName);
                if (IsEmpty == true)
                {
                    MessageBox.Show("Все инструктажи пройдены!");
                }
            }
            if (ChiefTabControl.SelectedTab.Text == "Внеплановые инструктажи")
            {
                await SyncManuallyInstrWithDBInternal();
                await SyncNamesWithDBInternal();
            }

            if (ChiefTabControl.SelectedTab.Text == "Создание инструктажа")
            {
                try
                {
                    SyncNamesWithDB.Enabled = false; // Assuming this is a button, disable it to prevent multiple clicks
                    checkedListBoxNamesOfPeopleCreatingInstr.Items.Clear();

                    using (var httpClient = new HttpClient())
                    {
                        string jwtToken = _loginForm._jwtToken;
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                        var response = await httpClient.GetAsync(urlSyncNames);

                        if (response.IsSuccessStatusCode)
                        {

                            string responseBody = await response.Content.ReadAsStringAsync();
                            List<Tuple<string, string>>? result = JsonConvert.DeserializeObject<List<Tuple<string, string>>>(responseBody);
                            if (result is null)
                            {
                                throw new Exception("тело ответа пусто");
                            }
                            string[] resultArray = result.Select(t => $"{t.Item1} ({t.Item2})").ToArray<string>();

                            //ListBoxNamesOfPeople.Items.AddRange(resultArray);
                            checkedListBoxNamesOfPeopleCreatingInstr.Items.AddRange(resultArray);
                            // Successfully called the ImportIntoDB endpoint, handle accordingly
                            //MessageBox.Show("Names successfully synced with database.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            MessageBox.Show("Имена успешно синхронизированы с базой данных.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            string errorMessage = await response.Content.ReadAsStringAsync();
                            //MessageBox.Show($"Failed to sync names with DB. Status code: {response.StatusCode}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            MessageBox.Show($"Не получилось синхронизировать имена с базой данных. Status code: {response.StatusCode} {errorMessage}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Exception handling for networking errors, etc.
                    MessageBox.Show($"Произошла ошибка: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    SyncNamesWithDB.Enabled = true; // Re-enable the button after the operation completes
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
                MessageBox.Show($"Ошибка: {ex.Message}");
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
                PassInstruction.Enabled = false;
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
                            PassInstruction.Enabled = true;
                            HyperLinkForInstructionsFolder.Enabled = false;
                            return;
                        }
                        else
                        {
                            MessageBox.Show("Ooops, Что-то пошло не так. Проверь эту строчку на предмет присутствия файлов инструктажа!");
                            PassInstruction.Enabled = false;
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
                PassInstruction.Enabled = false;
                return;
            }
            Dictionary<string, object> selectedDict = GetDictFromSelectedInstruction(ListOfInstructionsForUser.SelectedItem.ToString()); //most likely suppress it, cause its not null.
            string? pathStr = selectedDict[DataBaseNames.tableName_sql_pathToInstruction].ToString();

            if (pathStr is null || pathStr.Length == 0)
            {
                MessageBox.Show("Путь пуст или отсутствует.");
                PassInstruction.Enabled = false;
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
                PassInstruction.Enabled = false;
                return;
            }

            // Get the full path and check if it exists
            string fullPath = Path.GetFullPath(path);
            if (!Directory.Exists(fullPath))
            {
                MessageBox.Show($"Путь '{fullPath}' не существует.");
                PassInstruction.Enabled = false;
                return;
            }

            // Open the folder in Windows Explorer
            try
            {
                Process.Start("explorer.exe", fullPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не получилось открыть папку: {ex.Message}");
                PassInstruction.Enabled = false;
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
            if (!PassInstruction.Checked) { return; }
            if (ConfirmAction())
            {
                MessageBox.Show("Вы согласились с прохождением инструктажа.", "Действите подтверждено", MessageBoxButtons.OK, MessageBoxIcon.Information);
                PassInstruction.Enabled = false;
                if (ListOfInstructionsForUser.SelectedItem == null)
                {
                    MessageBox.Show("Вы не выбрали инструктаж.");
                    PassInstruction.Enabled = false;
                    return;
                }
                Dictionary<string, object> selectedDict = GetDictFromSelectedInstruction(ListOfInstructionsForUser.SelectedItem.ToString());
                await SendInstructionIsPassedToDB(selectedDict);
                FilesOfInstructionCheckedListBox.Items.Clear();
            }
            else
            {
                MessageBox.Show("Вы не согласились с прохождением инструктажа.", "Действие отменено", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                PassInstruction.Checked = false;
                return;
            }
        }

        private bool ConfirmAction()
        {
            var result = MessageBox.Show("Вы прошли инструктаж?", "Подтвердить действие", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

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
                        MessageBox.Show("Все хорошо, обновляем лист инструктажей.");
                        ListOfInstructionsForUser.Items.Clear();
                        DownloadInstructionsForUserFromServer(_userName);
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
                PassInstruction.Checked = false;
            }
        }


        private void buttonChooseHyperLinkToInstruction_Click(object sender, EventArgs e)
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
                    PassInstruction.Enabled = true;
                }
                else
                {
                    PassInstruction.Enabled = false;
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
                MessageBox.Show($"Не получилось открыть файл: {ex.Message}");
            }
        }

        private async void TestButtonForInstructions_Click(object sender, EventArgs e)
        {
            try
            {

                dataGridViewPeopleThatNotPassedInstr.Rows.Clear();
                listBoxOfNotPassedByInstructions.Items.Clear();
                using (var httpClient = new HttpClient())
                {
                    string jwtToken = _loginForm._jwtToken;
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                    var response = await httpClient.GetAsync(getNotPassedInstructionURL);


                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        var result = System.Text.Json.JsonSerializer.Deserialize<List<InstructionForChief>>(jsonResponse);

                        if (result is null)
                        {
                            MessageBox.Show("Произошла ошибка при получении данных с сервера!");
                            return;
                        }

                        if (result.Count == 0)
                        {
                            MessageBox.Show("Похоже все инструктажи всеми пройдены!");
                            return;
                        }
                        instructionForChiefs_global = result;
                        List<string> namesOfInstructions = result
                            .Select(i => $"[{i.InstructionId}]: {i.CauseOfInstruction}")
                            .ToList();
                        listBoxOfNotPassedByInstructions.Items.AddRange(namesOfInstructions.ToArray());


                    }
                    else
                    {
                        string errorMessage = await response.Content.ReadAsStringAsync();
                        //MessageBox.Show($"Failed to sync names with DB. Status code: {response.StatusCode}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        MessageBox.Show($"Провал. Status code: {response.StatusCode} {errorMessage}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                // Exception handling for networking errors, etc.
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void listBoxOfNotPassedByInstructions_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataGridViewPeopleThatNotPassedInstr.Rows.Clear();
            if (listBoxOfNotPassedByInstructions.SelectedItem is null)
            {
                return;
            }

            var instructionString = listBoxOfNotPassedByInstructions.SelectedItem.ToString();
            string pattern = @"^\[(\d+)\]:\s(.+)$";
            if (Regex.IsMatch(instructionString, pattern))
            {
                Match match = Regex.Match(instructionString, pattern);
                if (match.Success)
                {
                    int instructionId = int.Parse(match.Groups[1].Value);
                    string causeOfInstruction = match.Groups[2].Value;

                    // Find the corresponding InstructionForChief object
                    var matchingInstruction = instructionForChiefs_global.FirstOrDefault(i =>
                        i.InstructionId == instructionId &&
                        i.CauseOfInstruction == causeOfInstruction);




                    if (matchingInstruction != null)
                    {
                        foreach (var person in matchingInstruction.Persons)
                        {
                            int rowIndex = dataGridViewPeopleThatNotPassedInstr.Rows.Add(person.PersonName, person.Passed ? "Да" : "Нет");
                            if (!person.Passed)
                            {
                                dataGridViewPeopleThatNotPassedInstr.Rows[rowIndex].DefaultCellStyle.BackColor = Color.Red;
                            }
                            else
                            {
                                dataGridViewPeopleThatNotPassedInstr.Rows[rowIndex].DefaultCellStyle.BackColor = Color.Green;
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("No matching instruction found.");
                    }
                }
            }
            else
            {
                MessageBox.Show("Invalid format.");
            }
        }

        private async void RefreshTasksButton_Click(object sender, EventArgs e)
        {
            string url = RefreshTaskForChiefUrl;
            TrayOfTasksList.Items.Clear();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string jwtToken = _loginForm._jwtToken;
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var jsonResponse = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(jsonResponse))
                    {
                        MessageBox.Show("Новых заданий нет.");
                        return;
                    }

                    var tasks = JsonConvert.DeserializeObject<List<TaskDto>>(jsonResponse);

                    // Check for null or empty list
                    if (tasks != null && tasks.Count > 0)
                    {

                        // Add tasks to the ListView
                        foreach (var task in tasks)
                        {
                            TrayOfTasksList.Items.Add(task.Description);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Новых заданий нет!");
                    }

                }
            }
            catch (HttpRequestException ex)
            {

                // Handle any exceptions here
                MessageBox.Show($"Error: {ex.Message}");
            }
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

                    var uri = new Uri(instructionDataExportURL);
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

        private void SelectAllThePeopleInListBoxButton_Click(object sender, EventArgs e)
        {
            // Check if all people are selected
            bool allSelected = true;
            for (int i = 0; i < checkedListBoxNamesOfPeople.Items.Count; i++)
            {
                if (!checkedListBoxNamesOfPeople.GetItemChecked(i))
                {
                    allSelected = false;
                    break;
                }
            }

            // Toggle the selection
            for (int i = 0; i < checkedListBoxNamesOfPeople.Items.Count; i++)
            {
                checkedListBoxNamesOfPeople.SetItemChecked(i, !allSelected);
            }

            // Change button text based on selection state
            SelectAllThePeopleInListBoxButton.Text = allSelected ? "Выбрать всех людей" : "Не выбрать никого"; 
        }
    }
}
