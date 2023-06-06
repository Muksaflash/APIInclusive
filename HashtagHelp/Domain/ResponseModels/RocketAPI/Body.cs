namespace HashtagHelp.Domain.ResponseModels.RocketAPI
{
    public class Body<T>
    {
        public List<T> Users { get; set; }
        public bool BigList { get; set; }
        public int PageSize { get; set; }
        public string NextMaxId { get; set; }
        public bool HasMore { get; set; }
        public bool ShouldLimitListOfFollowers { get; set; }
        public string Status { get; set; }
    }
}
