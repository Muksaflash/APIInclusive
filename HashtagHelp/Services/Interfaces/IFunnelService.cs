using HashtagHelp.Domain.Models;
using HashtagHelp.Domain.RequestModels;

namespace HashtagHelp.Services.Interfaces
{
    public interface IFunnelService
    
    {
        public IApiRequestService InstaParserApiRequestService { get; set; }

        public IHashtagApiRequestService HashtagApiRequestService { get; set; }

        public IParserDataService ParserDataService { get; set; }

        Task StartTaskChainAsync();
        
        public IProcessLogger ProcessLogger{ get; set; }

        public IGoogleApiRequestService GoogleApiRequestService { get; set; }

        public IDataRepository DataRepository { get; set; }

        Task SetConfigureAsync(GeneralTaskEntity generalTask);

        Task WaitCompletionGeneralTask();
    }
}
