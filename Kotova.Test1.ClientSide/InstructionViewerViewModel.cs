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
using System.Windows.Input;
using Kotova.CommonClasses;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace Kotova.Test1.ClientSide.ManagementWPF.ViewModels
{
    public class InstructionViewerViewModel : INotifyPropertyChanged
    {
        #region Private Fields
        private InstructionService _instructionService;
        private string _jwtToken;
        private string _userName;
        private bool _isChief;
        private bool _allowCompletion;
        private bool _isInstructionSelected;
        private bool _isInstructionPassed;
        private string _selectedInstruction;
        private PassedInstructionItem _selectedPassedInstruction;
        private List<Dictionary<string, object>> _normativeInstructionsOfNewInstr;
        private List<Dictionary<string, object>> _normativeInstructionsOfOldInstr;
        private List<Dictionary<string, object>> _listOfNewInstructions;
        private List<Dictionary<string, object>> _listOfOldInstructions;
        private HashSet<int> _clickedNormativeIds = new HashSet<int>();
        private int _totalNormativeLinksForCurrentInstruction = 0;
        #endregion

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

        #region Observable Collections
        public ObservableCollection<string> Instructions { get; set; }
        public ObservableCollection<NormativeInstructionItem> NormativeInstructions { get; set; }
        public ObservableCollection<PassedInstructionItem> PassedInstructions { get; set; }
        public ObservableCollection<string> RelatedFiles { get; set; }
        #endregion

        #region Properties
        public string JwtToken
        {
            get => _jwtToken;
            set
            {
                _jwtToken = value;
                OnPropertyChanged();
                if (!string.IsNullOrEmpty(value))
                {
                    _instructionService = new InstructionService(value, new ViewModelLogger(this));
                }
            }
        }

        public string UserName
        {
            get => _userName;
            set
            {
                _userName = value;
                OnPropertyChanged();
            }
        }

        public bool IsChief
        {
            get => _isChief;
            set
            {
                _isChief = value;
                OnPropertyChanged();
            }
        }

        public bool AllowCompletion
        {
            get => _allowCompletion;
            set
            {
                _allowCompletion = value;
                OnPropertyChanged();
            }
        }

        public bool IsInstructionSelected
        {
            get => _isInstructionSelected;
            set
            {
                _isInstructionSelected = value;
                OnPropertyChanged();
            }
        }

        public bool IsInstructionPassed
        {
            get => _isInstructionPassed;
            set
            {
                _isInstructionPassed = value;
                OnPropertyChanged();
            }
        }

        public string SelectedInstruction
        {
            get => _selectedInstruction;
            set
            {
                _selectedInstruction = value;
                OnPropertyChanged();
                IsInstructionSelected = !string.IsNullOrEmpty(value);
                LoadNormativeInstructionsForSelectedInstruction();
            }
        }

        public ICommand PassInstructionCommand { get; private set; }
        public ICommand OpenNormativeDocumentCommand { get; private set; }
        public ICommand OpenRelatedFileCommand { get; private set; }
        #endregion

        #region Constructor
        public InstructionViewerViewModel(string jwtToken, string userName, bool isChief = false, bool allowCompletion = false)
        {
            _jwtToken = jwtToken;
            _userName = userName;
            _isChief = isChief;
            _allowCompletion = allowCompletion;

            InitializeCollections();
            InitializeCommands();

            if (!string.IsNullOrEmpty(jwtToken))
            {
                _instructionService = new InstructionService(jwtToken, new ViewModelLogger(this));
                LoadInstructionsAsync();
            }
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

        private void InitializeCommands()
        {
            PassInstructionCommand = new RelayCommand(
                execute: async () => await PassInstructionAsync(),
                canExecute: () => IsInstructionSelected && AllowCompletion);

            OpenNormativeDocumentCommand = new RelayCommand(
                execute: () => OpenNormativeDocument(),
                canExecute: () => true);

            OpenRelatedFileCommand = new RelayCommand(
                execute: () => OpenRelatedFile(),
                canExecute: () => true);
        }
        #endregion

        #region Data Loading
        private async Task LoadInstructionsAsync()
        {
            try
            {
                await LoadNotPassedInstructionsAsync();
                await LoadPassedInstructionsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке инструктажей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadNotPassedInstructionsAsync()
        {
            try
            {
                // GetNotPassedInstructionsAsync returns a tuple, not an object with IsSuccess/Data
                var instructionsResult = await _instructionService.GetNotPassedInstructionsAsync();
                if (instructionsResult != null) // Changed from instructionsResult.IsSuccess
                {
                    _listOfNewInstructions = instructionsResult.Value.Instructions; // Access tuple's Instructions property

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Instructions.Clear();
                        foreach (var instruction in _listOfNewInstructions)
                        {
                            if (instruction.ContainsKey("cause_of_instruction"))
                            {
                                Instructions.Add(instruction["cause_of_instruction"].ToString());
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке непройденных инструктажей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadPassedInstructionsAsync()
        {
            try
            {
                // GetPassedInstructionsAsync returns a tuple, not an object with IsSuccess/Data
                var instructionsResult = await _instructionService.GetPassedInstructionsAsync();
                if (instructionsResult != null) // Changed from instructionsResult.IsSuccess
                {
                    _listOfOldInstructions = instructionsResult.Value.Instructions; // Access tuple's Instructions property

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        PassedInstructions.Clear();
                        foreach (var instruction in _listOfOldInstructions)
                        {
                            var passedItem = new PassedInstructionItem
                            {
                                DatePassed = instruction.ContainsKey("when_was_send_to_user") ?
                                    DateTime.Parse(instruction["when_was_send_to_user"].ToString()).ToString("dd.MM.yyyy") : "N/A",
                                Type = instruction.ContainsKey("type_of_instruction") ?
                                    GetInstructionTypeText(Convert.ToByte(instruction["type_of_instruction"])) : "N/A",
                                Cause = instruction.ContainsKey("cause_of_instruction") ?
                                    instruction["cause_of_instruction"].ToString() : "N/A",
                                InstructionId = instruction.ContainsKey("instruction_id") ?
                                    Convert.ToInt32(instruction["instruction_id"]) : 0
                            };
                            PassedInstructions.Add(passedItem);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке пройденных инструктажей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadNormativeInstructionsForSelectedInstruction()
        {
            if (string.IsNullOrEmpty(SelectedInstruction) || _listOfNewInstructions == null)
                return;

            var selectedInstruction = _listOfNewInstructions.FirstOrDefault(i =>
                i.ContainsKey("cause_of_instruction") && i["cause_of_instruction"].ToString() == SelectedInstruction);

            if (selectedInstruction != null)
            {
                LoadNormativeInstructionsAsync(selectedInstruction);
            }
        }

        private async Task LoadNormativeInstructionsAsync(Dictionary<string, object> instruction)
        {
            try
            {
                int instructionId = instruction.ContainsKey("instruction_id") ?
                    Convert.ToInt32(instruction["instruction_id"]) : 0;

                string instructionType = instruction.ContainsKey("type_of_instruction") ?
                    instruction["type_of_instruction"].ToString() : "0";

                // Check if this is an introductory instruction (type 0)
                if (instructionType == "0")
                {
                    // No normative instructions needed for introductory instructions
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        NormativeInstructions.Clear();
                    });
                    _totalNormativeLinksForCurrentInstruction = 0;
                    return;
                }

                // Load normative instructions from the data already fetched
                if (_normativeInstructionsOfNewInstr != null)
                {
                    _totalNormativeLinksForCurrentInstruction = 0;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        NormativeInstructions.Clear();

                        foreach (var normativeInstruction in _normativeInstructionsOfNewInstr)
                        {
                            if (Convert.ToInt32(normativeInstruction[dB_instructionId].ToString()) == instructionId)
                            {
                                string name = normativeInstruction[db_normativeInstructionName]?.ToString() ?? "";
                                string url = normativeInstruction[db_normativeInstructionUrl]?.ToString() ?? "";
                                int normativeId = Convert.ToInt32(normativeInstruction[db_normativeInstructionId].ToString());

                                // Check if this is an unplanned instruction with multiple URLs that need to be split
                                if (ShouldSplitUnplannedUrls(instructionType, url))
                                {
                                    var splitItems = ProcessUnplannedNormativeInstructions(url, normativeId);
                                    foreach (var item in splitItems)
                                    {
                                        NormativeInstructions.Add(item);
                                        _totalNormativeLinksForCurrentInstruction++;
                                    }
                                }
                                else
                                {
                                    // For regular instructions
                                    var normativeItem = new NormativeInstructionItem
                                    {
                                        Id = normativeId,
                                        Name = string.IsNullOrEmpty(name) ? "Нормативный документ" : name,
                                        Url = url
                                    };
                                    NormativeInstructions.Add(normativeItem);
                                    _totalNormativeLinksForCurrentInstruction++;
                                }
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке нормативных документов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadRelatedFilesAsync(Dictionary<string, object> instruction)
        {
            try
            {
                int instructionId = instruction.ContainsKey("instruction_id") ?
                    Convert.ToInt32(instruction["instruction_id"]) : 0;

                // Load related files from the data already fetched (don't call MarkInstructionAsPassedAsync)
                if (_normativeInstructionsOfOldInstr != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        RelatedFiles.Clear();

                        var relatedInstructions = _normativeInstructionsOfOldInstr
                            .Where(dict => dict.ContainsKey(dB_instructionId) &&
                                           Convert.ToInt32(dict[dB_instructionId]) == instructionId)
                            .ToList();

                        if (relatedInstructions != null)
                        {
                            foreach (var dict in relatedInstructions)
                            {
                                string name = dict[db_normativeInstructionName]?.ToString() ?? "";
                                string url = dict[db_normativeInstructionUrl]?.ToString() ?? "";

                                // Get the instruction type to check if it's unplanned
                                var instructionType = GetInstructionTypeForId(instructionId);

                                // Check if this is an unplanned instruction with multiple URLs that need to be split
                                if (ShouldSplitUnplannedUrls(instructionType, url))
                                {
                                    // Split URLs by "|" separator for unplanned instructions
                                    var urlParts = url.Split(new[] { " | " }, StringSplitOptions.RemoveEmptyEntries)
                                                     .Select(u => u.Trim())
                                                     .Where(u => !string.IsNullOrEmpty(u))
                                                     .ToArray();

                                    // Create individual entries with sequential names
                                    for (int i = 0; i < urlParts.Length; i++)
                                    {
                                        RelatedFiles.Add($"Нормативный документ {i + 1} ({urlParts[i]})");
                                    }
                                }
                                else
                                {
                                    // For regular instructions or unplanned instructions with single URL
                                    RelatedFiles.Add($"{name} ({url})");
                                }
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке связанных файлов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        /// <summary>
        /// Processes unplanned instruction URLs, splitting them by "|" separator and giving them sequential names
        /// </summary>
        /// <param name="combinedUrl">Combined URLs separated by "|"</param>
        /// <param name="normativeId">The normative instruction ID</param>
        /// <returns>List of NormativeInstructionItem objects</returns>
        private List<NormativeInstructionItem> ProcessUnplannedNormativeInstructions(string combinedUrl, int normativeId)
        {
            var result = new List<NormativeInstructionItem>();

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
                    Id = normativeId + i, // Give each split item a unique ID for tracking
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
        private bool ShouldSplitUnplannedUrls(string instructionType, string url)
        {
            return instructionType == "1" && !string.IsNullOrEmpty(url) && url.Contains(" | ");
        }

        /// <summary>
        /// Gets the instruction type for a given instruction ID
        /// </summary>
        /// <param name="instructionId">The instruction ID</param>
        /// <returns>The instruction type as string</returns>
        private string GetInstructionTypeForId(int instructionId)
        {
            try
            {
                // Look for the instruction in the passed instructions list
                var instruction = _listOfOldInstructions?.FirstOrDefault(dict =>
                    dict.ContainsKey(dB_instructionId) &&
                    Convert.ToInt32(dict[dB_instructionId]) == instructionId);

                if (instruction != null && instruction.ContainsKey(db_typeOfInstruction))
                {
                    return instruction[db_typeOfInstruction]?.ToString() ?? "0";
                }

                return "0"; // Default to introductory if not found
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting instruction type: {ex.Message}");
                return "0";
            }
        }

        // Add helper method for getting selected instruction dictionary
        private Dictionary<string, object> GetDictFromSelectedInstruction(string selectedInstruction)
        {
            return _listOfNewInstructions?.FirstOrDefault(dict =>
                dict.ContainsKey("cause_of_instruction") &&
                dict["cause_of_instruction"]?.ToString() == selectedInstruction);
        }


        #region Command Methods
        private async Task PassInstructionAsync()
        {
            if (!IsInstructionSelected || string.IsNullOrEmpty(SelectedInstruction))
                return;

            try
            {
                var selectedInstruction = _listOfNewInstructions.FirstOrDefault(i =>
                    i.ContainsKey("cause_of_instruction") && i["cause_of_instruction"].ToString() == SelectedInstruction);

                if (selectedInstruction == null)
                    return;

                int instructionId = selectedInstruction.ContainsKey("instruction_id") ?
                    Convert.ToInt32(selectedInstruction["instruction_id"]) : 0;

                // MarkInstructionAsPassedAsync returns bool, not an object with IsSuccess
                bool result = await _instructionService.MarkInstructionAsPassedAsync(instructionId);
                if (result) // Changed from result.IsSuccess to just result
                {
                    MessageBox.Show("Инструктаж успешно пройден!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Refresh the instructions
                    await LoadInstructionsAsync();

                    // Clear selection
                    SelectedInstruction = null;
                    IsInstructionPassed = false;
                }
                else
                {
                    // Removed result.ErrorMessage since bool doesn't have ErrorMessage property
                    MessageBox.Show("Ошибка при прохождении инструктажа", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при прохождении инструктажа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void OpenNormativeDocument()
        {
            // Implementation for opening normative document
            // This would typically open a URL or file
        }

        public void OpenRelatedFile()
        {
            // Implementation for opening related file
            // This would typically open a URL or file
        }
        #endregion

        #region Helper Methods
        private string GetInstructionTypeText(byte typeOfInstruction)
        {
            return typeOfInstruction switch
            {
                1 => "Вводный",
                2 => "Первичный",
                3 => "Повторный",
                4 => "Внеплановый",
                5 => "Целевой",
                _ => "Неизвестный"
            };
        }
        #endregion

        #region Event Handling
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
        private class ViewModelLogger : InstructionService.ILogger
        {
            private readonly InstructionViewerViewModel _viewModel;

            public ViewModelLogger(InstructionViewerViewModel viewModel)
            {
                _viewModel = viewModel;
            }

            public void LogInfo(string message)
            {
                Console.WriteLine($"INFO: {message}");
                // You can add status updates here if needed
            }

            public void LogError(string message, Exception ex = null)
            {
                string errorMessage = ex != null ? $"{message}: {ex.Message}" : message;
                Console.WriteLine($"ERROR: {errorMessage}");
                // You can add error handling here if needed
            }
        }
        #endregion
    }

    #region RelayCommand Implementation
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }

        public void Execute(object parameter)
        {
            _execute();
        }
    }
    #endregion
}