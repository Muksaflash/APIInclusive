using HashtagHelp.Domain.ExternalApiModels.RocketAPI;

namespace HashtagHelp.Services.Interfaces
{
    public interface IHashtagApiRequestService
    {
        Task<BodyData> GetMediaCountAsync(string apiKey, string hashtag);
    }
}
