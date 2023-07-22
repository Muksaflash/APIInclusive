/* using HashtagHelp.Domain.Models;
using HashtagHelp.Domain.Enums;
using HashtagHelp.Services.Interfaces;
using HashtagHelp.Domain.RequestModels;

namespace HashtagHelp.Services.Implementations
{
    public class FunnelServiceStab : IFunnelService
    {
        public IApiRequestService InstaParserApiRequestService { get; set; }
        public IHashtagApiRequestService HashtagApiRequestService { get; set; }
        public IProcessLogger ProcessLogger { get; set; }
        public IParserDataService ParserDataService { get; set; }
        public IDataRepository DataRepository { get; set; }
        public IGoogleApiRequestService GoogleApiRequestService { get; set; }
        private Timer followersTimer;
        private Timer followingTagsTimer;
        private string configGoogleSpreadsheetID = "1BXVyKV6ScRArAx3qdYxqOiHPJsO5ByfBrSs58I8vojA";
        private string configGoogleSheet = "config1";
        private string instaParserKey;
        private string hashtagApiKey;
        private GeneralTaskEntity _generalTask;
        private double checkTimerMinutes = 0.2;
        private long minTagMediaCount = 1000;
        private long maxTagMediaCount = 500000;
        private long minMediaCountInterval = 3000;
        private long hashtagsNumber = 30;
        private TaskCompletionSource<bool> _funnelCompletionSource = new TaskCompletionSource<bool>();

        public async Task SetConfigure(FunnelRequestModel requestModel)
        {
            var hashtagAreas = await GoogleApiRequestService.GetAreasListAsync(configGoogleSheet, configGoogleSpreadsheetID);
            var data = await GoogleApiRequestService.GetAllSheetData(configGoogleSheet, configGoogleSpreadsheetID);
            if (!hashtagAreas.Contains(requestModel.HashtagArea))
            {
                throw new Exception("Неправильно указана ниша");
            }
            instaParserKey = await GoogleApiRequestService.GetParameterAsync(configGoogleSheet, configGoogleSpreadsheetID, "B2");
        }

        public async Task AddFollowersTaskAsync(GeneralTaskEntity generalTask)
        {
            ProcessLogger.Log("App was started");
            _generalTask = generalTask;
            var userNames = _generalTask.CollectionTask.ResearchedUsers
                .Select(researchedUser => researchedUser.NickName).ToList();
            _generalTask.CollectionTask.InParserId = "2405075";
            DataRepository.UpdateParserTask(_generalTask.CollectionTask);
            await DataRepository.SaveChangesAsync();
            StartCheckingTimer(_generalTask.CollectionTask, ref followersTimer, CheckFollowersTaskStatusAsync);
            await Task.CompletedTask;
        }

        public async Task AddFollowingTagsTaskAsync()
        {
            var userNames = _generalTask.CollectionTask.ResearchedUsers
                .Select(researchedUser => researchedUser.NickName).ToList();
            var taskId = _generalTask.CollectionTask.InParserId;
            _generalTask.FiltrationTask.InParserId = "2406109";
            DataRepository.UpdateParserTask(_generalTask.FiltrationTask);
            await DataRepository.SaveChangesAsync();
            StartCheckingTimer(_generalTask.FiltrationTask, ref followingTagsTimer, CheckFollowingTagsTaskStatusAsync);
            await Task.CompletedTask;
        }

        private async Task<Domain.ExternalApiModels.RocketAPI.BodyData> GetHashtagInfoAsync(string requiredHashtagText)
        {
            Domain.ExternalApiModels.RocketAPI.BodyData hashtagInfo = new();
            var exists = DataRepository.DoesFieldExist("Hashtags", requiredHashtagText);
            HashtagEntity hashtag;
            if (exists)
            {
                hashtag = await DataRepository.GetEntityByFieldValueAsync<HashtagEntity>(
                    "Hashtags", "Name", requiredHashtagText);
                hashtagInfo.id = hashtag.InstagramId;
                hashtagInfo.media_count = hashtag.MediaCount;
                return hashtagInfo;
            }
            else
            {
                hashtagInfo = await HashtagApiRequestService.GetHashtagInfoAsync(hashtagApiKey, requiredHashtagText);
                return hashtagInfo;
            }
        }

        public async Task FunnelCreateAsync()
        {
            try
            {
                var tagsTaskContent = File.ReadAllText("/Users/aleksandrsytov/Desktop/Subscribers_Tags_filtration_of-__marrvita.txt");
                var tagFreq = ParserDataService.RedoFiles(tagsTaskContent);
                ValidateTagFreq(tagFreq);
                ParserDataService.RareFreqTagsRemove(tagFreq);
                /* var hashtags = await ProcessHashtagsAsync(tagFreq);
                await SaveHashtagsAsync(hashtags);
                var areaHashtags = await GoogleApiRequestService.GetDataAsync(_generalTask.HashtagArea);
                var areaHashtagsEntities = await ProcessHashtagsAsync(areaHashtags.ToDictionary(x => x, x => 50));
                hashtags.AddRange(areaHashtagsEntities);
                await SaveHashtagsAsync(areaHashtagsEntities); 
                var hashtags = CreateHashtags();
                var funnel = new FunnelEntity(minTagMediaCount, maxTagMediaCount, minMediaCountInterval, hashtagsNumber);
                var funelLines = ParserDataService.CreateFunnels(funnel, hashtags);
                funnel.FunnelText = string.Join("", funelLines);
                _generalTask.HashtagFunnel = funnel;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                ProcessLogger.Log(ex.Message);
                _funnelCompletionSource.SetException(ex);
            }
        }
        public async Task WaitCompletionGeneralTask()
        {
            await _funnelCompletionSource.Task;
        }

        private void ValidateTagFreq(Dictionary<string, int> tagFreq)
        {
            if (tagFreq.Count < 100)
            {
                _generalTask.FiltrationTask.Status = StatusTaskEnum.Error;
                throw new Exception("Too few hashtags have followers of researched user(s)");
            }
        }

        private async Task<List<HashtagEntity>> ProcessHashtagsAsync(Dictionary<string, int> tagFreq)
        {
            var hashtags = new List<HashtagEntity>();

            foreach (var tag in tagFreq)
            {
                var hashtagInfo = await GetHashtagInfoAsync(tag.Key);
                var hashtagEntity = new HashtagEntity
                {
                    Name = tag.Key,
                    MediaCount = hashtagInfo.media_count,
                    InstagramId = hashtagInfo.id
                };

                hashtags.Add(hashtagEntity);
            }
            return hashtags;
        }

        private async Task SaveHashtagsAsync(List<HashtagEntity> hashtags)
        {
            foreach (var hashtag in hashtags)
            {
                if (!DataRepository.DoesFieldExist("Hashtags", hashtag.Name))
                {
                    DataRepository.AddHashtag(hashtag);
                }
            }
            await DataRepository.SaveChangesAsync();
        }

        private void StartCheckingTimer(ParserTaskEntity parserTask, ref Timer timer, Func<ParserTaskEntity, Task> timerAction)
        {
            var interval = TimeSpan.FromMinutes(checkTimerMinutes);
            timer = new Timer(async state =>
            {
                await timerAction(parserTask);
            }, null, interval, interval);
        }

        private async Task CheckFollowersTaskStatusAsync(ParserTaskEntity parserTask)
        {
            try
            {
                var taskStatus = new Domain.ExternalApiModels.InstaParser.TaskStatusResponse
                {
                    tid_status = "completed"
                };
                Console.WriteLine(taskStatus.tid_status);
                if (taskStatus.tid_status == "completed")
                {
                    await followersTimer.DisposeAsync();
                    Console.WriteLine("Приступаем к парсингу подписок подпищиков");
                    await AddFollowingTagsTaskAsync();
                    _generalTask.CollectionTask.Status = StatusTaskEnum.Completed;
                    DataRepository.UpdateParserTask(_generalTask.CollectionTask);
                    await DataRepository.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                ProcessLogger.Log(ex.Message);
            }
        }

        private async Task CheckFollowingTagsTaskStatusAsync(ParserTaskEntity parserTask)
        {
            try
            {
                var taskStatus = new Domain.ExternalApiModels.InstaParser.TaskStatusResponse
                {
                    tid_status = "completed"
                };
                Console.WriteLine(taskStatus.tid_status);
                if (taskStatus.tid_status == "completed")
                {
                    await followingTagsTimer.DisposeAsync();
                    Console.WriteLine("Приступаем к созданию воронки");
                    await FunnelCreateAsync();
                    _generalTask.FiltrationTask.Status = StatusTaskEnum.Completed;
                    DataRepository.UpdateParserTask(_generalTask.FiltrationTask);
                    await DataRepository.SaveChangesAsync();
                    _funnelCompletionSource.SetResult(true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                ProcessLogger.Log(ex.Message);

            }
        }
        private List<HashtagEntity> CreateHashtags()
        {
            var hashtags = new List<HashtagEntity>();
            var random = new Random();

            for (int i = 1; i <= 200; i++)
            {
                var hashtag = new HashtagEntity
                {
                    Name = $"hashtag{i}", // Пример формата имени хэштега
                    InstagramId = $"instagramId{i}", // Пример формата идентификатора Instagram
                    MediaCount = random.Next(1000, 500001).ToString() // Случайное число от 1000 до 500000
                };

                hashtags.Add(hashtag);
            }

            return hashtags;
        }
    }
}
 */