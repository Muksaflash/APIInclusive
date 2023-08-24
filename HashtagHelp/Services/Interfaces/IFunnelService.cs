using HashtagHelp.Domain.Models;
using HashtagHelp.Domain.RequestModels;

namespace HashtagHelp.Services.Interfaces
{
    public interface IFunnelService
    
    {
        Task StartTaskChainAsync();

        Task SetConfigureAsync(GeneralTaskEntity generalTask);

        Task WaitCompletionGeneralTaskAsync();
    }
}
