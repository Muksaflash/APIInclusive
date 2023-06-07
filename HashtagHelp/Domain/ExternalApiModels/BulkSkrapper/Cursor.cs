namespace HashtagHelp.Domain.ExternalApiModels.BulkSkrapper
{
    public class Cursor
    {
        public bool MoreAvailable { get; set; }
        public string? NextMaxId { get; set; }
    }
}
