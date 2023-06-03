using HashtagHelp.Services.Interfaces;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.ComponentModel.DataAnnotations.Schema;

namespace HashtagHelp.Domain.Models
{
    public class ResearchedUserEntity : UserEntity
    {
        [NotMapped]
        [NonSerialized]
        public IFollowersGetterService? FollowersGetter;

        public uint FollowersNumber { get; set; }

        //public Dictionary<HashtagEntity, uint> FollowersTagFreq { get; set; } = new Dictionary<HashtagEntity, uint>();

        public List<FollowerEntity> Followers { get; set; } = new List<FollowerEntity>();

        public FunnelEntity? Funnel { get; set; }

        public async Task GetFollowersAsync()
        {
            if (FollowersGetter == null) throw new NullReferenceException();
            Followers = await FollowersGetter.GetFollowersByNameAsync(NickName);
            await Task.CompletedTask;
        }
    }
}
