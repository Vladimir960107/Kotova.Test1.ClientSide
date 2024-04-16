using System.Text;
using System;
using System.Data.SqlClient;
using System.Globalization;
using System.Transactions;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Collections.Generic;
using Kotova.CommonClasses;

namespace Kotova.Test1.ClientSide
{

    public partial class Form2 : Form
    {
        static string? selectedFolderPath = null;
        public Form2()
        {
            InitializeComponent();
            InitializeListBox();
        }
        private void InitializeListBox()
        {
            listBox1.Items.Add("Тестовая опция 1");
        }

        private void buttonCreateNotification_Click(object sender, EventArgs e)
        {
            if (selectedFolderPath is null)
            {
                MessageBox.Show("Путь до инструктажа не выбран!");
                return;
            }

            string tableName_sql_BeginDate = "BeginDate";
            string tableName_sql_EndDate = "EndDate";
            string tableName_sql_IsForDrivers = "IsForDrivers";
            string tableName_sql_PathToInstruction = "PathToInstruction";
            string tableName_sql_NameOfInstrutcion = "NameOfInstruction";

            DateTime startTime = DateTime.Now;

            DateTime endDate = datePickerEnd.Value.Date;

            if (endDate <= startTime)
            {
                MessageBox.Show("End date must be after start date.");
                return;
            }

            string endDateString = endDate.ToString("dd/MM/yyyy");


            bool isForDrivers = checkBoxIsForDrivers.Checked;

            int bitValueIsForDrivers = isForDrivers ? 1 : 0;

            string nameOfInstruction = InstructionTextBox.Text;


            // Validation for end date

            var parameters = new Dictionary<string, object>
            {
                { tableName_sql_BeginDate, startTime },
                { tableName_sql_EndDate, endDate },
                { tableName_sql_IsForDrivers, bitValueIsForDrivers},
                { tableName_sql_PathToInstruction, selectedFolderPath},
                { tableName_sql_NameOfInstrutcion, nameOfInstruction}
            };

            try
            {
                // Create a new instance of SqlInsertCommandBuilder for the Notifications table
                var builder = new SqlInsertCommandBuilder("dbo.Notifications");

                // Add column values
                foreach (var parameter in parameters)
                {
                    builder.AddColumnValue(parameter.Key, parameter.Value);
                }

                // Use DatabaseManager to execute the constructed query with parameters
                DatabaseManager.ExecuteQueryWithBuilder(builder); // Adjusted for compatibility

                MessageBox.Show("Notification created successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private void buttonChoosePathToInstruction_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                // Optionally set the initial directory
                // folderBrowserDialog.SelectedPath = @"C:\Initial\Folder\Path";

                DialogResult result = folderBrowserDialog.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
                {
                    // Store the selected folder path in a variable
                    selectedFolderPath = folderBrowserDialog.SelectedPath;

                    // Use the selectedFolderPath variable as needed in your code
                    MessageBox.Show($"Selected Folder: {selectedFolderPath}");

                }
            }
        }
        private async void buttonTest_Click(object sender, EventArgs e)
        {
            string url = "https://localhost:7052/WeatherForecast/greeting";
            try
            {
                using (HttpClient client = new HttpClient())
                {
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

        private async void UploadFileToServer_Click(object sender, EventArgs e)
        {
            UploadFileToServer.Enabled = false;
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var filePath = openFileDialog.FileName;
                    var url = "https://localhost:7052/WeatherForecast/upload";
                    using (var client = new HttpClient())
                    using (var content = new MultipartFormDataContent())
                    {
                        try
                        {
                            string mimeType = Path.GetExtension(filePath).ToLowerInvariant() switch
                            {
                                ".xls" => "application/vnd.ms-excel",
                                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                _ => throw new InvalidOperationException("Unsupported file type.")
                            };

                            var fileContent = new ByteArrayContent(File.ReadAllBytes(filePath));
                            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(mimeType);

                            // "file" parameter name should match the name of the parameter in the server action
                            content.Add(fileContent, "file", Path.GetFileName(filePath));

                            // Post the file to the server
                            var response = await client.PostAsync(url, content);

                            // Instead of calling EnsureSuccessStatusCode, check the status code manually
                            if (!response.IsSuccessStatusCode)
                            {
                                // Read the response content and include it in the error message
                                var errorContent = await response.Content.ReadAsStringAsync();
                                MessageBox.Show($"Error uploading file: {errorContent}");
                            }
                            else
                            {
                                // If the call was successful, read the response and display it on the form
                                var responseBody = await response.Content.ReadAsStringAsync();
                                MessageBox.Show($"File uploaded successfully: {responseBody}");
                            }
                        }
                        catch (Exception ex) when (
                        ex is InvalidOperationException ||
                        ex is IOException ||
                        ex is HttpRequestException)
                        {
                            MessageBox.Show($"Error: {ex.Message}");
                        }
                        catch (Exception ex)
                        {
                            // Handle all other exceptions that were not caught by the previous block
                            MessageBox.Show($"General Error: {ex.Message}");
                        }

                    }
                }
            }
            UploadFileToServer.Enabled = true;
        }

        private async void Download_file_excel_Click(object sender, EventArgs e)
        {
            Download_file_excel.Enabled = false;
            var url = "https://localhost:7052/WeatherForecast/download-newest"; // URL to your download endpoint

            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead); // Use ResponseHeadersRead to avoid buffering the entire file

                    if (response.IsSuccessStatusCode)
                    {
                        var contentDisposition = response.Content.Headers.ContentDisposition;
                        var filename = contentDisposition?.FileNameStar ?? contentDisposition?.FileName ?? "downloaded_file.xlsx";
                        var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), filename); // Save to My Documents or another appropriate location

                        using (var stream = await response.Content.ReadAsStreamAsync())
                        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await stream.CopyToAsync(fileStream);
                        }

