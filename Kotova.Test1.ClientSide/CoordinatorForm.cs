using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;


namespace Kotova.Test1.ClientSide
{
    public partial class CoordinatorForm : Form
    {
        private const string DownloadDepartmentsForUserURL = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/download-list-of-departments";
        private Form? _loginForm;
        private string? _userName;
        public CoordinatorForm()
        {
            InitializeComponent();
        }

        public CoordinatorForm(Form loginForm, string userName)
        {
            _userName = userName;
            _loginForm = loginForm;
            InitializeComponent();
        }

        private async void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 3)
            {
                DepartmentForNewcomer.Items.Clear();
                if (await refreshDepartmentsFromDB(DepartmentForNewcomer))
                {
                    MessageBox.Show("Отделы обновились успешно.");
                    return;

                }
                else
                {
                    MessageBox.Show("Не обновились данные (скорее всего отсутствует соединение с сервером)");
                }
                
            }
        }

        private async Task<bool> refreshDepartmentsFromDB(ListBox departmentForNewcomer)
        {
            string url = DownloadDepartmentsForUserURL;
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string jwtToken = Decryption_stuff.DecryptedJWTToken();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();


                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(jsonResponse);

                    if (result.Count == 0)
                    {
                        return true;
                    }

                    listOfInstructions_global = result;
                    foreach (Dictionary<string, object> temp in result)
                    {
                        ListOfInstructions.Items.Add(temp[dB_pos_users_causeOfInstruction]);

                        /*foreach (KeyValuePair<string, object> kvp in temp)
                        {
                           var tempValue = kvp.Value.ToString() is null ? "Null" : kvp.Value.ToString();
                            
                        }*/

                    }
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                // Handle any exceptions here
                MessageBox.Show($"Error: {ex.Message}");
                return false;
            }
        }
    }
}
