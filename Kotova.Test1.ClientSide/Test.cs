using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kotova.Test1.ClientSide
{
    internal static class Test
    {
        public static async Task<bool> connectionToUrlGet(string url)
        {
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
                    return true; //Good response
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                return false; //Bad response
            }
        }
        public static async Task<HttpStatusCode> connectionToUrlPatch(string url, HttpContent content)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string jwtToken = Decryption_stuff.DecryptedJWTToken();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);


                   
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, url)
                    {
                        Content = content 
                    };

                    HttpResponseMessage response = await client.SendAsync(request);
                    try
                    {
                        // Attempt to use the response.
                        response.EnsureSuccessStatusCode(); // This will throw an exception for non-success status codes.

                        // If response is successful, read and show the response body.
                        string responseBody = await response.Content.ReadAsStringAsync();
                        MessageBox.Show(responseBody);
                        return response.StatusCode;
                    }
                    catch (HttpRequestException)
                    {
                        // Read the response content which contains the detailed error message.
                        var errorResponse = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Server returned a failure response: {errorResponse}");

                        // You may want to parse this message if it's JSON or structured differently
                        return response.StatusCode;
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                return HttpStatusCode.BadRequest;
            }
        }
    }
}
