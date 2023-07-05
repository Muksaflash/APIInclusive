namespace HashtagHelp.Domain.ExternalApiModels.RocketAPI
{
    public class Body
    {
        public List<User>? Users { get; set; }
        public bool BigList { get; set; }
        public int PageSize { get; set; }
        public string? NextMaxId { get; set; }
        public bool HasMore { get; set; }
        public bool ShouldLimitListOfFollowers { get; set; }
        public string? Status { get; set; }
    }
}
