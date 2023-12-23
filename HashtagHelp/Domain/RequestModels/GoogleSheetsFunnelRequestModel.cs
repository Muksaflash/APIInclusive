namespace HashtagHelp.Domain.RequestModels;
public class GoogleSheetsFunnelRequestModel
{
    public string TableName { get; set; } = string.Empty;

    public string Id { get; set; } = string.Empty;

    public string SemiAreasSheetName { get; set; } = string.Empty;

    public string AreaSheetName { get; set; } = string.Empty;

    public string ParsedSheetName { get; set; } = string.Empty;

    public string MinHashtagFollowers { get; set; } = string.Empty;

    public string OutputGoogleSheet { get; set; } = string.Empty;
}
