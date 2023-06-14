namespace HashtagHelp.Services.Implementations.InstaParser
{
    public class InstaParserAPIRequestService 
    {
        private readonly HttpClient _httpClient;

        public InstaParserAPIRequestService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> AddTaskAPIAsync(string apiKey, List<string> userNames)
        {
            var namesString = string.Join(",", userNames);
            var taskName =  "Subscribers collection of: " + " " + namesString;

            string apiUrl = $"https://instaparser.ru/api.php?key={apiKey}&mode=create&type=p1&name={taskName}&links={namesString}";

            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();

            // Обработка ответа, если необходимо
            string responseBody = await response.Content.ReadAsStringAsync();
            var taskId = GetTaskId(responseBody);
            return taskId;
        }

        private string GetTaskId(string response) 
        {
            string[] parts = response.Split(',');
            string taskId = string.Empty;
            foreach (string part in parts)
            {
                if (part.StartsWith("tid:"))
                {
                    // Извлекаем идентификаторы заданий из части, начинающейся с "tid"
                    taskId = part.Substring(4);
                    break;
                }
            }
            return taskId;
        }
    }
}
