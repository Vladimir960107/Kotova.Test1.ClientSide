using Kotova.CommonClasses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

// WPF namespaces
using WPFWindow = System.Windows.Window;
using WPF = System.Windows;
using WPFControls = System.Windows.Controls;
using WPFData = System.Windows.Data;
using WPFDocs = System.Windows.Documents;
using WPFInput = System.Windows.Input;
using WPFMedia = System.Windows.Media;
using WPFShapes = System.Windows.Shapes;

// Windows Forms namespaces
using WinForms = System.Windows.Forms;

namespace Kotova.Test1.ClientSide
{
    /// <summary>
    /// Interaction logic for InstructionAssignmentManagerUnplanned.xaml
    /// Simplified form for assigning unplanned instructions with pre-determined normative instructions
    /// </summary>
    public partial class InstructionAssignmentManagerUnplanned : WPFWindow
    {
        private string _instructionName;
        private int _instructionId;
        private List<EmployeeInfo> _employees;
        private List<NormativeInstructionInfo> _predeterminedNormativeInstructions;
        private string _jwtToken;
        private string _assignmentEndpoint;
        private string _currentUserRole;

        private WPFControls.ListView employeesListView;
        private WPFControls.ListView normativeInstructionsListView;
        private WPFControls.TextBlock instructionInfoTextBlock;
        private WPFControls.TextBlock normativeInfoTextBlock;

        public InstructionAssignmentManagerUnplanned(
            string instructionName,
            int instructionId,
            List<EmployeeInfo> employees,
            List<NormativeInstructionInfo> predeterminedNormativeInstructions,
            string jwtToken,
            string assignmentEndpoint)
        {
            _instructionName = instructionName;
            _instructionId = instructionId;
            _employees = employees;
            _predeterminedNormativeInstructions = predeterminedNormativeInstructions;
            _jwtToken = jwtToken;
            _assignmentEndpoint = assignmentEndpoint;
            _currentUserRole = GetRoleFromJWT.GetRoleFromToken(jwtToken);

            Title = $"Назначение внепланового инструктажа: {instructionName}";
            Width = 900;
            Height = 600;
            WindowStartupLocation = WPF.WindowStartupLocation.CenterOwner;

            InitializeComponent();
            BuildUI();
            LoadEmployees();
            LoadPredeterminedNormativeInstructions();
        }

        private void BuildUI()
        {
            var grid = new WPFControls.Grid();
            grid.RowDefinitions.Add(new WPFControls.RowDefinition { Height = WPF.GridLength.Auto }); // Info section
            grid.RowDefinitions.Add(new WPFControls.RowDefinition { Height = new WPF.GridLength(1, WPF.GridUnitType.Star) }); // Main content
            grid.RowDefinitions.Add(new WPFControls.RowDefinition { Height = WPF.GridLength.Auto }); // Buttons

            // Info section
            var infoPanel = new WPFControls.StackPanel
            {
                Orientation = WPFControls.Orientation.Vertical,
                Margin = new WPF.Thickness(10),
                Background = new WPFMedia.SolidColorBrush(WPFMedia.Color.FromRgb(240, 248, 255))
            };

            instructionInfoTextBlock = new WPFControls.TextBlock
            {
                Text = $"Внеплановый инструктаж: {_instructionName}",
                FontWeight = WPF.FontWeights.Bold,
                FontSize = 14,
                Margin = new WPF.Thickness(5)
            };

            normativeInfoTextBlock = new WPFControls.TextBlock
            {
                Text = "Нормативные инструкции будут назначены автоматически согласно настройкам данного внепланового инструктажа",
                FontStyle = WPF.FontStyles.Italic,
                Foreground = new WPFMedia.SolidColorBrush(WPFMedia.Colors.DarkBlue),
                Margin = new WPF.Thickness(5)
            };

            infoPanel.Children.Add(instructionInfoTextBlock);
            infoPanel.Children.Add(normativeInfoTextBlock);
            WPFControls.Grid.SetRow(infoPanel, 0);

            // Main content panel with two columns
            var mainPanel = new WPFControls.Grid();
            mainPanel.ColumnDefinitions.Add(new WPFControls.ColumnDefinition { Width = new WPF.GridLength(1, WPF.GridUnitType.Star) });
            mainPanel.ColumnDefinitions.Add(new WPFControls.ColumnDefinition { Width = new WPF.GridLength(1, WPF.GridUnitType.Star) });
            WPFControls.Grid.SetRow(mainPanel, 1);

            // Left column - Employees
            var employeesPanel = CreateEmployeesPanel();
            WPFControls.Grid.SetColumn(employeesPanel, 0);

            // Right column - Normative Instructions (read-only)
            var normativePanel = CreateNormativeInstructionsPanel();
            WPFControls.Grid.SetColumn(normativePanel, 1);

            mainPanel.Children.Add(employeesPanel);
            mainPanel.Children.Add(normativePanel);

            // Buttons panel
            var buttonsPanel = CreateButtonsPanel();
            WPFControls.Grid.SetRow(buttonsPanel, 2);

            grid.Children.Add(infoPanel);
            grid.Children.Add(mainPanel);
            grid.Children.Add(buttonsPanel);

            Content = grid;
        }

