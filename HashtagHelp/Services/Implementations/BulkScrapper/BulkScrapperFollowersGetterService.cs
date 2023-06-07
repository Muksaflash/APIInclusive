using HashtagHelp.Domain.Models;
using HashtagHelp.Domain.ExternalApiModels.BulkSkrapper;
using HashtagHelp.Services.Interfaces;

namespace HashtagHelp.Services.Implementations.BulkScrapper
{
    public class BulkScrapperFollowersGetterService : IFollowersGetterService
    {
        public IFollowingTagsGetterService? FollowingTagsGetter { get; set; }
        public async Task<List<FollowerEntity>> GetFollowersByNameAsync(ResearchedUserEntity researchedUser)
        {
            // Логика получения объектов FollowerEntity по nickName из базы данных или другого источника данных
            var apiKey = "a8f3f7e68amsh2703987539fa87cp17165ajsn6d5c6feed1e9";
            BulkScrapperAPIRequestService<Follower> apiRequestService = new BulkScrapperAPIRequestService<Follower>();
            var followers = await apiRequestService.GetObjectsBulkAPIAsync(apiKey, researchedUser.NickName);
            var resultFollowers = followers.Select(follower => new FollowerEntity()
            {
                InstagramId = follower.Pk.ToString(),
                ResearchedUserNickName = researchedUser.NickName,
                NickName = follower.Username,
                FollowingTagsGetter = FollowingTagsGetter
            }).ToList();
            return resultFollowers;
        }
    }
}
