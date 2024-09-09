using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Kotova.Test1.ClientSide
{
    public partial class AdminForm : Form
    {

        private Login_Russian? _loginForm;
        string? _userName;
        private HubConnection? _hubConnection = null;
        public AdminForm(Login_Russian loginForm, string userName)
        {
            _loginForm = loginForm;
            _userName = userName;
            InitializeComponent();
            InitializeSignalRConnection();
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
    }

}