        private WPFControls.DockPanel CreateEmployeesPanel()
        {
            var panel = new WPFControls.DockPanel
            {
                Margin = new WPF.Thickness(5),
                LastChildFill = true
            };

            var header = new WPFControls.TextBlock
            {
                Text = "Выберите сотрудников для назначения",
                FontWeight = WPF.FontWeights.Bold,
                Margin = new WPF.Thickness(5)
            };
            WPFControls.DockPanel.SetDock(header, WPFControls.Dock.Top);

            var buttonsPanel = new WPFControls.StackPanel
            {
                Orientation = WPFControls.Orientation.Horizontal,
                Margin = new WPF.Thickness(5)
            };

            var selectAllButton = new WPFControls.Button
            {
                Content = "Выбрать всех",
                Margin = new WPF.Thickness(2),
                Padding = new WPF.Thickness(10, 5, 10, 5)
            };
            selectAllButton.Click += SelectAllEmployees_Click;

            var deselectAllButton = new WPFControls.Button
            {
                Content = "Снять выбор",
                Margin = new WPF.Thickness(2),
                Padding = new WPF.Thickness(10, 5, 10, 5)
            };
            deselectAllButton.Click += DeselectAllEmployees_Click;

            buttonsPanel.Children.Add(selectAllButton);
            buttonsPanel.Children.Add(deselectAllButton);
            WPFControls.DockPanel.SetDock(buttonsPanel, WPFControls.Dock.Top);

            employeesListView = new WPFControls.ListView
            {
                Margin = new WPF.Thickness(5),
                BorderBrush = new WPFMedia.SolidColorBrush(WPFMedia.Colors.LightGray),
                BorderThickness = new WPF.Thickness(1)
            };

            panel.Children.Add(header);
            panel.Children.Add(buttonsPanel);
            panel.Children.Add(employeesListView);

            return panel;
        }

        private WPFControls.DockPanel CreateNormativeInstructionsPanel()
        {
            var panel = new WPFControls.DockPanel
            {
                Margin = new WPF.Thickness(5),
                LastChildFill = true
            };

            var header = new WPFControls.TextBlock
            {
                Text = "Нормативные инструкции (автоматически)",
                FontWeight = WPF.FontWeights.Bold,
                Margin = new WPF.Thickness(5)
            };
            WPFControls.DockPanel.SetDock(header, WPFControls.Dock.Top);

            var infoText = new WPFControls.TextBlock
            {
                Text = "Эти нормативные инструкции будут автоматически назначены выбранным сотрудникам",
                FontStyle = WPF.FontStyles.Italic,
                Foreground = new WPFMedia.SolidColorBrush(WPFMedia.Colors.Gray),
                Margin = new WPF.Thickness(5),
                TextWrapping = WPF.TextWrapping.Wrap
            };
            WPFControls.DockPanel.SetDock(infoText, WPFControls.Dock.Top);

            normativeInstructionsListView = new WPFControls.ListView
            {
                Margin = new WPF.Thickness(5),
                BorderBrush = new WPFMedia.SolidColorBrush(WPFMedia.Colors.LightGray),
                BorderThickness = new WPF.Thickness(1),
                IsEnabled = false // Make it read-only
            };

            panel.Children.Add(header);
            panel.Children.Add(infoText);
            panel.Children.Add(normativeInstructionsListView);

            return panel;
        }

