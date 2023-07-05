using HashtagHelp.Domain.ExternalApiModels.RocketAPI;
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

