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
using Kotova.CommonClasses;
using System.Net.Http;

namespace Kotova.Test1.ClientSide
{
    public partial class ChiefForm : Form
    {
        const string urlCreateInstruction = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/add-new-instruction-into-db";
        const string urlTest = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/greeting";
        const string urlSyncInstructions = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/sync-instructions-with-db";
        const string urlSyncNames = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/sync-names-with-db";
        const string urlSubmitInstructionToPeople = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/send-instruction-and-names";
        const string DownloadInstructionForUserURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get_instructions_for_user";
        const string SendInstructionIsPassedURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/instruction_is_passed_by_user";
        const string PingToServerIsOnlineURL = ConfigurationClass.BASE_URL_DEVELOPMENT + "/ping-is-online";
        const string PingToServerIsOfflineURL = ConfigurationClass.BASE_URL_DEVELOPMENT + "/ping-is-offline";
        const string CheckStatusOfChiefOnServerURL = ConfigurationClass.BASE_URL_DEVELOPMENT + "/status";
        const string GetDepartmentIdByUserName = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get-department-id-by";
        const string getNotPassedInstructionURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get-not-passed-instructions-for-chief";

        public const string dB_instructionId = "instruction_id"; //ВЫНЕСИ ЭТИ 2 СТРОЧКИ В ОБЩИЙ ФАЙЛ!
        public const string db_filePath = "file_path";

        System.Windows.Forms.Timer myTimer = new System.Windows.Forms.Timer();

        private static readonly HttpClient _client = new HttpClient();

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

            PingServer();
            myTimer.Interval = 30000;  // 30 seconds
            myTimer.Tick += new EventHandler(TimerEventProcessor);
            myTimer.Start();


        }

        private async void TimerEventProcessor(Object myObject, EventArgs myEventArgs)
        {
            myTimer.Stop();
            if (!(await PingServer()))
            {
                LogOutForm_Click(myObject, myEventArgs);
            }

            myTimer.Start();
        }

