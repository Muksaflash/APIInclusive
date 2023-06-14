using HashtagHelp.Domain.Models;

namespace HashtagHelp.Services.Interfaces
{
    public interface IFollowersTaskService
    {
        public Task AddFollowersTaskAsync(ParserTaskEntity parserTask);
    }
}
