using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using HashtagHelp.Domain.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HashtagHelp.Domain.Models
{
    public class GeneralTaskEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public UserEntity? User { get; set; }

        public ParserTaskEntity? CollectionTask { get; set; }

        public ParserTaskEntity? FiltrationTask { get; set; }

        [NotMapped]
        public FunnelEntity? HashtagFunnel;

        public string HashtagArea { get; set; } = string.Empty;
        
        [JsonProperty("status")]
        [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
        public StatusTaskEnum Status { get; set; } = StatusTaskEnum.Initiated;

        public string MainParserApiKey { get; set; } = string.Empty;

        public string ParserUrl { get; set; } = string.Empty;

        public string ErrorInfo { get; set; } = string.Empty; // новое
    }
}