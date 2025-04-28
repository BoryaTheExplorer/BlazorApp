using System.Net.Http.Headers;
using System.Text.Json;
using BlazorApp.Models;
using static System.Net.WebRequestMethods;

namespace BlazorApp
{
    public class GitHubService : IGitHubService
    {
        private readonly HttpClient _client;
        private readonly string _url = "https://api.github.com/repos/";

        private JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        public GitHubService()
        {
            _client = new HttpClient();
            _client.DefaultRequestHeaders.UserAgent.ParseAdd("UmpaLumpa");
        }

        public async Task<List<Tag>> GetTagsAsync(string token, string owner, string repo)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, _url + $"{owner}/{repo}/tags");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            List<Tag> tags = JsonSerializer.Deserialize<List<Tag>>(json, _serializerOptions);

            return tags ?? new List<Tag>();
        }
    }
}
