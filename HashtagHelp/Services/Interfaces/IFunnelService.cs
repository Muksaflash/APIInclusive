using HashtagHelp.Domain.Models;

namespace HashtagHelp.Services.Interfaces
{
    public interface IFunnelService
    {
        public IApiRequestService ApiRequestService { get; set; }

        public IHashtagApiRequestService HashtagApiRequestService { get; set; }

        public IParserDataService ParserDataService { get; set; }

        Task AddFollowersTaskAsync(GeneralTaskEntity generalTaskEntity);
        
        public IProcessLogger ProcessLogger{ get; set; }

        public IGoogleApiRequestService GoogleApiRequestService { get; set; }

        public IDataRepository DataRepository { get; set; }
    }
}
