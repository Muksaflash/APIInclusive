using HashtagHelp.Domain.Models;

namespace HashtagHelp.Services.Interfaces
{
    public interface IDataRepository
    {
        void AddTask(ParserTaskEntity task);

        void AddTelegramUser(TelegramUserEntity user);

        Task SaveChangesAsync();
    }
}
