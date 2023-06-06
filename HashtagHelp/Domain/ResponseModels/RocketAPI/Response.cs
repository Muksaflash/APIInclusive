namespace HashtagHelp.Domain.ResponseModels.RocketAPI
{
    public class Response<T>
    {
        public int StatusCode { get; set; }
        public string ContentType { get; set; }
        public Body<T> Body { get; set; }
    }
}
