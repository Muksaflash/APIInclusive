namespace HashtagHelp.Domain.ExternalApiModels.BulkSkrapper
{
    public class DataResponce<T>
    {
        public List<T>? Data { get; set; }
        public Cursor? Cursor { get; set; }
    }
}
