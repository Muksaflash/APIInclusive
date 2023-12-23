
namespace HashtagHelp.Domain.Models
{
    public class FunnelServiceInfoEntity
    {
        public Guid Id { get; set; }

        public string SocialId { get; set; } = string.Empty;

        public string ParsedHashtagNumber {get; set;} = string.Empty;

        public string FilteredHashtagNumber {get; set;} = string.Empty;

        public string AreaHashtagNumber {get; set; } = string.Empty;

        public string SemiAreaHashtagNumber {get; set; } = string.Empty;

        public string HashtagsReadyNumber {get; set; } = string.Empty;

        public string Hashtag {get; set; } = string.Empty;

        public string IsRequestToHashtagParser { get; set; } = string.Empty;

        public string Status {get; set;} = "Initiated"; 
    }
}
