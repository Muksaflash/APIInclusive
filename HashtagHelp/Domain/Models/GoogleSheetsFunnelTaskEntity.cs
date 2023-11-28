using System.ComponentModel.DataAnnotations.Schema;
using HashtagHelp.Domain.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace HashtagHelp.Domain.Models;
public class GoogleSheetsFunnelTaskEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [NotMapped]
    public FunnelEntity? HashtagFunnel;

    [JsonProperty("status")]
    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
    public StatusTaskEnum Status { get; set; } = StatusTaskEnum.Initiated;

    public string ErrorInfo { get; set; } = string.Empty; // новое

    public string TableId { get; set; } = string.Empty;

    public string SemiAreasSheetName { get; set; } = string.Empty;

    public string AreaSheetName { get; set; } = string.Empty;

    public string ParsedSheetName { get; set; } = string.Empty;

    public string MinHashtagFollowers { get; set; } = string.Empty;

    public string OutputGoogleSheet { get; set; } = string.Empty;
}
