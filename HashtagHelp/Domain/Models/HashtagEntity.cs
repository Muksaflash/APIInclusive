namespace HashtagHelp.Domain.Models
{
    public class HashtagEntity
    {
        public string Name { get; set; } = string.Empty;
        public Guid Id { get; set; } = Guid.NewGuid();
        public long MediaCount { get; set; }
    }
}
