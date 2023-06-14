using HashtagHelp.DAL;
using HashtagHelp.Domain.Models;
using HashtagHelp.Services.Interfaces;

namespace HashtagHelp.Services.Implementations
{
    public class DataRepository : IDataRepository
    {
        private readonly AppDbContext _context;

        public DataRepository(AppDbContext context)
        {
            _context = context;
        }

        public void AddTask(ParserTaskEntity task)
        {
            _context.Tasks.Add(task);
        }

        public void AddTelegramUser(TelegramUserEntity user)
        {
            _context.TelegramUsers.Add(user);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
