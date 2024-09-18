using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Kotova.CommonClasses;
using Newtonsoft.Json;

namespace Kotova.Test1.ClientSide
{
    public partial class AdminForm : Form
    {

        private const string DownloadDepartmentsForUserURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/download-list-of-departments";
        private const string DownloadRolesForUsersUrl = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get-roles-for-newcomer";
        private const string PostCustomTaskUrl = ConfigurationClass.BASE_TASK_URL_DEVELOPMENT + "/create-custom-task";

        private Login_Russian? _loginForm;
        string? _userName;
        private HubConnection? _hubConnection = null;
        public AdminForm(Login_Russian loginForm, string userName)
        {
            _loginForm = loginForm;
            _userName = userName;
            InitializeComponent();
            InitializeSignalRConnection();

            RefreshDepartmentsAndRoles();
        }


        private async void RefreshDepartmentsAndRoles()
        {
            if (await refreshDepartmentsFromDB(DepartmentForNewcomer))
            {
                if (await refreshRolesFromDB(RoleOfNewcomerListBox))
                {
                    MessageBox.Show("Отделы и роли обновились успешно.");
                    return;

                }
                else
                {
                    MessageBox.Show($"Отделы обновились, но роли нет! Что-то не так. Проверь эту строку.");
                }
                return;

            }
            else
            {
                MessageBox.Show("Не обновились данные (скорее всего отсутствует соединение с сервером)");
            }
        }

        private async Task<bool> refreshDepartmentsFromDB(ListBox departmentForNewcomer)
        {
            string url = DownloadDepartmentsForUserURL;
            departmentForNewcomer.Items.Clear();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string jwtToken = _loginForm._jwtToken;
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var jsonResponse = await response.Content.ReadAsStringAsync();

                    // Parse the JSON response to extract the $values array
                    var jsonDocument = JsonDocument.Parse(jsonResponse);
                    var valuesElement = jsonDocument.RootElement.GetProperty("$values");
                    List<string> result = System.Text.Json.JsonSerializer.Deserialize<List<string>>(valuesElement.GetRawText());

                    departmentForNewcomer.Items.AddRange(result.ToArray());
                    return true;
                }
            }
            catch (HttpRequestException ex)
            {
                // Handle any exceptions here
                MessageBox.Show($"Error: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> refreshRolesFromDB(ListBox listBoxRoles)
        {
            string url = DownloadRolesForUsersUrl;
            listBoxRoles.Items.Clear();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string jwtToken = _loginForm._jwtToken;
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var jsonResponse = await response.Content.ReadAsStringAsync();

                    // Parse the JSON response to extract the $values array
                    var jsonDocument = JsonDocument.Parse(jsonResponse);
                    var valuesElement = jsonDocument.RootElement.GetProperty("$values");
                    List<string>? rolesOfUsers = RolesDBToRoleNames(System.Text.Json.JsonSerializer.Deserialize<List<string>>(valuesElement.GetRawText()));

                    if (rolesOfUsers == null || rolesOfUsers.Count == 0)
                    {
                        return false;
                    }

                    listBoxRoles.Items.AddRange(rolesOfUsers.ToArray());
                    return true;
                }
            }
            catch (HttpRequestException ex)
            {
                // Handle any exceptions here
                MessageBox.Show($"Error: {ex.Message}");
                return false;
            }
        }

        private List<string>? RolesDBToRoleNames(List<string>? list)
        {
            var roles = new List<string>();
            if (list == null || !list.Any()) { return list; }
            {
                foreach (var role in list)
                {
                    switch (role)
                    {
                        case "user":
                            roles.Add("Сотрудник");
                            break;
                        case "chief of department":
                            roles.Add("Руководство ОТДЕЛА");
                            break;
                        case "coordinator":
                            roles.Add("Охрана труда");
                            break;
                        case "management":
                            roles.Add("Руководство ФИЛИАЛА");
                            break;
                        default:
                            break;
                    }
                }
            }
            return roles;
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
                MessageBox.Show($"{user}: {message}", "Сообщение");
            });

            try
            {
                await _hubConnection.StartAsync();
                MessageBox.Show("Подключен к SignalR hub.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось подключиться к SignalR hub: {ex.Message}");
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            _loginForm.Show();
            this.Dispose();
        }

        private async void SendMessageToEveryoneButton_Click(object sender, EventArgs e)
        {
            string message = MessageTextBox.Text;
            if (string.IsNullOrWhiteSpace(message))
            {
                MessageBox.Show("Сообщение пустое.");
                return;
            }
            if (_hubConnection.State == HubConnectionState.Connected)
            {
                try
                {
                    await _hubConnection.InvokeAsync("SendMessageToEveryone", _userName, message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to send message: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("You are not connected to the SignalR hub.");
            }

        }

        private async void PostCustomTaskButton_Click(object sender, EventArgs e)
        {
            int? userRole = RoleNameToRoleDB(RoleOfNewcomerListBox.SelectedItem.ToString());
            int? departmentId = DepartmentNameToDepartmentIdDB(DepartmentForNewcomer.SelectedItem.ToString());

            string token = _loginForm._jwtToken;
            CustomTask customTask = new CustomTask(CustomTaskDescriptionTextBox.Text, departmentId.Value, userRole.Value, DueDateTimePicker.Value.Date);
            var response = await PostCustomTask(customTask, token);
            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Все работает при отправке кастомного задания!");
            }
            else
            {
                MessageBox.Show("Что-то пошло не так при отправке кастомного задания" ,"Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private int? DepartmentNameToDepartmentIdDB(string? v)
        {
            switch (v)
            {
                case "Общестроительный отдел":
                    return 1;
                case "Технический отдел":
                    return 2;
                case "Начальство":
                    return 5;
                default:
                    return null;
            }
        }

        private int? RoleNameToRoleDB(string? v)
        {
            switch (v)
            {
                case "Сотрудник":
                    return 1;
                case "Руководство ОТДЕЛА":
                    return 2;
                case "Охрана труда":
                    return 3;
                case "Руководство ФИЛИАЛА":
                    return 4;
                default:
                    return null;
            }
        }

        private async Task<HttpResponseMessage> PostCustomTask(CustomTask customTask, string token)
        {

            using (HttpClient client = new HttpClient())
            {
                string url = PostCustomTaskUrl;
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                var json = JsonConvert.SerializeObject(customTask);
                var data = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, data); 
                return response;
            }

        }
    }

}
