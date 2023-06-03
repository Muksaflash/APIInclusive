using HashtagHelp.Domain.Models;
using HashtagHelp.Domain.ResponseModels.BulkSkrapper;
using HashtagHelp.Services.Interfaces;

namespace HashtagHelp.Services.Implementations.BulkScrapper
{
    public class FollowersGetterService : IFollowersGetterService
    {
        public IFollowingTagsGetterService? FollowingTagsGetter { get; set; }
        public async Task<List<FollowerEntity>> GetFollowersByNameAsync(string nickName)
        {
            // Логика получения объектов FollowerEntity по nickName из базы данных или другого источника данных
            var apiKey = "a8f3f7e68amsh2703987539fa87cp17165ajsn6d5c6feed1e9";
            APIRequestService<Follower> apiRequestService = new APIRequestService<Follower>();
            var followers = await apiRequestService.GetObjectsBulkAPIAsync(apiKey, nickName);
            var resultFollowers = followers.Select(follower => new FollowerEntity()
            {
                InstagramId = (uint)follower.Pk,
                ResearchedUserNickName = nickName,
                NickName = follower.Username,
                FollowingTagsGetter = FollowingTagsGetter
            }).ToList();
            return resultFollowers;
        }
    }
}
