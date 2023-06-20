//using HashtagHelp.Domain.Models;
//using HashtagHelp.Services.Implementations.InstData2;
//using HashtagHelp.Services.Interfaces;

//namespace HashtagHelp.Services.Implementations.InstaParser
//{
//    public class InstaParserFollowersTaskService : IFollowersTaskService
//    {
//        public async Task AddFollowersTaskAsync(ParserTaskEntity parserTask)
//        {
//            var apiKey = "eMjDt55n11RuhCa7";
//            var userNames = parserTask.ResearchedUsers.Select(researchedUser => researchedUser.NickName).ToList();
//            InstaParserAPIRequestService ApiRequestService = new();
//            parserTask.InParserId = await ApiRequestService.AddTaskAPIAsync(apiKey, userNames);
//        }
//    }
//}
