using HashtagHelp.Services.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace HashtagHelp.Domain.Models
{
    public class FollowerEntity : UserEntity
    {
        [ForeignKey("ResearchedUserEntity")]
        public string ResearchedUserNickName { get; set; } = string.Empty;
        public List<HashtagEntity> FollowedTags { get; set; } = new List<HashtagEntity>();

        [NotMapped]
        [NonSerialized]
        public IFollowingTagsGetterService? FollowingTagsGetter;
    }
}
