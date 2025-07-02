using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Kotova.CommonClasses;
using Newtonsoft.Json;
using CheckBox = System.Windows.Controls.CheckBox;
using Color = System.Windows.Media.Color;
using MessageBox = System.Windows.MessageBox;

namespace Kotova.Test1.ClientSide
{
    public partial class DatabaseDifferencesResolverForm : Window
    {
        private readonly string _personnelNumber;
        private readonly string _jwtToken;
        private EmployeeComparisonDto _currentEmployeeComparison;

        // API endpoints
        private static readonly string GetEmployeeDetailsUrl =
            ConfigurationClass.BASE_URL_DEVELOPMENT + "/api/DatabaseComparison/employee-details";
        private static readonly string SynchronizeEmployeeUrl =
            ConfigurationClass.BASE_URL_DEVELOPMENT + "/api/DatabaseComparison/synchronize-employee";

        public DatabaseDifferencesResolverForm(string personnelNumber, string jwtToken)
        {
            InitializeComponent();
            _personnelNumber = personnelNumber;
            _jwtToken = jwtToken;

            // Initialize the form
            InitializeFormAsync();
        }

        private async void InitializeFormAsync()
        {
            try
            {
                // Show loading state
                this.Cursor = System.Windows.Input.Cursors.Wait;
                EmployeeNameHeader.Text = "Загрузка данных сотрудника...";

                // Load employee comparison data
                await LoadEmployeeComparisonDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                this.Cursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        private async Task LoadEmployeeComparisonDataAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);

                    var response = await client.GetAsync($"{GetEmployeeDetailsUrl}/{_personnelNumber}");

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        _currentEmployeeComparison = JsonConvert.DeserializeObject<EmployeeComparisonDto>(jsonResponse);

                        // Update the UI
                        UpdateUI();
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Ошибка при получении данных сотрудника: {response.StatusCode}\n{errorContent}",
                                      "Ошибка API", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка соединения с сервером: {ex.Message}",
                              "Ошибка сети", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateUI()
        {
            if (_currentEmployeeComparison == null) return;

            // Update header
            EmployeeNameHeader.Text = $"Разрешение различий для: {_currentEmployeeComparison.FullName}";
            PersonnelNumberHeader.Text = $"Табельный номер: {_currentEmployeeComparison.PersonnelNumber}";

            // Update Lynks data (left side)
            var lynksData = _currentEmployeeComparison.LynksData;
            if (lynksData != null)
            {
                LynksFullNameTextBox.Text = lynksData.FullName ?? "";
                LynksDepartmentTextBox.Text = lynksData.DepartmentName ?? "";
                LynksPositionTextBox.Text = lynksData.PositionName ?? "";
                LynksEmailTextBox.Text = lynksData.Email ?? "";
                LynksPersonnelNumberTextBox.Text = lynksData.PersonnelNumber ?? "";
            }

            // Update TransElectro data (right side)
            var transElectroData = _currentEmployeeComparison.TransElectroData;
            if (transElectroData != null)
            {
                TransElectroFullNameTextBox.Text = transElectroData.FullName ?? "";
                TransElectroDepartmentTextBox.Text = transElectroData.DepartmentName ?? "";
                TransElectroPositionTextBox.Text = transElectroData.PositionName ?? "";
                TransElectroEmailTextBox.Text = transElectroData.Email ?? "";
                TransElectroPersonnelNumberTextBox.Text = transElectroData.PersonnelNumber ?? "";

                NotFoundMessageTextBlock.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Employee not found in TransElectro database
                TransElectroFullNameTextBox.Text = "Не найден";
                TransElectroDepartmentTextBox.Text = "Не найден";
                TransElectroPositionTextBox.Text = "Не найден";
                TransElectroEmailTextBox.Text = "Не найден";
                TransElectroPersonnelNumberTextBox.Text = "Не найден";

                NotFoundMessageTextBlock.Visibility = Visibility.Visible;
            }

            // Highlight differences and update differences section
            HighlightDifferences();
            UpdateDifferencesSection();
        }

        private void HighlightDifferences()
        {
            if (_currentEmployeeComparison?.DifferenceFields == null) return;

            // Reset all styles to default
            LynksFullNameTextBox.Style = (Style)Resources["ReadOnlyTextBoxStyle"];
            LynksDepartmentTextBox.Style = (Style)Resources["ReadOnlyTextBoxStyle"];
            LynksPositionTextBox.Style = (Style)Resources["ReadOnlyTextBoxStyle"];
            LynksEmailTextBox.Style = (Style)Resources["ReadOnlyTextBoxStyle"];

            TransElectroFullNameTextBox.Style = (Style)Resources["ReadOnlyTextBoxStyle"];
            TransElectroDepartmentTextBox.Style = (Style)Resources["ReadOnlyTextBoxStyle"];
            TransElectroPositionTextBox.Style = (Style)Resources["ReadOnlyTextBoxStyle"];
            TransElectroEmailTextBox.Style = (Style)Resources["ReadOnlyTextBoxStyle"];

            // Apply different style to fields with differences
            var differentFieldStyle = (Style)Resources["DifferentFieldStyle"];

            foreach (var fieldName in _currentEmployeeComparison.DifferenceFields)
            {
                switch (fieldName)
                {
                    case "FullName":
                        LynksFullNameTextBox.Style = differentFieldStyle;
                        TransElectroFullNameTextBox.Style = differentFieldStyle;
                        break;
                    case "DepartmentName":
                        LynksDepartmentTextBox.Style = differentFieldStyle;
                        TransElectroDepartmentTextBox.Style = differentFieldStyle;
                        break;
                    case "Position":
                        LynksPositionTextBox.Style = differentFieldStyle;
                        TransElectroPositionTextBox.Style = differentFieldStyle;
                        break;
                    case "Email":
                        LynksEmailTextBox.Style = differentFieldStyle;
                        TransElectroEmailTextBox.Style = differentFieldStyle;
                        break;
                }
            }
        }

        private void UpdateDifferencesSection()
        {
            if (_currentEmployeeComparison == null) return;

            if (_currentEmployeeComparison.HasDifferences && _currentEmployeeComparison.DifferenceFields?.Any() == true)
            {
                // Show differences
                var differences = _currentEmployeeComparison.DifferenceFields;
                var friendlyNames = differences.Select(GetFriendlyFieldName).ToList();

                DifferencesTextBlock.Text = $"🔴 Обнаружены различия в полях: {string.Join(", ", friendlyNames)}";
                DifferencesTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(231, 76, 60)); // #E74C3C

                // Show synchronization options
                SyncOptionsGrid.Visibility = Visibility.Visible;
                SynchronizeButton.Visibility = Visibility.Visible;

                // Create checkboxes for different fields
                CreateFieldCheckBoxes();
            }
            else
            {
                // No differences
                DifferencesTextBlock.Text = "✅ Различия не обнаружены";
                DifferencesTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(39, 174, 96)); // #27AE60

                SyncOptionsGrid.Visibility = Visibility.Collapsed;
                SynchronizeButton.Visibility = Visibility.Collapsed;
            }
        }

