using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Kotova.CommonClasses;
using Kotova.Test1.ClientSide.InstructionControlWPF;
using ListBox = System.Windows.Controls.ListBox;
using MessageBox = System.Windows.MessageBox;
using RelayCommand = Kotova.Test1.ClientSide.ManagementWPF.Helpers.RelayCommand;

namespace Kotova.Test1.ClientSide
{
    public partial class InstructionViewerWindow : Window, INotifyPropertyChanged
    {
        #region Constants
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
        #endregion

        #region Private Fields
        private bool _allowCompletion;
        private InstructionService _instructionService;
        private InstructionViewerViewModel _instructionViewModel;
        private bool _isInstructionSelected = false;
        private List<Dictionary<string, object>> _normativeInstructionsOfNewInstr;
        private List<Dictionary<string, object>> _normativeInstructionsOfOldInstr;
        private List<Dictionary<string, object>> _listOfNewInstructions;
        private List<Dictionary<string, object>> _listOfOldInstructions;
        private string _jwtToken;
        private bool _isChief;
        private string _userName;
        private bool _forceClose = true;
        private Login_Russian _loginForm;

        // Collections for data binding
        private ObservableCollection<string> _instructions;
        private ObservableCollection<NormativeInstructionItem> _normativeInstructions;
        private ObservableCollection<PassedInstructionItem> _passedInstructions;
        private ObservableCollection<string> _relatedFiles;
        private string _statusMessage = "Готов";
        private string _version = "v1.0";
        #endregion

        #region Constructor
        public InstructionViewerWindow(string jwtToken, string userName, bool isChief, Login_Russian loginForm = null)
        {
            InitializeComponent();

            _jwtToken = jwtToken;
            _userName = userName;
            _isChief = isChief;
            _loginForm = loginForm;

            // FOR DEBUGGING - Set AllowCompletion to true to see if checkbox appears
            _allowCompletion = true; // Make sure this is set to true for testing

            // Debug output
            Debug.WriteLine($"Constructor: AllowCompletion = {_allowCompletion}");
            Debug.WriteLine($"Constructor: IsChief = {_isChief}");
            Debug.WriteLine($"Constructor: UserName = {_userName}");

            InitializeCollections();
            InitializeWindow();
        }
        #endregion

        #region Properties for Data Binding
        public string JwtToken => _jwtToken;
        public string UserName => _userName;
        public bool IsChief => _isChief;
        public bool AllowCompletion => _allowCompletion;
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }
        public string Version
        {
            get => _version;
            set => SetProperty(ref _version, value);
        }

        public InstructionViewerViewModel InstructionViewModel => _instructionViewModel;

        public ObservableCollection<string> Instructions
        {
            get => _instructions;
            set => SetProperty(ref _instructions, value);
        }

        public ObservableCollection<NormativeInstructionItem> NormativeInstructions
        {
            get => _normativeInstructions;
            set => SetProperty(ref _normativeInstructions, value);
        }

        public ObservableCollection<PassedInstructionItem> PassedInstructions
        {
            get => _passedInstructions;
            set => SetProperty(ref _passedInstructions, value);
        }

        public ObservableCollection<string> RelatedFiles
        {
            get => _relatedFiles;
            set => SetProperty(ref _relatedFiles, value);
        }

        public bool IsInstructionSelected
        {
            get => _isInstructionSelected;
            set => SetProperty(ref _isInstructionSelected, value);
        }
        #endregion

        #region Initialization
        private void InitializeCollections()
        {
            Instructions = new ObservableCollection<string>();
            NormativeInstructions = new ObservableCollection<NormativeInstructionItem>();
            PassedInstructions = new ObservableCollection<PassedInstructionItem>();
            RelatedFiles = new ObservableCollection<string>();
        }

