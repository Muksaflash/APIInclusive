namespace HashtagHelp.Domain.ExternalApiModels.RocketAPI
{
    public class ResponseBody
    {
        public int count { get; set; }
        public BodyData data { get; set; }
        public string status { get; set; } = string.Empty;
        public List<User>? Users { get; set; }
    }
}
