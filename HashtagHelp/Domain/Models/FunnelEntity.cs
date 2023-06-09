﻿using Newtonsoft.Json.Linq;
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

        public List<string>? FunnelText { get; set; }

        public FunnelEntity(long minTagMediaCount, long maxTagMediaCount, long minMediaCountInterval, 
            long hashtagsNumber)
        {
            MinTagMediaCount = minTagMediaCount;
            MaxTagMediaCount = maxTagMediaCount;
            MinMediaCountInterval = minMediaCountInterval;
            HashtagsNumber = hashtagsNumber;
        }
    }
}
