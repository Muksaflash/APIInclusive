using HashtagHelp.Domain.Enums;

namespace HashtagHelp.Domain.Models
{
    public class ParserTaskEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string InParserId { get; set; } = string.Empty;
        public List<ResearchedUserEntity> ResearchedUsers { get; set; } = new List<ResearchedUserEntity>();
        public StatusTaskEnum Status { get; set; } = StatusTaskEnum.InProcess;
        
    }
}

