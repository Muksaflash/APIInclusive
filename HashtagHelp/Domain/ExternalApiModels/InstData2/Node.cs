namespace HashtagHelp.Domain.ExternalApiModels.InstData2
{
    public class Node
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsFollowing { get; set; }
        public string ProfilePicUrl { get; set; } = string.Empty;
        public long MediaCount { get; set; }
    }
}
