﻿using HashtagHelp.Domain.ExternalApiModels.InstaParser;

namespace HashtagHelp.Services.Interfaces
{
    public interface IApiRequestService
    {
        Task<string> AddFollowersTaskAPIAsync(string apiKey, List<string> userNames, string url);
        Task<TaskStatusResponse> GetTaskStatusAsync(string apiKey, string taskId, string url);
        Task<string> AddFollowingTagsTaskAPIAsync(string apiKey, string FollowersTaskId, List<string> researchedUsers, string url);
        Task<string> GetTagsTaskContentAPIAsync(string apiKey, string FollowingTagsTaskId, string url);
    }
}
