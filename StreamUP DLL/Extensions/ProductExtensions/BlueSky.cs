
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace StreamUP
{
    partial class StreamUpLib
    {
        

        public async Task<bool> PostToBlueSky(string username, string password, string message)
        {
            // Login to get access token
            var loginData = JsonConvert.SerializeObject(new { identifier = username, password });
            var loginResponse = await _httpClient.PostAsync("https://bsky.social/xrpc/com.atproto.server.createSession", new StringContent(loginData, Encoding.UTF8, "application/json"));
            if (!loginResponse.IsSuccessStatusCode)
            {
                var loginError = await loginResponse.Content.ReadAsStringAsync();
                LogError("Login failed: " + loginError);
                return false;
            }

            var jsonResponse = await loginResponse.Content.ReadAsStringAsync();
            var accessToken = JsonConvert.DeserializeObject<dynamic>(jsonResponse)?.accessJwt;
            if (accessToken == null)
            {
                LogError("Failed to retrieve access token.");
                return false;
            }

            // Prepare post data
            var postData = JsonConvert.SerializeObject(new { repo = username, collection = "app.bsky.feed.post", record = new { text = message, createdAt = DateTime.UtcNow.ToString("o") } });
            // Post to BlueSky
            var postRequest = new HttpRequestMessage(HttpMethod.Post, "https://bsky.social/xrpc/com.atproto.repo.createRecord")
            {
                Headers =
            {
                {
                    "Authorization",
                    $"Bearer {accessToken}"}
            },
                Content = new StringContent(postData, Encoding.UTF8, "application/json")
            };
            var postResponse = await _httpClient.SendAsync(postRequest);
            if (!postResponse.IsSuccessStatusCode)
            {
                var postError = await postResponse.Content.ReadAsStringAsync();
                LogError("Post failed: " + postError);
                return false;
            }

            return true;
        }





    }
}
