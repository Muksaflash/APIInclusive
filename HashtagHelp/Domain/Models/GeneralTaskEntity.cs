using HashtagHelp.Domain.Enums;

namespace HashtagHelp.Domain.Models
{
    public class GeneralTaskEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public UserEntity? User { get; set; }
        public ParserTaskEntity? CollectionTask { get; set; } 
        public ParserTaskEntity? FiltrationTask { get; set; } 
        public FunnelEntity? HashtagFunnel { get; set; }
        public HashtagsScopeEnum HashtagScopeEnum { get; set; }
        public string? InstaParserKey { get; set; }
    }
}