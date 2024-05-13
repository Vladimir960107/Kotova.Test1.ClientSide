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

namespace Kotova.Test1.ClientSide
{
    public partial class UserForm : Form
    {
        Form? _loginForm;
        string? _userName;
        const string DownloadInstructionForUserURL = ConfigurationClass.BASE_NOTIFICATION_URL_DEVELOPMENT + "/get_instructions_for_user";
        public UserForm()
        {
            InitializeComponent();
        }

        public UserForm(Form loginForm, string userName)
        {
            InitializeComponent();
            _loginForm = loginForm;
            _userName = userName;  
            UserLabel.Text = _userName;

        }

        private void CheckForNewInstructions_Click(object sender, EventArgs e)
        {
            DownloadInstructionsForUserFromServer(_userName);
        }

        private async void DownloadInstructionsForUserFromServer(string? userName)
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
                    string jwtToken = Decryption_stuff.DecryptedJWTToken();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    MessageBox.Show(responseBody);
                }
            }
            catch (HttpRequestException ex)
            {
                // Handle any exceptions here
                MessageBox.Show($"Error: {ex.Message}");
            }

        }
    }
}
