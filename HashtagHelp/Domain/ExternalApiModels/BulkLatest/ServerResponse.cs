namespace HashtagHelp.Domain.ExternalApiModels.BulkLatest
{
    public class ServerResponse
    {
        public List<Datum> data { get; set; }
        public string status { get; set; }
        public string message { get; set; }
    }
}
