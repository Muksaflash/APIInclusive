using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices.JavaScript;

namespace HashtagHelp.Domain.Models
{
    public class FunnelEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public uint MinTagMediaCount { get; set; }

        public uint MaxTagMediaCount { get; set; }

        public uint MinFollowersTagFreq { get; set; }

        public uint MinMediaCountInterval { get; set; }

        public uint HashtagsNumber { get; set; }

        public List<HashtagEntity> FilteredHashtag { get; set; } = new List<HashtagEntity>();

        public string FunnelText { get; set; } = string.Empty;
    }
}
