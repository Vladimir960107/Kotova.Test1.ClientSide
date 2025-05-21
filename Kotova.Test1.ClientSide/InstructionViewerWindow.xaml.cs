using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Kotova.CommonClasses;
using MessageBox = System.Windows.MessageBox;

namespace Kotova.Test1.ClientSide
{
    public partial class InstructionViewerWindow : Window
    {
        // Constants (same as UserForm)
        public const string dB_pos_users_isInstructionPassed = "is_instruction_passed";
        public const string dB_pos_users_causeOfInstruction = "cause_of_instruction";
        public const string dB_pos_users_pathToInstruction = "path_to_instruction";
        public const string dB_instructionId = "instruction_id";
        public const string db_filePath = "file_path";
        public const string db_typeOfInstruction = "type_of_instruction";
        public const string db_dateOfInstructionWasSentToUser = "when_was_send_to_user";
        public const string db_normativeInstructionId = "id";
        public const string db_normativeInstructionName = "name";
        public const string db_normativeInstructionUrl = "url";

        // Private fields
        private InstructionService _instructionService;
        private bool _isInstructionSelected = false;
        private List<Dictionary<string, object>> _normativeInstructionsOfNewInstr;
        private List<Dictionary<string, object>> _normativeInstructionsOfOldInstr;
        private List<Dictionary<string, object>> _listOfNewInstructions;
        private List<Dictionary<string, object>> _listOfOldInstructions;
        private string _jwtToken;
        private bool _isChief;
        private string _userName;

        // Observable collections for data binding
        public ObservableCollection<string> Instructions { get; set; }
        public ObservableCollection<NormativeInstructionItem> NormativeInstructions { get; set; }
        public ObservableCollection<PassedInstructionItem> PassedInstructions { get; set; }
        public ObservableCollection<string> RelatedFiles { get; set; }

        public InstructionViewerWindow(string jwtToken, string userName, bool isChief = false)
        {
            _jwtToken = jwtToken;
            _isChief = isChief;
            _userName = userName;
            _instructionService = new InstructionService(jwtToken, new WpfLogger(this));

            InitializeComponent();
            InitializeCollections();
            LoadInstructionsAsync();

            // Set username
            UsernameTextBlock.Text = userName;

            // Disable pass instruction checkbox for chiefs
            if (_isChief)
            {
                PassInstructionCheckBox.IsEnabled = false;
                PassInstructionCheckBox.ToolTip = "Руководители не могут отмечать инструктажи как пройденные для себя";
            }
        }

        private void InitializeCollections()
        {
            Instructions = new ObservableCollection<string>();
            NormativeInstructions = new ObservableCollection<NormativeInstructionItem>();
            PassedInstructions = new ObservableCollection<PassedInstructionItem>();
            RelatedFiles = new ObservableCollection<string>();

            InstructionsListBox.ItemsSource = Instructions;
            NormativeInstructionsListBox.ItemsSource = NormativeInstructions;
            PassedInstructionsDataGrid.ItemsSource = PassedInstructions;
            RelatedFilesListBox.ItemsSource = RelatedFiles;
        }

        private async void LoadInstructionsAsync()
        {
            await RefreshNewInstructionsAsync();
            await RefreshOldInstructionsAsync();
        }

        // Logger implementation for WPF
        private class WpfLogger : InstructionService.ILogger
        {
            private readonly InstructionViewerWindow _window;

            public WpfLogger(InstructionViewerWindow window)
            {
                _window = window;
            }

            public void LogInfo(string message)
            {
                Console.WriteLine($"INFO: {message}");
            }

            public void LogError(string message, Exception ex = null)
            {
                string errorMessage = ex != null ? $"{message}: {ex.Message}" : message;
                Console.WriteLine($"ERROR: {errorMessage}");
            }
        }

        #region Data Models

        public class NormativeInstructionItem : INotifyPropertyChanged
        {
            private bool _isChecked;

            public int Id { get; set; }
            public string Name { get; set; }
            public string Url { get; set; }
            public string DisplayText => $"{Name} ({Url})";

