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

        public async Task<WorkflowRunCheckResult> CheckRunningWorkflowsAsync(string token, string owner, string repo)
        {
            try 
            {
               using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, _url + $"{owner}/{repo}/actions/runs?status=in_progress&per_page=1");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                using HttpResponseMessage response = await _client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var parsed = JsonDocument.Parse(json);

                var runs = parsed.RootElement.GetProperty("workflow_runs");
                if (runs.GetArrayLength() <= 0)
                {
                    return null;
                }

                var run = runs[0];

                WorkflowRun workflowRun = new WorkflowRun
                {
                    Status = run.GetProperty("status").GetString(),
                    Name = run.GetProperty("name").GetString()
                };

                return WorkflowRunCheckResult.Success(workflowRun);
            }
            catch (HttpRequestException ex)
            {
                Console.Error.WriteLine($"Github API Error: {ex.Message}");
                return WorkflowRunCheckResult.Fail(ex.Message);
            }
            catch (JsonException ex)
            {
                Console.Error.WriteLine($"Json parsing Error: {ex.Message}");
                return WorkflowRunCheckResult.Fail(ex.Message);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"I have no idea why this happened: {ex.Message}");
                return WorkflowRunCheckResult.Fail(ex.Message);
            }
        }

        public async Task<string> GetBranchShaAsync(string token, string owner, string repo, string branch = "main")
        {
            try
            {
                using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, _url + $"{owner}/{repo}/branches/{branch}");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                using HttpResponseMessage response = await _client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                return doc.RootElement
                          .GetProperty("commit")
                          .GetProperty("sha")
                          .GetString()!;
            } 
            catch (HttpRequestException ex)
            {
                Console.Error.WriteLine($"Github API Error: {ex.Message}");
                return string.Empty;
            }
            catch (JsonException ex)
            {
                Console.Error.WriteLine($"Json parsing Error: {ex.Message}");
                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"I have no idea why this happened: {ex.Message}");
                return string.Empty;
            }
        }

        public async Task<List<Tag>> GetTagsAsync(string token, string owner, string repo)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, _url + $"{owner}/{repo}/tags");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string json = await response.Content.ReadAsStringAsync();
            Console.WriteLine(json);
            List<Tag> tags = JsonSerializer.Deserialize<List<Tag>>(json, _serializerOptions);

            return tags ?? new List<Tag>();
        }

        public async Task<bool> IsBranchAtCommitAsync(string token, string owner, string repo, string commitSha, string branch = "main")
        {
            string branchSha = await GetBranchShaAsync(token, owner, repo, branch);

            return string.Equals(commitSha, branchSha, StringComparison.OrdinalIgnoreCase);
        }


        public async Task<bool> SetBranchToCommitAsync(string token, string owner, string repo, string commitSha, string branch = "main")
        {
            var payload = new
            {
                sha = commitSha,
                @force = true
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Patch, _url + $"{owner}/{repo}/git/refs/heads/{branch}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            HttpResponseMessage response = await _client.SendAsync(request);
            return response.IsSuccessStatusCode;
        }

        public async Task TriggerWorkflowAsync(string token, string owner, string repo, string workflowFileName, string @ref = "main")
        {
            var payload = new
            {
                @ref = @ref
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _url + $"{owner}/{repo}/actions/workflows/{workflowFileName}/dispatches");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = content;

            HttpResponseMessage response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }
    }
}