        private void CreateFieldCheckBoxes()
        {
            FieldCheckBoxesPanel.Children.Clear();

            if (_currentEmployeeComparison?.DifferenceFields == null) return;

            foreach (var fieldName in _currentEmployeeComparison.DifferenceFields)
            {
                if (fieldName == "Employee not found in TransElectro database") continue;

                var checkBox = new CheckBox
                {
                    Content = GetFriendlyFieldName(fieldName),
                    Tag = fieldName,
                    IsChecked = true,
                    Style = (Style)Resources["FieldCheckBoxStyle"]
                };

                FieldCheckBoxesPanel.Children.Add(checkBox);
            }
        }

        private string GetFriendlyFieldName(string fieldName)
        {
            return fieldName switch
            {
                "FullName" => "ФИО",
                "DepartmentName" => "Отдел",
                "Position" => "Должность",
                "Email" => "Email",
                _ => fieldName
            };
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Cursor = System.Windows.Input.Cursors.Wait;
                RefreshButton.IsEnabled = false;
                RefreshButton.Content = "🔄 Обновление...";

                await LoadEmployeeComparisonDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении данных: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                this.Cursor = System.Windows.Input.Cursors.Arrow;
                RefreshButton.IsEnabled = true;
                RefreshButton.Content = "🔄 Обновить данные";
            }
        }

        private async void SynchronizeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get selected fields to synchronize
                var selectedFields = new List<string>();
                foreach (CheckBox checkBox in FieldCheckBoxesPanel.Children.OfType<CheckBox>())
                {
                    if (checkBox.IsChecked == true)
                    {
                        selectedFields.Add(checkBox.Tag.ToString());
                    }
                }

                if (!selectedFields.Any())
                {
                    MessageBox.Show("Выберите поля для синхронизации.",
                                  "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Get sync direction
                var selectedDirection = ((ComboBoxItem)SyncDirectionComboBox.SelectedItem).Tag.ToString();

                // Confirm synchronization
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите синхронизировать следующие поля:\n" +
                    $"{string.Join(", ", selectedFields.Select(GetFriendlyFieldName))}\n\n" +
                    $"Направление: {((ComboBoxItem)SyncDirectionComboBox.SelectedItem).Content}\n\n" +
                    $"Это действие изменит данные в базе данных.",
                    "Подтверждение синхронизации",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes) return;

                // Perform synchronization
                this.Cursor = System.Windows.Input.Cursors.Wait;
                SynchronizeButton.IsEnabled = false;
                SynchronizeButton.Content = "🔄 Синхронизация...";

                await PerformSynchronizationAsync(selectedFields, selectedDirection);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при синхронизации: {ex.Message}",
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                this.Cursor = System.Windows.Input.Cursors.Arrow;
                SynchronizeButton.IsEnabled = true;
                SynchronizeButton.Content = "🔄 Синхронизировать";
            }
        }

        private async Task PerformSynchronizationAsync(List<string> selectedFields, string syncDirection)
        {
            try
            {
                var syncRequest = new EmployeeSyncRequest
                {
                    FieldsToSync = selectedFields,
                    SyncDirection = syncDirection,
                    OverwriteExisting = true
                };

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);

                    var jsonContent = JsonConvert.SerializeObject(syncRequest);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync($"{SynchronizeEmployeeUrl}/{_personnelNumber}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Синхронизация выполнена успешно!",
                                      "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Refresh data to show updated state
                        await LoadEmployeeComparisonDataAsync();
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Ошибка при синхронизации: {response.StatusCode}\n{errorContent}",
                                      "Ошибка API", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка соединения с сервером: {ex.Message}",
                              "Ошибка сети", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    // Helper class for synchronization requests
    public class EmployeeSyncRequest
    {
        public List<string> FieldsToSync { get; set; } = new List<string>();
        public string SyncDirection { get; set; } = "ToLynks";
        public bool OverwriteExisting { get; set; } = false;
    }
}