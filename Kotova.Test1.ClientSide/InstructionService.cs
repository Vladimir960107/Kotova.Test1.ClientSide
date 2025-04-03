using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Kotova.CommonClasses;

namespace Kotova.Test1.ClientSide
{
    /// <summary>
    /// Provides common instruction-related functionality that can be used by different forms
    /// </summary>
    public class InstructionService
    {
        private static readonly string GetNotPassedInstructionsUrl = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get-not-passed-instructions";
        private static readonly string GetPassedInstructionsUrl = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/get-passed-instructions";
        private static readonly string MarkInstructionAsPassedUrl = ConfigurationClass.BASE_INSTRUCTIONS_URL_DEVELOPMENT + "/mark-instruction-as-passed";

        private readonly string _jwtToken;
        private readonly ILogger _logger;

        // Interface for logging that can be implemented by forms
        public interface ILogger
        {
            void LogInfo(string message);
            void LogError(string message, Exception ex = null);
        }

        /// <summary>
        /// Creates a new InstructionService with the provided JWT token
        /// </summary>
        /// <param name="jwtToken">JWT token for authentication</param>
        /// <param name="logger">Optional logger for service messages</param>
        public InstructionService(string jwtToken, ILogger logger = null)
        {
            _jwtToken = jwtToken ?? throw new ArgumentNullException(nameof(jwtToken));
            _logger = logger;
        }

        /// <summary>
        /// Fetches not passed instructions for the current user
        /// </summary>
        /// <returns>A tuple containing a list of instructions and a list of file paths, or null if an error occurred</returns>
        public async Task<(List<Dictionary<string, object>> Instructions, List<Dictionary<string, object>> Paths)?> GetNotPassedInstructionsAsync()
        {
            try
            {
                _logger?.LogInfo("Fetching not passed instructions");

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);
                    HttpResponseMessage response = await client.GetAsync(GetNotPassedInstructionsUrl);
                    response.EnsureSuccessStatusCode();

                    var jsonResponse = await response.Content.ReadAsStringAsync();

                    // Parse the response as a list of instruction objects
                    var instructions = JsonSerializer.Deserialize<List<InstructionDto>>(jsonResponse,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (instructions == null || instructions.Count == 0)
                    {
                        _logger?.LogInfo("No not-passed instructions found");
                        return null;
                    }

                    // Create the instructions list
                    var instructionsList = instructions.Select(instr => new Dictionary<string, object>
                    {
                        { "instruction_id", instr.InstructionId },
                        { "cause_of_instruction", instr.Cause },
                        { "type_of_instruction", instr.Type ?? "Unknown" },
                        { "when_was_send_to_user", instr.WhenAssigned },
                        { "path_to_instruction", instr.FilePaths?.FirstOrDefault() ?? "" }
                    }).ToList();

                    // Create the paths list
                    var pathsList = new List<Dictionary<string, object>>();
                    foreach (var instruction in instructions)
                    {
                        if (instruction.FilePaths != null && instruction.FilePaths.Any())
                        {
                            foreach (var filePath in instruction.FilePaths)
                            {
                                pathsList.Add(new Dictionary<string, object>
                                {
                                    { "instruction_id", instruction.InstructionId },
                                    { "file_path", filePath }
                                });
                            }
                        }
                        else
                        {
                            pathsList.Add(new Dictionary<string, object>
                            {
                                { "instruction_id", instruction.InstructionId },
                                { "file_path", null }
                            });
                        }
                    }

                    return (instructionsList, pathsList);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error fetching not passed instructions", ex);
                return null;
            }
        }

        /// <summary>
        /// Fetches passed instructions for the current user
        /// </summary>
        /// <returns>A tuple containing a list of instructions and a list of file paths, or null if an error occurred</returns>
        public async Task<(List<Dictionary<string, object>> Instructions, List<Dictionary<string, object>> Paths)?> GetPassedInstructionsAsync()
        {
            try
            {
                _logger?.LogInfo("Fetching passed instructions");

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);
                    HttpResponseMessage response = await client.GetAsync(GetPassedInstructionsUrl);
                    response.EnsureSuccessStatusCode();

                    var jsonResponse = await response.Content.ReadAsStringAsync();

                    // Parse the response as a list of instruction objects
                    var instructions = JsonSerializer.Deserialize<List<PassedInstructionDto>>(jsonResponse,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (instructions == null || instructions.Count == 0)
                    {
                        _logger?.LogInfo("No passed instructions found");
                        return null;
                    }

                    // Create the instructions list
                    var instructionsList = instructions.Select(instr => new Dictionary<string, object>
                    {
                        { "instruction_id", instr.InstructionId },
                        { "cause_of_instruction", instr.Cause ?? "" },
                        { "type_of_instruction", instr.Type ?? "Unknown" },
                        { "date_when_passed", instr.WhenPassed },
                        { "path_to_instruction", instr.FilePaths?.FirstOrDefault() ?? "" }
                    }).ToList();

                    // Create the paths list
                    var pathsList = new List<Dictionary<string, object>>();
                    foreach (var instruction in instructions)
                    {
                        if (instruction.FilePaths != null && instruction.FilePaths.Any())
                        {
                            foreach (var filePath in instruction.FilePaths)
                            {
                                pathsList.Add(new Dictionary<string, object>
                                {
                                    { "instruction_id", instruction.InstructionId },
                                    { "file_path", filePath }
                                });
                            }
                        }
                        else
                        {
                            pathsList.Add(new Dictionary<string, object>
                            {
                                { "instruction_id", instruction.InstructionId },
                                { "file_path", null }
                            });
                        }
                    }

                    return (instructionsList, pathsList);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError("Error fetching passed instructions", ex);
                return null;
            }
        }

        /// <summary>
        /// Marks an instruction as passed by the current user
        /// </summary>
        /// <param name="instructionId">The ID of the instruction to mark as passed</param>
        /// <returns>True if the operation was successful, false otherwise</returns>
        public async Task<bool> MarkInstructionAsPassedAsync(int instructionId)
        {
            try
            {
                _logger?.LogInfo($"Marking instruction {instructionId} as passed");

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);

                    string url = $"{MarkInstructionAsPassedUrl}/{instructionId}";
                    HttpResponseMessage response = await client.PostAsync(url, null);

                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError($"Error marking instruction {instructionId} as passed", ex);
                return false;
            }
        }

        #region DTOs

        // Add these classes to match the server response format
        private class InstructionDto
        {
            public int InstructionId { get; set; }
            public string Cause { get; set; }
            public DateTime BeginDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Type { get; set; }
            public DateTime? WhenAssigned { get; set; }
            public List<string> FilePaths { get; set; }
        }

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
    }
}