using HashtagHelp.Domain.ExternalApiModels.RocketAPI;
using HashtagHelp.Services.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace HashtagHelp.Services.Implementations.RocketAPI
{
    public class RocketAPIRequestService : IHashtagApiRequestService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl = "https://rocketapi-for-instagram.p.rapidapi.com";
        private readonly string _rapidApiKeyHeader = "X-RapidAPI-Key";
        private readonly string _rapidApiHostHeader = "X-RapidAPI-Host";
        private readonly string _rapidApiHostHeaderValue = "rocketapi-for-instagram.p.rapidapi.com";
        private readonly int _maxAttempts = 5; // Максимальное количество повторных попыток
        private readonly int _maxBackoffTime = 64000; // Максимальное время отсрочки (64 секунды)

        public RocketAPIRequestService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add(_rapidApiHostHeader, _rapidApiHostHeaderValue);
        }

        public async Task<string> GetIdAPIAsync(string nickName)
        {
            string json = $"{{ \"username\": \"{nickName}\" }}";
            string apiUrl = $"{_apiUrl}/instagram/user/get_info";
            return await GetRocketApiData(apiUrl, json);
        }

        public async Task<BodyData> GetHashtagInfoAsync(string apiKey, string hashtag)
        {
            _httpClient.DefaultRequestHeaders.Add(_rapidApiKeyHeader, apiKey);
            string jsonRequestData = $"{{ \"name\": \"{hashtag}\" }}";
            string apiUrl = $"{_apiUrl}/instagram/hashtag/get_info";
            var response = await GetRocketApiData(apiUrl, jsonRequestData);
            ServerResponse serverResponse = JsonConvert.DeserializeObject<ServerResponse>(response);
            var hashtagInfo = serverResponse.response.Body.data;
            return hashtagInfo;
        }

        public async Task<List<User>> GetObjectsAPIAsync(string userId)
        {
            var objects = new List<User>();
            RootObject dataResponse;
            string? maxId = null;

            do
            {
                string jsonSetup = $@"{{
                    ""id"": {userId},
                    ""count"": 100, 
                    ""max_id"": {maxId ?? "null"}
                }}";

                string apiUrl = $"{_apiUrl}/instagram/user/get_followers";
                var response = await GetRocketApiData(apiUrl, jsonSetup);
                dataResponse = JsonConvert.DeserializeObject<RootObject>(response);
                objects.AddRange(dataResponse.Response.Body.Users);
                try
                {
                    var jsonData = JObject.Parse(response);
                    maxId = jsonData["response"]["body"]["next_max_id"].ToString();
                }
                catch
                {
                    break;
                }
                await Task.Delay(1000);
            } while (true);

            return objects;
        }

        private async Task<string> GetRocketApiData(string apiUrl, string json)
        {
            string responseBody = await GetValuesWithRetryAsync(_maxAttempts, async () =>
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(apiUrl),
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
                using var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            });

            return responseBody;
        }

        private async Task<T> GetValuesWithRetryAsync<T>(int maxAttempts, Func<Task<T>> action)
        {
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex) when (IsRetryableError(ex) && attempt < maxAttempts)
                {
                    // Обработка ошибки с повторной попыткой
                    await HandleRetryableError(ex, attempt, _maxBackoffTime);
                }
                catch (Exception ex) when (!IsRetryableError(ex))
                {
                    // Обработка неповторяемых ошибок
                    Console.WriteLine($"Неповторяемая ошибка (попытка {attempt}): {ex.Message}");
                    throw;
                }
            }

            // Если достигли этой точки, значит не удалось получить данные после максимального числа повторных попыток
            throw new Exception("Не удалось выполнить действие после максимального числа повторных попыток.");
        }

        private bool IsRetryableError(Exception ex)
        {
            // Проверяем, является ли ошибка повторяемой
            // Возможно, здесь нужно добавить дополнительные условия для определения повторяемой ошибки
            return ex is HttpRequestException;
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


/* using HashtagHelp.Domain.ExternalApiModels.RocketAPI;
using HashtagHelp.Services.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace HashtagHelp.Services.Implementations.RocketAPI
{
    public class RocketAPIRequestService : IHashtagApiRequestService
    {
        public async Task<string> GetIdAPIAsync(string apiKey, string nickName)
        {
            string json = $"{{ \"username\": \"{nickName}\" }}";
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://rocketapi-for-instagram.p.rapidapi.com/instagram/user/get_info"),
                Headers =
                {
                    { "X-RapidAPI-Key", apiKey },
                    { "X-RapidAPI-Host", "rocketapi-for-instagram.p.rapidapi.com" },
                },
                Content = new StringContent(json)
                {
                    Headers =
                    {
                    ContentType = new MediaTypeHeaderValue("application/json")
                    }
                }
            };

            HttpResponseMessage response;
            string body;

            using (response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                body = await response.Content.ReadAsStringAsync();
                //Console.WriteLine(body);
            }

            if (response.IsSuccessStatusCode)
            {
                var userId = ProcessApiIdResponse(body);
                return userId;
            }
            else
            {
                // Обработка ошибки при выполнении запроса
                throw new Exception("Error: " + response.ReasonPhrase);
            }
        }

        public async Task<BodyData> GetHashtagInfoAsync(string apiKey, string hashtag)
        {
            string json = $"{{ \"name\": \"{hashtag}\" }}";
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://rocketapi-for-instagram.p.rapidapi.com/instagram/hashtag/get_info"),
                Headers =
                {
                    { "X-RapidAPI-Key", apiKey },
                    { "X-RapidAPI-Host", "rocketapi-for-instagram.p.rapidapi.com" },
                },
                Content = new StringContent(json)
                {
                    Headers =
                    {
                    ContentType = new MediaTypeHeaderValue("application/json")
                    }
                }
            };

            HttpResponseMessage response;
            string body;

            using (response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                body = await response.Content.ReadAsStringAsync();
            }

            if (response.IsSuccessStatusCode)
            {
                ServerResponse serverResponse = JsonConvert.DeserializeObject<ServerResponse>(body);
                var hashtagInfo = serverResponse.response.Body.data;
                return hashtagInfo;
            }
            else
            {
                throw new Exception("Error: " + response.ReasonPhrase);
            }
        }

        public async Task<List<User>> GetObjectsAPIAsync(string apiKey, string userId)
        {
            const string apiUrl = "https://rocketapi-for-instagram.p.rapidapi.com";
            const string followersEndpoint = "/instagram/user/get_followers";
            const string rapidApiKeyHeader = "X-RapidAPI-Key";
            const string rapidApiHostHeader = "X-RapidAPI-Host";

            var objects = new List<User>();
            RootObject dataResponse = new();
            string? maxId = null;

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add(rapidApiKeyHeader, apiKey);
            client.DefaultRequestHeaders.Add(rapidApiHostHeader, "rocketapi-for-instagram.p.rapidapi.com");

            do
            {
                string jsonSetup = $@"{{
            ""id"": {userId},
            ""count"": 100, 
            ""max_id"": {maxId ?? "null"}
        }}";

                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(apiUrl + followersEndpoint),
                    Content = new StringContent(jsonSetup, Encoding.UTF8, "application/json")
                };
                 
                using var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                string text = await response.Content.ReadAsStringAsync();
                JObject jsonData = new JObject();
                jsonData = JObject.Parse(text);

                dataResponse = ProcessApiFollowerResponse(text);
                objects.AddRange(dataResponse.Response.Body.Users);
                try
                {
                    maxId = jsonData["response"]["body"]["next_max_id"].ToString();
                }
                catch 
                {
                    break;
                }
                await Task.Delay(1000);
            } while(true);

            return objects;
        }

        string ProcessApiIdResponse(string responseContent)
        {
            string jsonString = responseContent;
            JObject jsonResponse = new JObject();
            jsonResponse = JObject.Parse(jsonString);
            var userId = jsonResponse["response"]["body"]["data"]["user"]["id"].ToString();
            return userId;
        }

        RootObject ProcessApiFollowerResponse(string responseContent)
        {
            string json = responseContent;
            RootObject root = JsonConvert.DeserializeObject<RootObject>(json);
            return root;
        }
    }
}

 */