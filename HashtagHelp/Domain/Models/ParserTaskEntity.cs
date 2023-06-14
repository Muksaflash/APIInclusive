namespace HashtagHelp.Domain.Models
{
    public class ParserTaskEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string InParserId { get; set; }
        public List<ResearchedUserEntity> ResearchedUsers { get; set; } = new List<ResearchedUserEntity>();
    }
}