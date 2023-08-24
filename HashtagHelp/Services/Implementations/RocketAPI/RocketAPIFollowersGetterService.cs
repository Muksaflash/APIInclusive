/* using HashtagHelp.Domain.Models;
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
            
            RocketAPIRequestService APIRequestService = new RocketAPIRequestService();
            //var UserId = await APIRequestService.GetIdAPIAsync(apiKey, researchedUser.NickName);
            var userId = researchedUser.SocialId.ToString();
            var followers = await APIRequestService.GetObjectsAPIAsync(userId);
            var resultFollowers = followers.Select(follower => new FollowerEntity()
            {
                SocialId = follower.Pk,
                ResearchedUserNickName = researchedUser.NickName,
                NickName = follower.Username,
                FollowingTagsGetter = FollowingTagsGetter
            }).ToList();
            return resultFollowers;
        }
    }
}
 */