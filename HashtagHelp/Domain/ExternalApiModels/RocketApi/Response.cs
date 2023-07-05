namespace HashtagHelp.Domain.ExternalApiModels.RocketAPI
{
    public class Response
    {
        public int StatusCode { get; set; }
        public string? ContentType { get; set; }
        public ResponseBody? Body { get; set; }
    }
}
