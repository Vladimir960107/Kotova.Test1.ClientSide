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

        public PassedInstructionItem SelectedPassedInstruction
        {
            get => _selectedPassedInstruction;
            set
            {
                _selectedPassedInstruction = value;
                OnPropertyChanged();
                LoadRelatedFilesForSelectedInstruction();
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
                var instructionsResult = await _instructionService.GetNotPassedInstructionsAsync();
                if (instructionsResult.IsSuccess)
                {
                    _listOfNewInstructions = instructionsResult.Data;

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
                var instructionsResult = await _instructionService.GetPassedInstructionsAsync();
                if (instructionsResult.IsSuccess)
                {
                    _listOfOldInstructions = instructionsResult.Data;

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

                var normativeResult = await _instructionService.MarkInstructionAsPassedAsync(instructionId);
                if (normativeResult)
                {
                    _normativeInstructionsOfNewInstr = normativeResult.Data;
                    _totalNormativeLinksForCurrentInstruction = _normativeInstructionsOfNewInstr.Count;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        NormativeInstructions.Clear();
                        foreach (var normative in _normativeInstructionsOfNewInstr)
                        {
                            var normativeItem = new NormativeInstructionItem
                            {
                                Id = normative.ContainsKey("id") ? Convert.ToInt32(normative["id"]) : 0,
                                Name = normative.ContainsKey("name") ? normative["name"].ToString() : "N/A",
                                Url = normative.ContainsKey("url") ? normative["url"].ToString() : "N/A"
                            };
                            NormativeInstructions.Add(normativeItem);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке нормативных документов: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadRelatedFilesForSelectedInstruction()
        {
            if (SelectedPassedInstruction == null || _listOfOldInstructions == null)
                return;

            var selectedInstruction = _listOfOldInstructions.FirstOrDefault(i =>
                i.ContainsKey("instruction_id") && Convert.ToInt32(i["instruction_id"]) == SelectedPassedInstruction.InstructionId);

            if (selectedInstruction != null)
            {
                LoadRelatedFilesAsync(selectedInstruction);
            }
        }

        private async Task LoadRelatedFilesAsync(Dictionary<string, object> instruction)
        {
            try
            {
                int instructionId = instruction.ContainsKey("instruction_id") ?
                    Convert.ToInt32(instruction["instruction_id"]) : 0;

                var normativeResult = await _instructionService.MarkInstructionAsPassedAsync(instructionId);
                if (normativeResult.IsSuccess)
                {
                    _normativeInstructionsOfOldInstr = normativeResult.Data;

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        RelatedFiles.Clear();
                        foreach (var normative in _normativeInstructionsOfOldInstr)
                        {
                            if (normative.ContainsKey("url"))
                            {
                                RelatedFiles.Add(normative["url"].ToString());
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

                var result = await _instructionService.MarkInstructionAsPassedAsync(instructionId);
                if (result.IsSuccess)
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
                    MessageBox.Show($"Ошибка при прохождении инструктажа: {result.ErrorMessage}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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