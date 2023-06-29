using HashtagHelp.Domain.Models;
using HashtagHelp.Domain.ExternalApiModels.RocketAPI;
using HashtagHelp.Services.Interfaces;

namespace HashtagHelp.Services.Implementations.RocketAPI
{
    public class RocketAPIIdGetterService : IIdGetterService
    {
        public async Task<string> GetIdAsync(ResearchedUserEntity researchedUser)
        {
            var apiKey = "a8f3f7e68amsh2703987539fa87cp17165ajsn6d5c6feed1e9";
            RocketAPIRequestService APIRequestService = new();
            var UserId = await APIRequestService.GetIdAPIAsync(apiKey, researchedUser.NickName);
            return UserId;
        }
    }
}
