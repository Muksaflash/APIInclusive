namespace HashtagHelp.Domain.ExternalApiModels.RocketAPI
{
    public class RootObject<T>
    {
        public string Status { get; set; }
        public Response<T> Response { get; set; }
    }
}
