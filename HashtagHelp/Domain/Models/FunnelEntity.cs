using Microsoft.EntityFrameworkCore;

namespace HashtagHelp.Domain.Models
{
    [Keyless]
    public class FunnelEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public long MinTagMediaCount { get; set; }

        public long MaxTagMediaCount { get; set; }

        public long MinFollowersTagFreq { get; set; }

        public long MinMediaCountInterval { get; set; }

        public long HashtagsNumber { get; set; }

        public List<HashtagEntity>? ParsedHashtagEntities { get; set; }

        public List<HashtagEntity>? AreaHashtagEntities { get; set; }

        public List<HashtagEntity>? SemiAreaHashtagEntities { get; set; }

        public string? FunnelText { get; set; } = string.Empty;

        public FunnelEntity(long minTagMediaCount, long maxTagMediaCount, long minMediaCountInterval,
            long hashtagsNumber, List<HashtagEntity>? parsedHashtagEntities, List<HashtagEntity>? areaHashtagEntities,
            List<HashtagEntity>? semiAreaHashtagEntities)
        {
            MinTagMediaCount = minTagMediaCount;
            MaxTagMediaCount = maxTagMediaCount;
            MinMediaCountInterval = minMediaCountInterval;
            HashtagsNumber = hashtagsNumber;
            ParsedHashtagEntities = parsedHashtagEntities;
            AreaHashtagEntities = areaHashtagEntities;
            SemiAreaHashtagEntities = semiAreaHashtagEntities;
        }

    }
}
