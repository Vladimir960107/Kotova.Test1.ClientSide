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

        public Login_Russian? _loginForm;
        public SignUpForm _signUpForm;
        string? _userName;
        const string DownloadInstructionForUserURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get_instructions_for_user";
        const string SendInstructionIsPassedURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/instruction_is_passed_by_user";
        public UserForm()
        {
            InitializeComponent();
        }

        public UserForm(Login_Russian loginForm, string userName)
        {
            InitializeComponent();
            _loginForm = loginForm;
            _userName = userName;
            UserLabel.Text = _userName;
            PassInstruction.Enabled = false;

            SignUpForm signUpForm = new SignUpForm(loginForm, this);
            _signUpForm = signUpForm;
        }

        private async void CheckForNewInstructions_Click(object sender, EventArgs e)
        {
            ListOfInstructionsForUser.Items.Clear();
            bool IsEmpty = await DownloadInstructionsForUserFromServer(_userName);
            if (IsEmpty)
            {
                MessageBox.Show("All the instructions passed!");
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
                throw ex;
            }

        }

        private void ListOfInstructions_SelectedValueChanged(object sender, EventArgs e)
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
                    if (listOfPaths[db_filePath] == null)
                    {
                        if (selectedDict[db_typeOfInstruction].ToString() == "0") // Проверка что мы входим в вводный инструктаж только!
                        {
                            _IsInstructionSelected = true;
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

            _IsInstructionSelected = true;
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
            if (selectedDict[db_typeOfInstruction].ToString() == "0")
            {
                MessageBox.Show("Ты не должен был входить в эту строчку, исправляй (HyperLinkForInstructionsFolder!");
                return;
            }
            string? pathStr = selectedDict[dB_pos_users_pathToInstruction].ToString();

            if (pathStr is null || pathStr.Length == 0)
            {
                MessageBox.Show("Path is null or empty.");
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
                Dictionary<string, object> selectedDictionary = listOfInstructions_global.FirstOrDefault(tempD => tempD[dB_pos_users_causeOfInstruction].ToString() == selectedItemStr);
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
            if (ListOfInstructionsForUser.SelectedItem == null)
            {
                MessageBox.Show("You haven't select the Instruction.");
                PassInstruction.Enabled = false;
                return;
            }
            if (ConfirmAction())
            {
                MessageBox.Show("You agreed with the action.", "Action Confirmed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                PassInstruction.Enabled = false;
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


        private void UserForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _loginForm.Dispose();
            this.Dispose();
        }

        private void SignOut_Click(object sender, EventArgs e)
        {

            if (_signUpForm != null)
            {
                _signUpForm.Dispose();
            }
            Decryption_stuff.DeleteJWTToken();

            _loginForm.Show();
            this.Dispose(true);
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
    }
}
