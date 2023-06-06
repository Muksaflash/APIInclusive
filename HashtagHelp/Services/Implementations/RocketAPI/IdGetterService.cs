using HashtagHelp.Domain.Models;
using HashtagHelp.Domain.ResponseModels.RocketAPI;
using HashtagHelp.Services.Interfaces;

namespace HashtagHelp.Services.Implementations.RocketAPI
{
    public class IdGetterService : IIdGetterService
    {
        public async Task<uint> GetIdAsync(ResearchedUserEntity researchedUser)
        {
            var apiKey = "a8f3f7e68amsh2703987539fa87cp17165ajsn6d5c6feed1e9";
            RocketAPIRequestService<User> APIRequestService = new();
            var UserId = await APIRequestService.GetIdAPIAsync(apiKey, researchedUser.NickName);
            return uint.Parse(UserId);
        }
    }
}
