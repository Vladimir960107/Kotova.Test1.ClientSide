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

namespace Kotova.Test1.ClientSide.InstructionControlWPF
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
        private string _statusMessage = "Готов к работе";
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
                OnInstructionSelectionChanged(value);
            }
        }

        public PassedInstructionItem SelectedPassedInstruction
        {
            get => _selectedPassedInstruction;
            set
            {
                _selectedPassedInstruction = value;
                OnPropertyChanged();
                OnPassedInstructionSelectionChanged(value);
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public ICommand PassInstructionCommand { get; private set; }
        public ICommand OpenNormativeDocumentCommand { get; private set; }
        public ICommand OpenRelatedFileCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }
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
                canExecute: () => IsInstructionPassed && AllowCompletion);

            OpenNormativeDocumentCommand = new RelayCommand(
                execute: () => OpenNormativeDocument(),
                canExecute: () => true);

            OpenRelatedFileCommand = new RelayCommand(
                execute: () => OpenRelatedFile(),
                canExecute: () => true);

            RefreshCommand = new RelayCommand(
                execute: async () => await RefreshInstructionsAsync(),
                canExecute: () => true);
        }
        #endregion

        #region Data Loading
        public async Task LoadInstructionsAsync()
        {
            UpdateStatus("Загрузка инструктажей...");
            await RefreshNewInstructionsAsync();
            await RefreshOldInstructionsAsync();
            UpdateStatus("Готов к работе");
        }

        private async Task RefreshNewInstructionsAsync()
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Instructions.Clear();
                    NormativeInstructions.Clear();
                });

                var result = await _instructionService.GetNotPassedInstructionsAsync();

                if (result == null)
                {
                    MessageBox.Show("Все инструктажи пройдены!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Update the global lists
                _listOfNewInstructions = result.Value.Instructions;
                _normativeInstructionsOfNewInstr = result.Value.NormativeInstructions;

                // Update the UI
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var instruction in _listOfNewInstructions)
                    {
                        if (instruction.ContainsKey("cause_of_instruction") && instruction["cause_of_instruction"] != null)
                        {
                            Instructions.Add(instruction["cause_of_instruction"].ToString());
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in RefreshNewInstructionsAsync: {ex}");
                MessageBox.Show($"Ошибка при обновлении инструктажей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task RefreshOldInstructionsAsync()
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    PassedInstructions.Clear();
                });

                var result = await _instructionService.GetPassedInstructionsAsync();

                if (result == null)
                {
                    return;
                }

                // Update the global lists
                _listOfOldInstructions = result.Value.Instructions;
                _normativeInstructionsOfOldInstr = result.Value.NormativeInstructions;

                // Update the UI
                Application.Current.Dispatcher.Invoke(() =>
                {
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
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in RefreshOldInstructionsAsync: {ex}");
                MessageBox.Show($"Ошибка при обновлении пройденных инструктажей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task RefreshInstructionsAsync()
        {
            UpdateStatus("Обновление...");
            await RefreshNewInstructionsAsync();
            await RefreshOldInstructionsAsync();
            UpdateStatus("Обновление завершено");
        }
        #endregion

        #region Selection Handling
        private void OnInstructionSelectionChanged(string selectedInstruction)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                NormativeInstructions.Clear();
                IsInstructionSelected = false;
                IsInstructionPassed = false;

                // Reset tracking for new instruction
                _clickedNormativeIds.Clear();
                _totalNormativeLinksForCurrentInstruction = 0;
            });

            if (string.IsNullOrEmpty(selectedInstruction))
                return;

            var selectedDict = GetDictFromSelectedInstruction(selectedInstruction);

            if (selectedDict == null)
                return;

            int instructionId = Convert.ToInt32(selectedDict[dB_instructionId].ToString());
            string instructionType = selectedDict[db_typeOfInstruction].ToString();

            // Check if this is an introductory instruction (type 0)
            if (instructionType == "0")
            {
                IsInstructionSelected = true;
                UpdatePassInstructionState();
                return;
            }

            // Load normative instructions and count them
            bool hasNormativeInstructions = false;
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var normativeInstruction in _normativeInstructionsOfNewInstr)
                {
                    if (Convert.ToInt32(normativeInstruction[dB_instructionId].ToString()) == instructionId)
                    {
                        string name = normativeInstruction[db_normativeInstructionName]?.ToString() ?? "";
                        string url = normativeInstruction[db_normativeInstructionUrl]?.ToString() ?? "";
                        int normativeId = Convert.ToInt32(normativeInstruction[db_normativeInstructionId]);

                        // Check if this is an unplanned instruction with multiple URLs
                        if (ShouldSplitUnplannedUrls(instructionType, url))
                        {
                            // Process unplanned instruction with multiple URLs
                            var splitItems = ProcessUnplannedNormativeInstructions(url, normativeId);
                            foreach (var item in splitItems)
                            {
                                NormativeInstructions.Add(item);
                                _totalNormativeLinksForCurrentInstruction++;
                            }
                        }
                        else
                        {
                            // For regular instructions or unplanned instructions with single URL
                            NormativeInstructions.Add(new NormativeInstructionItem
                            {
                                Id = normativeId,
                                Name = name,
                                Url = url
                            });
                            _totalNormativeLinksForCurrentInstruction++;
                        }

                        hasNormativeInstructions = true;
                    }
                }
            });

            if (hasNormativeInstructions)
            {
                IsInstructionSelected = true;
                UpdatePassInstructionState();

                // Show initial status
                UpdateStatus($"Необходимо просмотреть все нормативные документы: 0/{_totalNormativeLinksForCurrentInstruction}");
            }
        }

        private void OnPassedInstructionSelectionChanged(PassedInstructionItem selectedItem)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                RelatedFiles.Clear();
            });

            if (selectedItem == null)
                return;

            // Load related files for the selected passed instruction
            int instructionId = selectedItem.InstructionId;
            LoadRelatedFilesForInstruction(instructionId);
        }
        #endregion

        #region Unplanned Instruction Processing
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
        #endregion

        #region Commands Implementation
        private async Task PassInstructionAsync()
        {
            if (!IsInstructionSelected || string.IsNullOrEmpty(SelectedInstruction))
                return;

            try
            {
                var selectedDict = GetDictFromSelectedInstruction(SelectedInstruction);

                if (selectedDict == null)
                    return;

                int instructionId = Convert.ToInt32(selectedDict[dB_instructionId].ToString());

                UpdateStatus("Отправка данных...");

                bool success = await _instructionService.MarkInstructionAsPassedAsync(instructionId);

                if (success)
                {
                    MessageBox.Show("Инструктаж успешно отмечен как пройденный!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    await RefreshNewInstructionsAsync();
                    await RefreshOldInstructionsAsync();
                    UpdateStatus("Инструктаж отмечен как пройденный");

                    // Clear selection
                    SelectedInstruction = null;
                    IsInstructionPassed = false;
                    _clickedNormativeIds.Clear();
                }
                else
                {
                    MessageBox.Show("Ошибка при отправке данных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    UpdateStatus("Ошибка при отправке");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                UpdateStatus("Ошибка");
            }
        }

        public void OpenNormativeDocument()
        {
            try
            {
                // This method is called from the UI after MarkNormativeAsAccessed
                // So we just need to open the most recently accessed document
                // The UI handler already calls MarkNormativeAsAccessed before this

                // For now, just show a message since the URL opening is handled by the UI
                // The actual URL opening happens in the Control's double-click handler
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии документа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void OpenNormativeDocumentByItem(NormativeInstructionItem item)
        {
            try
            {
                if (item != null && !string.IsNullOrEmpty(item.Url))
                {
                    // Track that this normative instruction was clicked
                    _clickedNormativeIds.Add(item.Id);

                    // Update pass instruction state
                    UpdatePassInstructionState();

                    // Open the URL
                    OpenUrl(item.Url);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии документа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void OpenRelatedFile()
        {
            try
            {
                // Get selected related file from RelatedFiles collection
                var selectedFile = RelatedFiles.FirstOrDefault(); // You can modify this to get actual selection
                if (!string.IsNullOrEmpty(selectedFile))
                {
                    // Extract URL if the format is "Name (URL)"
                    string url = selectedFile;
                    int urlStart = selectedFile.LastIndexOf('(');
                    int urlEnd = selectedFile.LastIndexOf(')');

                    if (urlStart > 0 && urlEnd > urlStart)
                    {
                        url = selectedFile.Substring(urlStart + 1, urlEnd - urlStart - 1);
                    }

                    OpenUrl(url);
                }
                else
                {
                    MessageBox.Show("Нет связанных файлов для выбранного инструктажа", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии файла: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

        #region Pass Instruction State Management
        private void UpdatePassInstructionState()
        {
            if (!IsInstructionSelected)
            {
                IsInstructionPassed = false;
                return;
            }

            // For introductory instructions (type 0), enable immediately
            if (!string.IsNullOrEmpty(SelectedInstruction))
            {
                var selectedDict = GetDictFromSelectedInstruction(SelectedInstruction);

                if (selectedDict != null)
                {
                    string instructionType = selectedDict[db_typeOfInstruction].ToString();
                    if (instructionType == "0")
                    {
                        // Introductory instruction - enable immediately
                        IsInstructionPassed = IsChief ? CanChiefCompleteInstruction() : true;
                        return;
                    }
                }
            }

            // For other instructions, check if all normative links have been clicked
            bool allLinksClicked = _clickedNormativeIds.Count >= _totalNormativeLinksForCurrentInstruction;

            if (allLinksClicked)
            {
                IsInstructionPassed = IsChief ? CanChiefCompleteInstruction() : true;

                // Update UI to show completion status
                UpdateStatus($"Все нормативные документы просмотрены ({_clickedNormativeIds.Count}/{_totalNormativeLinksForCurrentInstruction})");
            }
            else
            {
                IsInstructionPassed = false;
                UpdateStatus($"Просмотрено документов: {_clickedNormativeIds.Count}/{_totalNormativeLinksForCurrentInstruction}. Нажмите на все ссылки для продолжения.");
            }
        }

        public void MarkNormativeAsAccessed(int normativeId)
        {
            _clickedNormativeIds.Add(normativeId);
            UpdatePassInstructionState();
        }
        #endregion

        #region Helper Methods
        private Dictionary<string, object> GetDictFromSelectedInstruction(string selectedInstruction)
        {
            return _listOfNewInstructions?.FirstOrDefault(dict =>
                dict.ContainsKey("cause_of_instruction") &&
                dict["cause_of_instruction"]?.ToString() == selectedInstruction);
        }

        private bool CanChiefCompleteInstruction()
        {
            return _allowCompletion;
        }

        private void LoadRelatedFilesForInstruction(int instructionId)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    RelatedFiles.Clear();
                });

                var relatedInstructions = _normativeInstructionsOfOldInstr?
                    .Where(dict => dict.ContainsKey(dB_instructionId) &&
                                   Convert.ToInt32(dict[dB_instructionId]) == instructionId)
                    .ToList();

                if (relatedInstructions != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
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
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading related files: {ex.Message}");
            }
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

        public void RefreshInstructions()
        {
            LoadInstructionsAsync();
        }

        private void UpdateStatus(string message)
        {
            StatusMessage = message;
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
        public class ViewModelLogger : InstructionService.ILogger
        {
            private readonly InstructionViewerViewModel _viewModel;

            public ViewModelLogger(InstructionViewerViewModel viewModel)
            {
                _viewModel = viewModel;
            }

            public void LogInfo(string message)
            {
                Console.WriteLine($"INFO: {message}");
                _viewModel.UpdateStatus(message);
            }

            public void LogError(string message, Exception ex = null)
            {
                string errorMessage = ex != null ? $"{message}: {ex.Message}" : message;
                Console.WriteLine($"ERROR: {errorMessage}");
                _viewModel.UpdateStatus($"Ошибка: {message}");
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