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
        public async Task<bool> PushToUserPortalAsync(string apiKey, string broadcasterId, object content, string endpoint)
        {
            var json = JsonConvert.SerializeObject(content);
            using var requestContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            requestContent.Headers.Add("X-API-KEY", apiKey);
            var response = await _httpClient.PostAsync($"https://user.streamup.tips/{broadcasterId}/{endpoint}", requestContent);
            return response.IsSuccessStatusCode;
        }

        //! TESTING ACTIONS INTERNAL USE ONLY
        public async Task<bool> SendTestDataToUserPortal(string apiKey, string broadcasterId, object content, string port, string endpoint)
        {
            var json = JsonConvert.SerializeObject(content);
            using var requestContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            requestContent.Headers.Add("X-API-KEY", apiKey);
            var response = await _httpClient.PostAsync($"http://localhost:{port}/{broadcasterId}/{endpoint}", requestContent);
            return response.IsSuccessStatusCode;
        }
    }
    
}