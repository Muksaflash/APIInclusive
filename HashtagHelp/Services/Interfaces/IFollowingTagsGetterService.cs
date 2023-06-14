using HashtagHelp.Domain.Models;

namespace HashtagHelp.Services.Interfaces
{
    public interface IFollowingTagsGetterService
    {
        public Task<List<HashtagEntity>> GetFollowingTagsByIdAsync(string id);
    }
}
