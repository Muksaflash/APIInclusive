using HashtagHelp.Domain.Models;

namespace HashtagHelp.Services.Interfaces
{
    public interface IFunnelService
    {
        public IFollowersTaskService followersTaskService { get; set; }

        Task AddFollowersTaskAsync(ParserTaskEntity parserTak);
    }
}
