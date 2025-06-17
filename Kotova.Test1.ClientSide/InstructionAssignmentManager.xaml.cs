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
    /// Interaction logic for InstructionAssignmentManager.xaml
    /// </summary>
    public partial class InstructionAssignmentManager : WPFWindow
    {
        private string _instructionName;
        private byte _instructionType;
        private List<EmployeeInfo> _employees;
        private List<NormativeInstructionInfo> _normativeInstructions;
        private string _jwtToken;
        private string _assignmentEndpoint;
        private string _currentUserRole; // Add this field

        private ObservableCollection<GroupAssignment> _groups = new ObservableCollection<GroupAssignment>();
        private WPFControls.ListView groupsListView;
        private WPFControls.ListView employeesListView;
        private WPFControls.ListView normativeInstructionsListView;

        public InstructionAssignmentManager(
            string instructionName,
            List<EmployeeInfo> employees,
            List<NormativeInstructionInfo> normativeInstructions,
            string jwtToken,
            string assignmentEndpoint,
            byte instructionType = 255)
        {
            _instructionName = instructionName;
            _employees = employees;
            _normativeInstructions = normativeInstructions;
            _jwtToken = jwtToken;
            _assignmentEndpoint = assignmentEndpoint;
            _instructionType = instructionType;
            _currentUserRole = GetRoleFromJWT.GetRoleFromToken(jwtToken);

            Title = $"Назначение инструктажа: {instructionName} ({AllowedRoles.GetRoleDisplayName(_currentUserRole)})";
            Width = 1000;
            Height = 700;
            WindowStartupLocation = WPF.WindowStartupLocation.CenterScreen;

            BuildUI(); // Build UI first

            // MOVED: Apply role-based filtering AFTER UI is built
            ApplyRoleBasedFiltering();
        }

        /// <summary>
        /// Applies role-based filtering to determine which employees can be assigned instructions
        /// </summary>
        private void ApplyRoleBasedFiltering()
        {
            // Add null check for employeesListView
            if (employeesListView == null)
            {
                Console.WriteLine("Warning: employeesListView is null, cannot apply filtering");
                return;
            }

            // Clear current employee list
            employeesListView.Items.Clear();

            // Get allowed roles based on current user's role
            var allowedRoles = AllowedRoles.GetAllowedRolesForAssignment(_currentUserRole);

            // Debug output
            //Console.WriteLine($"Current user role: {_currentUserRole}");
            //Console.WriteLine($"Allowed roles: {string.Join(", ", allowedRoles)}");
            //Console.WriteLine($"Total employees: {_employees?.Count ?? 0}");

            // Filter employees based on allowed roles
            if (_employees != null)
            {
                foreach (var employee in _employees)
                {
                    //Console.WriteLine($"Checking employee: {employee.FullName}, Role: {employee.Role}");

                    // Check if this employee's role is allowed for assignment
                    if (allowedRoles.Contains(employee.Role))
                    {
                        // Additional filtering based on instruction type
                        if (ShouldIncludeEmployeeForInstructionType(employee, _instructionType))
                        {
                            employeesListView.Items.Add(new WPFControls.CheckBox
                            {
                                Content = $"{employee.FullName} ({employee.BirthDate}) - {AllowedRoles.GetRoleDisplayName(employee.Role)}",
                                Tag = employee,
                                Margin = new WPF.Thickness(2)
                            });
                            //Console.WriteLine($"Added employee: {employee.FullName}");
                        }
                        /*else
                        {
                           Console.WriteLine($"Employee {employee.FullName} excluded by instruction type filter");
                        }*/
                    }
                    /*else
                    {
                        Console.WriteLine($"Employee {employee.FullName} excluded by role filter");
                    }*/
                }
            }

            //Console.WriteLine($"Final employee count in ListView: {employeesListView.Items.Count}");

            // Show information about filtering
            ShowFilteringInfo();
        }

        /// <summary>
        /// Determines if an employee should be included based on instruction type restrictions
        /// </summary>
        private bool ShouldIncludeEmployeeForInstructionType(EmployeeInfo employee, byte instructionType)
        {
            // For Вводный (0) and Внеплановый (1) instructions, 
            // don't allow assignment to DeputyChief regardless of current user role
            if (instructionType == 0 || instructionType == 1)
            {
                return employee.Role != "DeputyChief";
            }

            // For other instruction types, no additional restrictions
            return true;
        }


        /* // Add this enhanced debugging to ApplyRoleBasedFiltering method

         private void ApplyRoleBasedFiltering()
         {
             Console.WriteLine("=== APPLY ROLE FILTERING DEBUG ===");

             if (employeesListView == null)
             {
                 Console.WriteLine("ERROR: employeesListView is null! UI not built properly.");
                 WPF.MessageBox.Show("Error: Employee list view is not initialized!", "Debug Error", WPF.MessageBoxButton.OK);
                 return;
             }

             Console.WriteLine($"employeesListView found, current items count: {employeesListView.Items.Count}");
             employeesListView.Items.Clear();

             var allowedRoles = GetAllowedRolesForAssignment(_currentUserRole);
             Console.WriteLine($"Current user role: '{_currentUserRole}'");
             Console.WriteLine($"Allowed roles: [{string.Join(", ", allowedRoles)}]");

             // **CRITICAL DEBUG: Check the actual employee data**
             Console.WriteLine($"=== EMPLOYEE DATA DEBUG ===");
             Console.WriteLine($"_employees is null: {_employees == null}");
             Console.WriteLine($"_employees count: {_employees?.Count ?? 0}");

             if (_employees == null)
             {
                 Console.WriteLine("ERROR: _employees is null!");
                 WPF.MessageBox.Show("ERROR: No employee data was passed to InstructionAssignmentManager!", "Debug Error", WPF.MessageBoxButton.OK);
                 return;
             }

             if (_employees.Count == 0)
             {
                 Console.WriteLine("ERROR: _employees is empty!");
                 WPF.MessageBox.Show("ERROR: Employee list is empty!", "Debug Error", WPF.MessageBoxButton.OK);
                 return;
             }

             int addedCount = 0;
             int processedCount = 0;

             foreach (var employee in _employees)
             {
                 processedCount++;
                 Console.WriteLine($"\n--- Processing Employee {processedCount} ---");
                 Console.WriteLine($"Employee object is null: {employee == null}");

                 if (employee == null)
                 {
                     Console.WriteLine("Skipping null employee");
                     continue;
                 }

                 Console.WriteLine($"FullName: '{employee.FullName ?? "NULL"}'");
                 Console.WriteLine($"Role: '{employee.Role ?? "NULL"}'");
                 Console.WriteLine($"BirthDate: '{employee.BirthDate ?? "NULL"}'");

                 // Check if any of the employee properties are null/empty
                 if (string.IsNullOrEmpty(employee.FullName))
                 {
                     Console.WriteLine("WARNING: Employee FullName is null or empty!");
                     continue;
                 }

                 if (string.IsNullOrEmpty(employee.Role))
                 {
                     Console.WriteLine("WARNING: Employee Role is null or empty!");
                     continue;
                 }

                 // **CRITICAL: Check exact role matching**
                 Console.WriteLine($"Checking if '{employee.Role}' is in allowed roles:");
                 foreach (var allowedRole in allowedRoles)
                 {
                     bool matches = allowedRole == employee.Role;
                     Console.WriteLine($"  '{allowedRole}' == '{employee.Role}' ? {matches}");
                 }

                 bool roleAllowed = allowedRoles.Contains(employee.Role);
                 Console.WriteLine($"Role allowed result: {roleAllowed}");

                 if (roleAllowed)
                 {
                     bool instructionTypeAllowed = ShouldIncludeEmployeeForInstructionType(employee, _instructionType);
                     Console.WriteLine($"Instruction type allowed: {instructionTypeAllowed}");

                     if (instructionTypeAllowed)
                     {
                         var checkBox = new WPFControls.CheckBox
                         {
                             Content = $"{employee.FullName} ({employee.BirthDate}) - {GetRoleDisplayName(employee.Role)}",
                             Tag = employee,
                             Margin = new WPF.Thickness(2)
                         };

                         employeesListView.Items.Add(checkBox);
                         addedCount++;
                         Console.WriteLine($"✓ SUCCESSFULLY ADDED: {employee.FullName}");
                     }
                     else
                     {
                         Console.WriteLine($"✗ Excluded by instruction type filter");
                     }
                 }
                 else
                 {
                     Console.WriteLine($"✗ Excluded by role filter");
                 }
             }

             Console.WriteLine($"\n=== FILTERING SUMMARY ===");
             Console.WriteLine($"Total employees processed: {processedCount}");
             Console.WriteLine($"Total employees added: {addedCount}");
             Console.WriteLine($"Final ListView count: {employeesListView.Items.Count}");

             if (addedCount == 0)
             {
                 string message = $"No employees were added!\n\n" +
                                 $"Your role: {GetRoleDisplayName(_currentUserRole)}\n" +
                                 $"You can assign to: {string.Join(", ", allowedRoles.Select(GetRoleDisplayName))}\n" +
                                 $"Total employees checked: {processedCount}";

                 Console.WriteLine($"SHOWING MESSAGE BOX: {message}");
                 WPF.MessageBox.Show(message, "No Employees Available", WPF.MessageBoxButton.OK, WPF.MessageBoxImage.Information);
             }

             Console.WriteLine("=== APPLY ROLE FILTERING DEBUG END ===");
         }

         // Also add this method to debug the ShouldIncludeEmployeeForInstructionType method
         private bool ShouldIncludeEmployeeForInstructionType(EmployeeInfo employee, byte instructionType)
         {
             Console.WriteLine($"  ShouldIncludeEmployeeForInstructionType: Employee={employee.FullName}, Type={instructionType}");

             // For Вводный (0) and Внеплановый (1) instructions, 
             // don't allow assignment to DeputyChief regardless of current user role
             if (instructionType == 0 || instructionType == 1)
             {
                 bool shouldExclude = employee.Role == "DeputyChief";
                 Console.WriteLine($"  Instruction type {instructionType} - excluding DeputyChief: {shouldExclude}");
                 return !shouldExclude;
             }

             // For other instruction types, no additional restrictions
             Console.WriteLine($"  Instruction type {instructionType} - no additional restrictions");
             return true;
         }*/
        

        /// <summary>
        /// Shows information about current filtering applied
        /// </summary>
        private void ShowFilteringInfo()
        {
            var allowedRoles = AllowedRoles.GetAllowedRolesForAssignment(_currentUserRole);
            var roleNames = allowedRoles.Select(AllowedRoles.GetRoleDisplayName).ToList();

            string infoText = $"Доступно для назначения: {string.Join(", ", roleNames)}";

            if (_instructionType == 0 || _instructionType == 1)
            {
                infoText += "\n(Заместители исключены для данного типа инструктажа)";
            }

            // You can display this info in a status bar or info panel if needed
            Console.WriteLine($"Role filtering info: {infoText}");
        }

        

        // Rest of your existing code remains the same...
        private void BuildUI()
        {
            // Create main grid
            var grid = new WPFControls.Grid();
            grid.RowDefinitions.Add(new WPFControls.RowDefinition { Height = new WPF.GridLength(1, WPF.GridUnitType.Star) });
            grid.RowDefinitions.Add(new WPFControls.RowDefinition { Height = WPF.GridLength.Auto });

            // Create upper panel with three columns
            var mainPanel = new WPFControls.Grid();
            mainPanel.ColumnDefinitions.Add(new WPFControls.ColumnDefinition { Width = new WPF.GridLength(1, WPF.GridUnitType.Star) });
            mainPanel.ColumnDefinitions.Add(new WPFControls.ColumnDefinition { Width = new WPF.GridLength(1, WPF.GridUnitType.Star) });
            mainPanel.ColumnDefinitions.Add(new WPFControls.ColumnDefinition { Width = new WPF.GridLength(1, WPF.GridUnitType.Star) });

            // Left column - Groups
            var groupsPanel = new WPFControls.DockPanel();
            groupsPanel.LastChildFill = true;

            var groupsHeader = new WPFControls.TextBlock
            {
                Text = "Группы",
                FontWeight = WPF.FontWeights.Bold,
                Margin = new WPF.Thickness(5)
            };
            WPFControls.DockPanel.SetDock(groupsHeader, WPFControls.Dock.Top);

            var groupsButtons = new WPFControls.StackPanel { Orientation = WPFControls.Orientation.Horizontal, Margin = new WPF.Thickness(5) };
            var addGroupButton = new WPFControls.Button { Content = "Добавить группу", Margin = new WPF.Thickness(2), Padding = new WPF.Thickness(5) };
            var editGroupButton = new WPFControls.Button { Content = "Изменить группу", Margin = new WPF.Thickness(2), Padding = new WPF.Thickness(5) };
            var deleteGroupButton = new WPFControls.Button { Content = "Удалить группу", Margin = new WPF.Thickness(2), Padding = new WPF.Thickness(5) };

            addGroupButton.Click += AddGroupButton_Click;
            editGroupButton.Click += EditGroupButton_Click;
            deleteGroupButton.Click += DeleteGroupButton_Click;

            groupsButtons.Children.Add(addGroupButton);
            groupsButtons.Children.Add(editGroupButton);
            groupsButtons.Children.Add(deleteGroupButton);

            WPFControls.DockPanel.SetDock(groupsButtons, WPFControls.Dock.Top);

            groupsListView = new WPFControls.ListView
            {
                Margin = new WPF.Thickness(5),
                SelectionMode = WPFControls.SelectionMode.Single
            };
            groupsListView.SelectionChanged += GroupsListView_SelectionChanged;

            groupsListView.ItemsSource = _groups;

            groupsPanel.Children.Add(groupsHeader);
            groupsPanel.Children.Add(groupsButtons);
            groupsPanel.Children.Add(groupsListView);

            WPFControls.Grid.SetColumn(groupsPanel, 0);
            mainPanel.Children.Add(groupsPanel);

            // Middle column - Employees
            var employeesPanel = new WPFControls.DockPanel();
            employeesPanel.LastChildFill = true;

            var employeesHeader = new WPFControls.TextBlock
            {
                Text = "Доступные сотрудники",
                FontWeight = WPF.FontWeights.Bold,
                Margin = new WPF.Thickness(5)
            };
            WPFControls.DockPanel.SetDock(employeesHeader, WPFControls.Dock.Top);

            // Add info text about filtering
            var filterInfoText = new WPFControls.TextBlock
            {
                Text = GetFilteringInfoText(),
                FontSize = 10,
                Foreground = WPF.Media.Brushes.DarkBlue,
                Margin = new WPF.Thickness(5, 0, 5, 5),
                TextWrapping = WPF.TextWrapping.Wrap
            };
            WPFControls.DockPanel.SetDock(filterInfoText, WPFControls.Dock.Top);

            employeesListView = new WPFControls.ListView
            {
                Margin = new WPF.Thickness(5),
                SelectionMode = WPFControls.SelectionMode.Multiple
            };

            employeesPanel.Children.Add(employeesHeader);
            employeesPanel.Children.Add(filterInfoText);
            employeesPanel.Children.Add(employeesListView);

            WPFControls.Grid.SetColumn(employeesPanel, 1);
            mainPanel.Children.Add(employeesPanel);

            // Right column - Normative Instructions
            var normativePanel = new WPFControls.DockPanel();
            normativePanel.LastChildFill = true;

            var normativeHeader = new WPFControls.TextBlock
            {
                Text = "Нормативные инструкции",
                FontWeight = WPF.FontWeights.Bold,
                Margin = new WPF.Thickness(5)
            };
            WPFControls.DockPanel.SetDock(normativeHeader, WPFControls.Dock.Top);

            normativeInstructionsListView = new WPFControls.ListView
            {
                Margin = new WPF.Thickness(5),
                SelectionMode = WPFControls.SelectionMode.Multiple
            };

            foreach (var instruction in _normativeInstructions)
            {
                normativeInstructionsListView.Items.Add(new WPFControls.CheckBox
                {
                    Content = instruction.Name,
                    Tag = instruction,
                    Margin = new WPF.Thickness(2)
                });
            }

            normativePanel.Children.Add(normativeHeader);
            normativePanel.Children.Add(normativeInstructionsListView);

            WPFControls.Grid.SetColumn(normativePanel, 2);
            mainPanel.Children.Add(normativePanel);

            WPFControls.Grid.SetRow(mainPanel, 0);
            grid.Children.Add(mainPanel);

            // Bottom buttons
            var buttonPanel = new WPFControls.StackPanel
            {
                Orientation = WPFControls.Orientation.Horizontal,
                HorizontalAlignment = WPF.HorizontalAlignment.Right,
                Margin = new WPF.Thickness(10)
            };

            var saveButton = new WPFControls.Button
            {
                Content = "Сохранить и назначить",
                Padding = new WPF.Thickness(10, 5, 10, 5),
                Margin = new WPF.Thickness(5)
            };
            saveButton.Click += SaveButton_Click;

            var cancelButton = new WPFControls.Button
            {
                Content = "Отмена",
                Padding = new WPF.Thickness(10, 5, 10, 5),
                Margin = new WPF.Thickness(5)
            };
            cancelButton.Click += CancelButton_Click;

            buttonPanel.Children.Add(saveButton);
            buttonPanel.Children.Add(cancelButton);

            WPFControls.Grid.SetRow(buttonPanel, 1);
            grid.Children.Add(buttonPanel);

            Content = grid;
        }

        /// <summary>
        /// Gets filtering information text for display
        /// </summary>
        private string GetFilteringInfoText()
        {
            var allowedRoles = AllowedRoles.GetAllowedRolesForAssignment(_currentUserRole);
            var roleNames = allowedRoles.Select(AllowedRoles.GetRoleDisplayName).ToList();

            string baseText = $"Вы можете назначать инструктажи: {string.Join(", ", roleNames)}";

            if (_instructionType == 0 || _instructionType == 1)
            {
                baseText += "\nЗаместители исключены для данного типа инструктажа.";
            }

            return baseText;
        }

        // Validation method to check assignment permissions before saving
        private bool ValidateAssignmentPermissions()
        {
            var allowedRoles = AllowedRoles.GetAllowedRolesForAssignment(_currentUserRole);

            foreach (var group in _groups)
            {
                foreach (var employee in group.Employees)
                {
                    // Check if this employee's role is allowed
                    if (!allowedRoles.Contains(employee.Role))
                    {
                        WPF.MessageBox.Show($"У вас нет прав назначать инструктажи сотруднику {employee.FullName} (роль: {AllowedRoles.GetRoleDisplayName(employee.Role)})",
                            "Недостаточно прав", WPF.MessageBoxButton.OK, WPF.MessageBoxImage.Warning);
                        return false;
                    }

                    // Check instruction type restrictions
                    if (!ShouldIncludeEmployeeForInstructionType(employee, _instructionType))
                    {
                        WPF.MessageBox.Show($"Данный тип инструктажа не может быть назначен сотруднику {employee.FullName} (роль: {AllowedRoles.GetRoleDisplayName(employee.Role)})",
                            "Ограничение по типу инструктажа", WPF.MessageBoxButton.OK, WPF.MessageBoxImage.Warning);
                        return false;
                    }
                }
            }

            return true;
        }

        // Rest of your existing event handlers remain the same, but add validation to SaveButton_Click
        private async void SaveButton_Click(object sender, WPF.RoutedEventArgs e)
        {
            if (_groups.Count == 0)
            {
                WPF.MessageBox.Show("Добавьте хотя бы одну группу.");
                return;
            }

            // Validate assignment permissions
            if (!ValidateAssignmentPermissions())
            {
                return; // Validation failed, don't proceed
            }

            if (WPF.MessageBox.Show("Вы уверены, что хотите сохранить и назначить инструктаж выбранным группам?",
                "Подтверждение", WPF.MessageBoxButton.YesNo) == WPF.MessageBoxResult.Yes)
            {
                try
                {
                    int successCount = 0;
                    int totalAssignments = 0;

                    foreach (var group in _groups)
                    {
                        // Skip empty groups
                        if (group.Employees.Count == 0 || group.NormativeInstructionIds.Count == 0)
                            continue;

                        // Convert employees to tuples
                        var employeeTuples = group.Employees
                            .Select(e => Tuple.Create(e.FullName, e.BirthDate))
                            .ToList();

                        // Create package
                        InstructionPackage package = new InstructionPackage(
                            employeeTuples,
                            _instructionName,
                            group.NormativeInstructionIds
                        );

                        // Serialize and encrypt
                        string jsonData = JsonConvert.SerializeObject(package);
                        string encryptedJsonData = Encryption_Kotova.EncryptString(jsonData);

                        // Send request
                        using (var httpClient = new HttpClient())
                        {
                            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);

                            var uri = new Uri(_assignmentEndpoint);
                            var content = new StringContent(encryptedJsonData, Encoding.UTF8, "application/json");

                            var response = await httpClient.PostAsync(uri, content);

                            if (response.IsSuccessStatusCode)
                            {
                                successCount++;
                            }
                            else
                            {
                                var errorMessage = await response.Content.ReadAsStringAsync();
                                WPF.MessageBox.Show($"Ошибка при назначении группы '{group.Name}': {errorMessage}");
                            }
                        }

                        totalAssignments++;
                    }

                    if (successCount == totalAssignments)
                    {
                        WPF.MessageBox.Show("Все группы успешно назначены.");
                        DialogResult = true;
                        Close();
                    }
                    else
                    {
                        WPF.MessageBox.Show($"Назначено {successCount} из {totalAssignments} групп.");
                    }
                }
                catch (Exception ex)
                {
                    WPF.MessageBox.Show($"Ошибка при назначении: {ex.Message}");
                }
            }
        }

        // Keep all your other existing event handlers (GroupsListView_SelectionChanged, AddGroupButton_Click, etc.)
        private void GroupsListView_SelectionChanged(object sender, WPFControls.SelectionChangedEventArgs e)
        {
            if (groupsListView.SelectedItem is GroupAssignment selectedGroup)
            {
                // Update employee and normative instruction selection based on group
                foreach (WPFControls.CheckBox cb in employeesListView.Items)
                {
                    if (cb.Tag is EmployeeInfo employee)
                    {
                        bool isSelected = selectedGroup.Employees.Any(emp =>
                            emp.FullName == employee.FullName &&
                            emp.BirthDate == employee.BirthDate);

                        cb.IsChecked = isSelected;
                    }
                }

                foreach (WPFControls.CheckBox cb in normativeInstructionsListView.Items)
                {
                    if (cb.Tag is NormativeInstructionInfo instruction)
                    {
                        cb.IsChecked = selectedGroup.NormativeInstructionIds.Contains(instruction.Id);
                    }
                }
            }
            else
            {
                // Clear all selections
                foreach (WPFControls.CheckBox cb in employeesListView.Items)
                {
                    cb.IsChecked = false;
                }

                foreach (WPFControls.CheckBox cb in normativeInstructionsListView.Items)
                {
                    cb.IsChecked = false;
                }
            }
        }

        private void AddGroupButton_Click(object sender, WPF.RoutedEventArgs e)
        {
            var groupName = "Группа " + (_groups.Count + 1);

            var inputDialog = new TextInputDialog("Введите название группы", groupName);
            if (inputDialog.ShowDialog() == true)
            {
                var newGroup = new GroupAssignment
                {
                    Name = inputDialog.InputText,
                    Employees = new List<EmployeeInfo>(),
                    NormativeInstructionIds = new List<int>()
                };

                _groups.Add(newGroup);
                groupsListView.SelectedItem = newGroup;
            }
        }

        private void EditGroupButton_Click(object sender, WPF.RoutedEventArgs e)
        {
            if (groupsListView.SelectedItem is GroupAssignment selectedGroup)
            {
                // Get selected employees
                var selectedEmployees = new List<EmployeeInfo>();
                foreach (WPFControls.CheckBox cb in employeesListView.Items)
                {
                    if (cb.IsChecked == true && cb.Tag is EmployeeInfo employee)
                    {
                        selectedEmployees.Add(employee);
                    }
                }

                // Get selected normative instructions
                var selectedInstructionIds = new List<int>();
                foreach (WPFControls.CheckBox cb in normativeInstructionsListView.Items)
                {
                    if (cb.IsChecked == true && cb.Tag is NormativeInstructionInfo instruction)
                    {
                        selectedInstructionIds.Add(instruction.Id);
                    }
                }

                // Require at least one employee and one normative instruction
                if (selectedEmployees.Count == 0)
                {
                    WPF.MessageBox.Show("Выберите хотя бы одного сотрудника.");
                    return;
                }

                if (selectedInstructionIds.Count == 0)
                {
                    WPF.MessageBox.Show("Выберите хотя бы одну нормативную инструкцию.");
                    return;
                }

                // Update the group
                selectedGroup.Employees = selectedEmployees;
                selectedGroup.NormativeInstructionIds = selectedInstructionIds;

                // Refresh the ListView
                groupsListView.Items.Refresh();
            }
            else
            {
                WPF.MessageBox.Show("Выберите группу для изменения.");
            }
        }

        private void DeleteGroupButton_Click(object sender, WPF.RoutedEventArgs e)
        {
            if (groupsListView.SelectedItem is GroupAssignment selectedGroup)
            {
                if (WPF.MessageBox.Show($"Вы уверены, что хотите удалить группу '{selectedGroup.Name}'?",
                    "Подтверждение удаления", WPF.MessageBoxButton.YesNo) == WPF.MessageBoxResult.Yes)
                {
                    _groups.Remove(selectedGroup);
                }
            }
            else
            {
                WPF.MessageBox.Show("Выберите группу для удаления.");
            }
        }

        private void CancelButton_Click(object sender, WPF.RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

    // Keep your existing GroupAssignment and TextInputDialog classes unchanged
    public class GroupAssignment : INotifyPropertyChanged
    {
        private string _name;
        private List<EmployeeInfo> _employees = new List<EmployeeInfo>();
        private List<int> _normativeInstructionIds = new List<int>();

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(DisplayText));
            }
        }

        public List<EmployeeInfo> Employees
        {
            get => _employees;
            set
            {
                _employees = value;
                OnPropertyChanged(nameof(Employees));
                OnPropertyChanged(nameof(DisplayText));
            }
        }

        public List<int> NormativeInstructionIds
        {
            get => _normativeInstructionIds;
            set
            {
                _normativeInstructionIds = value;
                OnPropertyChanged(nameof(NormativeInstructionIds));
                OnPropertyChanged(nameof(DisplayText));
            }
        }

        public string DisplayText => $"{Name} ({Employees.Count} сотрудников, {NormativeInstructionIds.Count} инструкций)";

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TextInputDialog : WPFWindow
    {
        private WPFControls.TextBox textBox;
        public string InputText { get; private set; }

        public TextInputDialog(string prompt, string defaultText = "")
        {
            Title = prompt;
            Width = 300;
            Height = 150;
            WindowStartupLocation = WPF.WindowStartupLocation.CenterOwner;

            var grid = new WPFControls.Grid();
            grid.RowDefinitions.Add(new WPFControls.RowDefinition { Height = new WPF.GridLength(1, WPF.GridUnitType.Star) });
            grid.RowDefinitions.Add(new WPFControls.RowDefinition { Height = WPF.GridLength.Auto });

            textBox = new WPFControls.TextBox { Margin = new WPF.Thickness(10), Text = defaultText };
            WPFControls.Grid.SetRow(textBox, 0);
            grid.Children.Add(textBox);

            var buttonPanel = new WPFControls.StackPanel
            {
                Orientation = WPFControls.Orientation.Horizontal,
                HorizontalAlignment = WPF.HorizontalAlignment.Right,
                Margin = new WPF.Thickness(10)
            };

            var okButton = new WPFControls.Button
            {
                Content = "OK",
                IsDefault = true,
                Width = 60,
                Margin = new WPF.Thickness(5)
            };
            okButton.Click += (s, e) =>
            {
                InputText = textBox.Text;
                DialogResult = true;
                Close();
            };

            var cancelButton = new WPFControls.Button
            {
                Content = "Отмена",
                IsCancel = true,
                Width = 60,
                Margin = new WPF.Thickness(5)
            };
            cancelButton.Click += (s, e) =>
            {
                DialogResult = false;
                Close();
            };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);

            WPFControls.Grid.SetRow(buttonPanel, 1);
            grid.Children.Add(buttonPanel);

            Content = grid;

            Loaded += (s, e) => textBox.Focus();
        }
    }
}