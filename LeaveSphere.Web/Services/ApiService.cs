using Newtonsoft.Json;
using System.Text;

namespace LeaveSphere.Web.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public ApiService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
            var baseUrl = _config["ApiBaseUrl"] ?? "http://localhost:5100/api/";
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        public async Task<string> PostAsync(string endpoint, object data, string? token = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
            
            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            if (data != null)
            {
                var json = JsonConvert.SerializeObject(data);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var response = await _httpClient.SendAsync(request);
            var resultStr = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"API ERROR [POST {endpoint}]: {response.StatusCode} - {resultStr}");
                return $"Error: {response.StatusCode} - {resultStr}";
            }
            return resultStr;
        }

        public async Task<string> GetAsync(string endpoint, string? token = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            
            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            var resultStr = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"API ERROR [GET {endpoint}]: {response.StatusCode} - {resultStr}");
                return $"Error: {response.StatusCode} - {resultStr}";
            }
            return resultStr;
        }

        public async Task<string> PutAsync(string endpoint, object? data, string? token = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Put, endpoint);
            
            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            if (data != null)
            {
                var json = JsonConvert.SerializeObject(data);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var response = await _httpClient.SendAsync(request);
            var resultStr = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"API ERROR [PUT {endpoint}]: {response.StatusCode} - {resultStr}");
                return $"Error: {response.StatusCode} - {resultStr}";
            }
            return resultStr;
        }

        public async Task<string> DeleteAsync(string endpoint, string? token = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
            
            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request);
            var resultStr = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"API ERROR [DELETE {endpoint}]: {response.StatusCode} - {resultStr}");
                return $"Error: {response.StatusCode} - {resultStr}";
            }
            return resultStr;
        }
    }
}