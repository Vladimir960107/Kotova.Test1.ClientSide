using Kotova.CommonClasses;
using Kotova.Test1.ClientSide.ManagementWPF.Helpers;
using Kotova.Test1.ClientSide.ManagementWPF.Models;
using Kotova.Test1.ClientSide.ManagementWPF.Services;
using Kotova.Test1.ClientSide.ManagementWPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace Kotova.Test1.ClientSideManagementWPF.ViewModels
{
    public class UnplannedInstructionViewModel : BaseViewModel
    {
        public bool IsNotLoading => !IsLoading;

        private readonly IApiService _apiService;

        private bool _markNormativeAsUnplanned = true;

        private string _instructionCause = "Работа в зоне железнодорожных путей СТО-357";
        private DateTime _endDate = DateTime.Now.AddDays(7);
        private bool _isLoading;
        private string _statusMessage;
        private string _normativeBaseNames = "";
        private string _normativeBaseLinks = "";

        public UnplannedInstructionViewModel(IApiService apiService)
        {
            _apiService = apiService;

            // Change from ChiefsSelection to DepartmentsSelection
            DepartmentsSelection = new ObservableCollection<DepartmentSelectionItem>();
            InstructionStatuses = new ObservableCollection<UnplannedInstructionStatusDto>();

            LoadDataCommand = new RelayCommand(async () => await LoadDataAsync());
            AssignInstructionCommand = new RelayCommand(async () => await AssignInstructionAsync(), CanAssignInstruction);
            RefreshStatusCommand = new RelayCommand(async () => await LoadInstructionStatusesAsync());

            // Load data on initialization with proper error handling
            _ = InitializeAsync();
        }

        // Change from ChiefsSelection to DepartmentsSelection
        public ObservableCollection<DepartmentSelectionItem> DepartmentsSelection { get; }
        public ObservableCollection<UnplannedInstructionStatusDto> InstructionStatuses { get; }

        public string InstructionCause
        {
            get => _instructionCause;
            set => SetProperty(ref _instructionCause, value);
        }

        public DateTime EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        // Add new properties for normative base
        public string NormativeBaseNames
        {
            get => _normativeBaseNames;
            set => SetProperty(ref _normativeBaseNames, value);
        }

        public bool MarkNormativeAsUnplanned
        {
            get => _markNormativeAsUnplanned;
            set => SetProperty(ref _markNormativeAsUnplanned, value);
        }

        public string NormativeBaseLinks
        {
            get => _normativeBaseLinks;
            set => SetProperty(ref _normativeBaseLinks, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                SetProperty(ref _isLoading, value);
                OnPropertyChanged(nameof(IsNotLoading));
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public RelayCommand LoadDataCommand { get; }
        public RelayCommand AssignInstructionCommand { get; }
        public RelayCommand RefreshStatusCommand { get; }

        // Separate initialization method with better error handling
        private async Task InitializeAsync()
        {
            try
            {
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                // Make sure loading is turned off even if initialization fails
                IsLoading = false;
                StatusMessage = $"Ошибка инициализации: {ex.Message}";

                // Show error to user but don't crash the application
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    MessageBox.Show($"Ошибка загрузки данных: {ex.Message}\n\nПриложение будет работать в ограниченном режиме.",
                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                }));
            }
        }

        private async Task LoadDataAsync()
        {
            IsLoading = true;
            StatusMessage = "Загрузка данных...";

            try
            {
                await Task.WhenAll(
                    LoadDepartmentsAsync(),
                    LoadInstructionStatusesAsync()
                );

                StatusMessage = "Данные загружены успешно";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка загрузки: {ex.Message}";

                // Show error on UI thread
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }));
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Updated method to load departments instead of individual chiefs
        private async Task LoadDepartmentsAsync()
        {
            try
            {
                var departments = await _apiService.GetDepartmentsWithChiefsAsync();

                DepartmentsSelection.Clear();
                foreach (var dept in departments)
                {
                    // Only include departments that have chiefs
                    if (dept.Chiefs != null && dept.Chiefs.Any())
                    {
                        var chiefInfo = dept.Chiefs.Select(c => new ChiefInfo
                        {
                            UserId = c.UserId,
                            FullName = c.FullName,
                            Role = c.Role,
                            JobPosition = c.JobPosition
                        }).ToList();

                        DepartmentsSelection.Add(new DepartmentSelectionItem
                        {
                            DepartmentId = dept.DepartmentId,
                            DepartmentName = dept.DepartmentName,
                            ChiefsCount = dept.Chiefs.Count,
                            Chiefs = chiefInfo
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка загрузки отделов: {ex.Message}", ex);
            }
        }

        private async Task LoadInstructionStatusesAsync()
        {
            try
            {
                var statuses = await _apiService.GetUnplannedInstructionsStatusAsync();

                InstructionStatuses.Clear();
                foreach (var status in statuses)
                {
                    InstructionStatuses.Add(status);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка загрузки статусов инструктажей: {ex.Message}", ex);
            }
        }

        // Helper method to process multiline text
        private string ProcessMultilineText(string multilineText)
        {
            if (string.IsNullOrWhiteSpace(multilineText))
                return "";

            // Split by newlines, trim each line, remove empty lines, and join with "|"
            return string.Join(" | ",
                multilineText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrEmpty(line)));
        }

        private bool CanAssignInstruction()
        {
            // Basic validation
            if (IsLoading)
                return false;

            // Check if instruction cause is not empty
            if (string.IsNullOrWhiteSpace(InstructionCause))
            {
                StatusMessage = "❗ Причина внепланового инструктажа обязательна для заполнения";
                return false;
            }

            // Check if end date is in the future
            if (EndDate <= DateTime.Now)
            {
                StatusMessage = "❗ Дата окончания должна быть больше текущей даты";
                return false;
            }

            // Check if at least one department is selected
            if (!DepartmentsSelection.Any(d => d.IsSelected))
            {
                StatusMessage = "❗ Необходимо выбрать хотя бы один отдел";
                return false;
            }

            // Enhanced validation for normative base
            bool hasNormativeNames = !string.IsNullOrWhiteSpace(NormativeBaseNames);
            bool hasNormativeLinks = !string.IsNullOrWhiteSpace(NormativeBaseLinks);

            // If user provided normative names, links become mandatory and must not be empty
            if (hasNormativeNames && !hasNormativeLinks)
            {
                StatusMessage = "❗ При указании нормативной базы, ссылки являются обязательными и не должны быть пустыми";
                return false;
            }

            // If user provided links, names become mandatory and must not be empty
            if (hasNormativeLinks && !hasNormativeNames)
            {
                StatusMessage = "❗ При указании ссылок, названия нормативных документов являются обязательными и не должны быть пустыми";
                return false;
            }

            // If validation passes, clear any previous error messages
            if (StatusMessage.StartsWith("❗") || StatusMessage.StartsWith("⚠️"))
            {
                StatusMessage = "Готово к назначению внепланового инструктажа";
            }

            return true;
        }

        // Add this method to validate normative base consistency
        private bool ValidateNormativeBase(out string errorMessage)
        {
            errorMessage = string.Empty;

            bool hasNormativeNames = !string.IsNullOrWhiteSpace(NormativeBaseNames);
            bool hasNormativeLinks = !string.IsNullOrWhiteSpace(NormativeBaseLinks);

            if (!hasNormativeNames && !hasNormativeLinks)
            {
                // No normative base provided - this is OK
                return true;
            }

            if (hasNormativeNames && !hasNormativeLinks)
            {
                errorMessage = "При указании названий нормативных документов, ссылки являются обязательными и не должны быть пустыми.";
                return false;
            }

            if (!hasNormativeNames && hasNormativeLinks)
            {
                errorMessage = "При указании ссылок, названия нормативных документов являются обязательными и не должны быть пустыми.";
                return false;
            }

            // Additional validation: check that after trimming and removing empty lines, both still have content
            var nameLines = NormativeBaseNames?.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList() ?? new List<string>();

            var linkLines = NormativeBaseLinks?.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList() ?? new List<string>();

            if (!nameLines.Any())
            {
                errorMessage = "Названия нормативных документов не должны быть пустыми или содержать только пробелы.";
                return false;
            }

            if (!linkLines.Any())
            {
                errorMessage = "Ссылки на нормативные документы не должны быть пустыми или содержать только пробелы.";
                return false;
            }

            return true;
        }

        // Update the AssignInstructionAsync method with better validation:
        private async Task AssignInstructionAsync()
        {
            // Enhanced validation before proceeding
            if (!CanAssignInstruction())
                return;

            // Additional validation for normative base
            if (!ValidateNormativeBase(out string normativeError))
            {
                StatusMessage = $"❗ {normativeError}";
                MessageBox.Show(normativeError, "Ошибка валидации", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            IsLoading = true;
            StatusMessage = "Назначение внепланового инструктажа...";

            try
            {
                // Add null checks and validation
                if (DepartmentsSelection == null)
                {
                    throw new InvalidOperationException("Список отделов не инициализирован");
                }

                var selectedDepartmentIds = DepartmentsSelection
                    .Where(d => d != null && d.IsSelected && d.DepartmentId > 0)
                    .Select(d => d.DepartmentId)
                    .ToList();

                if (!selectedDepartmentIds.Any())
                {
                    StatusMessage = "❗ Не выбраны отделы для назначения инструктажа";
                    MessageBox.Show("Пожалуйста, выберите хотя бы один отдел для назначения внепланового инструктажа.",
                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Process normative instructions if provided
                string normativeBaseText = null;

                if (!string.IsNullOrWhiteSpace(NormativeBaseNames) || !string.IsNullOrWhiteSpace(NormativeBaseLinks))
                {
                    // Transform multiline text to single string with "|" separators
                    string processedNames = ProcessMultilineText(NormativeBaseNames);
                    string processedLinks = ProcessMultilineText(NormativeBaseLinks);

                    // Create combined normative instruction text
                    normativeBaseText = $"NAMES: {processedNames} | LINKS: {processedLinks}";

                    StatusMessage += " (включая нормативную базу)";
                }

                // Use the new package for departments
                var package = new UnplannedInstructionForDepartmentsPackage
                {
                    Instruction = new InstructionCreateDto
                    {
                        CauseOfInstruction = InstructionCause,
                        EndDate = EndDate,
                        TypeOfInstruction = 1 // Unplanned - Fixed value, no user input needed
                    },
                    SelectedDepartmentIds = selectedDepartmentIds,
                    FilePaths = new List<string>(),
                    NormativeInstructionIds = new List<int>(),
                    NormativeBaseText = normativeBaseText,
                    MarkNormativeAsUnplanned = MarkNormativeAsUnplanned
                };

                var result = await _apiService.AssignUnplannedInstructionToDepartmentsAsync(package).ConfigureAwait(true);

                StatusMessage = result ?? "✅ Внеплановый инструктаж успешно назначен";

                MessageBox.Show(result ?? "Внеплановый инструктаж успешно назначен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Clear form after successful assignment
                InstructionCause = "";
                EndDate = DateTime.Now.AddDays(7);
                NormativeBaseNames = "";
                NormativeBaseLinks = "";

                foreach (var dept in DepartmentsSelection.Where(d => d != null))
                {
                    dept.IsSelected = false;
                }

                // Refresh statuses
                await LoadInstructionStatusesAsync().ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                StatusMessage = $"❗ Ошибка: {ex.Message}";
                MessageBox.Show($"Ошибка назначения внепланового инструктажа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }



    }
}