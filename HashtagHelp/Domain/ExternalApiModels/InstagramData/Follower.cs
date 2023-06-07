namespace HashtagHelp.Domain.ExternalApiModels.InstagramData
{
    public class Follower
    {
        public string? Id { get; set; }
        public string? Username { get; set; }
        public string? Full_Name { get; set; }
        public string? Profile_Pic_Url { get; set; }
        public bool Is_Private { get; set; }
        public bool Is_Verified { get; set; }
    }
}
