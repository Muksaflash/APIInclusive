using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using HashtagHelp.Services.Interfaces;

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
        public async Task<string> GetDataAsync()
        {
            string spreadsheetId = "1GB4pkp8M2H2twaliGdhD5pEDlsmEPC9gBT_FUmPq270";
            string range = "food!A1";

            SpreadsheetsResource.ValuesResource.GetRequest request =
                sheetsService.Spreadsheets.Values.Get(spreadsheetId, range);

            ValueRange response = await request.ExecuteAsync();
            IList<IList<object>> values = response.Values;

            if (values != null && values.Count > 0)
            {
                string cellValue = values[0][0].ToString();
                Console.WriteLine("Значение ячейки A1: " + cellValue);
                return cellValue;
            }
            return "don't work GoogleSheet API"; 
        }
    }
}
