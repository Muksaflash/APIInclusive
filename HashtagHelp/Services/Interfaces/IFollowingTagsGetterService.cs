using HashtagHelp.Domain.Models;

namespace HashtagHelp.Services.Interfaces
{
    public interface IFollowingTagsGetterService
    {
        public List<FollowerEntity> GetFollowingTagsById(uint id);
    }
}
