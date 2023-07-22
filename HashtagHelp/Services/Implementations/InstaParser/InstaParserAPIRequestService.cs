using System.Net.Http;
using System.Threading.Tasks;
using HashtagHelp.Domain.ExternalApiModels.InstaParser;
using HashtagHelp.Services.Interfaces;

namespace HashtagHelp.Services.Implementations.InstaParser
{
    public class InstaParserAPIRequestService : IApiRequestService
    {
        private readonly HttpClient _httpClient;
        private readonly int maxAttempts = 5; // Максимальное количество повторных попыток
        private readonly int maxBackoffTime = 64000; // Максимальное время отсрочки (64 секунды)

        public InstaParserAPIRequestService()
        {
            _httpClient = new HttpClient();
        }

        private async Task<T> ExecuteWithRetry<T>(Func<Task<T>> action)
        {
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    return await action();
                }
                catch (Exception ex) when (IsRetryableError(ex) && attempt < maxAttempts)
                {
                    await HandleRetryableError(ex, attempt, maxBackoffTime);
                }
                catch (Exception ex) when (!IsRetryableError(ex))
                {
                    Console.WriteLine($"Неповторяемая ошибка (попытка {attempt}): {ex.Message}");
                    throw;
                }
            }
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
        
        public async Task<string> GetTagsTaskContentAPIAsync(string apiKey, string FollowingTagsTaskId, string url)
        {
            return await ExecuteWithRetry(async () =>
            {
                string apiUrl = $"{url}api.php?key={apiKey}&mode=result&tid={FollowingTagsTaskId}";
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                return responseBody;
            });
        }

        // Пример для метода AddFollowingTagsTaskAPIAsync
        public async Task<string> AddFollowingTagsTaskAPIAsync(string apiKey, string FollowersTaskId, List<string> researchedUsers, string url)
        {
            return await ExecuteWithRetry(async () =>
            {
                var namesString = string.Join(",", researchedUsers);
                var taskName = "Subscribers Tags filtration of: " + " " + namesString;
                string apiUrl = $"{url}api.php?key={apiKey}&mode=create&type=f1&name={taskName}&links={FollowersTaskId}&dop=7";
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                var taskId = GetTaskId(responseBody);
                return taskId;
            });
        }

        public async Task<string> AddFollowersTaskAPIAsync(string apiKey, List<string> userNames, string url)
        {
            return await ExecuteWithRetry(async () =>
            {
                var namesString = string.Join(",", userNames);
                var taskName = "Subscribers collection of: " + " " + namesString;
                string apiUrl = $"{url}api.php?key={apiKey}&mode=create&type=p1&name={taskName}&links={namesString}";
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                var taskId = GetTaskId(responseBody);
                return taskId;
            });
        }

        public async Task<TaskStatusResponse> GetTaskStatusAsync(string apiKey, string taskId, string url)
        {
            return await ExecuteWithRetry(async () =>
            {
                string apiUrl = $"{url}api.php?key={apiKey}&mode=status&tid={taskId}";
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();
                TaskStatusResponse? taskStatus = await response.Content.ReadFromJsonAsync<TaskStatusResponse>();
                return taskStatus; 
            });
        }

        private string GetTaskId(string response) 
        {
            string[] parts = response.Split(',');
            string taskId = string.Empty;
            foreach (string part in parts)
            {
                if (part.StartsWith("\"tid\":\""))
                {
                    taskId = part[7..];
                    break;
                }
            }                
            taskId = new string(taskId.Where(char.IsDigit).ToArray());
            return taskId;
        }
    }
}

/* using HashtagHelp.Domain.ExternalApiModels.InstaParser;
using HashtagHelp.Services.Interfaces;

namespace HashtagHelp.Services.Implementations.InstaParser
{
    public class InstaParserAPIRequestService : IApiRequestService
    {
        private readonly HttpClient _httpClient;

        public InstaParserAPIRequestService()
        {
            _httpClient = new HttpClient();
        }
        public async Task<string> GetTagsTaskContentAPIAsync(string apiKey, string FollowingTagsTaskId, string url)
        {
            string apiUrl = $"{url}api.php?key={apiKey}&mode=result&tid={FollowingTagsTaskId}";
            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }
        public async Task<string> AddFollowingTagsTaskAPIAsync(string apiKey, string FollowersTaskId, List<string> researchedUsers, string url)
        {
            var namesString = string.Join(",", researchedUsers);
            var taskName = "Subscribers Tags filtration of: " + " " + namesString;
            string apiUrl = $"{url}api.php?key={apiKey}&mode=create&type=f1&name={taskName}&links={FollowersTaskId}&dop=7";
            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            var taskId = GetTaskId(responseBody);
            return taskId;
        }

        public async Task<string> AddFollowersTaskAPIAsync(string apiKey, List<string> userNames, string url)
        {
            var namesString = string.Join(",", userNames);
            var taskName = "Subscribers collection of: " + " " + namesString;
            string apiUrl = $"{url}api.php?key={apiKey}&mode=create&type=p1&name={taskName}&links={namesString}";
            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();
            var taskId = GetTaskId(responseBody);
            return taskId;
        }

        public async Task<TaskStatusResponse> GetTaskStatusAsync(string apiKey, string taskId, string url)
        {
            string apiUrl = $"{url}api.php?key={apiKey}&mode=status&tid={taskId}";
            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
            response.EnsureSuccessStatusCode();
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
                    taskId = part[7..];
                    break;
                }
            }                
            taskId = new string(taskId.Where(char.IsDigit).ToArray());
            return taskId;
        }
    }
}
 */