        private WPFControls.StackPanel CreateButtonsPanel()
        {
            var panel = new WPFControls.StackPanel
            {
                Orientation = WPFControls.Orientation.Horizontal,
                HorizontalAlignment = WPF.HorizontalAlignment.Right,
                Margin = new WPF.Thickness(10)
            };

            var assignButton = new WPFControls.Button
            {
                Content = "Назначить инструктаж",
                IsDefault = true,
                Width = 150,
                Height = 35,
                Margin = new WPF.Thickness(5),
                Background = new WPFMedia.SolidColorBrush(WPFMedia.Color.FromRgb(0, 122, 204)),
                Foreground = new WPFMedia.SolidColorBrush(WPFMedia.Colors.White),
                FontWeight = WPF.FontWeights.Bold
            };
            assignButton.Click += AssignButton_Click;

            var cancelButton = new WPFControls.Button
            {
                Content = "Отмена",
                IsCancel = true,
                Width = 80,
                Height = 35,
                Margin = new WPF.Thickness(5)
            };
            cancelButton.Click += CancelButton_Click;

            panel.Children.Add(assignButton);
            panel.Children.Add(cancelButton);

            return panel;
        }

        private void LoadEmployees()
        {
            employeesListView.Items.Clear();

            var allowedRoles = GetAllowedRolesForAssignmentUnplanned(_currentUserRole);

            foreach (var employee in _employees)
            {
                if (string.IsNullOrEmpty(employee.Role) || !allowedRoles.Contains(employee.Role))
                    continue;

                var checkBox = new WPFControls.CheckBox
                {
                    Content = $"{employee.FullName} ({AllowedRoles.GetRoleDisplayName(employee.Role)}) - {employee.BirthDate}",
                    Tag = employee,
                    Margin = new WPF.Thickness(5)
                };

                employeesListView.Items.Add(checkBox);
            }
        }

        private List<string> GetAllowedRolesForAssignmentUnplanned(string currentUserRole)
        {
            var allowedRoles = new List<string>();

            //Console.WriteLine($"Determining allowed roles for current user role: '{currentUserRole}'");

            switch (currentUserRole?.ToLower())
            {
                case "chiefofdepartment":
                case "chief":
                    // Chief can assign to: Users, Coordinators, and Deputies
                    allowedRoles.AddRange(new[] { "User", "Coordinator"});
                    //Console.WriteLine("User is Chief - can assign to User, Coordinator, DeputyChief");
                    break;

                case "deputychief":
                case "deputy":
                    // Deputy can assign to: Users and Coordinators (NOT Chiefs)
                    allowedRoles.AddRange(new[] { "User", "Coordinator" });
                    //Console.WriteLine("User is Deputy - can assign to User, Coordinator");
                    break;

                default:
                    // Default case - no assignment permissions
                    Console.WriteLine($"Unknown or unauthorized role: '{currentUserRole}' - no assignment permissions");
                    break;
            }

            //Console.WriteLine($"Final allowed roles: [{string.Join(", ", allowedRoles)}]");
            return allowedRoles;
        }

        private void LoadPredeterminedNormativeInstructions()
        {
            normativeInstructionsListView.Items.Clear();

            if (_predeterminedNormativeInstructions == null || !_predeterminedNormativeInstructions.Any())
            {
                var noInstructionsText = new WPFControls.TextBlock
                {
                    Text = "Нормативные инструкции не назначены для данного внепланового инструктажа",
                    FontStyle = WPF.FontStyles.Italic,
                    Foreground = new WPFMedia.SolidColorBrush(WPFMedia.Colors.Gray),
                    Margin = new WPF.Thickness(5)
                };
                normativeInstructionsListView.Items.Add(noInstructionsText);
                return;
            }

            foreach (var normative in _predeterminedNormativeInstructions)
            {
                var textBlock = new WPFControls.TextBlock
                {
                    Text = normative.Name,
                    Margin = new WPF.Thickness(5),
                    TextWrapping = WPF.TextWrapping.Wrap
                };

                // Add URL if available
                if (!string.IsNullOrEmpty(normative.Url))
                {
                    textBlock.Text += $"\nСсылка: {normative.Url}";
                }

                normativeInstructionsListView.Items.Add(textBlock);
            }

            // Update info text
            normativeInfoTextBlock.Text = $"Будет назначено {_predeterminedNormativeInstructions.Count} нормативных инструкций";
        }

