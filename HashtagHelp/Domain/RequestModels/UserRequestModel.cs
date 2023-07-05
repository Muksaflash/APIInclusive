using HashtagHelp.Domain.Enums;

namespace HashtagHelp.Domain.RequestModels
{
    public class UserRequestModel
    {
        public string NickName { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public List<string> RequestNickNames { get; set; } = new List<string>();
        public string HashtagArea { get; set; } = string.Empty;
    }
}
