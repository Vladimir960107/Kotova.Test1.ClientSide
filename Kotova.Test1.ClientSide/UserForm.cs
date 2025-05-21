using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Kotova.Test1.ClientSide.Login_Russian;

using System.Text.Json;
using System.Diagnostics;
using Kotova.CommonClasses;
using Microsoft.AspNetCore.SignalR.Client;
using System.Reflection;
using System.Configuration;
using System.Timers;
using System.Net.Http;
using System.IO;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;
using System.Runtime.InteropServices;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Windows.Controls;
using Control = System.Windows.Forms.Control;
using System.Windows.Forms.Integration;
using WPF = System.Windows;
using Windows.UI.WindowManagement;
using WinFormsColor = System.Drawing.Color;
using WpfColor = System.Windows.Media.Color;
using WpfWindow = System.Windows.Window;
using WinFormsForm = System.Windows.Forms.Form;



namespace Kotova.Test1.ClientSide
{
    public partial class UserForm : Form
    {

        [DllImport("kernel32.dll")]
        static extern bool AllocConsole();

        public const string dB_pos_users_isInstructionPassed = "is_instruction_passed";
        public const string dB_pos_users_causeOfInstruction = "cause_of_instruction";
        public const string dB_pos_users_pathToInstruction = "path_to_instruction";

        public Dictionary<Control, Rectangle> controlsOriginalSizes;

        private static List<WpfWindow> _openWpfWindows = new List<WpfWindow>();

        public const string dB_instructionId = "instruction_id"; //ВЫНЕСИ ЭТИ СЛЕДУЮЩИЕ СТРОЧКИ В ОБЩИЙ ФАЙЛ!
        public const string db_filePath = "file_path";
        public const string db_typeOfInstruction = "type_of_instruction";
        public const string db_dateOfInstructionWasSentToUser = "when_was_send_to_user";

        // Added these new constants
        public const string db_normativeInstructionId = "id";
        public const string db_normativeInstructionName = "name";
        public const string db_normativeInstructionUrl = "url";

        // Changed global variable names
        private List<Dictionary<string, object>> listsOfNormativeInstructionsOfNewInstr_global; // was listsOfPathsOfNewInstr_global
        private List<Dictionary<string, object>> listsOfNormativeInstructionsOfOldInstr_global; // was listsOfPathsOfOldInstr_global

        private bool _IsInstructionSelected = false;
        private List<Dictionary<string, object>> listsOfPathsOfNewInstr_global;

        private List<Dictionary<string, object>> listOfNewInstructions_global;

        private List<Dictionary<string, object>> listsOfPathsOfOldInstr_global;

        private List<Dictionary<string, object>> listOfOldInstructions_global;

        private NotifyIcon notifyIcon;

        private static bool canYouCloseTheApplication = false;

        private InstructionService _instructionService;


        public Login_Russian? _loginForm;
        public SignUpForm _signUpForm;
        string? _userName;
        static readonly string DownloadInstructionForUserURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get-not-passed-instructions";
        static readonly string DownloadOldInstructionForUserURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get-passed-instructions";
        static readonly string SendInstructionIsPassedURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/mark-instruction-as-passed";


        private HubConnection? _hubConnection = null;

        public UserForm()
        {
            InitializeComponent();
            InitializeSignalRConnection();
            ToastNotificationManagerCompat.OnActivated += ToastNotificationManagerCompat_OnActivated;
        }

        private void ToastNotificationManagerCompat_OnActivated(ToastNotificationActivatedEventArgsCompat e)
        {
            MessageBox.Show("I've got something!");
            // Parse the arguments from the toast
            var args = ToastArguments.Parse(e.Argument);

            // Check if the action argument is "openApp"
            if (args["action"] == "openApp")
            {
                // Bring the application to the foreground
                Application.OpenForms[0]?.Invoke(new Action(() =>
                {
                    this.Show();
                    this.WindowState = FormWindowState.Normal;
                    this.BringToFront();
                    this.Activate();
                }));
            }
        }

        public UserForm(Login_Russian loginForm, string userName, string fullName, string departmentName)
        {
            InitializeComponent();

            // Initialize the service with the JWT token from the login form
            _instructionService = new InstructionService(loginForm._jwtToken, new FormLogger(this));

            // The rest of your constructor code...
            StartTimer();
            exitApplicationToolStripMenuItem.Enabled = false;
            _loginForm = loginForm;
            _userName = userName;
            UserLabel.Text = _userName;
            PassInstruction.Enabled = false;
            InitializeSignalRConnection();

            _signUpForm = new SignUpForm(loginForm, this);
            _ = RefreshNewInstructionsInternal();
            _ = RefreshOldInstructionsInternal();

            InitializeWpfIntegration();
        }

        // Logger implementation for the form
        private class FormLogger : InstructionService.ILogger
        {
            private readonly UserForm _form;

            public FormLogger(UserForm form)
            {
                _form = form;
            }

            public void LogInfo(string message)
            {
                Console.WriteLine($"INFO: {message}");
            }

            public void LogError(string message, Exception ex = null)
            {
                string errorMessage = ex != null ? $"{message}: {ex.Message}" : message;
                Console.WriteLine($"ERROR: {errorMessage}");

                // Optionally show a message box for critical errors
                // _form.Invoke(new Action(() => MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)));
            }
        }

        #region WPF Integration for UserForm

