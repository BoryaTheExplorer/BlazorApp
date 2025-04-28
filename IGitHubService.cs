using BlazorApp.Models;

namespace BlazorApp
{
    public interface IGitHubService
    {
        public Task<List<Tag>> GetTagsAsync(string token, string owner, string repo);
    }
}
