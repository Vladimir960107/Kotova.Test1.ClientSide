using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Kotova.Test1.ClientSide.ManagementWPF.Helpers;
using Kotova.Test1.ClientSide.ManagementWPF.Models;
using Kotova.Test1.ClientSide.ManagementWPF.Services;
using Kotova.Test1.ClientSide.ManagementWPF.ViewModels;
using MessageBox = System.Windows.MessageBox;

namespace Kotova.Test1.ClientSideManagementWPF.ViewModels
{
    public class UnplannedInstructionViewModel : BaseViewModel
    {
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

            // Load data on initialization
            _ = LoadDataAsync();
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
            set => SetProperty(ref _isLoading, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public RelayCommand LoadDataCommand { get; }
        public RelayCommand AssignInstructionCommand { get; }
        public RelayCommand RefreshStatusCommand { get; }

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
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                var selectedChiefIds = ChiefsSelection
                    .Where(c => c.IsSelected)
                    .Select(c => c.ChiefId)
                    .ToList();

                var package = new UnplannedInstructionForChiefsPackage
                {
                    Instruction = new InstructionCreateDto
                    {
                        CauseOfInstruction = InstructionCause,
                        EndDate = EndDate,
                        TypeOfInstruction = 1 // Unplanned
                    },
                    SelectedChiefIds = selectedChiefIds,
                    FilePaths = new List<string>(), // TODO: Add file selection
                    NormativeInstructionIds = new List<int>() // TODO: Add normative selection
                };

                var result = await _apiService.AssignUnplannedInstructionToChiefsAsync(package);

                StatusMessage = result;
                MessageBox.Show(result, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                // Clear form
                InstructionCause = "";
                EndDate = DateTime.Now.AddDays(7);
                foreach (var chief in ChiefsSelection)
                {
                    chief.IsSelected = false;
                }

                // Refresh statuses
                await LoadInstructionStatusesAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Ошибка: {ex.Message}";
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