        private void InitializeWindow()
        {
            try
            {
                // Initialize instruction service
                _instructionService = new InstructionService(_jwtToken, new WpfLogger(this));

                // Initialize the ViewModel for the control
                _instructionViewModel = new InstructionViewerViewModel(_jwtToken, _userName, _isChief, _allowCompletion);

                // Set up data context - THIS IS CRITICAL
                DataContext = this;

                // IMPORTANT: Initialize the control with the ViewModel AFTER the window is loaded
                this.Loaded += (s, e) => {
                    if (InstructionViewerControl != null)
                    {
                        // Set the control's DataContext to the ViewModel
                        InstructionViewerControl.DataContext = _instructionViewModel;

                        // OR use the Initialize method if available:
                        // InstructionViewerControl.Initialize(_jwtToken, _userName, _isChief, _allowCompletion);
                    }
                };

                // Load initial data
                LoadInstructionsAsync();
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка инициализации: {ex.Message}");
            }
        }
        #endregion

        #region Event Handlers - Window Controls
        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadInstructionsAsync();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция настроек будет реализована в следующей версии", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // Handle window closing logic
            if (_loginForm?.activeForm == null) // User form handling
            {
                _forceClose = false;
            }

            if (!_forceClose)
            {
                e.Cancel = true;
                Hide();
                ShowInTaskbar = false;
                _forceClose = true;
            }
            else
            {
                base.OnClosing(e);
            }
        }
        #endregion

        #region Event Handlers - Management Functions (For Chiefs)

        private void ViewEmployeesButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция просмотра сотрудников будет реализована в следующей версии", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ReportsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция отчетов будет реализована в следующей версии", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SyncDataButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция синхронизации данных будет реализована в следующей версии", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SystemSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция настроек системы будет реализована в следующей версии", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DocumentManagementButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция управления документами будет реализована в следующей версии", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion

        #region Event Handlers - Instructions List
        private void InstructionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListBox listBox && listBox.SelectedItem is string selectedInstruction)
            {
                IsInstructionSelected = true;
                LoadNormativeInstructionsForSelected(selectedInstruction);
                LoadRelatedFilesForSelected(selectedInstruction);
            }
            else
            {
                IsInstructionSelected = false;
                NormativeInstructions.Clear();
                RelatedFiles.Clear();
            }
        }

        private void NormativeInstructionsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBox listBox && listBox.SelectedItem is NormativeInstructionItem item)
            {
                OpenUrl(item.Url);
            }
        }

        private void RelatedFilesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBox listBox && listBox.SelectedItem is string selectedText)
            {
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
        }

        private void PassInstructionButton_Click(object sender, RoutedEventArgs e)
        {
            // Delegate to the ViewModel's command
            _instructionViewModel?.PassInstructionCommand?.Execute(null);
        }
        #endregion

        #region Data Loading
        private async void LoadInstructionsAsync()
        {
            try
            {
                UpdateStatus("Загрузка инструктажей...");

                // Load instructions based on user type
                if (_isChief)
                {
                    await LoadInstructionsForChief();
                }
                else
                {
                    await LoadInstructionsForUser();
                }

                // Update UI collections
                await UpdateUICollections();

                UpdateStatus("Инструктажи загружены");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка загрузки: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task LoadInstructionsForChief()
        {
            // Load instructions for chief users using correct method names
            var notPassedResult = await _instructionService.GetNotPassedInstructionsAsync();
            var passedResult = await _instructionService.GetPassedInstructionsAsync();

            if (notPassedResult != null)
            {
                _listOfNewInstructions = notPassedResult.Value.Instructions;
                _normativeInstructionsOfNewInstr = notPassedResult.Value.NormativeInstructions;
            }

            if (passedResult != null)
            {
                _listOfOldInstructions = passedResult.Value.Instructions;
                _normativeInstructionsOfOldInstr = passedResult.Value.NormativeInstructions;
            }
        }

        private async System.Threading.Tasks.Task LoadInstructionsForUser()
        {
            // Load instructions for regular users using correct method names
            var notPassedResult = await _instructionService.GetNotPassedInstructionsAsync();

            if (notPassedResult != null)
            {
                _listOfNewInstructions = notPassedResult.Value.Instructions;
                _normativeInstructionsOfNewInstr = notPassedResult.Value.NormativeInstructions;
            }
        }

        private async System.Threading.Tasks.Task UpdateUICollections()
        {
            await Dispatcher.InvokeAsync(() =>
            {
                // Update instructions list
                Instructions.Clear();
                if (_listOfNewInstructions != null)
                {
                    foreach (var instruction in _listOfNewInstructions)
                    {
                        if (instruction.ContainsKey(dB_pos_users_causeOfInstruction) &&
                            instruction[dB_pos_users_causeOfInstruction] != null)
                        {
                            Instructions.Add(instruction[dB_pos_users_causeOfInstruction].ToString());
                        }
                    }
                }

                // Update passed instructions list
                PassedInstructions.Clear();
                if (_listOfOldInstructions != null)
                {
                    foreach (var instruction in _listOfOldInstructions)
                    {
                        var passedItem = new PassedInstructionItem
                        {
                            DatePassed = instruction.ContainsKey(db_dateOfInstructionWasSentToUser) ?
                                DateTime.Parse(instruction[db_dateOfInstructionWasSentToUser].ToString()).ToString("dd.MM.yyyy") : "N/A",
                            Type = instruction.ContainsKey(db_typeOfInstruction) ?
                                GetInstructionTypeText(Convert.ToByte(instruction[db_typeOfInstruction])) : "N/A",
                            Cause = instruction.ContainsKey(dB_pos_users_causeOfInstruction) ?
                                instruction[dB_pos_users_causeOfInstruction].ToString() : "N/A",
                            InstructionId = instruction.ContainsKey(dB_instructionId) ?
                                Convert.ToInt32(instruction[dB_instructionId]) : 0
                        };
                        PassedInstructions.Add(passedItem);
                    }
                }
            });
        }
        #endregion

        #region Helper Methods
        private void LoadNormativeInstructionsForSelected(string selectedInstruction)
        {
            try
            {
                NormativeInstructions.Clear();

                var instructionDict = GetDictFromSelectedInstruction(selectedInstruction);
                if (instructionDict == null) return;

                int instructionId = Convert.ToInt32(instructionDict[dB_instructionId]);
                var relatedNormatives = _normativeInstructionsOfNewInstr?
                    .Where(n => Convert.ToInt32(n[dB_instructionId]) == instructionId)
                    .ToList();

                if (relatedNormatives != null)
                {
                    foreach (var normative in relatedNormatives)
                    {
                        var item = new NormativeInstructionItem
                        {
                            Id = Convert.ToInt32(normative[db_normativeInstructionId]),
                            Name = normative[db_normativeInstructionName].ToString(),
                            Url = normative[db_normativeInstructionUrl].ToString()
                        };

                        // Check if this is unplanned and needs URL splitting
                        string instructionType = GetInstructionType(instructionId);
                        if (ShouldSplitUrls(instructionType, item.Url))
                        {
                            var splitItems = ProcessUnplannedNormativeInstructions(item.Url, item.Id);
                            foreach (var splitItem in splitItems)
                            {
                                NormativeInstructions.Add(splitItem);
                            }
                        }
                        else
                        {
                            NormativeInstructions.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading normative instructions: {ex.Message}");
            }
        }

        private void LoadRelatedFilesForSelected(string selectedInstruction)
        {
            try
            {
                RelatedFiles.Clear();

                var instructionDict = GetDictFromSelectedInstruction(selectedInstruction);
                if (instructionDict == null) return;

                // Add logic to load related files based on instruction
                // This would depend on your specific file storage structure
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading related files: {ex.Message}");
            }
        }

        private Dictionary<string, object> GetDictFromSelectedInstruction(string selectedInstruction)
        {
            return _listOfNewInstructions?.FirstOrDefault(dict =>
                dict.ContainsKey(dB_pos_users_causeOfInstruction) &&
                dict[dB_pos_users_causeOfInstruction]?.ToString() == selectedInstruction);
        }

        private string GetInstructionType(int instructionId)
        {
            try
            {
                // First check in passed instructions
                var instruction = _listOfOldInstructions?.FirstOrDefault(dict =>
                    dict.ContainsKey(dB_instructionId) &&
                    Convert.ToInt32(dict[dB_instructionId]) == instructionId);

                if (instruction != null && instruction.ContainsKey(db_typeOfInstruction))
                {
                    return instruction[db_typeOfInstruction]?.ToString() ?? "";
                }

                // If not found in passed instructions, check in new instructions
                instruction = _listOfNewInstructions?.FirstOrDefault(dict =>
                    dict.ContainsKey(dB_instructionId) &&
                    Convert.ToInt32(dict[dB_instructionId]) == instructionId);

                if (instruction != null && instruction.ContainsKey(db_typeOfInstruction))
                {
                    return instruction[db_typeOfInstruction]?.ToString() ?? "";
                }

                return "";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting instruction type: {ex.Message}");
                return "";
            }
        }

        private string GetInstructionTypeText(byte typeValue)
        {
            // Convert numeric type to readable text
            return typeValue switch
            {
                1 => "Плановый",
                2 => "Внеплановый",
                3 => "Целевой",
                _ => "Неизвестный"
            };
        }

        private void OpenUrl(string url)
        {
            try
            {
                if (!string.IsNullOrEmpty(url))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось открыть URL: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Normative Instructions Processing
        /// <summary>
        /// Processes unplanned instruction URLs, splitting them by "|" separator and giving them sequential names
        /// </summary>
        /// <param name="combinedUrl">Combined URLs separated by "|"</param>
        /// <param name="normativeId">The normative instruction ID</param>
        /// <returns>List of NormativeInstructionItem objects</returns>
        private List<NormativeInstructionItem> ProcessUnplannedNormativeInstructions(string combinedUrl, int normativeId)
        {
            var result = new List<NormativeInstructionItem>();

            if (string.IsNullOrEmpty(combinedUrl))
                return result;

            // Split URLs by "|" separator
            var urlParts = combinedUrl.Split(new[] { " | " }, StringSplitOptions.RemoveEmptyEntries)
                                     .Select(u => u.Trim())
                                     .Where(u => !string.IsNullOrEmpty(u))
                                     .ToArray();

            // Create individual normative instruction items with sequential names
            for (int i = 0; i < urlParts.Length; i++)
            {
                result.Add(new NormativeInstructionItem
                {
                    Id = normativeId + i,
                    Name = $"Нормативный документ {i + 1}",
                    Url = urlParts[i]
                });
            }

            return result;
        }

        /// <summary>
        /// Checks if the instruction is unplanned and has multiple URLs that need to be split
        /// </summary>
        /// <param name="instructionType">The type of instruction</param>
        /// <param name="url">The URL string to check</param>
        /// <returns>True if URLs should be split, false otherwise</returns>
        private bool ShouldSplitUrls(string instructionType, string url)
        {
            return instructionType == "Внеплановый" && !string.IsNullOrEmpty(url) && url.Contains(" | ");
        }
        #endregion

        #region UI Updates
        private void UpdateStatus(string message)
        {
            // Update status bar or status text
            Dispatcher.Invoke(() =>
            {
                StatusMessage = message;
                Console.WriteLine($"Status: {message}");
            });
        }
        #endregion

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

        #region Logger Implementation
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
                _window.UpdateStatus(message);
            }

            public void LogError(string message, Exception ex = null)
            {
                string errorMessage = ex != null ? $"{message}: {ex.Message}" : message;
                Console.WriteLine($"ERROR: {errorMessage}");
                _window.UpdateStatus($"Ошибка: {message}");
            }
        }
        #endregion

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, value))
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}