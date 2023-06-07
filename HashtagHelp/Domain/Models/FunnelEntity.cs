using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices.JavaScript;

namespace HashtagHelp.Domain.Models
{
    public class FunnelEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public long MinTagMediaCount { get; set; }

        public long MaxTagMediaCount { get; set; }

        public long MinFollowersTagFreq { get; set; }

        public long MinMediaCountInterval { get; set; }

        public long HashtagsNumber { get; set; }

        public List<HashtagEntity> FilteredHashtag { get; set; } = new List<HashtagEntity>();

        public string FunnelText { get; set; } = string.Empty;
    }
}