        /// <summary>
        /// Opens the WPF instruction viewer window
        /// </summary>
        private void ShowWpfInstructionViewer()
        {
            try
            {
                // Check if window is already open
                var existingWindow = _openWpfWindows.OfType<InstructionViewerWindow>().FirstOrDefault();
                if (existingWindow != null && existingWindow.IsLoaded)
                {
                    // Bring existing window to front
                    existingWindow.Activate();
                    existingWindow.WindowState = System.Windows.WindowState.Normal;
                    return;
                }

                // Create new WPF window
                var wpfWindow = new InstructionViewerWindow(_loginForm._jwtToken, _userName, isChief: false);
                wpfWindow.Title = $"Просмотр инструктажей - {_userName}";

                // Center over parent form
                CenterWpfWindowOverWinForm(wpfWindow, this);

                // Apply theme to match parent
                ApplyWinFormThemeToWpf(wpfWindow, this.BackColor);

                // Set icon safely
                SetWpfWindowIconSafe(wpfWindow, this);

                // Show window
                wpfWindow.Show();

                // Track the window
                TrackWpfWindow(wpfWindow);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии окна просмотра инструктажей: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Centers WPF window over Windows Forms parent
        /// </summary>
        private static void CenterWpfWindowOverWinForm(WpfWindow wpfWindow, WinFormsForm parentForm)
        {
            if (parentForm != null && wpfWindow != null)
            {
                // Ensure WPF window size is loaded
                wpfWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;

                // Calculate center position
                var parentLocation = parentForm.Location;
                var parentSize = parentForm.Size;

                wpfWindow.Left = parentLocation.X + (parentSize.Width - wpfWindow.Width) / 2;
                wpfWindow.Top = parentLocation.Y + (parentSize.Height - wpfWindow.Height) / 2;

                // Ensure window is visible on screen
                var screen = Screen.FromControl(parentForm);
                if (wpfWindow.Left < screen.WorkingArea.Left)
                    wpfWindow.Left = screen.WorkingArea.Left;
                if (wpfWindow.Top < screen.WorkingArea.Top)
                    wpfWindow.Top = screen.WorkingArea.Top;
                if (wpfWindow.Left + wpfWindow.Width > screen.WorkingArea.Right)
                    wpfWindow.Left = screen.WorkingArea.Right - wpfWindow.Width;
                if (wpfWindow.Top + wpfWindow.Height > screen.WorkingArea.Bottom)
                    wpfWindow.Top = screen.WorkingArea.Bottom - wpfWindow.Height;
            }
        }

        /// <summary>
        /// Applies Windows Forms theme colors to WPF window
        /// </summary>
        private static void ApplyWinFormThemeToWpf(WpfWindow wpfWindow, WinFormsColor formsBackColor)
        {
            try
            {
                // Convert Windows Forms color to WPF color
                var wpfColor = WpfColor.FromArgb(
                    formsBackColor.A,
                    formsBackColor.R,
                    formsBackColor.G,
                    formsBackColor.B);

                wpfWindow.Background = new System.Windows.Media.SolidColorBrush(wpfColor);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not apply theme: {ex.Message}");
            }
        }

        /// <summary>
        /// Safely sets icon for WPF window
        /// </summary>
        private static void SetWpfWindowIconSafe(WpfWindow wpfWindow, WinFormsForm parentForm)
        {
            try
            {
                // Try to get icon from parent form
                if (parentForm?.Icon != null)
                {
                    var bitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                        parentForm.Icon.Handle,
                        System.Windows.Int32Rect.Empty,
                        System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                    wpfWindow.Icon = bitmap;
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not set icon from parent: {ex.Message}");
            }

            // Fallback: Create simple programmatic icon
            try
            {
                CreateSimpleWpfIcon(wpfWindow);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not create fallback icon: {ex.Message}");
                // No icon is fine too
            }
        }

        /// <summary>
        /// Creates a simple blue square icon programmatically
        /// </summary>
        private static void CreateSimpleWpfIcon(WpfWindow wpfWindow)
        {
            var drawingGroup = new System.Windows.Media.DrawingGroup();

            // Create a simple blue square icon
            var geometryDrawing = new System.Windows.Media.GeometryDrawing(
                new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.DodgerBlue),
                new System.Windows.Media.Pen(new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Navy), 1),
                new System.Windows.Media.RectangleGeometry(new System.Windows.Rect(0, 0, 16, 16))
            );

            drawingGroup.Children.Add(geometryDrawing);
            var drawingImage = new System.Windows.Media.DrawingImage(drawingGroup);
            wpfWindow.Icon = drawingImage;
        }

        /// <summary>
        /// Tracks WPF windows to prevent duplicates and manage cleanup
        /// </summary>
        private static void TrackWpfWindow(WpfWindow window)
        {
            // Clean up closed windows first
            _openWpfWindows.RemoveAll(w => w == null || !w.IsLoaded);

            // Add new window to tracking
            _openWpfWindows.Add(window);

            // Remove from tracking when window is closed
            window.Closed += (s, e) => _openWpfWindows.Remove(window);
        }

        /// <summary>
        /// Closes all tracked WPF windows
        /// </summary>
        private static void CloseAllWpfWindows()
        {
            foreach (var window in _openWpfWindows.ToList())
            {
                try
                {
                    if (window != null && window.IsLoaded)
                    {
                        window.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error closing WPF window: {ex.Message}");
                }
            }
            _openWpfWindows.Clear();
        }

        #endregion

        #region UserForm Integration Methods

        /// <summary>
        /// Initialize WPF integration features
        /// Call this in UserForm constructor after InitializeComponent()
        /// </summary>
        private void InitializeWpfIntegration()
        {
            // Add to context menu
            AddWpfViewerToContextMenu();

            // Add button to form
            //AddWpfViewerButton();

            // Handle form closing to cleanup WPF windows
            this.FormClosing += UserForm_FormClosing_WpfCleanup;
        }

        /// <summary>
        /// Adds WPF viewer option to the context menu
        /// </summary>
        private void AddWpfViewerToContextMenu()
        {
            try
            {
                if (AdditionalSettingsForUserContextMenuStrip != null)
                {
                    // Add separator
                    AdditionalSettingsForUserContextMenuStrip.Items.Add(new ToolStripSeparator());

                    // Add WPF viewer menu item
                    var wpfViewerMenuItem = new ToolStripMenuItem("Новый просмотрщик инструктажей (WPF)")
                    {
                        ToolTipText = "Открыть современный просмотрщик инструктажей",
                        BackColor = System.Drawing.Color.LightBlue
                    };
                    wpfViewerMenuItem.Click += (sender, e) => ShowWpfInstructionViewer();

                    AdditionalSettingsForUserContextMenuStrip.Items.Add(wpfViewerMenuItem);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding WPF viewer to context menu: {ex.Message}");
            }
        }

        /*/// <summary>
        /// Adds a button to open WPF viewer
        /// </summary>
        private void AddWpfViewerButton()
        {
            try
            {
                var btnWpfViewer = new WPF.Forms.Button
                {
                    Text = "WPF Просмотрщик",
                    Size = new Size(160, 35),
                    Location = new Point(10, 10),
                    BackColor = WinFormsColor.LightGreen,
                    ForeColor = WinFormsColor.Black,
                    UseVisualStyleBackColor = false,
                    Font = new System.Drawing.Font("Segoe UI", 9F, FontStyle.Regular),
                    Cursor = Cursors.Hand,
                    FlatStyle = FlatStyle.Flat
                };

                btnWpfViewer.FlatAppearance.BorderColor = WinFormsColor.DarkGreen;
                btnWpfViewer.FlatAppearance.BorderSize = 1;
                btnWpfViewer.FlatAppearance.MouseOverBackColor = WinFormsColor.LimeGreen;

                btnWpfViewer.Click += (sender, e) => ShowWpfInstructionViewer();

                // Add to the form (you might want to add it to a specific panel)
                this.Controls.Add(btnWpfViewer);
                btnWpfViewer.BringToFront();

                // Optional: Add tooltip
                var toolTip = new WPF.Forms.ToolTip();
                toolTip.SetToolTip(btnWpfViewer, "Открыть новый просмотрщик инструктажей с современным интерфейсом");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding WPF viewer button: {ex.Message}");
            }
        }*/

        /// <summary>
        /// Handles form closing to cleanup WPF windows
        /// </summary>
        private void UserForm_FormClosing_WpfCleanup(object sender, FormClosingEventArgs e)
        {
            CloseAllWpfWindows();
        }


        #endregion




        public void EnableExitTheProgrammEntirelyButton()
        {
            // We're on the UI thread, so we can directly modify the control
            exitApplicationToolStripMenuItem.Enabled = true;
        }

        public void StartTimer()
        {
            System.Timers.Timer timer = new System.Timers.Timer(6000); // 6 seconds. Cause because if not initialized properly everything - this will cause the programm to throw exception. so we wait :)
            timer.Elapsed += (sender, e) =>
            {
                EnableExitTheProgrammEntirelyButton();
                timer.Dispose();
            };
            timer.AutoReset = false;
            timer.Start();
        }


        private async void InitializeSignalRConnection()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(ConfigurationClass.BASE_SIGNALR_CONNECTION_URL_DEVELOPMENT, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult(_loginForm._jwtToken);
                })
                .Build();

            _hubConnection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                // Handle incoming messages from the SignalR hub
                MessageBox.Show($"{user}: {message}", "Message from Hub");
            });

