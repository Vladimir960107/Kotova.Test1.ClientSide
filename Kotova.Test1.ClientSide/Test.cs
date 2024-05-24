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
                    response.EnsureSuccessStatusCode(); // Throws an exception if the HTTP response status is an error code.

                    string responseBody = await response.Content.ReadAsStringAsync();
                    MessageBox.Show(responseBody); // Consider handling the GUI components differently if not in a GUI context.
                    return response.StatusCode;
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
