using HashtagHelp.Domain.ExternalApiModels.BulkSkrapper;
using Newtonsoft.Json;
using RestSharp;

namespace HashtagHelp.Services.Implementations.BulkScrapper
{
    public class BulkScrapperAPIRequestService<T>
    {
        public async Task<List<T>> GetObjectsBulkAPIAsync(string apiKey, string nickName)
        {
            int nextMaxId = 0;
            var client = new RestClient("https://instagram-bulk-profile-scrapper.p.rapidapi.com/clients/api/ig/followers");
            var request = new RestRequest(Method.Get.ToString());
            request.AddParameter("username", nickName);
            request.AddParameter("nextMaxId", nextMaxId);
            request.AddParameter("corsEnabled", "false");
            request.AddHeader("X-RapidAPI-Key", apiKey);
            request.AddHeader("X-RapidAPI-Host", "instagram-bulk-profile-scrapper.p.rapidapi.com");

            RestResponse response = await client.ExecuteAsync(request);

            if (response.IsSuccessful && response.Content != null)
            {
                // Обработка успешного ответа и преобразование в список
                var dataResponce = ProcessApiResponse(response.Content);
                var objects = dataResponce.Data;
                    while (dataResponce.Cursor.MoreAvailable)
                    {
                        nextMaxId = int.Parse(dataResponce.Cursor.NextMaxId);
                        request.AddParameter("nextMaxId", nextMaxId.ToString());
                        response = await client.ExecuteAsync(request);
                        if (!response.IsSuccessful)
                            throw new Exception("Error: " + response.ErrorMessage);
                        dataResponce = ProcessApiResponse(response.Content);
                        objects.AddRange(dataResponce.Data);
                    }
                    return objects;
            }
            else
            {
                // Обработка ошибки при выполнении запроса
                throw new Exception("Error: " + response.ErrorMessage);
            }

            DataResponce<T> ProcessApiResponse(string responseContent)
            {
                // Разбор JSON-ответа и преобразование в список Follower
                return JsonConvert.DeserializeObject<DataResponce<T>>(responseContent);
            }
        }


    }
}
