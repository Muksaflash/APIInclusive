using HashtagHelp.Domain.ExternalApiModels.RocketAPI;

namespace HashtagHelp.Services.Interfaces
{
    public interface IHashtagApiRequestService
    {
        Task<BodyData> GetHashtagInfoAsync(string apiKey, string hashtag);
    }
}
