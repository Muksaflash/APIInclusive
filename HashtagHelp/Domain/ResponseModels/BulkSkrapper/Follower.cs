namespace HashtagHelp.Domain.ResponseModels.BulkSkrapper
{
    public class Follower
    {
        public long Pk { get; set; }
        public string? Username { get; set; }
        public string? Full_Name { get; set; }
        public bool Is_Verified { get; set; }
        public bool Is_Private { get; set; }
        public string? Profile_Pic_Url { get; set; }
    }
}
