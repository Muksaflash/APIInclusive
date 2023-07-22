using HashtagHelp.Domain.ExternalApiModels;

namespace HashtagHelp.Services.Interfaces
{
    public interface IGoogleApiRequestService
    {
        public string HashtagArea {get; set;}
        Task<List<string>> GetAreaHashtags();
        Task<List<string>> GetAreasListAsync();
        Task<string> GetParameterAsync(string cellAddress);
        Task<List<string>> GetAllConfigSheetData();
        Task SetParameterAsync(string cellAddress, string newValue);
    }
}
