using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Kotova.Test1.ClientSide.ManagementWPF.Models;

namespace Kotova.Test1.ClientSide.ManagementWPF.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private string _jwtToken;

        public ApiService(string baseUrl)
        {
            _httpClient = new HttpClient();
            _baseUrl = baseUrl;
        }

        public void SetAuthToken(string jwtToken)
        {
            _jwtToken = jwtToken;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
        }

        public async Task<List<DepartmentWithChiefsDto>> GetDepartmentsWithChiefsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/get-departments-with-chiefs");
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<DepartmentWithChiefsDto>>(jsonResponse);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting departments with chiefs: {ex.Message}", ex);
            }
        }

        public async Task<string> AssignUnplannedInstructionToChiefsAsync(UnplannedInstructionForChiefsPackage package)
        {
            try
            {
                var jsonData = JsonConvert.SerializeObject(package);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/assign-unplanned-instruction-to-chiefs", content);

                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    return result.Message;
                }
                else
                {
                    throw new Exception($"Server error: {responseContent}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error assigning instruction: {ex.Message}", ex);
            }
        }

        public async Task<List<UnplannedInstructionStatusDto>> GetUnplannedInstructionsStatusAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/get-unplanned-instructions-for-chiefs-status");
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<UnplannedInstructionStatusDto>>(jsonResponse);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting instructions status: {ex.Message}", ex);
            }
        }

        public async Task<List<NormativeInstructionDto>> GetNormativeInstructionsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/normative-instructions");
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<NormativeInstructionDto>>(jsonResponse);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting normative instructions: {ex.Message}", ex);
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}