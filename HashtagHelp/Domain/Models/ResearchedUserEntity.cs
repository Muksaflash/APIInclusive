using HashtagHelp.Services.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;

namespace HashtagHelp.Domain.Models
{
    public class ResearchedUserEntity : UserEntity
    {
        [NotMapped]
        [NonSerialized]
        public IFollowersGetterService? FollowersGetter;

        [NotMapped]
        [NonSerialized]
        public IIdGetterService? IdGetter;

        public uint FollowersNumber { get; set; }

        //[NotMapped]
        //[NonSerialized]
        //public Dictionary<HashtagEntity, uint> FollowersTagFreq { get; set; } = new Dictionary<HashtagEntity, uint>();

        public List<FollowerEntity> Followers { get; set; } = new List<FollowerEntity>();

        public FunnelEntity? Funnel { get; set; }

        public async Task GetFollowersAsync()
        {
            if (FollowersGetter == null) throw new NullReferenceException();
            Followers = await FollowersGetter.GetFollowersByNameAsync(this);
            await Task.CompletedTask;
        }
        public async Task GetIdAsync()
        {
            if (IdGetter == null) throw new NullReferenceException();
            SocialId = await IdGetter.GetIdAsync(this);
            await Task.CompletedTask;
        }
    }
}
