using HashtagHelp.Domain.ResponseModels.InstagramData;
using Newtonsoft.Json;
using RestSharp;
using System.Net.Http.Headers;

namespace HashtagHelp.Services.Implementations.InstagramData
{
    public class InstagramDataAPIRequestService<T>
    {
        public async Task<List<T>> GetObjectsBulkAPIAsync(string apiKey, string nickName)
        {
            //var end_cursor = 0;

            //var client = new RestClient($"https://instagram-data1.p.rapidapi.com/followers?username={nickName}");
            //var request = new RestRequest(Method.Get.ToString());
            ////request.AddParameter("username", nickName);
            //request.AddHeader("X-RapidAPI-Key", apiKey);
            //request.AddHeader("X-RapidAPI-Host", "instagram-data1.p.rapidapi.com");

            //RestResponse response = await client.ExecuteAsync(request);

            //if (response.IsSuccessful)
            //{
            //    // Обработка успешного ответа и преобразование в список
            //    var dataResponce = ProcessApiResponse(response.Content);
            //    var objects = dataResponce.Collector;
            //    while (dataResponce.Has_More)
            //    {
            //        end_cursor = int.Parse(dataResponce.End_Cursor);
            //        request.AddParameter("end_cursor", end_cursor.ToString());
            //        response = await client.ExecuteAsync(request);
            //        if (!response.IsSuccessful)
            //            throw new Exception("Error: " + response.ErrorMessage);
            //        dataResponce = ProcessApiResponse(response.Content);
            //        objects.AddRange(dataResponce.Collector);
            //    }
            //    return objects;
            //}
            //else
            //{
            //    // Обработка ошибки при выполнении запроса
            //    throw new Exception("Error: " + response.ErrorMessage);
            //}

            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://instagram-data1.p.rapidapi.com/followers?username={nickName}"),
                Headers =
    {
        { "X-RapidAPI-Key", apiKey },
        { "X-RapidAPI-Host", "instagram-data1.p.rapidapi.com" },
    },
            };

            HttpResponseMessage response;
            string body;

            using (response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                body = await response.Content.ReadAsStringAsync();
                Console.WriteLine(body);
            }

            if (response.IsSuccessStatusCode)
            {
                // Обработка успешного ответа и преобразование в список
                var dataResponce = ProcessApiResponse(body);
                var objects = dataResponce.Collector;

                while (dataResponce.Has_More)
                {
                    var end_cursor = dataResponce.End_Cursor;
                    request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri($"https://instagram-data1.p.rapidapi.com/followers?username={nickName}&end_cursor={end_cursor}"),
                        Headers =
            {
                { "X-RapidAPI-Key", apiKey },
                { "X-RapidAPI-Host", "instagram-data1.p.rapidapi.com" },
            },
                    };

                    using (response = await client.SendAsync(request))
                    {
                        response.EnsureSuccessStatusCode();
                        body = await response.Content.ReadAsStringAsync();
                    }

                    dataResponce = ProcessApiResponse(body);
                    objects.AddRange(dataResponce.Collector);
                }

                return objects;
            }
            else
            {
                // Обработка ошибки при выполнении запроса
                throw new Exception("Error: " + response.ReasonPhrase);
            }

            RootObject<T> ProcessApiResponse(string responseContent)
            {
                // Разбор JSON-ответа 
                return JsonConvert.DeserializeObject<RootObject<T>>(responseContent);
            }

        }
    }
}