                        MessageBox.Show($"File downloaded successfully: {filePath}");
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Error downloading file: {errorContent}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}");
                }
            }

            Download_file_excel.Enabled = true;
        }

        private async void syncExcelAndDB_Click(object sender, EventArgs e)
        {
            try
            {
                syncExcelAndDB.Enabled = false; // Assuming this is a button, disable it to prevent multiple clicks

                using (var httpClient = new HttpClient())
                {
                    // Assuming you're calling a GET method based on your ImportIntoDB action
                    var response = await httpClient.GetAsync("https://localhost:7052/WeatherForecast/import-into-db");

                    if (response.IsSuccessStatusCode)
                    {
                        // Successfully called the ImportIntoDB endpoint, handle accordingly
                        MessageBox.Show("Data successfully imported into the database.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        // The call was not successful, handle errors or retry logic
                        MessageBox.Show($"Failed to import data. Status code: {response.StatusCode}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                // Exception handling for networking errors, etc.
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                syncExcelAndDB.Enabled = true; // Re-enable the button after the operation completes
            }
        }

        private async void SyncNamesWithDB_Click(object sender, EventArgs e) // Next code just repeats the same as syncExcelAndDB, so create func or something
        {
            try
            {
                syncExcelAndDB.Enabled = false; // Assuming this is a button, disable it to prevent multiple clicks
                listBox1.Items.Clear();

                using (var httpClient = new HttpClient())
                {
                    // Assuming you're calling a GET method based on your ImportIntoDB action
                    var response = await httpClient.GetAsync("https://localhost:7052/WeatherForecast/sync-names-with-db");

                    if (response.IsSuccessStatusCode)
                    {

                        string responseBody = await response.Content.ReadAsStringAsync();
                        List<string>? result = JsonConvert.DeserializeObject<List<string>>(responseBody);
                        if (result is null)
                        {
                            throw new Exception("responseBody is empty");
                        }
                        string[] resultArray = result.ToArray<string>();
                        listBox1.Items.AddRange(resultArray);
                        // Successfully called the ImportIntoDB endpoint, handle accordingly
                        MessageBox.Show("Names successfully synced with database.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        string errorMessage = await response.Content.ReadAsStringAsync();
                        // The call was not successful, handle errors or retry logic
                        //MessageBox.Show($"Failed to sync names with DB. Status code: {response.StatusCode}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        MessageBox.Show($"Failed to sync names with DB. Status code: {response.StatusCode} {errorMessage}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                // Exception handling for networking errors, etc.
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                syncExcelAndDB.Enabled = true; // Re-enable the button after the operation completes
            }
        }

        private async void buttonSyncManualyInstrWithDB_Click(object sender, EventArgs e)
        {
            try
            {
                syncExcelAndDB.Enabled = false; // Assuming this is a button, disable it to prevent multiple clicks
                listOfInstructions.Items.Clear();

                using (var httpClient = new HttpClient())
                {
                    // Assuming you're calling a GET method based on your ImportIntoDB action
                    var response = await httpClient.GetAsync("https://localhost:7052/WeatherForecast/sync-instructions-with-db");
                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        if (string.IsNullOrWhiteSpace(responseBody))
                        {
                            throw new Exception("responseBody is empty"); //throw here better something
                        }
                        List<Notification> result = JsonConvert.DeserializeObject<List<Notification>>(responseBody); //checked that is not null before!

                        string[] resultArray = result.Select(n => n.NameOfInstruction).ToArray();
                        listOfInstructions.Items.AddRange(resultArray);
                        MessageBox.Show("Names successfully synced with database.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        string errorMessage = await response.Content.ReadAsStringAsync();
                        // The call was not successful, handle errors or retry logic
                        //MessageBox.Show($"Failed to sync names with DB. Status code: {response.StatusCode}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        MessageBox.Show($"Failed to sync names with DB. Status code: {response.StatusCode} {errorMessage}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                // Exception handling for networking errors, etc.
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                syncExcelAndDB.Enabled = true; // Re-enable the button after the operation completes
            }
        }
    }



    public static class DatabaseManager
    {
        private static string connectionString = "Server=localhost;Database=TestDB;Integrated Security=True;";

        public static void SetConnectionString(string newConnectionString)
        {
            connectionString = newConnectionString;
        }

        public static void ExecuteQueryWithTransaction(string query, Dictionary<string, object> parameters)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (var command = new SqlCommand(query, connection, transaction))
                        {
                            foreach (var param in parameters)
                            {
                                command.Parameters.AddWithValue($"@{param.Key}", param.Value);
                            }
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw; // Re-throw the caught exception to be handled by the caller
                    }
                }
            }
        }
        public static void ExecuteQueryWithBuilder(SqlInsertCommandBuilder builder)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    using (var command = new SqlCommand("", connection, transaction))
                    {
                        builder.ApplyToCommand(command);
                        try
                        {
                            command.ExecuteNonQuery();
                            transaction.Commit();
                        }
                        catch
                        {
                            transaction.Rollback();
                            throw; // Re-throw the caught exception to be handled by the caller
                        }
                    }
                }
            }
        }
    }

    public class SqlInsertCommandBuilder
    {
        private string tableName;
        private Dictionary<string, object> columnValueMappings;

        public SqlInsertCommandBuilder(string tableName)
        {
            this.tableName = tableName;
            columnValueMappings = new Dictionary<string, object>();
        }

        public void AddColumnValue(string columnName, object value)
        {
            columnValueMappings[columnName] = value;
        }

        public void ApplyToCommand(SqlCommand command)
        {
            StringBuilder columnsPart = new StringBuilder();
            StringBuilder parametersPart = new StringBuilder();

            foreach (var mapping in columnValueMappings)
            {
                if (columnsPart.Length > 0)
                {
                    columnsPart.Append(", ");
                    parametersPart.Append(", ");
                }

                columnsPart.Append($"[{mapping.Key}]");
                parametersPart.Append($"@{mapping.Key}"); // Use the column name as the parameter name

                // Add the parameter to the command
                command.Parameters.AddWithValue($"@{mapping.Key}", mapping.Value);
            }

            command.CommandText = $"INSERT INTO {tableName} ({columnsPart}) VALUES ({parametersPart})";
        }
    }


}
