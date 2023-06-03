namespace HashtagHelp.Domain.ResponseModels.BulkSkrapper
{
    public class Cursor
    {
        public bool MoreAvailable { get; set; }
        public string? NextMaxId { get; set; }
    }
}
