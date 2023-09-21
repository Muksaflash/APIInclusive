using HashtagHelp.Domain.ExternalApiModels.InstData2;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HashtagHelp.Services.Implementations.InstData2
{
    public class InstData2APIRequestService
    {
        public async Task<List<Node>> GetHashtagAPIAsync(string apiKey, string userId)
        {
            const string apiUrl = "https://instagram-data12.p.rapidapi.com";
            const string hashtagEndpoint = "/user/following-hashtags";
            const string rapidApiKeyHeader = "X-RapidAPI-Key";
            const string rapidApiHostHeader = "X-RapidAPI-Host";

            var objects = new List<Node>();
            RootObject dataResponse = new();

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add(rapidApiKeyHeader, apiKey);
            client.DefaultRequestHeaders.Add(rapidApiHostHeader, "instagram-data12.p.rapidapi.com");

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"{apiUrl}{hashtagEndpoint}?user_id={userId}")
            };
            using var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string body = await response.Content.ReadAsStringAsync();
            JObject jsonData = JObject.Parse(body);

            dataResponse = ProcessApiNodeResponse(body);
            objects.AddRange(dataResponse.User.EdgeFollowingHashtag.Edges);

            return objects;
        }
        RootObject ProcessApiNodeResponse(string responseContent)
        {
            string json = responseContent;
            RootObject root = JsonConvert.DeserializeObject<RootObject>(json);
            return root;
        }
    }
}
