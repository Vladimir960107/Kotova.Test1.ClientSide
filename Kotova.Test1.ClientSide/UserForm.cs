using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Kotova.Test1.ClientSide.Login_Russian;

using System.Text.Json;
using System.Diagnostics;
using Kotova.CommonClasses;
using Microsoft.AspNetCore.SignalR.Client;
using System.Reflection;
using System.Configuration;
using System.Timers;
using System.Net.Http;
using System.IO;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;
using System.Runtime.InteropServices;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Windows.Controls;
using Control = System.Windows.Forms.Control;


namespace Kotova.Test1.ClientSide
{
    public partial class UserForm : Form
    {

        [DllImport("kernel32.dll")]
        static extern bool AllocConsole();

        public const string dB_pos_users_isInstructionPassed = "is_instruction_passed";
        public const string dB_pos_users_causeOfInstruction = "cause_of_instruction";
        public const string dB_pos_users_pathToInstruction = "path_to_instruction";

        public Dictionary<Control, Rectangle> controlsOriginalSizes;

        public const string dB_instructionId = "instruction_id"; //ВЫНЕСИ ЭТИ СЛЕДУЮЩИЕ СТРОЧКИ В ОБЩИЙ ФАЙЛ!
        public const string db_filePath = "file_path";
        public const string db_typeOfInstruction = "type_of_instruction";
        public const string db_dateOfInstructionWasSentToUser = "when_was_send_to_user";

        private bool _IsInstructionSelected = false;
        private List<Dictionary<string, object>> listsOfPathsOfNewInstr_global;

        private List<Dictionary<string, object>> listOfNewInstructions_global;

        private List<Dictionary<string, object>> listsOfPathsOfOldInstr_global;

        private List<Dictionary<string, object>> listOfOldInstructions_global;

        private NotifyIcon notifyIcon;

        private static bool canYouCloseTheApplication = false;


        public Login_Russian? _loginForm;
        public SignUpForm _signUpForm;
        string? _userName;
        static readonly string DownloadInstructionForUserURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get_not_passed_instructions_for_user";
        static readonly string DownloadOldInstructionForUserURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get_passed_instructions_for_user";
        static readonly string SendInstructionIsPassedURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/instruction_is_passed_by_user";

        private HubConnection? _hubConnection = null;

        public UserForm()
        {
            InitializeComponent();
            InitializeSignalRConnection();
            ToastNotificationManagerCompat.OnActivated += ToastNotificationManagerCompat_OnActivated;
        }

        private void ToastNotificationManagerCompat_OnActivated(ToastNotificationActivatedEventArgsCompat e)
        {
            MessageBox.Show("I've got something!");
            // Parse the arguments from the toast
            var args = ToastArguments.Parse(e.Argument);

            // Check if the action argument is "openApp"
            if (args["action"] == "openApp")
            {
                // Bring the application to the foreground
                Application.OpenForms[0]?.Invoke(new Action(() =>
                {
                    this.Show();
                    this.WindowState = FormWindowState.Normal;
                    this.BringToFront();
                    this.Activate();
                }));
            }
        }

        public UserForm(Login_Russian loginForm, string userName)
        {
            InitializeComponent();


            // The rest of your initialization code...
            StartTimer();
            exitApplicationToolStripMenuItem.Enabled = false;
            _loginForm = loginForm;
            _userName = userName;
            UserLabel.Text = _userName;
            PassInstruction.Enabled = false;
            InitializeSignalRConnection();

            _signUpForm = new SignUpForm(loginForm, this);
            _ = RefreshNewInstructionsInternal();
            _ = RefreshOldInstructionsInternal();
        }


        public void EnableExitTheProgrammEntirelyButton()
        {
            // We're on the UI thread, so we can directly modify the control
            exitApplicationToolStripMenuItem.Enabled = true;
        }

        public void StartTimer()
        {
            System.Timers.Timer timer = new System.Timers.Timer(6000); // 6 seconds. Cause because if not initialized properly everything - this will cause the programm to throw exception. so we wait :)
            timer.Elapsed += (sender, e) =>
            {
                EnableExitTheProgrammEntirelyButton();
                timer.Dispose();
            };
            timer.AutoReset = false;
            timer.Start();
        }


        private async void InitializeSignalRConnection()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(ConfigurationClass.BASE_SIGNALR_CONNECTION_URL_DEVELOPMENT, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(_loginForm._jwtToken);
                })
                .Build();

