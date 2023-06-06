using HashtagHelp.Domain.Models;

namespace HashtagHelp.Services.Interfaces
{
    public interface IFunnelCreatorService
    {
        Task CreateFunnelAsync(ResearchedUserEntity researchedUser);
    }
}
