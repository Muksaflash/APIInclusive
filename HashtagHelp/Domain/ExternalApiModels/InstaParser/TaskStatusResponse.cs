namespace HashtagHelp.Domain.ExternalApiModels.InstaParser
{
    public class TaskStatusResponse
    {
        public string Status { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
        public string tid_status { get; set; }
        public DateTime AddTime { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