            public bool IsChecked
            {
                get => _isChecked;
                set
                {
                    _isChecked = value;
                    OnPropertyChanged();
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public class PassedInstructionItem
        {
            public string DatePassed { get; set; }
            public string Type { get; set; }
            public string Cause { get; set; }
            public int InstructionId { get; set; }
        }

        #endregion

        #region Refresh Instructions Methods

        private async Task RefreshNewInstructionsAsync()
        {
            try
            {
                Instructions.Clear();
                NormativeInstructions.Clear();

                var result = await _instructionService.GetNotPassedInstructionsAsync();

                if (result == null)
                {
                    MessageBox.Show("Все инструктажи пройдены!");
                    return;
                }

                // Update the global lists
                _listOfNewInstructions = result.Value.Instructions;
                _normativeInstructionsOfNewInstr = result.Value.NormativeInstructions;

                // Update the UI
                foreach (var instruction in _listOfNewInstructions)
                {
                    if (instruction.ContainsKey("cause_of_instruction") && instruction["cause_of_instruction"] != null)
                    {
                        Instructions.Add(instruction["cause_of_instruction"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in RefreshNewInstructionsAsync: {ex}");
                MessageBox.Show($"Error refreshing instructions: {ex.Message}");
            }
        }

        private async Task RefreshOldInstructionsAsync()
        {
            try
            {
                PassedInstructions.Clear();

                var result = await _instructionService.GetPassedInstructionsAsync();

                if (result == null)
                {
                    return;
                }

                // Update the global lists
                _listOfOldInstructions = result.Value.Instructions;
                _normativeInstructionsOfOldInstr = result.Value.NormativeInstructions;

                // Update the UI
                foreach (var instruction in _listOfOldInstructions)
                {
                    DateTime? whenPassed = null;
                    if (instruction.ContainsKey("date_when_passed") && instruction["date_when_passed"] != null)
                    {
                        whenPassed = Convert.ToDateTime(instruction["date_when_passed"]);
                    }

                    string type = instruction.ContainsKey("type") ?
                         instruction["type"]?.ToString() ?? "Unknown" : "Unknown";

                    string cause = instruction.ContainsKey("cause_of_instruction") ?
                        instruction["cause_of_instruction"]?.ToString() ?? "" : "";

                    int instructionId = 0;
                    if (instruction.ContainsKey("instruction_id"))
                    {
                        var idValue = instruction["instruction_id"];
                        if (idValue is JsonElement jsonElement)
                        {
                            instructionId = jsonElement.GetInt32();
                        }
                        else
                        {
                            instructionId = Convert.ToInt32(idValue);
                        }
                    }

                    PassedInstructions.Add(new PassedInstructionItem
                    {
                        DatePassed = whenPassed?.ToString("yyyy-MM-dd") ?? "",
                        Type = type,
                        Cause = cause,
                        InstructionId = instructionId
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in RefreshOldInstructionsAsync: {ex}");
                MessageBox.Show($"Error refreshing passed instructions: {ex.Message}");
            }
        }

        #endregion

        #region Event Handlers

        private void InstructionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NormativeInstructions.Clear();
            _isInstructionSelected = false;
            PassInstructionCheckBox.IsEnabled = false;

            if (InstructionsListBox.SelectedItem == null)
                return;

            string selectedInstruction = InstructionsListBox.SelectedItem.ToString();
            var selectedDict = GetDictFromSelectedInstruction(selectedInstruction);

            if (selectedDict == null)
                return;

            int instructionId = Convert.ToInt32(selectedDict[dB_instructionId].ToString());

            // Check if this is an introductory instruction (type 0)
            string instructionType = selectedDict[db_typeOfInstruction].ToString();
            if (instructionType == "0")
            {
                _isInstructionSelected = true;
                PassInstructionCheckBox.IsEnabled = !_isChief;
                return;
            }

            // Load normative instructions
            bool hasNormativeInstructions = false;
            foreach (var normativeInstruction in _normativeInstructionsOfNewInstr)
            {
                if (Convert.ToInt32(normativeInstruction[dB_instructionId].ToString()) == instructionId)
                {
                    NormativeInstructions.Add(new NormativeInstructionItem
                    {
                        Id = Convert.ToInt32(normativeInstruction[db_normativeInstructionId]),
                        Name = normativeInstruction[db_normativeInstructionName]?.ToString() ?? "",
                        Url = normativeInstruction[db_normativeInstructionUrl]?.ToString() ?? "",
                        IsChecked = false
                    });
                    hasNormativeInstructions = true;
                }
            }

            if (hasNormativeInstructions)
            {
                _isInstructionSelected = true;
            }
            else
            {
                MessageBox.Show("Для данного инструктажа не найдены нормативные документы.");
                _isInstructionSelected = false;
            }
        }

        private Dictionary<string, object> GetDictFromSelectedInstruction(string selectedItemStr)
        {
            if (_listOfNewInstructions == null)
                return null;

            return _listOfNewInstructions.FirstOrDefault(instr =>
                instr.ContainsKey(dB_pos_users_causeOfInstruction) &&
                instr[dB_pos_users_causeOfInstruction]?.ToString() == selectedItemStr);
        }

        private void NormativeInstruction_Checked(object sender, RoutedEventArgs e)
        {
            UpdatePassInstructionState();

            // Open URL when checked
            if (sender is System.Windows.Controls.CheckBox checkBox && checkBox.Tag is NormativeInstructionItem item)
            {
                OpenUrl(item.Url);
            }
        }

        private void NormativeInstruction_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdatePassInstructionState();
        }

        private void UpdatePassInstructionState()
        {
            if (_isChief)
            {
                PassInstructionCheckBox.IsEnabled = false;
                return;
            }

            if (!_isInstructionSelected)
            {
                PassInstructionCheckBox.IsEnabled = false;
                return;
            }

            // Check if this is an introductory instruction
            if (InstructionsListBox.SelectedItem != null)
            {
                var selectedDict = GetDictFromSelectedInstruction(InstructionsListBox.SelectedItem.ToString());
                if (selectedDict != null)
                {
                    string instructionType = selectedDict[db_typeOfInstruction].ToString();
                    if (instructionType == "0")
                    {
                        PassInstructionCheckBox.IsEnabled = true;
                        return;
                    }
                }
            }

            // For other instruction types, check if all normative instructions are checked
            bool allChecked = NormativeInstructions.Count > 0 && NormativeInstructions.All(ni => ni.IsChecked);
            PassInstructionCheckBox.IsEnabled = allChecked;
        }

        private async void PassInstructionCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (_isChief)
            {
                PassInstructionCheckBox.IsChecked = false;
                MessageBox.Show("Руководители не могут отмечать инструктажи как пройденные для себя.");
                return;
            }

            if (InstructionsListBox.SelectedItem == null)
            {
                MessageBox.Show("Вы не выбрали инструктаж.");
                PassInstructionCheckBox.IsChecked = false;
                return;
            }

            var result = MessageBox.Show("Вы подтверждаете прохождение инструктажа?",
                "Подтвердите действие", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                MessageBox.Show("Вы согласились, что прошли инструктаж.",
                    "Действие подтверждено", MessageBoxButton.OK, MessageBoxImage.Information);

                PassInstructionCheckBox.IsEnabled = false;
                var selectedDict = GetDictFromSelectedInstruction(InstructionsListBox.SelectedItem.ToString());
                await SendInstructionIsPassedToDB(selectedDict);

                NormativeInstructions.Clear();
                _isInstructionSelected = false;
            }
            else
            {
                MessageBox.Show("Вы не подтвердили, что прошли инструктаж.",
                    "Действие отменено", MessageBoxButton.OK, MessageBoxImage.Warning);
                PassInstructionCheckBox.IsChecked = false;
            }
        }

        private async Task SendInstructionIsPassedToDB(Dictionary<string, object> selectedDict)
        {
            try
            {
                // Extract the instruction ID from the selected dictionary
                int instructionId;

                object idValue = selectedDict["instruction_id"];
                if (idValue is JsonElement jsonElement)
                {
                    instructionId = jsonElement.GetInt32();
                }
                else
                {
                    instructionId = Convert.ToInt32(idValue);
                }

                // Use the service to mark the instruction as passed
                bool success = await _instructionService.MarkInstructionAsPassedAsync(instructionId);

                if (success)
                {
                    // Refresh both instruction lists
                    await RefreshNewInstructionsAsync();
                    await RefreshOldInstructionsAsync();

                    MessageBox.Show("Инструктаж успешно отмечен как пройденный.",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Не удалось отметить инструктаж как пройденный.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                PassInstructionCheckBox.IsChecked = false;
            }
        }

        private void PassedInstructionsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RelatedFiles.Clear();

            if (PassedInstructionsDataGrid.SelectedItem is not PassedInstructionItem selectedItem)
                return;

            var normativeInstructions = GetNormativeInstructionsForPassedInstruction(selectedItem.Cause);

            if (normativeInstructions == null || normativeInstructions.Count == 0)
            {
                RelatedFiles.Add("Нормативные инструкции для данного инструктажа не найдены");
                return;
            }

            foreach (var instruction in normativeInstructions)
            {
                RelatedFiles.Add(instruction);
            }
        }

        private List<string> GetNormativeInstructionsForPassedInstruction(string cause)
        {
            if (string.IsNullOrEmpty(cause) || _normativeInstructionsOfOldInstr == null)
                return new List<string>();

            var instructionIds = GetInstructionIdsOfGivenCause(_listOfOldInstructions, cause);

            if (instructionIds == null || instructionIds.Count != 1)
                return new List<string>();

            var firstId = instructionIds[0];
            if (firstId == null)
                return new List<string>();

            int id;
            if (firstId is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Number)
            {
                id = jsonElement.GetInt32();
            }
            else
            {
                id = Convert.ToInt32(firstId);
            }

            return GetNormativeInstructionsOfGivenId(_normativeInstructionsOfOldInstr, id);
        }

        private List<string> GetNormativeInstructionsOfGivenId(List<Dictionary<string, object>> list, int id)
        {
            return list
                .Where(dict => dict.ContainsKey(dB_instructionId) && GetIntValue(dict[dB_instructionId]) == id)
                .Select(dict =>
                {
                    string name = dict.ContainsKey(db_normativeInstructionName) ?
                        dict[db_normativeInstructionName]?.ToString() ?? "Unknown" : "Unknown";
                    string url = dict.ContainsKey(db_normativeInstructionUrl) ?
                        dict[db_normativeInstructionUrl]?.ToString() ?? "" : "";
                    return $"{name} ({url})";
                })
                .ToList();
        }

        private static int GetIntValue(object obj)
        {
            if (obj is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Number)
            {
                return jsonElement.GetInt32();
            }
            return Convert.ToInt32(obj);
        }

        private List<object> GetInstructionIdsOfGivenCause(List<Dictionary<string, object>> list, string cause)
        {
            if (list == null)
                return new List<object>();

            return list
                .Where(dict => dict != null &&
                       dict.ContainsKey(dB_pos_users_causeOfInstruction) &&
                       dict[dB_pos_users_causeOfInstruction]?.ToString() == cause)
                .Select(dict => dict.ContainsKey(dB_instructionId) ? dict[dB_instructionId] : null)
                .Where(id => id != null)
                .ToList();
        }

        private void RelatedFilesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (RelatedFilesListBox.SelectedItem is not string selectedText)
                return;

            // Extract URL from the display text (format: "Name (URL)")
            int urlStart = selectedText.LastIndexOf('(');
            int urlEnd = selectedText.LastIndexOf(')');

            if (urlStart > 0 && urlEnd > urlStart)
            {
                string url = selectedText.Substring(urlStart + 1, urlEnd - urlStart - 1);
                if (!string.IsNullOrEmpty(url))
                {
                    OpenUrl(url);
                }
            }
        }

        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось открыть URL: {ex.Message}");
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await RefreshNewInstructionsAsync();
            await RefreshOldInstructionsAsync();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // Create context menu for settings
            var contextMenu = new ContextMenu();

            var refreshItem = new MenuItem { Header = "Обновить инструктажи" };
            refreshItem.Click += async (s, args) =>
            {
                await RefreshNewInstructionsAsync();
                await RefreshOldInstructionsAsync();
            };

            contextMenu.Items.Add(refreshItem);
            contextMenu.Items.Add(new Separator());

            var closeItem = new MenuItem { Header = "Закрыть окно" };
            closeItem.Click += (s, args) => this.Close();
            contextMenu.Items.Add(closeItem);

            contextMenu.IsOpen = true;
        }

        #endregion

        protected override void OnClosing(CancelEventArgs e)
        {
            // Optional: Add confirmation dialog
            // var result = MessageBox.Show("Вы уверены, что хотите закрыть окно?", 
            //     "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            // 
            // if (result == MessageBoxResult.No)
            // {
            //     e.Cancel = true;
            //     return;
            // }

            base.OnClosing(e);
        }
    }
}