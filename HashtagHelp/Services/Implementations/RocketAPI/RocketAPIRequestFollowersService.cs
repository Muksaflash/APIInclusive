using HashtagHelp.Domain.ExternalApiModels.BulkSkrapper;
using HashtagHelp.Domain.ExternalApiModels.RocketAPI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NuGet.Packaging;
using RestSharp;
using System.Net.Http.Headers;
using System.Reflection.PortableExecutable;
using System.Text;

namespace HashtagHelp.Services.Implementations.RocketAPI
{
    public class RocketAPIRequestService<T>
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
                // Обработка успешного ответа и преобразование в список
                var UserId = ProcessApiIdResponse(body);
                return UserId;
            }
            else
            {
                // Обработка ошибки при выполнении запроса
                throw new Exception("Error: " + response.ReasonPhrase);
            }
        }

        public async Task<List<T>> GetObjectsAPIAsync(string apiKey, string userId)
        {
            const string apiUrl = "https://rocketapi-for-instagram.p.rapidapi.com";
            const string followersEndpoint = "/instagram/user/get_followers";
            const string rapidApiKeyHeader = "X-RapidAPI-Key";
            const string rapidApiHostHeader = "X-RapidAPI-Host";

            var objects = new List<T>();
            RootObject<T> dataResponse = new();
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

        RootObject<T> ProcessApiFollowerResponse(string responseContent)
        {
            string json = responseContent;
            RootObject<T> root = JsonConvert.DeserializeObject<RootObject<T>>(json);
            return root;
        }
    }
}

