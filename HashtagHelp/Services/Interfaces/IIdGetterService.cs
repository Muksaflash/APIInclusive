using HashtagHelp.Domain.Models;

namespace HashtagHelp.Services.Interfaces
{
    public interface IIdGetterService
    {
        public Task<string> GetIdAsync(ResearchedUserEntity researchedUser);
    }
}
