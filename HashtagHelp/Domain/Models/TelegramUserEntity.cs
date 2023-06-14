namespace HashtagHelp.Domain.Models
{
    public class TelegramUserEntity : UserEntity
    {
        public List<ParserTaskEntity> ParserTasks { get; set; } = new List<ParserTaskEntity>();
    }
}
