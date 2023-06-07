using HashtagHelp.Domain.Models;
using HashtagHelp.Domain.ExternalApiModels.InstagramData;
using HashtagHelp.Services.Interfaces;

namespace HashtagHelp.Services.Implementations.InstagramData
{
    public class InstagramDataAPIFollowersGetterService : IFollowersGetterService
    {
        public IFollowingTagsGetterService? FollowingTagsGetter { get; set; }
        public async Task<List<FollowerEntity>> GetFollowersByNameAsync(ResearchedUserEntity researchedUser)
        {
            // Логика получения объектов FollowerEntity по nickName из базы данных или другого источника данных
            var apiKey = "a8f3f7e68amsh2703987539fa87cp17165ajsn6d5c6feed1e9";
            InstagramDataApiRequestService<Follower> APIRequestService = new InstagramDataApiRequestService<Follower>();
            var followers = await APIRequestService.GetObjectsAPIAsync(apiKey, researchedUser.NickName);
            var resultFollowers = followers.Select(follower => new FollowerEntity()
            {
                InstagramId = follower.Id,
                ResearchedUserNickName = researchedUser.NickName,
                NickName = follower.Username,
                FollowingTagsGetter = FollowingTagsGetter
            }).ToList();
            return resultFollowers;
        }
    }
}
