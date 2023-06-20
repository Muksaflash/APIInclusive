using HashtagHelp.Domain.ExternalApiModels.InstaParser;
using HashtagHelp.Services.Interfaces;
using System.Net.Http;
using System.Net.Http.Headers;

namespace HashtagHelp.Services.Implementations.InstaParser
{
    public class InstaParserAPIRequestService : IApiRequestService
    {
        private readonly HttpClient _httpClient;

        public InstaParserAPIRequestService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> AddFollowingTagsTaskAPIAsync(string apiKey, string FollowersTaskId, List<string> researchedUsers)
        {
            var namesString = string.Join(",", researchedUsers);
            var taskName = "Subscribers Tags filtration of: " + " " + namesString;

            string apiUrl = $"https://instaparser.ru/api.php?key={apiKey}&mode=create&type=f1&name={taskName}&links={FollowersTaskId}&dop=7,10";

            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception();
            }
            string responseBody = await response.Content.ReadAsStringAsync();
            var taskId = GetTaskId(responseBody);
            return taskId;
        }

        public async Task<string> AddFollowersTaskAPIAsync(string apiKey, List<string> userNames)
        {
            var namesString = string.Join(",", userNames);
            var taskName = "Subscribers collection of: " + " " + namesString;

            string apiUrl = $"https://instaparser.ru/api.php?key={apiKey}&mode=create&type=p1&name={taskName}&links={namesString}";

            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            var taskId = GetTaskId(responseBody);
            return taskId;
        }

        public async Task<TaskStatusResponse> GetTaskStatusAsync(string apiKey, string taskId)
        {
            string apiUrl = $"https://instaparser.ru/api.php?key={apiKey}&mode=status&tid={taskId}";

            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            TaskStatusResponse? taskStatus = await response.Content.ReadFromJsonAsync<TaskStatusResponse>();
            return taskStatus; 
        }

        private string GetTaskId(string response) 
        {
            string[] parts = response.Split(',');
            string taskId = string.Empty;
            foreach (string part in parts)
            {
                if (part.StartsWith("\"tid\":\""))
                {
                    // Извлекаем идентификаторы заданий из части, начинающейся с "tid"
                    taskId = part[7..];
                    break;
                }
            }                
            taskId = new string(taskId.Where(char.IsDigit).ToArray());
            return taskId;
        }

        //private string GetTaskStatusAsync(string response)
        //{
        //    string[] parts = response.Split(',');
        //    string taskStatus = string.Empty;
        //    foreach (string part in parts)
        //    {
        //        if (part.StartsWith("tid_status:"))
        //        {
        //            // Извлекаем идентификаторы заданий из части, начинающейся с "tid"
        //            taskStatus = part[11..];
        //            break;
        //        }
        //    }
        //    return taskStatus;
        //}
    }
}
