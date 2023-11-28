namespace HashtagHelp.Services.Interfaces
{
    public interface IGoogleApiRequestService
    {
        public string HashtagArea {get; set;}
        public string UserAreaHashtagsSheetName {get; set;}
        public string UserSemiAreasHashtagsSheetName {get; set;}
        public string UserParsedHashtagsSheetName {get; set;}
        public string UserTable {get; set;}
        public string UserOutputSheet {get; set;}
        public List<string> HashtagSemiAreas {get; set;}
        Task<List<string>> GetAreaHashtags();
        Task<List<string>> GetSemiAreaHashtags();
        Task<List<string>> GetAreasListAsync();
        Task<List<string>> GetUserParsedContentAsync();
        Task<List<string>> GetUserAreaHashtagsAsync();
        Task<List<string>> GetUserSemiAreaHashtagsAsync();
        Task<string> GetParameterAsync(string cellAddress);
        Task<List<string>> GetAllConfigSheetDataAsync();
        Task SetParameterAsync(string cellAddress, string newValue);
        Task PublicListAsync(List<string> stringList);
    }
}
