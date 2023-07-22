using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using HashtagHelp.Services.Interfaces;

namespace HashtagHelp.Services.Implementations.InstaParser
{
    public class GoogleRequestApiService : IGoogleApiRequestService
    {
        private readonly string spreadsheetId = "1GB4pkp8M2H2twaliGdhD5pEDlsmEPC9gBT_FUmPq270";
        private readonly int maxAttempts = 5; // Максимальное количество повторных попыток
        private readonly int maxBackoffTime = 64000; // Максимальное время отсрочки (64 секунды)
        private readonly SheetsService sheetsService;
        public string configGoogleSpreadsheetID = "1BXVyKV6ScRArAx3qdYxqOiHPJsO5ByfBrSs58I8vojA";
        public string configGoogleSheet = "config1"; 
        public string HashtagArea {get; set;}

        public GoogleRequestApiService()
        {
            GoogleCredential credential;
            using (var stream = new FileStream("hashtaghelp-13d38e1b1284.json", FileMode.Open, FileAccess.Read))
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

        public async Task<List<string>> GetAllConfigSheetData()
        {
            string range = $"{configGoogleSheet}!A2:Z";
            return await ExecuteWithRetry(maxAttempts, async () =>
            {
                SpreadsheetsResource.ValuesResource.GetRequest request =
                    sheetsService.Spreadsheets.Values.Get(configGoogleSpreadsheetID, range);

                ValueRange response = await request.ExecuteAsync();
                IList<IList<object>> values = response.Values;
                List<string> Data = new();

                if (values != null && values.Count > 0)
                {
                    foreach (var row in values)
                    {
                        foreach (var cell in row)
                        {
                            Data.Add(cell.ToString());
                        }
                    }
                }
                return Data;
            });
        }

        public async Task<List<string>> GetAreaHashtags()
        {
            string sheetName = HashtagArea;
            string range = $"{sheetName}!A1:B";
            return await ExecuteWithRetry(maxAttempts, async () =>
            {
                SpreadsheetsResource.ValuesResource.GetRequest request =
                    sheetsService.Spreadsheets.Values.Get(spreadsheetId, range);

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
                }
                return hashtags;
            });
        }

        public async Task<List<string>> GetAreasListAsync()
        {
            string range = $"{configGoogleSheet}!A2:A";
            return await ExecuteWithRetry(maxAttempts, async () =>
            {
                SpreadsheetsResource.ValuesResource.GetRequest request =
                    sheetsService.Spreadsheets.Values.Get(configGoogleSpreadsheetID, range);

                ValueRange response = await request.ExecuteAsync();
                IList<IList<object>> values = response.Values;
                List<string> areas = new();

                if (values != null && values.Count > 0)
                {
                    foreach (var row in values)
                    {
                        foreach (var cell in row)
                        {
                            areas.Add(cell.ToString());
                        }
                    }
                }
                return areas;
            });
        }

        public async Task<string> GetParameterAsync(string cellAddress)
        {
            string range = $"{configGoogleSheet}!{cellAddress}";
            return await ExecuteWithRetry(maxAttempts, async () =>
            {
                SpreadsheetsResource.ValuesResource.GetRequest request =
                    sheetsService.Spreadsheets.Values.Get(configGoogleSpreadsheetID, range);

                ValueRange response = await request.ExecuteAsync();
                IList<IList<object>> values = response.Values;

                if (values != null && values.Count > 0)
                {
                    var cellValue = values[0][0].ToString();
                    Console.WriteLine($"Значение ячейки {cellAddress}: {cellValue}");
                    return cellValue;
                }

                throw new Exception("empty cell parameter Google Sheet");
            });
        }

        public async Task SetParameterAsync(string cellAddress, string newValue)
        {
            string range = $"{configGoogleSheet}!{cellAddress}";
            await ExecuteWithRetry(maxAttempts, async () =>
            {
                ValueRange updateRequest = new()
                {
                    Values = new List<IList<object>> { new List<object> { newValue } },
                };
                SpreadsheetsResource.ValuesResource.UpdateRequest updateCellRequest =
                    sheetsService.Spreadsheets.Values.Update(updateRequest, configGoogleSpreadsheetID, range);
                updateCellRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
                await updateCellRequest.ExecuteAsync();
                return $"Значение ячейки {cellAddress} обновлено: {newValue}";
            });
        }

        private async Task<T> ExecuteWithRetry<T>(int maxAttempts, Func<Task<T>> action)
        {
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex) when (IsRetryableError(ex) && attempt < maxAttempts)
                {
                    // Обработка ошибки Google Sheets API с повторной попыткой
                    await HandleRetryableError(ex, attempt, maxBackoffTime);
                }
                catch (Exception ex) when (!IsRetryableError(ex))
                {
                    // Обработка неповторяемых ошибок
                    Console.WriteLine($"Неповторяемая ошибка (попытка {attempt}): {ex.Message}");
                    throw;
                }
            }

            // Если достигли этой точки, значит не удалось получить данные после максимального числа повторных попыток
            throw new Exception("Не удалось получить данные из Google Sheet после максимального числа повторных попыток.");
        }

        private bool IsRetryableError(Exception ex)
        {
            // Проверяем, является ли ошибка повторяемой
            // Возможно, здесь нужно добавить дополнительные условия для определения повторяемой ошибки
            return ex is Google.GoogleApiException || ex is HttpRequestException;
        }

        private async Task HandleRetryableError(Exception ex, int attempt, int maxBackoffTime)
        {
            // Обработка повторяемой ошибки с задержкой перед повторной попыткой
            int randomMilliseconds = new Random().Next(1000);
            int backoffTime = Math.Min((int)Math.Pow(2, attempt) * 1000 + randomMilliseconds, maxBackoffTime);

            Console.WriteLine($"Ошибка (попытка {attempt}): {ex.Message}");
            Console.WriteLine($"Повторная попытка через {backoffTime} мс.");
            await Task.Delay(backoffTime);
        }
    }
}
