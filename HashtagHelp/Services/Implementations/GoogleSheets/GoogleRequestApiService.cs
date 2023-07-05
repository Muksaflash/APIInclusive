using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using HashtagHelp.Services.Interfaces;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HashtagHelp.Services.Implementations.InstaParser
{
    public class GoogleRequestApiService : IGoogleApiRequestService
    {
        public string credentialsPath = "hashtaghelp-13d38e1b1284.json";
        private readonly SheetsService sheetsService;
        public GoogleRequestApiService()
        {
            GoogleCredential credential;
            using (var stream = new FileStream(credentialsPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(SheetsService.Scope.Spreadsheets);
            }

            sheetsService = new(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "HashtagHelp"
            });
        }
        public async Task<List<string>> GetDataAsync(string hashtagArea)
        {
            string spreadsheetId = "1GB4pkp8M2H2twaliGdhD5pEDlsmEPC9gBT_FUmPq270";
            string sheetName = "hashtagArea";

            SpreadsheetsResource.ValuesResource.GetRequest request =
                sheetsService.Spreadsheets.Values.Get(spreadsheetId, sheetName);

            request.MajorDimension = SpreadsheetsResource.ValuesResource.GetRequest.MajorDimensionEnum.ROWS;

            ValueRange response = await request.ExecuteAsync();
            IList<IList<object>> values = response.Values;
            List<string> hashtags = new();

            if (values != null && values.Count > 0)
            {
                foreach (var row in values)
                {
                    if (row.Count >= 2)
                    {
                        var hashtag = row[0].ToString();
                        hashtags.Add(hashtag);
                    }
                }
                return hashtags;
            }
            return hashtags;
        }
    }
}
