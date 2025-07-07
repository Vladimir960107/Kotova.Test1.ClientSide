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

            }
            else
            {
                // Employee not found in TransElectro database
                TransElectroFullNameTextBox.Text = "Не найден";
                TransElectroDepartmentTextBox.Text = "Не найден";
                TransElectroPositionTextBox.Text = "Не найден";
                TransElectroEmailTextBox.Text = "Не найден";
                TransElectroPersonnelNumberTextBox.Text = "Не найден";

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
                SynchronizeButton.Visibility = Visibility.Visible;

                // Create checkboxes for different fields
                CreateFieldCheckBoxes();
            }
            else
            {
                // No differences
                DifferencesTextBlock.Text = "✅ Различия не обнаружены";
                DifferencesTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(39, 174, 96)); // #27AE60

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
                // Confirm synchronization (no field selection needed)
                var result = MessageBox.Show(
                    $"Вы уверены, что хотите синхронизировать ВСЕ данные сотрудника?\n\n" +
                    $"Будут синхронизированы: ФИО, Должность, Email, Отдел (если существует в LYNKS)\n" +
                    $"Направление: TransElectro → LYNKS\n\n" +
                    $"Это действие перезапишет данные в базе LYNKS.",
                    "Подтверждение полной синхронизации",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes) return;

                // Perform synchronization
                this.Cursor = System.Windows.Input.Cursors.Wait;
                SynchronizeButton.IsEnabled = false;
                SynchronizeButton.Content = "🔄 Синхронизация...";

                await PerformFullSynchronizationAsync();
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
                SynchronizeButton.Content = "🔄 Синхронизировать ВСЁ";
            }
        }

        private async Task PerformFullSynchronizationAsync()
        {
            try
            {
                // Simplified request - no field selection needed
                var syncRequest = new EmployeeSyncRequest
                {
                    SyncDirection = "ToLynks",
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
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var syncResult = JsonConvert.DeserializeObject<dynamic>(responseContent);

                        var message = $"Синхронизация выполнена успешно!\n\n";

                        if (syncResult.SyncedFields != null)
                        {
                            message += $"Синхронизированы поля: {string.Join(", ", syncResult.SyncedFields)}";
                        }

                        MessageBox.Show(message, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Refresh data to show updated state
                        await LoadEmployeeComparisonDataAsync();
                    }
                    else
                    {
                        // Handle different types of errors more gracefully
                        var errorContent = await response.Content.ReadAsStringAsync();
                        string userFriendlyMessage = GetUserFriendlyErrorMessage(response.StatusCode, errorContent);

                        MessageBox.Show(userFriendlyMessage, "Ошибка синхронизации", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка соединения с сервером: {ex.Message}",
                              "Ошибка сети", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetUserFriendlyErrorMessage(System.Net.HttpStatusCode statusCode, string errorContent)
        {
            try
            {
                // Try to parse the error response
                var errorResult = JsonConvert.DeserializeObject<dynamic>(errorContent);

                // Access properties correctly from JObject (case-sensitive)
                string serverMessage = errorResult?.message?.ToString() ?? "";
                string errorType = errorResult?.errorType?.ToString() ?? "";

                // Debug logging (remove in production)
                System.Diagnostics.Debug.WriteLine($"ErrorType from server: '{errorType}'");
                System.Diagnostics.Debug.WriteLine($"Server message: '{serverMessage}'");

                // Handle specific error cases with user-friendly messages
                if (errorType == "DepartmentNotFound")
                {
                    string departmentName = errorResult?.missingDepartment?.ToString() ?? "неизвестный";
                    string detailedMessage = errorResult?.detailedMessage?.ToString() ?? "";

                    return $"🚫 СИНХРОНИЗАЦИЯ НЕ ВЫПОЛНЕНА\n\n" +
                           $"❌ Причина: Отдел не найден\n\n" +
                           $"Отдел '{departmentName}' не существует в базе данных LYNKS.\n\n" +
                           $"📋 Что нужно сделать:\n" +
                           $"• Обратитесь к администратору системы\n" +
                           $"• Попросите создать отдел '{departmentName}' в LYNKS\n" +
                           $"• После создания отдела повторите синхронизацию\n\n" +
                           $"⚠️ Данные НЕ были изменены в системе.";
                }
                else if (serverMessage.Contains("not found in LYNKS database") && serverMessage.Contains("employee"))
                {
                    return "🚫 СИНХРОНИЗАЦИЯ НЕ ВЫПОЛНЕНА\n\n" +
                           "❌ Причина: Сотрудник не найден\n\n" +
                           "Сотрудник не найден в базе данных LYNKS.\n" +
                           "Синхронизация возможна только для существующих сотрудников.\n\n" +
                           "⚠️ Данные НЕ были изменены в системе.";
                }
                else if (serverMessage.Contains("not found in TransElectro database"))
                {
                    return "🚫 СИНХРОНИЗАЦИЯ НЕ ВЫПОЛНЕНА\n\n" +
                           "❌ Причина: Сотрудник не найден в TransElectro\n\n" +
                           "Сотрудник не найден в базе данных TransElectro.\n" +
                           "Невозможно синхронизировать данные.\n\n" +
                           "⚠️ Данные НЕ были изменены в системе.";
                }
                else if (serverMessage.Contains("User record not found"))
                {
                    return "🚫 СИНХРОНИЗАЦИЯ НЕ ВЫПОЛНЕНА\n\n" +
                           "❌ Причина: Запись пользователя не найдена\n\n" +
                           "Запись пользователя не найдена в базе LYNKS.\n" +
                           "Невозможно синхронизировать email.\n\n" +
                           "📞 Обратитесь к администратору системы.\n\n" +
                           "⚠️ Данные НЕ были изменены в системе.";
                }
                else if (serverMessage.Contains("foreign key") || serverMessage.Contains("constraint"))
                {
                    return "🚫 СИНХРОНИЗАЦИЯ НЕ ВЫПОЛНЕНА\n\n" +
                           "❌ Причина: Ошибка связей в базе данных\n\n" +
                           "Обнаружены проблемы с целостностью данных.\n\n" +
                           "📞 Обратитесь к администратору системы.\n\n" +
                           "⚠️ Данные НЕ были изменены в системе.";
                }
                else if (!string.IsNullOrEmpty(serverMessage))
                {
                    return $"🚫 СИНХРОНИЗАЦИЯ НЕ ВЫПОЛНЕНА\n\n" +
                           $"❌ Ошибка: {serverMessage}\n\n" +
                           $"⚠️ Данные НЕ были изменены в системе.";
                }
            }
            catch (Exception ex)
            {
                // Debug logging for parsing errors
                System.Diagnostics.Debug.WriteLine($"Error parsing response: {ex.Message}");
            }

            // Generic error message based on status code
            return statusCode switch
            {
                System.Net.HttpStatusCode.BadRequest => "🚫 СИНХРОНИЗАЦИЯ НЕ ВЫПОЛНЕНА\n\n❌ Неправильный запрос.\nПроверьте данные сотрудника.\n\n⚠️ Данные НЕ были изменены.",
                System.Net.HttpStatusCode.Unauthorized => "🚫 ОШИБКА АВТОРИЗАЦИИ\n\n❌ У вас нет прав для выполнения синхронизации.",
                System.Net.HttpStatusCode.Forbidden => "🚫 ДОСТУП ЗАПРЕЩЕН\n\n❌ У вас недостаточно прав для синхронизации данных.",
                System.Net.HttpStatusCode.NotFound => "🚫 ОШИБКА\n\n❌ Запрашиваемый ресурс не найден.",
                System.Net.HttpStatusCode.InternalServerError => "🚫 ОШИБКА СЕРВЕРА\n\n❌ Внутренняя ошибка сервера.\nПопробуйте позже или обратитесь к администратору.",
                _ => $"🚫 СИНХРОНИЗАЦИЯ НЕ ВЫПОЛНЕНА\n\n❌ Код ошибки: {statusCode}\n\nОбратитесь к администратору системы."
            };
        }


        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Helper class for synchronization requests
        /// <summary>
        /// Simplified request model for employee synchronization (client side)
        /// </summary>
        public class EmployeeSyncRequest
        {
            public string SyncDirection { get; set; } = "ToLynks";
            public bool OverwriteExisting { get; set; } = true;
        }

        
    }
}
