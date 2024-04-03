﻿using System.Text;
using System;
using System.Data.SqlClient;
using System.Globalization;
using System.Transactions;
using System.Net.Http.Headers;

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
            listBox1.Items.Add("Option 1");
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
            string url = "https://localhost:7052/WeatherForecast/greeting"; // Replace with your actual API URL
            try
            {
                using (HttpClientHandler handler = new HttpClientHandler())
                {
                    // Bypass SSL certificate validation (for development use only!)
                    handler.ServerCertificateCustomValidationCallback = (request, certificate, chain, sslPolicyErrors) => true;
                    using (HttpClient client = new HttpClient(handler))
                    {
                        HttpResponseMessage response = await client.GetAsync(url);
                        response.EnsureSuccessStatusCode();
                        string responseBody = await response.Content.ReadAsStringAsync();
                        MessageBox.Show(responseBody); // Update the label with the response
                    }
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
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var filePath = openFileDialog.FileName;
                    var url = "https://localhost:7052/WeatherForecast/upload";
                    using (var client = new HttpClient())
                    using (var content = new MultipartFormDataContent())
                    {
                        // Load the file and set the content
                        var fileContent = new ByteArrayContent(File.ReadAllBytes(filePath));
                        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");

                        // "file" parameter name should match the name of the parameter in the server action
                        content.Add(fileContent, "file", Path.GetFileName(filePath));

                        try
                        {
                            // Post the file to the server
                            var response = await client.PostAsync(url, content);
                            response.EnsureSuccessStatusCode();

                            // Read the response and display it on the form
                            var responseBody = await response.Content.ReadAsStringAsync();
                            MessageBox.Show($"File uploaded successfully: {responseBody}");
                        }
                        catch (HttpRequestException ex)
                        {
                            MessageBox.Show($"Error uploading file: {ex.Message}");
                        }
                    }
                }
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