            _hubConnection.On<string, string>("ReceiveMessage", (user, message) =>
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
                //MessageBox.Show("Подключён к SignalR hub.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось подключиться к SignalR hub: {ex.Message}");
            }
        }

        #region Download From Server Old and New Instructions

        private async void CheckForNewInstructions_Click(object sender, EventArgs e)
        {
            await RefreshNewInstructionsInternal();
            await RefreshOldInstructionsInternal();
        }
        private async Task<bool> RefreshNewInstructionsInternal()
        {
            ListOfInstructionsForUser.Items.Clear();
            bool? IsEmpty = await DownloadInstructionsForUserFromServer(_userName);
            if (IsEmpty == true)
            {
                MessageBox.Show("Все инструктажи пройдены!");
            }
            if (IsEmpty is null)
            {
                return false;
            }
            return true;
        }

        private async Task<bool?> DownloadInstructionsForUserFromServer(string? userName) // по факту эта функция должна быть вместе с в ChiefForm.cs в совершенно отдельном файле.
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
                    var result = JsonSerializer.Deserialize<QueryResult>(jsonResponse);
                    if (result.Result1.Count == 0)
                    {
                        return true;
                    }

                    listOfNewInstructions_global = result.Result1;
                    listsOfPathsOfNewInstr_global = result.Result2;
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
                return null;
            }

        }

        private async Task<bool> RefreshOldInstructionsInternal()
        {
            try
            {
                // Log start of method
                Console.WriteLine("Starting RefreshOldInstructionsInternal...");

                // Clear the list to ensure a fresh state
                dataGridViewPassedInstructions.Rows.Clear();
                Console.WriteLine("Cleared ListOfInstructionsForUser.");

                // Log user name
                Console.WriteLine($"_userName: {_userName}");

                // Call the method to download old instructions
                bool? IsEmpty = await DownloadOldInstructionsForUserFromServer(_userName);

                // Log result of the download
                Console.WriteLine($"DownloadOldInstructionsForUserFromServerIsEmpty returned: {IsEmpty}");

                if (IsEmpty == true)
                {
                    MessageBox.Show("Все инструктажи пройдены!");
                }

                if (IsEmpty is null)
                {
                    Console.WriteLine("IsEmpty is null. Exiting RefreshOldInstructionsInternal early.");
                    return false;
                }

                // Log end of method
                Console.WriteLine("RefreshOldInstructionsInternal completed successfully.");
                return true;
            }
            catch (Exception ex)
            {
                // Log any exceptions
                Console.WriteLine($"Exception in RefreshOldInstructionsInternal: {ex.Message}");
                MessageBox.Show($"Error during refresh: {ex.Message}");
                return false;
            }
        }

        private async Task<bool?> DownloadOldInstructionsForUserFromServer(string? userName) // по факту эта функция должна быть вместе с в ChiefForm.cs в совершенно отдельном файле.
        {
            if (userName is null)
            {
                throw new ArgumentNullException(nameof(userName));
            }
            string url = DownloadOldInstructionForUserURL;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string jwtToken = _loginForm._jwtToken;
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();


                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<QueryResult>(jsonResponse);
                    if (result.Result1.Count == 0)
                    {
                        return true;
                    }

                    listOfOldInstructions_global = result.Result1;
                    listsOfPathsOfOldInstr_global = result.Result2;
                    foreach (Dictionary<string, object> temp in result.Result1)
                    {

                        object dbValue = temp[db_dateOfInstructionWasSentToUser];
                        string formattedDate;
                        if (dbValue != null && DateTime.TryParse(dbValue.ToString(), out DateTime dateOfInstruction))
                        {
                            // Transforming into "Year-Month-Day" format
                            formattedDate = dateOfInstruction.ToString("yyyy-MM-dd");
                        }
                        else
                        {
                            Console.WriteLine("Invalid or null date value of temp[db_dateOfInstructionWasSentToUser] in DownloadOldInstructionsForUserFromServer()");
                            formattedDate = "Неправильный формат даты";
                        }

                        this.Invoke(new Action(() =>
                        {
                            dataGridViewPassedInstructions.Rows.Add(
                                formattedDate,
                                temp[db_typeOfInstruction],
                                temp[dB_pos_users_causeOfInstruction]
                            );
                        }));

                    }
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                // Handle any exceptions here
                MessageBox.Show($"Error: {ex.Message}");
                return null;
            }

        }
        #endregion

        #region Unpassed instructions list value changed (select instruction)
        private void ListOfInstructions_SelectedValueChanged(object sender, EventArgs e)
        {
            FilesOfInstructionCheckedListBox.Items.Clear();
            if (ListOfInstructionsForUser.SelectedItem == null)
            {
                MessageBox.Show("Вы не выбрали инструктаж.");
                PassInstruction.Enabled = false;
                return;
            }
            Dictionary<string, object> selectedDict = GetDictFromSelectedInstruction(ListOfInstructionsForUser.SelectedItem.ToString());
            int instructionId = Convert.ToInt32(selectedDict[dB_instructionId].ToString());
            foreach (var listOfPaths in listsOfPathsOfNewInstr_global)
            {
                if (Convert.ToInt32(listOfPaths[dB_instructionId].ToString()) == instructionId)
                {
                    if (listOfPaths[db_filePath] == null)
                    {
                        if (selectedDict[db_typeOfInstruction].ToString() == "0") // Проверка что мы входим в вводный инструктаж только!
                        {
                            _IsInstructionSelected = true;
                            PassInstruction.Enabled = true;
                            return;
                        }
                        else
                        {
                            MessageBox.Show("Ooops, Что-то пошло не так. Проверь эту строчку на предмет присутствия файлов инструктажа!");
                            PassInstruction.Enabled = false;
                            return;
                        }

                    }
                    FilesOfInstructionCheckedListBox.Items.Add(listOfPaths[db_filePath].ToString());
                }
            }

            _IsInstructionSelected = true;
        }

        #endregion

        #region Passed instructions list value changed (select instruction)
        private void ListOfPassedInstructions_SelectedValueChanged(object sender, EventArgs e)
        {
            FilesOfInstructionCheckedListBox.Items.Clear();
            if (ListOfInstructionsForUser.SelectedItem == null)
            {
                MessageBox.Show("Вы не выбрали инструктаж.");
                PassInstruction.Enabled = false;
                return;
            }
            Dictionary<string, object> selectedDict = GetDictFromSelectedInstruction(ListOfInstructionsForUser.SelectedItem.ToString());
            int instructionId = Convert.ToInt32(selectedDict[dB_instructionId].ToString());
            foreach (var listOfPaths in listsOfPathsOfNewInstr_global)
            {
                if (Convert.ToInt32(listOfPaths[dB_instructionId].ToString()) == instructionId)
                {
                    if (listOfPaths[db_filePath] == null)
                    {
                        if (selectedDict[db_typeOfInstruction].ToString() == "0") // Проверка что мы входим в вводный инструктаж только!
                        {
                            _IsInstructionSelected = true;
                            PassInstruction.Enabled = true;
                            return;
                        }
                        else
                        {
                            MessageBox.Show("Ooops, Что-то пошло не так. Проверь эту строчку на предмет присутствия файлов инструктажа!");
                            PassInstruction.Enabled = false;
                            return;
                        }

                    }
                    FilesOfInstructionCheckedListBox.Items.Add(listOfPaths[db_filePath].ToString());
                }
            }

            _IsInstructionSelected = true;
        }

        #endregion

        private void HyperLinkForInstructionsFolder_Click(object sender, EventArgs e)
        {
            if (ListOfInstructionsForUser.SelectedItem == null)
            {
                MessageBox.Show("Вы не выбрали инструктаж.");
                PassInstruction.Enabled = false;
                return;
            }
            Dictionary<string, object> selectedDict = GetDictFromSelectedInstruction(ListOfInstructionsForUser.SelectedItem.ToString()); //most likely suppress it, cause its not null.
            if (selectedDict[db_typeOfInstruction].ToString() == "0")
            {
                MessageBox.Show("Ты не должен был входить в эту строчку, исправляй (HyperLinkForInstructionsFolder!");
                return;
            }
            string? pathStr = selectedDict[dB_pos_users_pathToInstruction].ToString();

            if (pathStr is null || pathStr.Length == 0)
            {
                MessageBox.Show("Путь пуст или отсутствует.");
                PassInstruction.Enabled = false;
                return;
            }
            string path = Path.GetFullPath(pathStr);
            OpenFolderInExplorer(path);
            if (_IsInstructionSelected) { _IsInstructionSelected = false; }
        }

        private void OpenFolderInExplorer(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                MessageBox.Show("Указанный путь отсутствует.");
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
                MessageBox.Show($"Не удалось открыть папку: {ex.Message}");
                PassInstruction.Enabled = false;
            }
        }

        private Dictionary<string, object> GetDictFromSelectedInstruction(string selectedItemStr)
        {

            foreach (Dictionary<string, object> tempD in listOfNewInstructions_global)
            {
                Dictionary<string, object> selectedDictionary = listOfNewInstructions_global.FirstOrDefault(tempD => tempD[dB_pos_users_causeOfInstruction].ToString() == selectedItemStr);
                if (selectedDictionary != null)
                {
                    return selectedDictionary; // HERE WE DIDN't CHECK  THAT названия инструктажей не повторяется, а просто вернули первое попавшееся. Проверку бы!
                }
            }
            throw new Exception("Соответствующий инструктаж не был найден, ошибка!");
        }

        private async void PassInstruction_CheckedChanged(object sender, EventArgs e)
        {
            if (!PassInstruction.Checked) { return; }
            if (ListOfInstructionsForUser.SelectedItem == null)
            {
                MessageBox.Show("Вы не выбрали инструктаж.");
                PassInstruction.Enabled = false;
                return;
            }
            if (ConfirmAction())
            {
                MessageBox.Show("Вы согласились, что прошли инструктаж.", "Действие подтверждено", MessageBoxButtons.OK, MessageBoxIcon.Information);
                PassInstruction.Enabled = false;
                Dictionary<string, object> selectedDict = GetDictFromSelectedInstruction(ListOfInstructionsForUser.SelectedItem.ToString());
                await SendInstructionIsPassedToDB(selectedDict);
                FilesOfInstructionCheckedListBox.Items.Clear();

            }
            else
            {
                MessageBox.Show("Вы не подтвердили, что прошли инструктаж.", "Действие отменено", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                PassInstruction.Checked = false;
                return;
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

                    string jsonData = JsonSerializer.Serialize(selectedDict);

                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                    HttpResponseMessage response = await client.PostAsync(url, content);
                    response.EnsureSuccessStatusCode();

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode)
                    {
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
                PassInstruction.Checked = false;
            }
        }

        private bool ConfirmAction()
        {
            var result = MessageBox.Show("Вы подтверждаете прохождение инструктажа?", "Подтвердите действие", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void UserForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            this.ShowInTaskbar = false;
        }

        private void FilesOfInstructionCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            this.BeginInvoke((System.Windows.Forms.MethodInvoker)delegate
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
                MessageBox.Show($"Не удалось открыть файл: {ex.Message}");
            }
        }

        private void showNotification_Click(object sender, EventArgs e)
        {
            Notifications.ShowWindowsNotification("Уведомление", "Проверка.");

        }

        private async void UpdateInstructionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await RefreshNewInstructionsInternal();
            await RefreshOldInstructionsInternal();
        }



        private void changeCredentialsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _signUpForm.Show();
        }

        private void exitApplicationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Dispose(true);
            _loginForm.ExitApplication();
        }

        private void signOutToolStripMenuItem_Click(object sender, EventArgs e)
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

        private void AdditionalSettingsPicture_Click(object sender, EventArgs e)
        {
            // Get the mouse position relative to the screen
            Point screenPosition = AdditionalSettingsPicture.PointToScreen(new Point(0, AdditionalSettingsPicture.Height));

            // Show the context menu at the mouse click position
            AdditionalSettingsForUserContextMenuStrip.Show(screenPosition);
        }

        private async void PassedOrNotInstrTabControl_TabIndexChanged(object sender, EventArgs e)
        {
            await RefreshNewInstructionsInternal();
            await RefreshOldInstructionsInternal();

        }

        private void dataGridViewPassedInstructions_SelectionChanged(object sender, EventArgs e)
        {
            listBoxOfPathsOfPassedInstructions.Items.Clear();
            if (dataGridViewPassedInstructions.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridViewPassedInstructions.SelectedRows[0];
                string? cause;
                try
                {
                    object cellValue = selectedRow.Cells["CauseOfPassedInstr"]?.Value; //ЗДЕСЬ 2, ПОТОМУ ЧТО CauseOFPassedInstr НЕ РАБОТАЕТ ПОЧЕМУ-ТО
                    if (cellValue == null)
                    {
                        return;
                    }
                    cause = cellValue.ToString();
                }
                catch
                {
                    return;
                }
                // If we successfully retrieved a non-null cause, call your method
                List<string?>? listOfPaths = FetchOldInstructionsFilesByCause(cause);
                if (listOfPaths is null || listOfPaths.Count == 0) return;

                string[] pathArray = listOfPaths
                    .Where(item => item is not null) // Remove nulls
                    .Select(item => item!.ToString()!) // Convert to string safely
                    .ToArray();


                if (pathArray is null || pathArray.Length == 0)
                {
                    listBoxOfPathsOfPassedInstructions.Text = "Файлы для данного инструктажа не найдены";
                    return;
                }

                listBoxOfPathsOfPassedInstructions.Items.AddRange(pathArray);

            }
        }

        private void dataGridViewPassedInstructions_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            listBoxOfPathsOfPassedInstructions.Items.Clear();
            // Ensure it's not the header row or an invalid row index
            if (e.RowIndex >= 0)
            {
                DataGridViewRow selectedRow = dataGridViewPassedInstructions.Rows[e.RowIndex];
                string? cause;

                try
                {
                    // Safely check the cell for null before calling .ToString()
                    object cellValue = selectedRow.Cells["CauseOfPassedInstr"]?.Value;
                    if (cellValue == null)
                    {
                        // If the cell is null or we can’t get the value, just return
                        return;
                    }

                    cause = cellValue.ToString();
                }
                catch
                {
                    // If anything goes wrong (e.g. wrong column name, out of range, etc.), just return
                    return;
                }

                // If we successfully retrieved a non-null cause, call your method
                List<string?>? listOfPaths = FetchOldInstructionsFilesByCause(cause);
                if (listOfPaths is null || listOfPaths.Count == 0) return;

                string[] pathArray = listOfPaths
                    .Where(item => item is not null) // Remove nulls
                    .Select(item => item!.ToString()!) // Convert to string safely
                    .ToArray();


                if (pathArray is null || pathArray.Length == 0)
                {
                    listBoxOfPathsOfPassedInstructions.Text = "Файлы для данного инструктажа не найдены";
                    return;
                }

                listBoxOfPathsOfPassedInstructions.Items.AddRange(pathArray);

            }
        }

        private List<string?>? FetchOldInstructionsFilesByCause(string? cause)
        {
            if (cause == null)
            {
                MessageBox.Show("Причина выбранного инструктажа null. Как ты вообще сюда попал, User? :/");
                return null;
            }
            List<object?> listOfIds = GetInstructionIdsOfGivenCause(listOfOldInstructions_global, cause);
            if (listOfIds.Count != 1)
            {
                MessageBox.Show("Для данного инструктажа не найдены ID или найдены больше чем 1, что-то пошло не так :/");
                return null;
            }

            object? firstId = listOfIds[0];

            int id;
            if (firstId is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Number)
            {
                id = jsonElement.GetInt32(); // Extract int properly
            }
            else
            {
                id = Convert.ToInt32(firstId); // Standard conversion if not JsonElement
            }


            List<string?> listOfPaths = GetPathsOfGivenId(listsOfPathsOfOldInstr_global, id);
            return listOfPaths;
            //PrintListOfDictionary(listsOfPathsOfOldInstr_global);
            //PrintListOfDictionary(listOfOldInstructions_global);
        }

        private List<string?> GetPathsOfGivenId(List<Dictionary<string, object>> list, int id)
        {
            return list
             .Where(dict => dict.ContainsKey(dB_instructionId) &&
                GetIntValue(dict[dB_instructionId]) == id) // Safely extract int
            .Select(dict => dict.ContainsKey(db_filePath) ? dict[db_filePath]?.ToString() : null)
            .ToList();
        }

        private static int GetIntValue(object obj)
        {
            if (obj is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Number)
            {
                return jsonElement.GetInt32(); // Extract integer safely from JsonElement
            }
            return Convert.ToInt32(obj); // Standard conversion for other types
        }
        private List<object?> GetInstructionIdsOfGivenCause(List<Dictionary<string, object>> list, string cause)
        {
            return list
            .Where(dict => dict.ContainsKey(dB_pos_users_causeOfInstruction) && dict[dB_pos_users_causeOfInstruction]?.ToString() == cause)
            .Select(dict => dict.ContainsKey(dB_instructionId) ? dict[dB_instructionId] : null)
            .ToList();
        }
        private void PrintListOfDictionary(List<Dictionary<string, object>> list)
        {
            foreach (var dict in list)
            {
                Console.WriteLine("{" + string.Join(", ", dict.Select(kv => $"{kv.Key}: {kv.Value}")) + "}");
            }
        }

        private void listBoxOfPathsOfPassedInstructions_DoubleClick(object sender, EventArgs e)
        {
            var selectedItem = listBoxOfPathsOfPassedInstructions.SelectedItem;
            if (selectedItem is null)
            {
                return;
            }
            OpenFile(selectedItem.ToString());
        }

        private void AdditionalSettingsForUserContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            // If you’re relying on the default opening, you don't want to cancel it:
            e.Cancel = false;

            // Let's get the default position and shift it, for example.
            // ContextMenuStrip does not directly give us the "default" point 
            // but we can compute or adjust with SourceControl, etc.
            // For simplicity, if you only want a small shift from the cursor:
            Point cursorPos = Cursor.Position;
            int menuWidth = AdditionalSettingsForUserContextMenuStrip.Size.Width;
            Point adjustedPos = new Point(cursorPos.X - menuWidth, cursorPos.Y);
            AdditionalSettingsForUserContextMenuStrip.Show(adjustedPos);

            // You can let the control handle it or show manually:
            // AdditionalSettingsForUserContextMenuStrip.Show(adjustedPos);
            // e.Cancel = true;  // <-- would normally be used if you do a manual Show, but then you'd rely solely on Show().
        }

        // Add these class-level fields to track window state
        private bool isMaximized = false;
        private TabPage lastActiveTab = null;


        #region Resize of form for User
        private void UserForm_Resize(object sender, EventArgs e)
        {
            if (controlsOriginalSizes == null || PassedOrNotInstrTabControl == null)
            {
                return;
            }

            // Update our tracking of window state
            isMaximized = (this.WindowState == FormWindowState.Maximized);

            // Store current tab for reference
            lastActiveTab = PassedOrNotInstrTabControl.SelectedTab;

            // Call our resize handler
            HandleTabResize(lastActiveTab);
        }

        // Add this handler for tab changes
        private void PassedOrNotInstrTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabPage currentTab = PassedOrNotInstrTabControl.SelectedTab;

            // Only resize if we're maximized
            if (isMaximized)
            {
                HandleTabResize(currentTab);
            }
            else
            {
                // Restore original sizes for the new tab's controls
                RestoreTabControlsToOriginalSize(currentTab);
            }

            lastActiveTab = currentTab;
        }

        private void HandleTabResize(TabPage currentTab)
        {
            Control[] currentTabControls = GetControlsForTab(currentTab);
            if (currentTabControls == null) return;

            Rectangle originalPanelBounds = controlsOriginalSizes[PassedOrNotInstrTabControl];

            if (isMaximized)
            {
                float widthRatio = (float)this.ClientSize.Width / originalPanelBounds.Width;
                float heightRatio = (float)this.ClientSize.Height / originalPanelBounds.Height;

                PassedOrNotInstrTabControl.Width = (int)(originalPanelBounds.Width * widthRatio);
                PassedOrNotInstrTabControl.Height = (int)(originalPanelBounds.Height * heightRatio);

                int spacing = 40;
                int controlWidth = (PassedOrNotInstrTabControl.Width - (3 * spacing)) / 2;

                if (currentTab == NotPassedInstrTabPage)
                {
                    // First control and its label
                    var firstControl = currentTabControls[0]; // ListOfInstructionsForUser
                    if (controlsOriginalSizes.ContainsKey(firstControl))
                    {
                        Rectangle originalBounds = controlsOriginalSizes[firstControl];
                        firstControl.SetBounds(
                            spacing,
                            originalBounds.Y + 40,
                            controlWidth,
                            PassedOrNotInstrTabControl.Height - (2 * spacing) - 20
                        );

                        // Position its label
                        LabelOfNotPassedInstr.SetBounds(
                            spacing,
                            originalBounds.Y,
                            controlWidth,
                            20
                        );
                    }

                    // Second control and its label
                    var secondControl = currentTabControls[1]; // FilesOfInstructionCheckedListBox
                    if (controlsOriginalSizes.ContainsKey(secondControl))
                    {
                        Rectangle originalBounds = controlsOriginalSizes[secondControl];
                        secondControl.SetBounds(
                            firstControl.Right + spacing,
                            originalBounds.Y + 40,
                            controlWidth,
                            PassedOrNotInstrTabControl.Height - (2 * spacing) - 20
                        );
                    }
                }
                else if (currentTab == PassedInstrTabPage)
                {
                    // Grid control
                    var gridControl = currentTabControls[0]; // dataGridViewPassedInstructions
                    if (controlsOriginalSizes.ContainsKey(gridControl))
                    {
                        Rectangle originalBounds = controlsOriginalSizes[gridControl];
                        gridControl.SetBounds(
                            spacing,
                            originalBounds.Y + 40,
                            controlWidth,
                            PassedOrNotInstrTabControl.Height - (2 * spacing) - 80  // Added bottom spacing
                        );
                        // Position its label
                        LabelOfPassedInstr.SetBounds(
                            spacing,
                            originalBounds.Y,
                            controlWidth,
                            20
                        );
                    }

                    // List box control
                    var listBoxControl = currentTabControls[1]; // listBoxOfPathsOfPassedInstructions
                    if (controlsOriginalSizes.ContainsKey(listBoxControl))
                    {
                        Rectangle originalBounds = controlsOriginalSizes[listBoxControl];
                        listBoxControl.SetBounds(
                            gridControl.Right + spacing,
                            originalBounds.Y + 40,
                            controlWidth,
                            PassedOrNotInstrTabControl.Height - (2 * spacing) - 80  // Added bottom spacing
                        );
                    }
                }
            }
            else
            {
                RestoreTabControlsToOriginalSize(currentTab);
            }
        }

        private Control[] GetControlsForTab(TabPage tab)
        {
            if (tab == NotPassedInstrTabPage)
            {
                return new Control[]
                {
            ListOfInstructionsForUser,
            FilesOfInstructionCheckedListBox,
            LabelOfNotPassedInstr,    // Adding the label
                };
            }
            else if (tab == PassedInstrTabPage)
            {
                return new Control[]
                {
            dataGridViewPassedInstructions,
            listBoxOfPathsOfPassedInstructions,
            LabelOfPassedInstr,
                    // Add any labels for the second tab if they exist
                };
            }
            return null;
        }

        private void RestoreTabControlsToOriginalSize(TabPage tab)
        {
            // Get controls for this tab
            Control[] tabControls = GetControlsForTab(tab);
            if (tabControls == null) return;

            // Restore the tab control itself
            Rectangle tabControlBounds = controlsOriginalSizes[PassedOrNotInstrTabControl];
            PassedOrNotInstrTabControl.SetBounds(
                tabControlBounds.X,
                tabControlBounds.Y,
                tabControlBounds.Width,
                tabControlBounds.Height
            );

            // Restore each control to its original size
            foreach (Control ctrl in tabControls)
            {
                if (!controlsOriginalSizes.ContainsKey(ctrl)) continue;

                Rectangle originalBounds = controlsOriginalSizes[ctrl];
                ctrl.Visible = true;
                ctrl.SetBounds(
                    originalBounds.X,
                    originalBounds.Y,
                    originalBounds.Width,
                    originalBounds.Height
                );
            }
        }

        private void UserForm_Load(object sender, EventArgs e)
        {
            controlsOriginalSizes = new Dictionary<Control, Rectangle>();

            // Capture the main tab control
            controlsOriginalSizes[PassedOrNotInstrTabControl] = PassedOrNotInstrTabControl.Bounds;

            // Capture controls for the first tab
            controlsOriginalSizes[ListOfInstructionsForUser] = ListOfInstructionsForUser.Bounds;
            controlsOriginalSizes[FilesOfInstructionCheckedListBox] = FilesOfInstructionCheckedListBox.Bounds;
            controlsOriginalSizes[LabelOfNotPassedInstr] = LabelOfNotPassedInstr.Bounds;
            controlsOriginalSizes[LabelOfPassedInstr] = LabelOfPassedInstr.Bounds;

            // Capture controls for the second tab
            controlsOriginalSizes[dataGridViewPassedInstructions] = dataGridViewPassedInstructions.Bounds;
            controlsOriginalSizes[listBoxOfPathsOfPassedInstructions] = listBoxOfPathsOfPassedInstructions.Bounds;

            // Capture the tab pages themselves
            controlsOriginalSizes[NotPassedInstrTabPage] = NotPassedInstrTabPage.Bounds;
            controlsOriginalSizes[PassedInstrTabPage] = PassedInstrTabPage.Bounds;
        }
        #endregion
    }

}