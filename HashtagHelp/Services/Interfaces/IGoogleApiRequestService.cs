using HashtagHelp.Domain.ExternalApiModels;

namespace HashtagHelp.Services.Interfaces
{
    public interface IGoogleApiRequestService
    {
        Task<string> GetDataAsync();
    }
}
