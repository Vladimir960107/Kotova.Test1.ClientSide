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
        const string PingToServerURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/ping_to_server";

        System.Windows.Forms.Timer myTimer = new System.Windows.Forms.Timer();



        private List<Dictionary<string, object>> listOfInstructions_global;

        static string? selectedFolderPath = null;
        private Form? _loginForm;
        string? _userName;
        public ChiefForm()
        {
            InitializeComponent();

            myTimer.Interval = 30000;  // 30 seconds
            myTimer.Tick += new EventHandler(TimerEventProcessor);
            myTimer.Start();
        }
        public ChiefForm(Form loginForm, string userName)
        {
            _loginForm = loginForm;
            _userName = userName;
            InitializeComponent();

            myTimer.Interval = 30000;  // 30 seconds
            myTimer.Tick += new EventHandler(TimerEventProcessor);
            myTimer.Start();
        }

        private void TimerEventProcessor(Object myObject, EventArgs myEventArgs)
        {
            myTimer.Stop();
            PingServer();  
            myTimer.Start();
        }

        private void PingServer()
        {
            using (var client = new System.Net.Http.HttpClient())
            {
                // Replace 'your_server_endpoint' with your actual server URL
                var response = client.GetAsync(PingToServerURL).Result;
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Ping successful.");
                }
                else
                {
                    Console.WriteLine("Ping failed.");
                }
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
                    // Use the selectedFolderPath variable as needed in your code
                    MessageBox.Show($"Selected Folder: {selectedFolderPath}");


                }
            }
        }

        private async void testButton_Click(object sender, EventArgs e)
        {
            string url = urlTest;
            await Test.connectionToUrlGet(url);
        }

        private async void buttonCreateInstruction_Click(object sender, EventArgs e)
        {
            buttonCreateInstruction.Enabled = false;
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

            bool isForDrivers = checkBoxIsForDrivers.Checked;
            int bitValueIsForDrivers = isForDrivers ? 1 : 0;
            string causeOfInstruction = InstructionTextBox.Text;
            Byte typeOfInstruction = (Byte)(typeOfInstructionListBox.SelectedIndex + 2); //ЗДЕСЬ ПОДРАЗУМЕВАЕТСЯ, ЧТО ТИПОВ ИНСТРУКТАЖЕЙ НЕ БОЛЬШЕ 6 в listBox!
            Instruction instruction = new Instruction(causeOfInstruction, startTime, endDate, selectedFolderPath, typeOfInstruction);
            string json = JsonConvert.SerializeObject(instruction);
            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
            await Test.connectionToUrlPost(urlCreateInstruction, content, $"Инструктаж '{causeOfInstruction}' успешно добавлен в базу данных.");
            buttonCreateInstruction.Enabled = true;
            InstructionTextBox.Text = "";
            typeOfInstructionListBox.SelectedIndex = -1;
            selectedFolderPath = null;
            PathToFolderOfInstruction.Text = "Путь не выбран";
        }

        private async void buttonSyncManualyInstrWithDB_Click(object sender, EventArgs e)
        {
            try
            {
                buttonSyncManualyInstrWithDB.Enabled = false;
                ListOfInstructions.Items.Clear();

                using (var httpClient = new HttpClient())
                {
                    string jwtToken = Decryption_stuff.DecryptedJWTToken();
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                    var response = await httpClient.GetAsync(urlSyncInstructions);
                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        if (string.IsNullOrWhiteSpace(responseBody))
                        {
                            throw new Exception("responseBody is empty"); //throw here better something
                        }
                        List<Instruction> result = JsonConvert.DeserializeObject<List<Instruction>>(responseBody); //checked that is not null before!

                        string[] resultArray = result.Select(n => n.cause_of_instruction).ToArray(); //check that they are not null;
                        ListOfInstructions.Items.AddRange(resultArray);
                        MessageBox.Show("Names successfully synced with database.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        string errorMessage = await response.Content.ReadAsStringAsync();
                        // The call was not successful, handle errors or retry logic
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
                buttonSyncManualyInstrWithDB.Enabled = true;
            }
        }




        private async void SyncNamesWithDB_Click(object sender, EventArgs e) //ПРОДОЛЖАЙ ЭТОТ КОД! С 28.05.2024
        {
            try
            {
                SyncNamesWithDB.Enabled = false; // Assuming this is a button, disable it to prevent multiple clicks
                ListBoxNamesOfPeople.Items.Clear();

                using (var httpClient = new HttpClient())
                {
                    string jwtToken = Decryption_stuff.DecryptedJWTToken();
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

                        ListBoxNamesOfPeople.Items.AddRange(resultArray);
                        // Successfully called the ImportIntoDB endpoint, handle accordingly
                        MessageBox.Show("Names successfully synced with database.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            var listOfNames = ListBoxNamesOfPeople.SelectedItems;
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
                        string jwtToken = Decryption_stuff.DecryptedJWTToken();
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

        private void LoginForm_Click(object sender, EventArgs e)
        {

            _loginForm.Show();
            this.Dispose(true);
        }
        private void ChiefForm_FormClosed(object sender, FormClosedEventArgs e) //РАЗБЕРИСЬ ПОЧЕМУ ПРИ ЗАКРЫТИИ ФОРМЫ, ВСЕ РАВНО НЕ ЗАКРЫВАЕТСЯ VISUAL STUDIO (УТЕЧКА)
        {

            _loginForm.Dispose();
            this.Dispose(true);
        }

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
                    string jwtToken = Decryption_stuff.DecryptedJWTToken();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();


                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var result = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object>>>(jsonResponse);

                    if (result.Count == 0)
                    {
                        return true;
                    }

                    listOfInstructions_global = result;
                    ListOfInstructionsForUser.Items.Clear();
                    foreach (Dictionary<string, object> temp in result)
                    {
                        ListOfInstructionsForUser.Items.Add(temp[DataBaseNames.tableName_sql_INSTRUCTIONS_cause]);

                        /*foreach (KeyValuePair<string, object> kvp in temp)
                        {
                           var tempValue = kvp.Value.ToString() is null ? "Null" : kvp.Value.ToString();
                            
                        }*/

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
            HyperLinkForInstructionsFolder.Enabled = true;
            PassInstruction.Enabled = true;
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
                //После этого отправить запрос на выбранный Database через сервер что инструктаж пройден. And uncheck the checkbox.
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
                    string jwtToken = Decryption_stuff.DecryptedJWTToken();
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
    }
}
