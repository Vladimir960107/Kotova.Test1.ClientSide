using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kotova.Test1.ClientSide
{
    internal static class Test
    {
        public static async Task<int> connectionToUrlGet(string url)
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
                    return 1;
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                return 0;
            }
        }
        public static async Task<int> isSuccesfullyUpdateCredentialsInDB(string url, HttpContent content)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string jwtToken = Decryption_stuff.DecryptedJWTToken();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);


                    // Here, we use PatchAsync which requires setting up a new HttpRequestMessage and specifying HttpMethod.Patch
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, url)
                    {
                        Content = content // This is the content you are sending, typically in JSON format.
                    };

                    HttpResponseMessage response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode(); // Throws an exception if the HTTP response status is an error code.

                    string responseBody = await response.Content.ReadAsStringAsync();
                    MessageBox.Show(responseBody); // Consider handling the GUI components differently if not in a GUI context.
                    return 1; // Indicate success.
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Error: {ex.Message}"); // Better error handling can be implemented.
                return 0; // Indicate failure.
            }
        }
    }
}
