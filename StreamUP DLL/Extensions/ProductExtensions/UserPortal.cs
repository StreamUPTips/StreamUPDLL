using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using StreamUP;
using Newtonsoft.Json;
using System.IO;

namespace StreamUP
{
    public partial class StreamUpLib
    {
        public async Task<bool> PushQuotesToUserPortalAsync(string apiKey, string broadcasterId, object content)
        {
            var json = JsonConvert.SerializeObject(content);
            using var requestContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            requestContent.Headers.Add("X-API-KEY", apiKey);
            var response = await _httpClient.PostAsync($"https://user.streamup.tips/{broadcasterId}/quotes", requestContent);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> PushCommandsToUserPortalAsync(string apiKey, string broadcasterId, object content)
        {
            var json = JsonConvert.SerializeObject(content);
            using var requestContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            requestContent.Headers.Add("X-API-KEY", apiKey);
            var response = await _httpClient.PostAsync($"https://user.streamup.tips/{broadcasterId}/commands", requestContent);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> PushPointsToUserPortalAsync(string apiKey, string broadcasterId, object content)
        {
            var json = JsonConvert.SerializeObject(content);
            using var requestContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            requestContent.Headers.Add("X-API-KEY", apiKey);
            var response = await _httpClient.PostAsync($"https://user.streamup.tips/{broadcasterId}/leaderboard", requestContent);
            return response.IsSuccessStatusCode;
        }


        public async Task<bool> PushBitMenuToUserPortalAsync(string apiKey, string broadcasterId, object content)
        {
            var json = JsonConvert.SerializeObject(content);
            using var requestContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            requestContent.Headers.Add("X-API-KEY", apiKey);
            var response = await _httpClient.PostAsync($"https://user.streamup.tips/{broadcasterId}/bitmenu", requestContent);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> PushDeathCounterToUserPortalAsync(string apiKey, string broadcasterId, object content)
        {
            var json = JsonConvert.SerializeObject(content);
            using var requestContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            requestContent.Headers.Add("X-API-KEY", apiKey);
            var response = await _httpClient.PostAsync($"https://user.streamup.tips/{broadcasterId}/death_counter", requestContent);
            return response.IsSuccessStatusCode;
        }

        //! TESTING ACTIONS INTERNAL USE ONLY
        public async Task<bool> SendTestDataToQuotes(string apiKey, string broadcasterId, object content, string port = "5221")
        {
            var json = JsonConvert.SerializeObject(content);
            using var requestContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            requestContent.Headers.Add("X-API-KEY", apiKey);
            var response = await _httpClient.PostAsync($"http://localhost:{port}/{broadcasterId}/quotes", requestContent);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SendTestDataToCommands(string apiKey, string broadcasterId, object content, string port = "5221")
        {
            var json = JsonConvert.SerializeObject(content);
            using var requestContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            requestContent.Headers.Add("X-API-KEY", apiKey);
            var response = await _httpClient.PostAsync($"http://localhost:{port}/{broadcasterId}/commands", requestContent);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SendTestDataToBitMenu(string apiKey, string broadcasterId, object content, string port = "5221")
        {
            var json = JsonConvert.SerializeObject(content);
            using var requestContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            requestContent.Headers.Add("X-API-KEY", apiKey);
            var response = await _httpClient.PostAsync($"http://localhost:{port}/{broadcasterId}/bitmenu", requestContent);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SendTestDataToDeathCounter(string apiKey, string broadcasterId, object content, string port = "5221")
        {
            var json = JsonConvert.SerializeObject(content);
            using var requestContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            requestContent.Headers.Add("X-API-KEY", apiKey);
            var response = await _httpClient.PostAsync($"http://localhost:{port}/{broadcasterId}/death_counter", requestContent);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SendTestPoints(string apiKey, string broadcasterId, object content, string port = "5221")
        {
            var json = JsonConvert.SerializeObject(content);
            using var requestContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            requestContent.Headers.Add("X-API-KEY", apiKey);
            var response = await _httpClient.PostAsync($"http://localhost:{port}/{broadcasterId}/leaderboard", requestContent);
            return response.IsSuccessStatusCode;
        }

    }
}