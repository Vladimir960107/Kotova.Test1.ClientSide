using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Kotova.Test1.ClientSide.ManagementWPF.Helpers;
using Kotova.Test1.ClientSide.ManagementWPF.Models;
using Kotova.Test1.ClientSide.ManagementWPF.Services;
using Kotova.Test1.ClientSide.ManagementWPF.ViewModels;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace Kotova.Test1.ClientSideManagementWPF.ViewModels
{
    public class UnplannedInstructionViewModel : BaseViewModel
    {
        public bool IsNotLoading => !IsLoading;

        private readonly IApiService _apiService;

        private string _instructionCause = "Работа в зоне железнодорожных путей СТО-357";
        private DateTime _endDate = DateTime.Now.AddDays(7);
        private bool _isLoading;
        private string _statusMessage;

        public UnplannedInstructionViewModel(IApiService apiService)
        {
            _apiService = apiService;

            ChiefsSelection = new ObservableCollection<ChiefSelectionItem>();
            InstructionStatuses = new ObservableCollection<UnplannedInstructionStatusDto>();
            NormativeInstructions = new ObservableCollection<NormativeInstructionDto>();

            LoadDataCommand = new RelayCommand(async () => await LoadDataAsync());
            AssignInstructionCommand = new RelayCommand(async () => await AssignInstructionAsync(), CanAssignInstruction);
            RefreshStatusCommand = new RelayCommand(async () => await LoadInstructionStatusesAsync());

            // Load data on initialization with proper error handling
            _ = InitializeAsync();
        }

        public ObservableCollection<ChiefSelectionItem> ChiefsSelection { get; }
        public ObservableCollection<UnplannedInstructionStatusDto> InstructionStatuses { get; }
        public ObservableCollection<NormativeInstructionDto> NormativeInstructions { get; }

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
                    LoadDepartmentsWithChiefsAsync(),
                    LoadInstructionStatusesAsync(),
                    LoadNormativeInstructionsAsync()
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

        private async Task LoadDepartmentsWithChiefsAsync()
        {
            try
            {
                var departments = await _apiService.GetDepartmentsWithChiefsAsync();

                ChiefsSelection.Clear();
                foreach (var dept in departments)
                {
                    foreach (var chief in dept.Chiefs)
                    {
                        ChiefsSelection.Add(new ChiefSelectionItem
                        {
                            ChiefId = chief.UserId,
                            DepartmentId = dept.DepartmentId,
                            ChiefName = chief.FullName,
                            DepartmentName = dept.DepartmentName,
                            Role = chief.Role,
                            JobPosition = chief.JobPosition
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка загрузки отделов с начальниками: {ex.Message}", ex);
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

        private async Task LoadNormativeInstructionsAsync()
        {
            try
            {
                var normative = await _apiService.GetNormativeInstructionsAsync();

                NormativeInstructions.Clear();
                foreach (var item in normative)
                {
                    NormativeInstructions.Add(item);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка загрузки нормативных документов: {ex.Message}", ex);
            }
        }

        private async Task AssignInstructionAsync()
        {
            if (!CanAssignInstruction())
                return;

            IsLoading = true;
            StatusMessage = "Назначение инструктажа...";

            try
            {
                // Add null checks and validation
                if (ChiefsSelection == null)
                {
                    throw new InvalidOperationException("Список начальников не инициализирован");
                }

                var selectedChiefIds = ChiefsSelection
                    .Where(c => c != null && c.IsSelected && c.ChiefId > 0)
                    .Select(c => c.ChiefId)
                    .ToList();

                if (!selectedChiefIds.Any())
                {
                    StatusMessage = "Не выбраны начальники для назначения инструктажа";
                    // Remove Dispatcher call and show message directly
                    MessageBox.Show("Пожалуйста, выберите хотя бы одного начальника для назначения инструктажа.",
                        "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var package = new UnplannedInstructionForChiefsPackage
                {
                    Instruction = new InstructionCreateDto
                    {
                        CauseOfInstruction = InstructionCause,
                        EndDate = EndDate,
                        TypeOfInstruction = 1 // Unplanned
                    },
                    SelectedChiefIds = selectedChiefIds,
                    FilePaths = new List<string>(),
                    NormativeInstructionIds = new List<int>()
                };

                var result = await _apiService.AssignUnplannedInstructionToChiefsAsync(package).ConfigureAwait(true);

                StatusMessage = result ?? "Инструктаж успешно назначен";

                // Remove Dispatcher call
                MessageBox.Show(result ?? "Инструктаж успешно назначен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Clear form
                InstructionCause = "";
                EndDate = DateTime.Now.AddDays(7);

                foreach (var chief in ChiefsSelection.Where(c => c != null))
                {
                    chief.IsSelected = false;
                }

                // Refresh statuses
                await LoadInstructionStatusesAsync().ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка: {ex.Message}";

                // Remove Dispatcher call
                MessageBox.Show($"Ошибка назначения инструктажа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanAssignInstruction()
        {
            return !IsLoading &&
                   !string.IsNullOrWhiteSpace(InstructionCause) &&
                   EndDate > DateTime.Now &&
                   ChiefsSelection.Any(c => c.IsSelected);
        }
    }
}