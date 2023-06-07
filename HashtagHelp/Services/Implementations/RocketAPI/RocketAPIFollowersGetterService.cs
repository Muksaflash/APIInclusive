using HashtagHelp.Domain.Models;
using HashtagHelp.Domain.ExternalApiModels.RocketAPI;
using HashtagHelp.Services.Implementations.RocketAPI;
using HashtagHelp.Services.Interfaces;

namespace HashtagHelp.Services.Implementations.InstagramData
{
    public class RocketAPIFollowersGetterService : IFollowersGetterService
    {
        public IFollowingTagsGetterService? FollowingTagsGetter { get; set; }
        public async Task<List<FollowerEntity>> GetFollowersByNameAsync(ResearchedUserEntity researchedUser)
        {
            var apiKey = "a8f3f7e68amsh2703987539fa87cp17165ajsn6d5c6feed1e9";
            
            RocketAPIRequestService<User> APIRequestService = new RocketAPIRequestService<User>();
            //var UserId = await APIRequestService.GetIdAPIAsync(apiKey, researchedUser.NickName);
            var userId = researchedUser.InstagramId.ToString();
            var followers = await APIRequestService.GetObjectsAPIAsync(apiKey, userId);
            var resultFollowers = followers.Select(follower => new FollowerEntity()
            {
                InstagramId = follower.Pk,
                ResearchedUserNickName = researchedUser.NickName,
                NickName = follower.Username,
                FollowingTagsGetter = FollowingTagsGetter
            }).ToList();
            return resultFollowers;
        }
    }
}
