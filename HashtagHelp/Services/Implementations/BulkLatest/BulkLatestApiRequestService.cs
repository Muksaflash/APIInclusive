/* using HashtagHelp.Domain.ExternalApiModels.BulkLatest;
using HashtagHelp.Services.Interfaces;
using Newtonsoft.Json;

namespace HashtagHelp.Services.Implementations.RocketAPI
{
    public class BulkLatestApiRequestService: IHashtagApiRequestService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl = "https://instagram-bulk-scraper-latest.p.rapidapi.com";
        private readonly string _rapidApiKeyHeader = "X-RapidAPI-Key";
        private readonly string _rapidApiHostHeader = "X-RapidAPI-Host";
        private readonly string _rapidApiHostHeaderValue = "instagram-bulk-scraper-latest.p.rapidapi.com";
        private readonly int _maxAttempts = 5;
        private readonly int _maxBackoffTime = 64000;
        private readonly string _hashtagInfoEndpoint = "search_hashtag/";

        public BulkLatestApiRequestService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.DefaultRequestHeaders.Add(_rapidApiHostHeader, _rapidApiHostHeaderValue);
        }

        public async Task<BodyData> GetHashtagInfoAsync(string apiKey, string hashtag)
        {
            BodyData hashtagInfo = await GetValuesWithRetryAsync(_maxAttempts, async () =>
            {
                if (!_httpClient.DefaultRequestHeaders.Contains(_rapidApiKeyHeader))
                {
                    _httpClient.DefaultRequestHeaders.Add(_rapidApiKeyHeader, apiKey);
                }
                string apiUrl = _apiUrl + _hashtagInfoEndpoint + hashtag;
                var response = await GetApiDataAsync(apiUrl);
                ServerResponse serverResponse;
                BodyData hashtagInfo;
                try
                {
                    serverResponse = JsonConvert.DeserializeObject<ServerResponse>(response);
                    hashtagInfo = serverResponse.response.Body.data;
                }
                catch (Exception ex)
                {
                    if (ex is JsonSerializationException)
                    {
                        hashtagInfo = new() { media_count = "0" };
                    }
                    else throw;
                }
                hashtagInfo.media_count ??= "0";
                return hashtagInfo;
            });
            return hashtagInfo;
        }

        private async Task<string> GetApiDataAsync(string apiUrl)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(apiUrl)
            };
            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        private async Task<T> GetValuesWithRetryAsync<T>(int maxAttempts, Func<Task<T>> action)
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
            throw new Exception("Failed to perform the action after the maximum number of retry attempts." + error.ToString());
        }

        private static bool IsRetryableError(Exception ex)
        {
            return ex is HttpRequestException || ex is NullReferenceException || ex is TaskCanceledException;
        }

        private static async Task HandleRetryableError(Exception ex, int attempt, int maxBackoffTime)
        {
            int randomMilliseconds = new Random().Next(1000);
            int backoffTime = Math.Min((int)Math.Pow(2, attempt) * 1000 + randomMilliseconds, maxBackoffTime);
            Console.WriteLine("Error (attempt " + attempt + "): " + ex.Message);
            Console.WriteLine("Retry in " + backoffTime + " ms.");
            await Task.Delay(backoffTime);
        }
    }
}  */