            _hubConnection.On<string>("ReceiveAlert", message =>
            {
                Notifications.ShowWindowsNotification("Alert", message);
            });

            try
            {
                await _hubConnection.StartAsync();
                //MessageBox.Show("Подключён к SignalR hub.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось подключиться к SignalR hub: {ex.Message}");
            }
        }

        #region Download From Server Old and New Instructions

        private async void CheckForNewInstructions_Click(object sender, EventArgs e)
        {
            await RefreshNewInstructionsInternal();
            await RefreshOldInstructionsInternal();
        }
        // Updated refresh methods using the service
        private async Task<bool> RefreshNewInstructionsInternal()
        {
            try
            {
                ListOfInstructionsForUser.Items.Clear();

                var result = await _instructionService.GetNotPassedInstructionsAsync();

                if (result == null)
                {
                    MessageBox.Show("Все инструктажи пройдены!");
                    return true;
                }

                // Update the global lists
                listOfNewInstructions_global = result.Value.Instructions;
                listsOfNormativeInstructionsOfNewInstr_global = result.Value.NormativeInstructions; // Changed from Paths


                // Update the UI
                foreach (var instruction in listOfNewInstructions_global)
                {
                    if (instruction.ContainsKey("cause_of_instruction") && instruction["cause_of_instruction"] != null)
                    {
                        ListOfInstructionsForUser.Items.Add(instruction["cause_of_instruction"].ToString());
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in RefreshNewInstructionsInternal: {ex}");
                MessageBox.Show($"Error refreshing instructions: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> DownloadInstructionsForUserFromServer(string? userName)
        {
            if (userName is null)
            {
                throw new ArgumentNullException(nameof(userName));
            }

            string url = DownloadInstructionForUserURL;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string jwtToken = _loginForm._jwtToken;
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var jsonResponse = await response.Content.ReadAsStringAsync();

                    // Debug: Log the raw response to see what we're actually getting
                    Console.WriteLine($"Server response: {jsonResponse}");

                    // Try parsing the response as a list of instruction objects
                    var instructions = JsonSerializer.Deserialize<List<InstructionDto>>(jsonResponse,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (instructions == null || instructions.Count == 0)
                    {
                        Console.WriteLine("No not-passed instructions found.");
                        return true; // All instructions are passed
                    }

                    // Process the instructions
                    ListOfInstructionsForUser.Items.Clear();

                    // Create our Result1 format that the rest of the code expects
                    listOfNewInstructions_global = instructions.Select(instr => new Dictionary<string, object>
            {
                { dB_instructionId, instr.InstructionId },
                { dB_pos_users_causeOfInstruction, instr.Cause },
                { db_typeOfInstruction, instr.Type ?? "Unknown" },
                { db_dateOfInstructionWasSentToUser, instr.WhenAssigned }
            }).ToList();

                    // Create our Result2 format that the rest of the code expects
                    // Create our Result2 format that the rest of the code expects
                    listsOfPathsOfNewInstr_global = new List<Dictionary<string, object>>();

                    // For each instruction, create a separate dictionary entry for each file path
                    foreach (var instruction in instructions)
                    {
                        if (instruction.FilePaths != null && instruction.FilePaths.Any())
                        {
                            foreach (var filePath in instruction.FilePaths)
                            {
                                listsOfPathsOfNewInstr_global.Add(new Dictionary<string, object>
                        {
                            { dB_instructionId, instruction.InstructionId },
                            { db_filePath, filePath }
                        });
                            }
                        }
                        else
                        {
                            // Add a single entry with null path if there are no paths
                            listsOfPathsOfNewInstr_global.Add(new Dictionary<string, object>
                    {
                        { dB_instructionId, instruction.InstructionId },
                        { db_filePath, null }
                    });
                        }
                    }

                    // Populate the UI with instruction causes
                    foreach (var instruction in instructions)
                    {
                        ListOfInstructionsForUser.Items.Add(instruction.Cause);
                    }

                    return false; // Not all instructions are passed
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                Console.WriteLine($"Exception in DownloadInstructionsForUserFromServer: {ex}");
                return false; // Avoid indicating all instructions are passed when an error occurs
            }
        }

        // Add a new DTO class to match the server response format
        public class InstructionDto
        {
            public int InstructionId { get; set; }
            public string Cause { get; set; }
            public DateTime BeginDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Type { get; set; }
            public DateTime? WhenAssigned { get; set; }
            public List<string> FilePaths { get; set; }
        }

        private async Task<bool> RefreshOldInstructionsInternal()
        {
            try
            {
                dataGridViewPassedInstructions.Rows.Clear();

                var result = await _instructionService.GetPassedInstructionsAsync();

                if (result == null)
                {
                    MessageBox.Show("Нет пройденных инструктажей.");
                    return true;
                }

                // Update the global lists
                listOfOldInstructions_global = result.Value.Instructions;
                listsOfNormativeInstructionsOfOldInstr_global = result.Value.NormativeInstructions; // Changed from Paths

                // Update the UI
                this.Invoke(new Action(() =>
                {
                    foreach (var instruction in listOfOldInstructions_global)
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

                        dataGridViewPassedInstructions.Rows.Add(
                            whenPassed?.ToString("yyyy-MM-dd"),
                            type,
                            cause
                        );
                    }
                }));

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in RefreshOldInstructionsInternal: {ex}");
                MessageBox.Show($"Error refreshing passed instructions: {ex.Message}");
                return false;
            }
        }

        // Also fix the method to download old instructions
        private async Task<bool?> DownloadOldInstructionsForUserFromServer(string? userName)
        {
            if (userName is null)
            {
                throw new ArgumentNullException(nameof(userName));
            }

            string url = DownloadOldInstructionForUserURL;

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string jwtToken = _loginForm._jwtToken;
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var jsonResponse = await response.Content.ReadAsStringAsync();

                    // Debug: Log the raw JSON response
                    Console.WriteLine($"Raw JSON Response for passed instructions: {jsonResponse}");

                    // Use more flexible JSON options
                    var jsonSerializerOptions = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        AllowTrailingCommas = true,
                        ReadCommentHandling = JsonCommentHandling.Skip
                    };

                    List<PassedInstructionDto> passedInstructions;
                    try
                    {
                        passedInstructions = JsonSerializer.Deserialize<List<PassedInstructionDto>>(jsonResponse, jsonSerializerOptions);
                    }
                    catch (JsonException ex)
                    {
                        // More detailed error logging
                        Console.WriteLine($"JSON Deserialization Error: {ex.Message}");
                        MessageBox.Show($"JSON Parsing Error: {ex.Message}\n\nResponse: {jsonResponse}");
                        return null;
                    }

                    if (passedInstructions == null || passedInstructions.Count == 0)
                    {
                        return true;
                    }

                    // Initialize the global lists before populating them
                    listOfOldInstructions_global = new List<Dictionary<string, object>>();
                    listsOfPathsOfOldInstr_global = new List<Dictionary<string, object>>();

                    // Clear existing rows
                    this.Invoke(new Action(() =>
                    {
                        dataGridViewPassedInstructions.Rows.Clear();
                    }));

                    // Populate the grid with passed instructions
                    foreach (var instruction in passedInstructions)
                    {
                        this.Invoke(new Action(() =>
                        {
                            dataGridViewPassedInstructions.Rows.Add(
                                instruction.WhenPassed?.ToString("yyyy-MM-dd"),
                                instruction.Type ?? "Unknown Type",
                                instruction.Cause ?? "Unknown Cause"
                            );
                        }));
                    }

                    // Store the full instruction data for later use
                    listOfOldInstructions_global = passedInstructions.Select(i => new Dictionary<string, object>
            {
                { dB_instructionId, i.InstructionId },
                { dB_pos_users_causeOfInstruction, i.Cause ?? "" },
                { db_typeOfInstruction, i.Type ?? "" },
                { db_dateOfInstructionWasSentToUser, i.WhenPassed }
            }).ToList();

                    // Store file paths for later use - handle multiple file paths per instruction
                    foreach (var instruction in passedInstructions)
                    {
                        if (instruction.FilePaths != null && instruction.FilePaths.Any())
                        {
                            foreach (var filePath in instruction.FilePaths)
                            {
                                listsOfPathsOfOldInstr_global.Add(new Dictionary<string, object>
                        {
                            { dB_instructionId, instruction.InstructionId },
                            { db_filePath, filePath }
                        });
                            }
                        }
                        else
                        {
                            listsOfPathsOfOldInstr_global.Add(new Dictionary<string, object>
                    {
                        { dB_instructionId, instruction.InstructionId },
                        { db_filePath, null }
                    });
                        }
                    }

                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"HTTP Request Error: {ex.Message}");
                Console.WriteLine($"HTTP Request Error: {ex}");
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected Error: {ex.Message}");
                Console.WriteLine($"Unexpected Error: {ex}");
                return null;
            }
        }

        // Modify the DTO to match potential variations in the JSON
        private class PassedInstructionDto
        {
            public int InstructionId { get; set; }
            public string Cause { get; set; }
            public DateTime? BeginDate { get; set; }
            public DateTime? EndDate { get; set; }
            public string Type { get; set; }
            public DateTime? WhenPassed { get; set; }
            public List<string> FilePaths { get; set; } = new List<string>();
        }
        #endregion

        #region Unpassed instructions list value changed (select instruction)
        private void ListOfInstructions_SelectedValueChanged(object sender, EventArgs e)
        {
            FilesOfInstructionCheckedListBox.Items.Clear();
            _IsInstructionSelected = false;
            PassInstruction.Enabled = false;

            if (ListOfInstructionsForUser.SelectedItem == null)
            {
                MessageBox.Show("Вы не выбрали инструктаж.");
                return;
            }

            Dictionary<string, object> selectedDict = GetDictFromSelectedInstruction(ListOfInstructionsForUser.SelectedItem.ToString());
            int instructionId = Convert.ToInt32(selectedDict[dB_instructionId].ToString());

            // Check if this is an introductory instruction (type 0) - these don't require file checking
            string instructionType = selectedDict[db_typeOfInstruction].ToString();
            if (instructionType == "0")
            {
                // For introductory instructions, enable PassInstruction immediately
                _IsInstructionSelected = true;
                PassInstruction.Enabled = true;
                return;
            }

            // For other instruction types, load normative instructions
            bool hasNormativeInstructions = false;
            foreach (var normativeInstruction in listsOfNormativeInstructionsOfNewInstr_global)
            {
                if (Convert.ToInt32(normativeInstruction[dB_instructionId].ToString()) == instructionId)
                {
                    string displayText = $"{normativeInstruction[db_normativeInstructionName]} ({normativeInstruction[db_normativeInstructionUrl]})";
                    FilesOfInstructionCheckedListBox.Items.Add(displayText);
                    hasNormativeInstructions = true;
                }
            }

            if (hasNormativeInstructions)
            {
                _IsInstructionSelected = true;
                // PassInstruction will be enabled only when all normative instructions are checked
                PassInstruction.Enabled = false;
            }
            else
            {
                // If no normative instructions found, something might be wrong
                MessageBox.Show("Для данного инструктажа не найдены нормативные документы.");
                _IsInstructionSelected = false;
                PassInstruction.Enabled = false;
            }
        }

        #endregion

        #region Passed instructions list value changed (select instruction)
        private void ListOfPassedInstructions_SelectedValueChanged(object sender, EventArgs e)
        {
            FilesOfInstructionCheckedListBox.Items.Clear();
            if (ListOfInstructionsForUser.SelectedItem == null)
            {
                MessageBox.Show("Вы не выбрали инструктаж.");
                PassInstruction.Enabled = false;
                return;
            }
            Dictionary<string, object> selectedDict = GetDictFromSelectedInstruction(ListOfInstructionsForUser.SelectedItem.ToString());
            int instructionId = Convert.ToInt32(selectedDict[dB_instructionId].ToString());
            foreach (var listOfPaths in listsOfPathsOfNewInstr_global)
            {
                if (Convert.ToInt32(listOfPaths[dB_instructionId].ToString()) == instructionId)
                {
                    if (listOfPaths[db_filePath] == null)
                    {
                        if (selectedDict[db_typeOfInstruction].ToString() == "0") // Проверка что мы входим в вводный инструктаж только!
                        {
                            _IsInstructionSelected = true;
                            PassInstruction.Enabled = true;
                            return;
                        }
                        else
                        {
                            MessageBox.Show("Ooops, Что-то пошло не так. Проверь эту строчку на предмет присутствия файлов инструктажа!");
                            PassInstruction.Enabled = false;
                            return;
                        }

                    }
                    FilesOfInstructionCheckedListBox.Items.Add(listOfPaths[db_filePath].ToString());
                }
            }

            _IsInstructionSelected = true;
        }

        #endregion

        private void HyperLinkForInstructionsFolder_Click(object sender, EventArgs e)
        {
            if (ListOfInstructionsForUser.SelectedItem == null)
            {
                MessageBox.Show("Вы не выбрали инструктаж.");
                PassInstruction.Enabled = false;
                return;
            }
            Dictionary<string, object> selectedDict = GetDictFromSelectedInstruction(ListOfInstructionsForUser.SelectedItem.ToString()); //most likely suppress it, cause its not null.
            if (selectedDict[db_typeOfInstruction].ToString() == "0")
            {
                MessageBox.Show("Вводный инструктаж не имеет связанных файлов.");
                return;
            }
            int instructionId = Convert.ToInt32(selectedDict[dB_instructionId].ToString());
            var firstNormativeInstruction = listsOfNormativeInstructionsOfNewInstr_global
                .FirstOrDefault(ni => Convert.ToInt32(ni[dB_instructionId].ToString()) == instructionId);

            if (firstNormativeInstruction != null && firstNormativeInstruction[db_normativeInstructionUrl] != null)
            {
                string url = firstNormativeInstruction[db_normativeInstructionUrl].ToString();
                if (!string.IsNullOrEmpty(url))
                {
                    OpenUrl(url); // New method
                }
            }
            //if (_IsInstructionSelected) { _IsInstructionSelected = false; }
        }
        private void OpenUrl(string url)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось открыть URL: {ex.Message}");
            }
        }

        private Dictionary<string, object> GetDictFromSelectedInstruction(string selectedItemStr)
        {

            foreach (Dictionary<string, object> tempD in listOfNewInstructions_global)
            {
                Dictionary<string, object> selectedDictionary = listOfNewInstructions_global.FirstOrDefault(tempD => tempD[dB_pos_users_causeOfInstruction].ToString() == selectedItemStr);
                if (selectedDictionary != null)
                {
                    return selectedDictionary; // HERE WE DIDN't CHECK  THAT названия инструктажей не повторяется, а просто вернули первое попавшееся. Проверку бы!
                }
            }
            throw new Exception("Соответствующий инструктаж не был найден, ошибка!");
        }

        private async void PassInstruction_CheckedChanged(object sender, EventArgs e)
        {
            if (!PassInstruction.Checked) { return; }

            if (ListOfInstructionsForUser.SelectedItem == null)
            {
                MessageBox.Show("Вы не выбрали инструктаж.");
                PassInstruction.Enabled = false;
                PassInstruction.Checked = false;
                return;
            }

            if (ConfirmAction())
            {
                MessageBox.Show("Вы согласились, что прошли инструктаж.", "Действие подтверждено", MessageBoxButtons.OK, MessageBoxIcon.Information);
                PassInstruction.Enabled = false;

                Dictionary<string, object> selectedDict = GetDictFromSelectedInstruction(ListOfInstructionsForUser.SelectedItem.ToString());
                await SendInstructionIsPassedToDB(selectedDict);

                FilesOfInstructionCheckedListBox.Items.Clear();
                _IsInstructionSelected = false; // Reset the flag
            }
            else
            {
                MessageBox.Show("Вы не подтвердили, что прошли инструктаж.", "Действие отменено", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                PassInstruction.Checked = false;
                return;
            }
        }

        private async Task SendInstructionIsPassedToDB(Dictionary<string, object> selectedDict)
        {
            try
            {
                // Extract the instruction ID from the selected dictionary
                int instructionId;

                // Safely extract instruction ID
                object idValue = selectedDict["instruction_id"];
                if (idValue is JsonElement jsonElement)
                {
                    instructionId = jsonElement.GetInt32();
                }
                else
                {
                    instructionId = Convert.ToInt32(idValue);
                }

                // Use the service to mark the instruction as passed
                bool success = await _instructionService.MarkInstructionAsPassedAsync(instructionId);

                if (success)
                {
                    // Clear and refresh the instructions list
                    ListOfInstructionsForUser.Items.Clear();
                    await RefreshNewInstructionsInternal();
                    await RefreshOldInstructionsInternal();

                    // Optional: Show success message
                    MessageBox.Show("Instruction successfully marked as passed.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Failed to mark instruction as passed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                PassInstruction.Checked = false;
            }
        }

        private bool ConfirmAction()
        {
            var result = MessageBox.Show("Вы подтверждаете прохождение инструктажа?", "Подтвердите действие", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void UserForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
            this.ShowInTaskbar = false;
        }

        private void FilesOfInstructionCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            this.BeginInvoke((System.Windows.Forms.MethodInvoker)delegate
            {
                // Open URL when item is checked
                if (e.NewValue == CheckState.Checked)
                {
                    string selectedItem = FilesOfInstructionCheckedListBox.Items[e.Index].ToString();
                    // Extract URL from the display text (format: "Name (URL)")
                    int urlStart = selectedItem.LastIndexOf('(');
                    int urlEnd = selectedItem.LastIndexOf(')');
                    if (urlStart > 0 && urlEnd > urlStart)
                    {
                        string url = selectedItem.Substring(urlStart + 1, urlEnd - urlStart - 1);
                        OpenUrl(url);
                    }
                }

                // Update PassInstruction checkbox state based on conditions
                UpdatePassInstructionState();
            });
        }

        private void UpdatePassInstructionState()
        {
            // Only enable PassInstruction if:
            // 1. An instruction is selected
            // 2. Either it's an introductory instruction (no files to check) 
            //    OR all normative instructions have been checked

            if (!_IsInstructionSelected)
            {
                PassInstruction.Enabled = false;
                return;
            }

            // Check if this is an introductory instruction
            if (ListOfInstructionsForUser.SelectedItem != null)
            {
                Dictionary<string, object> selectedDict = GetDictFromSelectedInstruction(ListOfInstructionsForUser.SelectedItem.ToString());
                string instructionType = selectedDict[db_typeOfInstruction].ToString();

                if (instructionType == "0") // Introductory instruction
                {
                    PassInstruction.Enabled = true;
                    return;
                }
            }

            // For other instruction types, check if all normative instructions are checked
            if (FilesOfInstructionCheckedListBox.Items.Count > 0)
            {
                PassInstruction.Enabled = AreAllItemsChecked(FilesOfInstructionCheckedListBox);
            }
            else
            {
                PassInstruction.Enabled = false;
            }
        }

        private bool AreAllItemsChecked(CheckedListBox checkedListBox)
        {
            for (int index = 0; index < checkedListBox.Items.Count; index++)
            {
                if (!checkedListBox.GetItemChecked(index))
                {
                    return false;
                }
            }
            return true;
        }

        private void OpenFile(string filePath)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось открыть файл: {ex.Message}");
            }
        }

        private void showNotification_Click(object sender, EventArgs e)
        {
            Notifications.ShowWindowsNotification("Уведомление", "Проверка.");

        }

        private async void UpdateInstructionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await RefreshNewInstructionsInternal();
            await RefreshOldInstructionsInternal();
        }



        private void changeCredentialsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _signUpForm.Show();
        }

        private void exitApplicationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Dispose(true);
            _loginForm.ExitApplication();
        }

        private void signOutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_signUpForm != null)
            {
                _signUpForm.Dispose();
            }
            Decryption_stuff.DeleteJWTToken();
            this.Dispose(true);
            _loginForm.activeForm = _loginForm;
            _loginForm.Show();
        }

        private void AdditionalSettingsPicture_Click(object sender, EventArgs e)
        {
            // Get the mouse position relative to the screen
            Point screenPosition = AdditionalSettingsPicture.PointToScreen(new Point(0, AdditionalSettingsPicture.Height));

            // Show the context menu at the mouse click position
            AdditionalSettingsForUserContextMenuStrip.Show(screenPosition);
        }

        private async void PassedOrNotInstrTabControl_TabIndexChanged(object sender, EventArgs e)
        {
            await RefreshNewInstructionsInternal();
            await RefreshOldInstructionsInternal();

        }

        private void dataGridViewPassedInstructions_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                listBoxOfPathsOfPassedInstructions.Items.Clear();

                if (dataGridViewPassedInstructions.SelectedRows.Count <= 0)
                {
                    return;
                }

                DataGridViewRow selectedRow = dataGridViewPassedInstructions.SelectedRows[0];

                // Make sure we have the right column index
                int causeColumnIndex = dataGridViewPassedInstructions.Columns["CauseOfPassedInstr"]?.Index ?? 2;
                if (causeColumnIndex < 0 || causeColumnIndex >= selectedRow.Cells.Count)
                {
                    Console.WriteLine($"Invalid column index: {causeColumnIndex}, Cell count: {selectedRow.Cells.Count}");
                    return;
                }

                object cellValue = selectedRow.Cells[causeColumnIndex]?.Value;
                if (cellValue == null)
                {
                    return;
                }

                string cause = cellValue.ToString();

                // Ensure old instructions are loaded before trying to fetch paths
                if (listOfOldInstructions_global == null || listsOfPathsOfOldInstr_global == null)
                {
                    return;
                }

                List<string> listOfNormativeInstructions = FetchOldInstructionsNormativeInstructionsByCause(cause);
                if (listOfNormativeInstructions == null || listOfNormativeInstructions.Count == 0)
                {
                    listBoxOfPathsOfPassedInstructions.Items.Add("Нормативные инструкции для данного инструктажа не найдены");
                    return;
                }

                listBoxOfPathsOfPassedInstructions.Items.AddRange(listOfNormativeInstructions.ToArray());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in dataGridViewPassedInstructions_SelectionChanged: {ex}");
                MessageBox.Show($"Произошла ошибка при загрузке файлов: {ex.Message}");
            }
        }

        private void dataGridViewPassedInstructions_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            listBoxOfPathsOfPassedInstructions.Items.Clear();
            // Ensure it's not the header row or an invalid row index
            if (e.RowIndex >= 0)
            {
                DataGridViewRow selectedRow = dataGridViewPassedInstructions.Rows[e.RowIndex];
                string? cause;

                try
                {
                    // Safely check the cell for null before calling .ToString()
                    object cellValue = selectedRow.Cells["CauseOfPassedInstr"]?.Value;
                    if (cellValue == null)
                    {
                        // If the cell is null or we can’t get the value, just return
                        return;
                    }

                    cause = cellValue.ToString();
                }
                catch
                {
                    // If anything goes wrong (e.g. wrong column name, out of range, etc.), just return
                    return;
                }

                // If we successfully retrieved a non-null cause, call your method
                List<string?>? listOfPaths = FetchOldInstructionsNormativeInstructionsByCause(cause);
                if (listOfPaths is null || listOfPaths.Count == 0) return;

                string[] pathArray = listOfPaths
                    .Where(item => item is not null) // Remove nulls
                    .Select(item => item!.ToString()!) // Convert to string safely
                    .ToArray();


                if (pathArray is null || pathArray.Length == 0)
                {
                    listBoxOfPathsOfPassedInstructions.Text = "Файлы для данного инструктажа не найдены";
                    return;
                }

                listBoxOfPathsOfPassedInstructions.Items.AddRange(pathArray);

            }
        }

        private List<string?>? FetchOldInstructionsNormativeInstructionsByCause(string? cause)
        {
            if (cause == null)
            {
                MessageBox.Show("Причина выбранного инструктажа null. Как ты вообще сюда попал, User? :/");
                return null;
            }

            // Add null check for listOfOldInstructions_global
            if (listsOfNormativeInstructionsOfOldInstr_global == null)
            {
                MessageBox.Show("Ошибка: Список нормативных инструкций не загружен.");
                return new List<string>();
            }

            List<object?> listOfIds = GetInstructionIdsOfGivenCause(listOfOldInstructions_global, cause);

            if (listOfIds == null || listOfIds.Count != 1)
            {
                MessageBox.Show("Для данного инструктажа не найдены ID или найдены больше чем 1, что-то пошло не так :/");
                return null;
            }

            object? firstId = listOfIds[0];
            if (firstId == null)
            {
                MessageBox.Show("ID инструктажа равен null");
                return null;
            }

            int id;
            if (firstId is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Number)
            {
                id = jsonElement.GetInt32(); // Extract int properly
            }
            else
            {
                id = Convert.ToInt32(firstId); // Standard conversion if not JsonElement
            }

            if (listsOfNormativeInstructionsOfOldInstr_global == null)
            {
                Console.WriteLine("Warning: listsOfPathsOfOldInstr_global is null");
                MessageBox.Show("Ошибка: Список путей файлов не загружен.");
                return null;
            }

            List<string> listOfNormativeInstructions = GetNormativeInstructionsOfGivenId(listsOfNormativeInstructionsOfOldInstr_global, id);
            return listOfNormativeInstructions;
        }

        private List<string> GetNormativeInstructionsOfGivenId(List<Dictionary<string, object>> list, int id)
        {
            return list
                .Where(dict => dict.ContainsKey(dB_instructionId) &&
                    GetIntValue(dict[dB_instructionId]) == id)
                .Select(dict =>
                {
                    string name = dict.ContainsKey(db_normativeInstructionName) ? dict[db_normativeInstructionName]?.ToString() ?? "Unknown" : "Unknown";
                    string url = dict.ContainsKey(db_normativeInstructionUrl) ? dict[db_normativeInstructionUrl]?.ToString() ?? "" : "";
                    return $"{name} ({url})";
                })
                .ToList();
        }

        private static int GetIntValue(object obj)
        {
            if (obj is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Number)
            {
                return jsonElement.GetInt32(); // Extract integer safely from JsonElement
            }
            return Convert.ToInt32(obj); // Standard conversion for other types
        }
        private List<object?> GetInstructionIdsOfGivenCause(List<Dictionary<string, object>> list, string cause)
        {
            // Add null check for the list
            if (list == null)
            {
                Console.WriteLine("Warning: list is null in GetInstructionIdsOfGivenCause");
                return new List<object?>(); // Return empty list instead of throwing exception
            }

            return list
                .Where(dict => dict != null && dict.ContainsKey(dB_pos_users_causeOfInstruction) &&
                       dict[dB_pos_users_causeOfInstruction]?.ToString() == cause)
                .Select(dict => dict.ContainsKey(dB_instructionId) ? dict[dB_instructionId] : null)
                .ToList();
        }
        private void PrintListOfDictionary(List<Dictionary<string, object>> list)
        {
            foreach (var dict in list)
            {
                Console.WriteLine("{" + string.Join(", ", dict.Select(kv => $"{kv.Key}: {kv.Value}")) + "}");
            }
        }

        private void listBoxOfPathsOfPassedInstructions_DoubleClick(object sender, EventArgs e)
        {
            var selectedItem = listBoxOfPathsOfPassedInstructions.SelectedItem;
            if (selectedItem is null)
            {
                return;
            }
            string selectedText = selectedItem.ToString();
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

        private void AdditionalSettingsForUserContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            // If you’re relying on the default opening, you don't want to cancel it:
            e.Cancel = false;

            // Let's get the default position and shift it, for example.
            // ContextMenuStrip does not directly give us the "default" point 
            // but we can compute or adjust with SourceControl, etc.
            // For simplicity, if you only want a small shift from the cursor:
            Point cursorPos = Cursor.Position;
            int menuWidth = AdditionalSettingsForUserContextMenuStrip.Size.Width;
            Point adjustedPos = new Point(cursorPos.X - menuWidth, cursorPos.Y);
            AdditionalSettingsForUserContextMenuStrip.Show(adjustedPos);

            // You can let the control handle it or show manually:
            // AdditionalSettingsForUserContextMenuStrip.Show(adjustedPos);
            // e.Cancel = true;  // <-- would normally be used if you do a manual Show, but then you'd rely solely on Show().
        }

        // Add these class-level fields to track window state
        private bool isMaximized = false;
        private TabPage lastActiveTab = null;


        #region Resize of form for User
        private void UserForm_Resize(object sender, EventArgs e)
        {
            if (controlsOriginalSizes == null || PassedOrNotInstrTabControl == null)
            {
                return;
            }

            // Update our tracking of window state
            isMaximized = (this.WindowState == FormWindowState.Maximized);

            // Store current tab for reference
            lastActiveTab = PassedOrNotInstrTabControl.SelectedTab;

            // Call our resize handler
            HandleTabResize(lastActiveTab);
        }

        // Add this handler for tab changes
        private void PassedOrNotInstrTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabPage currentTab = PassedOrNotInstrTabControl.SelectedTab;

            // Only resize if we're maximized
            if (isMaximized)
            {
                HandleTabResize(currentTab);
            }
            else
            {
                // Restore original sizes for the new tab's controls
                RestoreTabControlsToOriginalSize(currentTab);
            }

            lastActiveTab = currentTab;
        }

        private void HandleTabResize(TabPage currentTab)
        {
            Control[] currentTabControls = GetControlsForTab(currentTab);
            if (currentTabControls == null) return;

            Rectangle originalPanelBounds = controlsOriginalSizes[PassedOrNotInstrTabControl];

            if (isMaximized)
            {
                float widthRatio = (float)this.ClientSize.Width / originalPanelBounds.Width;
                float heightRatio = (float)this.ClientSize.Height / originalPanelBounds.Height;

                PassedOrNotInstrTabControl.Width = (int)(originalPanelBounds.Width * widthRatio);
                PassedOrNotInstrTabControl.Height = (int)(originalPanelBounds.Height * heightRatio);

                int spacing = 40;
                int controlWidth = (PassedOrNotInstrTabControl.Width - (3 * spacing)) / 2;

                if (currentTab == NotPassedInstrTabPage)
                {
                    // First control and its label
                    var firstControl = currentTabControls[0]; // ListOfInstructionsForUser
                    if (controlsOriginalSizes.ContainsKey(firstControl))
                    {
                        Rectangle originalBounds = controlsOriginalSizes[firstControl];
                        firstControl.SetBounds(
                            spacing,
                            originalBounds.Y + 40,
                            controlWidth,
                            PassedOrNotInstrTabControl.Height - (2 * spacing) - 20
                        );

                        // Position its label
                        LabelOfNotPassedInstr.SetBounds(
                            spacing,
                            originalBounds.Y,
                            controlWidth,
                            20
                        );
                    }

                    // Second control and its label
                    var secondControl = currentTabControls[1]; // FilesOfInstructionCheckedListBox
                    if (controlsOriginalSizes.ContainsKey(secondControl))
                    {
                        Rectangle originalBounds = controlsOriginalSizes[secondControl];
                        secondControl.SetBounds(
                            firstControl.Right + spacing,
                            originalBounds.Y + 40,
                            controlWidth,
                            PassedOrNotInstrTabControl.Height - (2 * spacing) - 20
                        );
                    }
                }
                else if (currentTab == PassedInstrTabPage)
                {
                    // Grid control
                    var gridControl = currentTabControls[0]; // dataGridViewPassedInstructions
                    if (controlsOriginalSizes.ContainsKey(gridControl))
                    {
                        Rectangle originalBounds = controlsOriginalSizes[gridControl];
                        gridControl.SetBounds(
                            spacing,
                            originalBounds.Y + 40,
                            controlWidth,
                            PassedOrNotInstrTabControl.Height - (2 * spacing) - 80  // Added bottom spacing
                        );
                        // Position its label
                        LabelOfPassedInstr.SetBounds(
                            spacing,
                            originalBounds.Y,
                            controlWidth,
                            20
                        );
                    }

                    // List box control
                    var listBoxControl = currentTabControls[1]; // listBoxOfPathsOfPassedInstructions
                    if (controlsOriginalSizes.ContainsKey(listBoxControl))
                    {
                        Rectangle originalBounds = controlsOriginalSizes[listBoxControl];
                        listBoxControl.SetBounds(
                            gridControl.Right + spacing,
                            originalBounds.Y + 40,
                            controlWidth,
                            PassedOrNotInstrTabControl.Height - (2 * spacing) - 80  // Added bottom spacing
                        );
                    }
                }
            }
            else
            {
                RestoreTabControlsToOriginalSize(currentTab);
            }
        }

        private Control[] GetControlsForTab(TabPage tab)
        {
            if (tab == NotPassedInstrTabPage)
            {
                return new Control[]
                {
            ListOfInstructionsForUser,
            FilesOfInstructionCheckedListBox,
            LabelOfNotPassedInstr,    // Adding the label
                };
            }
            else if (tab == PassedInstrTabPage)
            {
                return new Control[]
                {
            dataGridViewPassedInstructions,
            listBoxOfPathsOfPassedInstructions,
            LabelOfPassedInstr,
                    // Add any labels for the second tab if they exist
                };
            }
            return null;
        }

        private void RestoreTabControlsToOriginalSize(TabPage tab)
        {
            // Get controls for this tab
            Control[] tabControls = GetControlsForTab(tab);
            if (tabControls == null) return;

            // Restore the tab control itself
            Rectangle tabControlBounds = controlsOriginalSizes[PassedOrNotInstrTabControl];
            PassedOrNotInstrTabControl.SetBounds(
                tabControlBounds.X,
                tabControlBounds.Y,
                tabControlBounds.Width,
                tabControlBounds.Height
            );

            // Restore each control to its original size
            foreach (Control ctrl in tabControls)
            {
                if (!controlsOriginalSizes.ContainsKey(ctrl)) continue;

                Rectangle originalBounds = controlsOriginalSizes[ctrl];
                ctrl.Visible = true;
                ctrl.SetBounds(
                    originalBounds.X,
                    originalBounds.Y,
                    originalBounds.Width,
                    originalBounds.Height
                );
            }
        }

        private void UserForm_Load(object sender, EventArgs e)
        {
            controlsOriginalSizes = new Dictionary<Control, Rectangle>();

            // Capture the main tab control
            controlsOriginalSizes[PassedOrNotInstrTabControl] = PassedOrNotInstrTabControl.Bounds;

            // Capture controls for the first tab
            controlsOriginalSizes[ListOfInstructionsForUser] = ListOfInstructionsForUser.Bounds;
            controlsOriginalSizes[FilesOfInstructionCheckedListBox] = FilesOfInstructionCheckedListBox.Bounds;
            controlsOriginalSizes[LabelOfNotPassedInstr] = LabelOfNotPassedInstr.Bounds;
            controlsOriginalSizes[LabelOfPassedInstr] = LabelOfPassedInstr.Bounds;

            // Capture controls for the second tab
            controlsOriginalSizes[dataGridViewPassedInstructions] = dataGridViewPassedInstructions.Bounds;
            controlsOriginalSizes[listBoxOfPathsOfPassedInstructions] = listBoxOfPathsOfPassedInstructions.Bounds;

            // Capture the tab pages themselves
            controlsOriginalSizes[NotPassedInstrTabPage] = NotPassedInstrTabPage.Bounds;
            controlsOriginalSizes[PassedInstrTabPage] = PassedInstrTabPage.Bounds;
        }
        #endregion


    }

}