        private void SelectAllEmployees_Click(object sender, WPF.RoutedEventArgs e)
        {
            foreach (WPFControls.CheckBox checkBox in employeesListView.Items.OfType<WPFControls.CheckBox>())
            {
                checkBox.IsChecked = true;
            }
        }

        private void DeselectAllEmployees_Click(object sender, WPF.RoutedEventArgs e)
        {
            foreach (WPFControls.CheckBox checkBox in employeesListView.Items.OfType<WPFControls.CheckBox>())
            {
                checkBox.IsChecked = false;
            }
        }

        private async void AssignButton_Click(object sender, WPF.RoutedEventArgs e)
        {
            try
            {
                var selectedEmployees = employeesListView.Items
                    .OfType<WPFControls.CheckBox>()
                    .Where(cb => cb.IsChecked == true && cb.Tag is EmployeeInfo)
                    .Select(cb => cb.Tag as EmployeeInfo)
                    .ToList();

                if (!selectedEmployees.Any())
                {
                    WPF.MessageBox.Show("Выберите хотя бы одного сотрудника для назначения.",
                        "Предупреждение", WPF.MessageBoxButton.OK, WPF.MessageBoxImage.Warning);
                    return;
                }

                var confirmationMessage = $"Назначить внеплановый инструктаж '{_instructionName}' " +
                    $"следующим сотрудникам ({selectedEmployees.Count}):\n\n" +
                    string.Join("\n", selectedEmployees.Select(e => $"• {e.FullName}"));

                if (_predeterminedNormativeInstructions?.Any() == true)
                {
                    confirmationMessage += $"\n\nТакже будет назначено {_predeterminedNormativeInstructions.Count} нормативных инструкций.";
                }

                if (WPF.MessageBox.Show(confirmationMessage, "Подтверждение назначения",
                    WPF.MessageBoxButton.YesNo, WPF.MessageBoxImage.Question) != WPF.MessageBoxResult.Yes)
                {
                    return;
                }

                await AssignUnplannedInstructionToEmployees(selectedEmployees);
            }
            catch (Exception ex)
            {
                WPF.MessageBox.Show($"Ошибка при назначении: {ex.Message}", "Ошибка",
                    WPF.MessageBoxButton.OK, WPF.MessageBoxImage.Error);
            }
        }

        private async Task AssignUnplannedInstructionToEmployees(List<EmployeeInfo> selectedEmployees)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);

                    // Create assignment package specifically for unplanned instructions
                    var assignmentPackage = new
                    {
                        InstructionCause = _instructionName,
                        InstructionId = _instructionId,
                        SelectedEmployees = selectedEmployees.Select(e => new
                        {
                            FullName = e.FullName,
                            BirthDate = e.BirthDate,
                            Role = e.Role
                        }).ToList(),
                        NormativeInstructionNameIds = _predeterminedNormativeInstructions?.Select(n => n.Id).ToList() ?? new List<int>()
                    };

                    string json = JsonConvert.SerializeObject(assignmentPackage);
                    HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync(_assignmentEndpoint, content);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();

                        WPF.MessageBox.Show($"Внеплановый инструктаж успешно назначен {selectedEmployees.Count} сотрудникам.",
                            "Успех", WPF.MessageBoxButton.OK, WPF.MessageBoxImage.Information);

                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        string errorMessage = await response.Content.ReadAsStringAsync();
                        throw new Exception($"Ошибка сервера: {response.StatusCode} - {errorMessage}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Не удалось назначить инструктаж: {ex.Message}", ex);
            }
        }

        private void CancelButton_Click(object sender, WPF.RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}