        private async Task<bool> PingServer()
        {
            try
            {
                string jwtToken = _loginForm._jwtToken;
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                var response = await _client.GetAsync($"{GetDepartmentIdByUserName}/{Uri.EscapeDataString(_userName)}");
                if (response.IsSuccessStatusCode)
                {
                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                    int departmentId = Int32.Parse(await response.Content.ReadAsStringAsync());
                    response = await _client.GetAsync($"{PingToServerIsOnlineURL}/{departmentId}"); // посмотри чтобы возвращалось время апдейта в БД. чтобы не дай бог в будущем в разных часовых поясах не путать.
                    if (response.IsSuccessStatusCode)
                    {
                        consoleTextBox.AppendText("пинг на сервер отправлен успешно");
                        //consoleTextBox.AppendText("pinged successfully"); // вместо message box сделать чтобы в консоль писалась или textbox или подобном. 
                        consoleTextBox.AppendText(Environment.NewLine);
                        return true;
                    }
                    consoleTextBox.AppendText("Получен DepartmentId, но пинг на сервер не успешен. Status code:" + $"{response.StatusCode}");
                    //consoleTextBox.AppendText("got DepartmentId, but ping failed. Status code:" + $"{response.StatusCode}");
                    consoleTextBox.AppendText(Environment.NewLine);
                    return true;
                }
                else
                {
                    MessageBox.Show("Пинг на сервер провален. Сервер не работает? Status code:" + $"{response.StatusCode}");
                    //MessageBox.Show("Ping failed. Server is not working? Status code:" + $"{response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка во время пинга: {ex.Message}");

                //MessageBox.Show($"Error occurred while pinging: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> PingToServerThatChiefIsOffline()
        {

            try
            {
                string jwtToken = _loginForm._jwtToken;
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                var response = await _client.GetAsync($"{GetDepartmentIdByUserName}/{Uri.EscapeDataString(_userName)}");
                if (response.IsSuccessStatusCode)
                {
                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                    int departmentId = Int32.Parse(await response.Content.ReadAsStringAsync());
                    response = await _client.GetAsync($"{PingToServerIsOfflineURL}/{departmentId}");
                    if (response.IsSuccessStatusCode)
                    {
                        //MessageBox.Show("Response about closing send successfully");
                        //consoleTextBox.AppendText(Environment.NewLine);
                        return true;
                    }
                    MessageBox.Show("Получен DepartmentId, Не отправлено уведомление на сервер о закрытии формы. Status code:" + $"{response.StatusCode}");
                    //MessageBox.Show("got DepartmentId, but didn't get response about closing form. Status code:" + $"{response.StatusCode}");
                    return true;
                }
                else
                {
                    MessageBox.Show("Не удалось отправить запрос на закрытие формы. Сервер не работает? Status code:" + $"{response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при отправке запроса на сервер о статусе \"оффлайн\": {ex.Message}");
                return false;
            }
        }



        private async void testButton_Click(object sender, EventArgs e)
        {
            string url = urlTest;
            await Test.connectionToUrlGet(url, _loginForm._jwtToken);
        }

        private async void buttonCreateInstruction_Click(object sender, EventArgs e)
        {
            buttonCreateInstruction.Enabled = false;
            var listOfNames = checkedListBoxNamesOfPeopleCreatingInstr.SelectedItems;

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
                            MessageBox.Show("Data successfully sent to the server and Instructions added to User.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            var errorMessage = await response.Content.ReadAsStringAsync();
                            MessageBox.Show($"Failed to send data to server. Status code: {response.StatusCode},Error: {errorMessage} ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while sending data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    submitInstructionToPeople.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Some error occurred during Submit:{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                submitInstructionToPeople.Enabled = true;
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


                bool isForDrivers = checkBoxIsForDrivers.Checked;
                int bitValueIsForDrivers = isForDrivers ? 1 : 0;
                string causeOfInstruction = InstructionTextBox.Text;
                Byte typeOfInstruction = (Byte)(typeOfInstructionListBox.SelectedIndex + 2); //ЗДЕСЬ ПОДРАЗУМЕВАЕТСЯ, ЧТО ТИПОВ ИНСТРУКТАЖЕЙ НЕ БОЛЬШЕ 6 в listBox!
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
                        //MessageBox.Show("Names successfully synced with database.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        MessageBox.Show("Имена успешно синхронизированы с базой данных.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
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




        private async void SyncNamesWithDB_Click(object sender, EventArgs e) //ПРОДОЛЖАЙ ЭТОТ КОД! С 28.05.2024
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
                            throw new Exception("responseBody is empty");
                        }
                        string[] resultArray = result.Select(t => $"{t.Item1} ({t.Item2})").ToArray<string>();

                        //ListBoxNamesOfPeople.Items.AddRange(resultArray);
                        checkedListBoxNamesOfPeople.Items.AddRange(resultArray);
                        // Successfully called the ImportIntoDB endpoint, handle accordingly
                        //MessageBox.Show("Names successfully synced with database.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        MessageBox.Show("Имена успешно синхронизированы с базой данных.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        string errorMessage = await response.Content.ReadAsStringAsync();
                        //MessageBox.Show($"Failed to sync names with DB. Status code: {response.StatusCode}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        MessageBox.Show($"Failed to sync names with DB. Status code: {response.StatusCode} {errorMessage}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            var listOfNames = checkedListBoxNamesOfPeople.SelectedItems;
            //var listOfNames = ListBoxNamesOfPeople.SelectedItems;

            List<Tuple<string, string>> listOfNamesAndBirthDateString = new List<Tuple<string, string>>();
            if (listOfNames.Count == 0)
            {
                MessageBox.Show("People not selected!");
                submitInstructionToPeople.Enabled = true;
                return;
            }
            var selectedInstruction = ListOfInstructions.SelectedItem;
            if (selectedInstruction is null)
            {
                MessageBox.Show("Instruction not selected!");
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
                            MessageBox.Show("Data successfully sent to the server and Instructions added to User.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            var errorMessage = await response.Content.ReadAsStringAsync();
                            MessageBox.Show($"Failed to send data to server. Status code: {response.StatusCode},Error: {errorMessage} ", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while sending data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    submitInstructionToPeople.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Some error occurred during Submit:{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            Decryption_stuff.DeleteJWTToken();
            _loginForm.Show();
            this.Dispose();
            myTimer.Stop();
            myTimer.Tick -= new EventHandler(TimerEventProcessor);
            await PingToServerThatChiefIsOffline();

        }

        private async void ChiefForm_FormClosed(object sender, FormClosedEventArgs e) //РАЗБЕРИСЬ ПОЧЕМУ ПРИ ЗАКРЫТИИ ФОРМЫ, ВСЕ РАВНО НЕ ЗАКРЫВАЕТСЯ VISUAL STUDIO (УТЕЧКА)
        {

            _loginForm.Dispose();
            myTimer.Stop();
            myTimer.Tick -= new EventHandler(TimerEventProcessor);
            await PingToServerThatChiefIsOffline();
            this.Dispose();
        }





        private void CreateInstructionWithPeopleButton_Click(object sender, EventArgs e)
        {

        }








        //ВОТ ЭТО ТРЕТЬЯ ВКЛАДКА! ПОДЕЛИ КАК СЧИТАЕШЬ НУЖНЫМ!



        private async void ChiefTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ChiefTabControl.SelectedIndex == 2)
            {
                ListOfInstructions.Items.Clear();
                bool IsEmpty = await DownloadInstructionsForUserFromServer(_userName);
                if (IsEmpty == true)
                {
                    MessageBox.Show("All the instructions passed!");
                }


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
                                throw new Exception("responseBody is empty");
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
                            MessageBox.Show($"Failed to sync names with DB. Status code: {response.StatusCode} {errorMessage}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                PassInstruction.Enabled = false;
                return;
            }
            Dictionary<string, object> selectedDict = GetDictFromSelectedInstruction(ListOfInstructionsForUser.SelectedItem.ToString()); //most likely suppress it, cause its not null.
            string? pathStr = selectedDict[DataBaseNames.tableName_sql_pathToInstruction].ToString();

            if (pathStr is null || pathStr.Length == 0)
            {
                MessageBox.Show("Path is null or empty.");
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
                MessageBox.Show("Provided path is null or empty.");
                PassInstruction.Enabled = false;
                return;
            }

            // Get the full path and check if it exists
            string fullPath = Path.GetFullPath(path);
            if (!Directory.Exists(fullPath))
            {
                MessageBox.Show($"The path '{fullPath}' does not exist.");
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
                MessageBox.Show($"Failed to open the folder: {ex.Message}");
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
                MessageBox.Show("You agreed with the action.", "Action Confirmed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                PassInstruction.Enabled = false;
                if (ListOfInstructionsForUser.SelectedItem == null)
                {
                    MessageBox.Show("You haven't select the Instruction.");
                    PassInstruction.Enabled = false;
                    return;
                }
                Dictionary<string, object> selectedDict = GetDictFromSelectedInstruction(ListOfInstructionsForUser.SelectedItem.ToString());
                await SendInstructionIsPassedToDB(selectedDict);
                FilesOfInstructionCheckedListBox.Items.Clear();
            }
            else
            {
                MessageBox.Show("You did not agree with the action.", "Action Canceled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                PassInstruction.Checked = false;
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

        public static List<string> GetSelectedFilePaths(TreeView treeView)
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
                MessageBox.Show($"Failed to open file: {ex.Message}");
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
                        MessageBox.Show("Синхронизация с базой данных непройденных инструктажей успешно", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        var result = System.Text.Json.JsonSerializer.Deserialize<List<InstructionForChief>>(jsonResponse);
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
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void listBoxOfNotPassedByInstructions_SelectedIndexChanged(object sender, EventArgs e)
        {
            dataGridViewPeopleThatNotPassedInstr.Rows.Clear();
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
    }
}
