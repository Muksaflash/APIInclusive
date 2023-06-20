using HashtagHelp.Domain.Models;

namespace HashtagHelp.Services.Interfaces
{
    public interface IFunnelService
    {
        public IApiRequestService ApiRequestService { get; set; }

        Task AddFollowersTaskAsync(ParserTaskEntity parserTak);

        //Task CheckFollowersTaskStatus(ParserTaskEntity parserTak);
    }
}
