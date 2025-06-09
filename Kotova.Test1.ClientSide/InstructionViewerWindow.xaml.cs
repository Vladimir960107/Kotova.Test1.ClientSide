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
        private bool _isInstructionSelected = false;
        private List<Dictionary<string, object>> _normativeInstructionsOfNewInstr;
        private List<Dictionary<string, object>> _normativeInstructionsOfOldInstr;
        private List<Dictionary<string, object>> _listOfNewInstructions;
        private List<Dictionary<string, object>> _listOfOldInstructions;
        private string _jwtToken;
        private bool _isChief;
        private string _userName;

        // References to main application forms (for context menu functionality)
        private Login_Russian _loginForm;
        private SignUpForm _signUpForm;
        #endregion

        #region Observable Collections
        public ObservableCollection<string> Instructions { get; set; }
        public ObservableCollection<NormativeInstructionItem> NormativeInstructions { get; set; }
        public ObservableCollection<PassedInstructionItem> PassedInstructions { get; set; }
        public ObservableCollection<string> RelatedFiles { get; set; }
        #endregion

        #region Constructor
        public InstructionViewerWindow(string jwtToken, string userName, bool isChief = false, bool allowCompletion = false, Login_Russian loginForm = null, SignUpForm signUpForm = null)
        {
            _jwtToken = jwtToken;
            _isChief = isChief;
            _userName = userName;
            _allowCompletion = allowCompletion;
            _instructionService = new InstructionService(jwtToken, new WpfLogger(this));
            _loginForm = loginForm;
            _signUpForm = signUpForm;

            InitializeComponent();
            InitializeCollections();
            InitializeInterface();
            LoadInstructionsAsync();

            // Create SignUpForm if not provided and loginForm is available
            if (_signUpForm == null && _loginForm != null)
            {
                // Note: SignUpForm expects a Form as second parameter, but we're passing a WPF Window
                // This might need adaptation or we could create a WPF-based credential change dialog
                try
                {
                    // For now, we'll create it with loginForm and this window
                    // This may require SignUpForm constructor modification
                    _signUpForm = new SignUpForm(_loginForm, this);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not create SignUpForm: {ex.Message}");
                    // SignUpForm will remain null, so we'll handle it in the settings menu
                }
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Sets the SignUpForm reference for the settings menu
        /// </summary>
        /// <param name="signUpForm">The SignUpForm instance</param>
        public void SetSignUpForm(SignUpForm signUpForm)
        {
            _signUpForm = signUpForm;
        }
        #endregion

        #region Initialization Methods
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

        private void InitializeInterface()
        {
            // Set username
            UsernameTextBlock.Text = _userName;

            // Set version information
            try
            {
                VersionTextBlock.Text = ConfigurationClass.BASE_VERSION ?? "v1.0.0";
            }
            catch
            {
                VersionTextBlock.Text = "v1.0.0";
            }

            // Show management tab for chiefs
            if (_isChief)
            {
                ManagementTab.Visibility = Visibility.Visible;
            }

            // Handle checkbox enabling logic
            if (_isChief && !_allowCompletion)
            {
                PassInstructionCheckBox.IsEnabled = false;
                PassInstructionCheckBox.ToolTip = "Руководители не могут отмечать инструктажи как пройденные для себя";
            }

            // Set initial status
            UpdateStatus("Готов к работе");
        }

        private async void LoadInstructionsAsync()
        {
            UpdateStatus("Загрузка инструктажей...");
            await RefreshNewInstructionsAsync();
            await RefreshOldInstructionsAsync();
            UpdateStatus("Готов к работе");
        }
        #endregion

        #region Status Management
        private void UpdateStatus(string message)
        {
            Dispatcher.Invoke(() =>
            {
                StatusTextBlock.Text = message;
            });
        }
        #endregion

        #region Data Refresh Methods
        private async Task RefreshNewInstructionsAsync()
        {
            try
            {
                Instructions.Clear();
                NormativeInstructions.Clear();

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
                MessageBox.Show($"Ошибка при обновлении инструктажей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show($"Ошибка при обновлении пройденных инструктажей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Header Event Handlers
        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            UpdateStatus("Обновление...");
            await RefreshNewInstructionsAsync();
            await RefreshOldInstructionsAsync();
            UpdateStatus("Обновление завершено");
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // Create enhanced context menu matching UserForm functionality
            var contextMenu = new ContextMenu();

            // Refresh Instructions
            var refreshItem = new MenuItem { Header = "🔄 Обновить инструктажи" };
            refreshItem.Click += async (s, args) =>
            {
                await RefreshNewInstructionsAsync();
                await RefreshOldInstructionsAsync();
            };
            contextMenu.Items.Add(refreshItem);

            contextMenu.Items.Add(new Separator());

            // Change Credentials (equivalent to changeCredentialsToolStripMenuItem_Click)
            var credentialsItem = new MenuItem { Header = "🔐 Сменить регистрационные данные" };
            credentialsItem.Click += (s, args) =>
            {
                if (_signUpForm != null)
                {
                    _signUpForm.Show();
                }
                else
                {
                    // Fallback: Create a simple WPF input dialog or show message
                    System.Windows.MessageBox.Show(
                        "Функция смены учётных данных временно недоступна.\nОбратитесь к администратору для смены пароля.",
                        "Смена учётных данных",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            };
            contextMenu.Items.Add(credentialsItem);

            // Sign Out (equivalent to signOutToolStripMenuItem_Click)
            if (_loginForm != null)
            {
                var signOutItem = new MenuItem { Header = "🚪 Выйти из учётной записи" };
                signOutItem.Click += (s, args) => SignOut();
                contextMenu.Items.Add(signOutItem);
            }

            contextMenu.Items.Add(new Separator());

            // Exit Application (equivalent to exitApplicationToolStripMenuItem_Click)
            if (_loginForm != null)
            {
                var exitItem = new MenuItem { Header = "❌ Выйти из программы" };
                exitItem.Click += (s, args) => ExitApplication();
                contextMenu.Items.Add(exitItem);
            }

            contextMenu.Items.Add(new Separator());

            // Close Window
            var closeItem = new MenuItem { Header = "🗙 Закрыть окно" };
            closeItem.Click += (s, args) => this.Close();
            contextMenu.Items.Add(closeItem);

            contextMenu.IsOpen = true;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите закрыть приложение?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                ExitApplication();
            }
        }
        #endregion

        #region Context Menu Actions (from UserForm)
        private void SignOut()
        {
            try
            {
                if (_signUpForm != null)
                {
                    _signUpForm.Dispose();
                }

                Decryption_stuff.DeleteJWTToken();

                if (_loginForm != null)
                {
                    _loginForm.activeForm = _loginForm;
                    _loginForm.Show();
                }

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выходе: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExitApplication()
        {
            try
            {
                if (_loginForm != null)
                {
                    _loginForm.ExitApplication();
                }
                else
                {
                    System.Windows.Application.Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при закрытии приложения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Windows.Application.Current.Shutdown();
            }
        }
        #endregion

        #region Main Event Handlers
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
                // Use the logic to determine if chiefs can complete this instruction
                if (_isChief)
                {
                    PassInstructionCheckBox.IsEnabled = CanChiefCompleteInstruction();
                }
                else
                {
                    PassInstructionCheckBox.IsEnabled = true;
                }
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
                        Url = normativeInstruction[db_normativeInstructionUrl]?.ToString() ?? ""
                    });
                    hasNormativeInstructions = true;
                }
            }

            if (hasNormativeInstructions)
            {
                _isInstructionSelected = true;
                if (_isChief)
                {
                    PassInstructionCheckBox.IsEnabled = CanChiefCompleteInstruction();
                }
                else
                {
                    PassInstructionCheckBox.IsEnabled = true;
                }
            }
        }

        private void NormativeInstructionsListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (NormativeInstructionsListBox.SelectedItem is NormativeInstructionItem selectedItem)
            {
                OpenUrl(selectedItem.Url);
            }
        }

        private async void PassInstructionCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!_isInstructionSelected || InstructionsListBox.SelectedItem == null)
                return;

            try
            {
                string selectedInstruction = InstructionsListBox.SelectedItem.ToString();
                var selectedDict = GetDictFromSelectedInstruction(selectedInstruction);

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

            // Load related files for the selected passed instruction
            int instructionId = selectedItem.InstructionId;
            LoadRelatedFilesForInstruction(instructionId);
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
        #endregion

        #region Management Event Handlers (For Chiefs)
        private void AssignInstructionButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция назначения инструктажей будет реализована в следующей версии", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

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
                var relatedInstructions = _normativeInstructionsOfOldInstr?
                    .Where(dict => dict.ContainsKey(dB_instructionId) &&
                                   Convert.ToInt32(dict[dB_instructionId]) == instructionId)
                    .Select(dict => $"{dict[db_normativeInstructionName]} ({dict[db_normativeInstructionUrl]})")
                    .ToList();

                if (relatedInstructions != null)
                {
                    foreach (var file in relatedInstructions)
                    {
                        RelatedFiles.Add(file);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading related files: {ex.Message}");
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

        #region Window Event Handlers
        protected override void OnClosing(CancelEventArgs e)
        {
            // Optional: Add confirmation dialog for critical operations
            base.OnClosing(e);
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
    }
}