using HashtagHelp.Domain.Models;
using HashtagHelp.Services.Interfaces;

namespace HashtagHelp.Services.Implementations.InstData2
{
    public class InstData2FollowingTagsGetterService : IFollowingTagsGetterService
    {
        public async Task<List<HashtagEntity>> GetFollowingTagsByIdAsync(string id)
        {
            var apiKey = "a8f3f7e68amsh2703987539fa87cp17165ajsn6d5c6feed1e9";

            InstData2APIRequestService ApiRequestService = new();
            var nodes = await ApiRequestService.GetHashtagAPIAsync(apiKey, id);
            var Hashtags = nodes.Select(node => new HashtagEntity()
            {
                InstagramId = node.Id,
                Name= node.Name,
                //MediaCount= node.MediaCount
            }).ToList();
            return Hashtags;
        }
    }
}
