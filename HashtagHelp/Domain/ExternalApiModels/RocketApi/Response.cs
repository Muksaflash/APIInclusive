namespace HashtagHelp.Domain.ExternalApiModels.RocketAPI
{
    public class Response<T>
    {
        public int StatusCode { get; set; }
        public string? ContentType { get; set; }
        public Body<T>? Body { get; set; }
    }
}
