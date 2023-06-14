using HashtagHelp.Domain.Models;
using HashtagHelp.Services.Interfaces;

namespace HashtagHelp.Services.Implementations
{
    public class FollowersService : IFunnelService
    {
        public IFollowersTaskService followersTaskService { get; set; }

        public async Task AddFollowersTaskAsync(ParserTaskEntity parserTask)
        {
            try
            {
                await followersTaskService.AddFollowersTaskAsync(parserTask);

                await Task.CompletedTask;


            }
            catch (Exception ex) { }
        }
    }
}
