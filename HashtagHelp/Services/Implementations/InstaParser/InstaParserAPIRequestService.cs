using HashtagHelp.Domain.ExternalApiModels.InstaParser;
using HashtagHelp.Services.Interfaces;

namespace HashtagHelp.Services.Implementations.InstaParser
{
    public class InstaParserAPIRequestService : IApiRequestService
    {
        private readonly HttpClient _httpClient;
        private readonly int maxAttempts = 5;
        private readonly int maxBackoffTime = 64000;

        public InstaParserAPIRequestService(IHttpClientFactory httpFactory)
        {
            _httpClient = httpFactory.CreateClient();
        }

        private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> action)
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
                    await HandleRetryableError(ex, attempt, maxBackoffTime);
                }
                catch (Exception ex) when (!IsRetryableError(ex))
                {
                    Console.WriteLine("Non-repeating error (attempt " + attempt + "): " + ex.Message);
                    throw;
                }
            }
            throw new Exception("Failed to perform the action after the maximum number of retry attempts." + error.ToString());
        }

        private bool IsRetryableError(Exception ex)
        {
            return ex is HttpRequestException;
        }

        private async Task HandleRetryableError(Exception ex, int attempt, int maxBackoffTime)
        {
            int randomMilliseconds = new Random().Next(1000);
            int backoffTime = Math.Min((int)Math.Pow(2, attempt) * 1000 + randomMilliseconds, maxBackoffTime);
            Console.WriteLine("Error (attempt " + attempt + "): " + ex.Message);
            Console.WriteLine("Retry in " + backoffTime + " ms.");
            await Task.Delay(backoffTime);
        }

        public async Task<string> GetTagsTaskContentAPIAsync(string apiKey, string FollowingTagsTaskId, string url)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                string apiUrl = url + "api.php?key=" + apiKey + "&mode=result&tid=" + FollowingTagsTaskId;
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                (bool hasError, string errorMessage) = GetErrorInfo(responseBody);
                if (hasError)
                {
                    throw new Exception(errorMessage);
                }
                return responseBody;
            });
        }

        public async Task<string> AddFiltrationTaskApiAsync(string apiKey, string FollowersTaskId, List<string> researchedUsers, string url)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                var namesString = string.Join(",", researchedUsers);
                var taskName = "Subscribers Tags filtration of: " + " " + namesString;
                string apiUrl = url + "api.php?key=" + apiKey + "&mode=create&type=f1&name=" + taskName + "&links=" + FollowersTaskId + "&dop=7";
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                (bool hasError, string errorMessage) = GetErrorInfo(responseBody);
                if (hasError)
                {
                    throw new Exception(errorMessage);
                }
                var taskId = GetTaskId(responseBody);
                return taskId;
            });
        }

        public async Task<string> AddCollectionTaskApiAsync(string apiKey, List<string> userNames, string url)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                var namesString = string.Join(",", userNames);
                var taskName = "Subscribers collection of: " + namesString;
                string apiUrl = url + "api.php?key=" + apiKey + "&mode=create&type=p1&name=" + taskName + "&links=" + namesString;
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                (bool hasError, string errorMessage) = GetErrorInfo(responseBody);
                if (hasError)
                {
                    throw new Exception(errorMessage);
                }
                var taskId = GetTaskId(responseBody);
                return taskId;
            });
        }

        public async Task<TaskStatusResponse> GetTaskStatusAsync(string apiKey, string taskId, string url)
        {
            return await ExecuteWithRetryAsync(async () =>
            {
                string apiUrl = url + "api.php?key=" + apiKey + "&mode=status&tid=" + taskId;
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                (bool hasError, string errorMessage) = GetErrorInfo(responseBody);
                if (hasError)
                {
                    throw new Exception(errorMessage);
                }
                TaskStatusResponse? taskStatus = await response.Content.ReadFromJsonAsync<TaskStatusResponse>();
                return taskStatus ?? throw new Exception("taskStatus is null"); ;
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

        private (bool HasError, string ErrorMessage) GetErrorInfo(string response)
        {
            int startIndex = response.IndexOf("\"status\":\"error\",\"text\":\"");
            if (startIndex != -1)
            {
                startIndex += "\"status\":\"error\",\"text\":\"".Length;
                int endIndex = response.IndexOf("\"", startIndex);
                if (endIndex != -1)
                {
                    string errorMessage = response.Substring(startIndex, endIndex - startIndex);
                    return (true, errorMessage);
                }
            }

            return (false, string.Empty);
        }
    }
}