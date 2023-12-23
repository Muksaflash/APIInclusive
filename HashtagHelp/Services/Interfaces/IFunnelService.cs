using HashtagHelp.Domain.Models;

namespace HashtagHelp.Services.Interfaces
{
    public interface IFunnelService
    
    {
        Task StartTaskChainAsync();

        Task SetConfigureAsync(GeneralTaskEntity generalTask);

        Task SetGoogleSheetsFunnelConfigureAsync(GoogleSheetsFunnelTaskEntity googleSheetsFunnelTask, FunnelServiceInfoEntity funnelServiceInfo);

        Task WaitCompletionGeneralTaskAsync();
    }
}
