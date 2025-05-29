using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Kotova.Test1.ClientSide.ManagementWPF.Models;
using System.Net;

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
            _httpClient.Timeout = TimeSpan.FromSeconds(30); // Add timeout
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

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"API Error ({response.StatusCode}): {errorContent}");
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(jsonResponse))
                {
                    throw new InvalidOperationException("Получен пустой ответ от сервера");
                }

                var result = JsonConvert.DeserializeObject<List<DepartmentWithChiefsDto>>(jsonResponse);
                return result ?? new List<DepartmentWithChiefsDto>();
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                throw new Exception("Время ожидания ответа от сервера истекло", ex);
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Ошибка сети при получении отделов с начальниками: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception($"Ошибка обработки данных от сервера: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Неожиданная ошибка при получении отделов с начальниками: {ex.Message}", ex);
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
                    try
                    {
                        var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                        return result?.Message?.ToString() ?? "Инструктаж успешно назначен";
                    }
                    catch (JsonException)
                    {
                        // If JSON parsing fails, return the raw response
                        return responseContent;
                    }
                }
                else
                {
                    throw new HttpRequestException($"Ошибка сервера ({response.StatusCode}): {responseContent}");
                }
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                throw new Exception("Время ожидания ответа от сервера истекло", ex);
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Ошибка сети при назначении инструктажа: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception($"Ошибка обработки данных: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Неожиданная ошибка при назначении инструктажа: {ex.Message}", ex);
            }
        }

        public async Task<List<UnplannedInstructionStatusDto>> GetUnplannedInstructionsStatusAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/get-unplanned-instructions-for-chiefs-status");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"API Error ({response.StatusCode}): {errorContent}");
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(jsonResponse))
                {
                    return new List<UnplannedInstructionStatusDto>();
                }

                var result = JsonConvert.DeserializeObject<List<UnplannedInstructionStatusDto>>(jsonResponse);
                return result ?? new List<UnplannedInstructionStatusDto>();
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                throw new Exception("Время ожидания ответа от сервера истекло", ex);
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Ошибка сети при получении статусов инструктажей: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception($"Ошибка обработки данных от сервера: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Неожиданная ошибка при получении статусов инструктажей: {ex.Message}", ex);
            }
        }

        public async Task<List<NormativeInstructionDto>> GetNormativeInstructionsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/normative-instructions");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"API Error ({response.StatusCode}): {errorContent}");
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(jsonResponse))
                {
                    return new List<NormativeInstructionDto>();
                }

                var result = JsonConvert.DeserializeObject<List<NormativeInstructionDto>>(jsonResponse);
                return result ?? new List<NormativeInstructionDto>();
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                throw new Exception("Время ожидания ответа от сервера истекло", ex);
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Ошибка сети при получении нормативных инструкций: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception($"Ошибка обработки данных от сервера: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Неожиданная ошибка при получении нормативных инструкций: {ex.Message}", ex);
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        // Add this method to your ApiService.cs class

        public async Task<string> AssignUnplannedInstructionToDepartmentsAsync(UnplannedInstructionForDepartmentsPackage package)
        {
            try
            {
                var jsonData = JsonConvert.SerializeObject(package);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}/assign-unplanned-instruction-to-departments", content);

                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                        return result?.Message?.ToString() ?? "Инструктаж успешно назначен";
                    }
                    catch (JsonException)
                    {
                        // If JSON parsing fails, return the raw response
                        return responseContent;
                    }
                }
                else
                {
                    throw new HttpRequestException($"Ошибка сервера ({response.StatusCode}): {responseContent}");
                }
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                throw new Exception("Время ожидания ответа от сервера истекло", ex);
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Ошибка сети при назначении инструктажа: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception($"Ошибка обработки данных: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Неожиданная ошибка при назначении инструктажа: {ex.Message}", ex);
            }
        }
    }

}