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


namespace Kotova.Test1.ClientSide
{
    public partial class UserForm : Form
    {

        public const string dB_pos_users_isInstructionPassed = "is_instruction_passed";
        public const string dB_pos_users_causeOfInstruction = "cause_of_instruction";
        public const string dB_pos_users_pathToInstruction = "path_to_instruction";



        public const string dB_instructionId = "instruction_id"; //ВЫНЕСИ ЭТИ 2 СТРОЧКИ В ОБЩИЙ ФАЙЛ!
        public const string db_filePath = "file_path";
        public const string db_typeOfInstruction = "type_of_instruction";

        private bool _IsInstructionSelected = false;
        private List<Dictionary<string, object>> listsOfPaths_global;

        private List<Dictionary<string, object>> listOfInstructions_global;

        private NotifyIcon notifyIcon;

        private static bool canYouCloseTheApplication = false;


        public Login_Russian? _loginForm;
        public SignUpForm _signUpForm;
        string? _userName;
        static readonly string DownloadInstructionForUserURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get_instructions_for_user";
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
            StartTimer();
            InitializeComponent();
            exitApplicationToolStripMenuItem.Enabled = false;
            _loginForm = loginForm;
            _userName = userName;
            UserLabel.Text = _userName;
            PassInstruction.Enabled = false;

            InitializeSignalRConnection();

            SignUpForm signUpForm = new SignUpForm(loginForm, this);
            _signUpForm = signUpForm;

            _ = RefreshNewInstructionsInternal();
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

        private async void CheckForNewInstructions_Click(object sender, EventArgs e)
        {
            await RefreshNewInstructionsInternal();
        }
        private async Task RefreshNewInstructionsInternal()
        {
            ListOfInstructionsForUser.Items.Clear();
            bool IsEmpty = await DownloadInstructionsForUserFromServer(_userName);
            if (IsEmpty)
            {
                MessageBox.Show("Все инструктажи пройдены!");
            }
        }

        private async Task<bool> DownloadInstructionsForUserFromServer(string? userName) // по факту эта функция должна быть вместе с в ChiefForm.cs в совершенно отдельном файле.
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
                return false;
            }

        }

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
            List<string> listOfPath = new List<string>();
            foreach (var listOfPaths in listsOfPaths_global)
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

            foreach (Dictionary<string, object> tempD in listOfInstructions_global)
            {
                Dictionary<string, object> selectedDictionary = listOfInstructions_global.FirstOrDefault(tempD => tempD[dB_pos_users_causeOfInstruction].ToString() == selectedItemStr);
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
    }
}
