using System.ComponentModel.DataAnnotations.Schema;
using HashtagHelp.Domain.Enums;
using HashtagHelp.Domain.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HashtagHelp.Domain.Models
{
    public class GeneralTaskEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [NotMapped]
        public FunnelEntity? HashtagFunnel;
        
        [JsonProperty("status")]
        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        public StatusTaskEnum Status { get; set; } = StatusTaskEnum.Initiated;

        public string ErrorInfo { get; set; } = string.Empty;
        public UserEntity? User { get; set; }

        public ParserTaskEntity? CollectionTask { get; set; }

        public ParserTaskEntity? FiltrationTask { get; set; }

        public string HashtagArea { get; set; } = string.Empty;

        public string HashtagSemiAreas { get; set; } = string.Empty;

        public string MainParserApiKey { get; set; } = string.Empty;

        public string ParserUrl { get; set; } = string.Empty;
    }
}