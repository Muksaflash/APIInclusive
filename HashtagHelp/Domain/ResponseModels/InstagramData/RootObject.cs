namespace HashtagHelp.Domain.ResponseModels.InstagramData
{
    public class RootObject<T>
    {
        public int Count { get; set; }
        public bool Has_More { get; set; }
        public string? End_Cursor { get; set; }
        public string? Id { get; set; }
        public List<T>? Collector { get; set; }
    }
}
