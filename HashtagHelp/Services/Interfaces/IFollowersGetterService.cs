using HashtagHelp.Domain.Models;

namespace HashtagHelp.Services.Interfaces
{
    public interface IFollowersGetterService
    {
        public IFollowingTagsGetterService? FollowingTagsGetter { get; set; }

        public Task<List<FollowerEntity>> GetFollowersByNameAsync(string nickName);
    }
}
