using BlazorApp.Models;

namespace BlazorApp
{
    public interface IGitHubService
    {
        public Task<List<Tag>> GetTagsAsync(string token, string owner, string repo);
        public Task<bool> IsBranchAtCommitAsync(string token, string owner, string repo, string sha, string branch = "main");
        public Task<string> GetBranchShaAsync(string token, string owner, string repo, string branch = "main");
        public Task<bool> SetBranchToCommitAsync(string token, string owner, string repo, string commitSha, string branch = "main");
        public Task TriggerWorkflowAsync(string token, string owner, string repo, string workflow, string @ref = "main");
        public Task<WorkflowRunCheckResult> CheckRunningWorkflowsAsync(string token, string owner, string repo);
    }
}
