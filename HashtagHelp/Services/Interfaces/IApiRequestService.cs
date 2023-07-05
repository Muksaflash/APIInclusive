using HashtagHelp.Domain.ExternalApiModels.InstaParser;

namespace HashtagHelp.Services.Interfaces
{
    public interface IApiRequestService
    {
        Task<string> AddFollowersTaskAPIAsync(string apiKey, List<string> userNames);
        Task<TaskStatusResponse> GetTaskStatusAsync(string apiKey, string taskId);
        Task<string> AddFollowingTagsTaskAPIAsync(string apiKey, string FollowersTaskId, List<string> researchedUsers);
        Task<string> GetTagsTaskContentAPIAsync(string apiKey, string FollowingTagsTaskId);
    }
}
