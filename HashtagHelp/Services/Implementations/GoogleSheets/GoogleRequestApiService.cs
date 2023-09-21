using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using HashtagHelp.Services.Interfaces;

namespace HashtagHelp.Services.Implementations.InstaParser
{
    public class GoogleRequestApiService : IGoogleApiRequestService
    {
        public string HashtagArea { get; set; }
        private readonly string _configGoogleSpreadsheetID = "1BXVyKV6ScRArAx3qdYxqOiHPJsO5ByfBrSs58I8vojA";
        private readonly string _configGoogleSheet = "config1";
        private readonly string _spreadsheetId = "1GB4pkp8M2H2twaliGdhD5pEDlsmEPC9gBT_FUmPq270";
        private readonly int _maxAttempts = 5;
        private readonly int _maxBackoffTime = 64000;
        private readonly SheetsService _sheetsService;
        private readonly string _credentialFileName = "hashtaghelp-13d38e1b1284.json";
        private readonly string _applicationName = "HashtagHelp";
        private readonly string _configRange = "!A2:Z";
        private readonly string _areaHashtagsRange = "!A1:B";
        private readonly string _areasListRange = "!A2:A";

        public GoogleRequestApiService()
        {
            GoogleCredential credential;
            using (var stream = new FileStream(_credentialFileName, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(SheetsService.Scope.Spreadsheets);
            }
            _sheetsService = new(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = _applicationName
            });
        }

        public async Task<List<string>> GetAllConfigSheetData()
        {
            return await ExecuteWithRetryAsync(_maxAttempts, async () =>
            {
                string range = _configGoogleSheet + _configRange;
                SpreadsheetsResource.ValuesResource.GetRequest request =
                    _sheetsService.Spreadsheets.Values.Get(_configGoogleSpreadsheetID, range);

                ValueRange response = await request.ExecuteAsync();
                IList<IList<object>> values = response.Values;
                List<string> Data = new();

                if (values != null && values.Count > 0)
                {
                    foreach (var row in values)
                    {
                        foreach (var cell in row)
                        {
                            Data.Add(cell.ToString().Trim());
                        }
                    }
                }
                return Data;
            });
        }

        public async Task<List<string>> GetAreaHashtags()
        {
            return await ExecuteWithRetryAsync(_maxAttempts, async () =>
            {
                string sheetName = HashtagArea;
                string range = sheetName + _areaHashtagsRange;
                SpreadsheetsResource.ValuesResource.GetRequest request =
                    _sheetsService.Spreadsheets.Values.Get(_spreadsheetId, range);

                request.MajorDimension = SpreadsheetsResource.ValuesResource.GetRequest.MajorDimensionEnum.ROWS;

                ValueRange response = await request.ExecuteAsync();
                IList<IList<object>> values = response.Values;
                List<string> hashtags = new();

                if (values != null && values.Count > 0)
                {
                    foreach (var row in values)
                    {
                        var hashtag = row[0].ToString().Trim();
                        hashtags.Add(hashtag);
                    }
                }
                return hashtags;
            });
        }

        public async Task<List<string>> GetAreasListAsync()
        {
            return await ExecuteWithRetryAsync(_maxAttempts, async () =>
            {
                string range = _configGoogleSheet + _areasListRange;
                SpreadsheetsResource.ValuesResource.GetRequest request =
                    _sheetsService.Spreadsheets.Values.Get(_configGoogleSpreadsheetID, range);
                ValueRange response = await request.ExecuteAsync();
                IList<IList<object>> values = response.Values;
                List<string> areas = new();
                if (values != null && values.Count > 0)
                {
                    foreach (var row in values)
                    {
                        foreach (var cell in row)
                        {
                            areas.Add(cell.ToString().Trim());
                        }
                    }
                }
                return areas;
            });
        }

        public async Task<string> GetParameterAsync(string cellAddress)
        {
            return await ExecuteWithRetryAsync(_maxAttempts, async () =>
            {
                string range = _configGoogleSheet + "!" + cellAddress;
                SpreadsheetsResource.ValuesResource.GetRequest request =
                    _sheetsService.Spreadsheets.Values.Get(_configGoogleSpreadsheetID, range);
                ValueRange response = await request.ExecuteAsync();
                IList<IList<object>> values = response.Values;
                if (values != null && values.Count > 0)
                {
                    var cellValue = values[0][0].ToString().Trim();
                    Console.WriteLine("Cell value " + cellAddress + ": " + cellValue);
                    return cellValue ?? throw new Exception("cellValue is null");
                }
                throw new Exception("empty cell parameter Google Sheet");
            });
        }

        public async Task SetParameterAsync(string cellAddress, string newValue)
        {
            await ExecuteWithRetryAsync(_maxAttempts, async () =>
            {
                string range = $"{_configGoogleSheet}!{cellAddress}";
                ValueRange updateRequest = new()
                {
                    Values = new List<IList<object>> { new List<object> { newValue } },
                };
                SpreadsheetsResource.ValuesResource.UpdateRequest updateCellRequest =
                    _sheetsService.Spreadsheets.Values.Update(updateRequest, _configGoogleSpreadsheetID, range);
                updateCellRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
                await updateCellRequest.ExecuteAsync();
                return "Cell value " + cellAddress + " updated: " + newValue;
            });
        }

        private async Task<T> ExecuteWithRetryAsync<T>(int maxAttempts, Func<Task<T>> action)
        {
            Exception error = null;
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex) when (IsRetryableError(ex) && attempt < maxAttempts)
                {
                    error = ex;
                    await HandleRetryableError(ex, attempt, _maxBackoffTime);
                }
                catch (Exception ex) when (!IsRetryableError(ex))
                {
                    Console.WriteLine("Non-repeating error (attempt " + attempt + "): " + ex.Message);
                    throw;
                }
            }
            throw new Exception("Failed to retrieve data from Google Sheet after the maximum number of retry attempts." + error.ToString());
        }

        private bool IsRetryableError(Exception ex)
        {
            return ex is Google.GoogleApiException || ex is HttpRequestException;
        }

        private async Task HandleRetryableError(Exception ex, int attempt, int maxBackoffTime)
        {
            int randomMilliseconds = new Random().Next(1000);
            int backoffTime = Math.Min((int)Math.Pow(2, attempt) * 1000 + randomMilliseconds, maxBackoffTime);
            Console.WriteLine("Error (attempt " + attempt + "): " + ex.Message);
            Console.WriteLine("Retry in " + backoffTime + " ms.");
            await Task.Delay(backoffTime);
        }
    }
}
