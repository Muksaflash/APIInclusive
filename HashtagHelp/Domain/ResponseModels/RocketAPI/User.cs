namespace HashtagHelp.Domain.ResponseModels.RocketAPI
{
    public class User
    {
        public bool HasAnonymousProfilePicture { get; set; }
        public string FbidV2 { get; set; }
        public string Pk { get; set; }
        public string PkId { get; set; }
        public string StrongId { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsVerified { get; set; }
        public string ProfilePicId { get; set; }
        public string ProfilePicUrl { get; set; }
        public List<object> AccountBadges { get; set; }
        public bool IsPossibleScammer { get; set; }
        public int ThirdPartyDownloadsEnabled { get; set; }
        public int LatestReelMedia { get; set; }
    }
}
