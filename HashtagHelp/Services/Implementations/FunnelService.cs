using HashtagHelp.Domain.Models;
using HashtagHelp.Domain.Enums;
using HashtagHelp.Services.Interfaces;
namespace HashtagHelp.Services.Implementations
{
    public class FunnelService : IFunnelService
    {
        public IApiRequestService ApiRequestService { get; set; }
        public IHashtagApiRequestService HashtagApiRequestService { get; set; }
        public IProcessLogger ProcessLogger { get; set; }
        public IParserDataService ParserDataService { get; set; }
        public IDataRepository DataRepository { get; set; }
        public IGoogleApiRequestService GoogleApiRequestService { get; set; }
        private Timer followersTimer;
        private Timer followingTagsTimer;
        private string apiKey = "eMjDt55n11RuhCa7";
        private string hashtagApiKey = "a8f3f7e68amsh2703987539fa87cp17165ajsn6d5c6feed1e9";
        private GeneralTaskEntity _generalTask;
        private double minutes  = 1;
        private long minTagMediaCount  = 1000;
        private long maxTagMediaCount  = 500000;
        private long minMediaCountInterval = 5000;
        private long hashtagsNumber = 30;

        public async Task AddFollowersTaskAsync(GeneralTaskEntity generalTask)
        {
            ProcessLogger.Log("App was started");
            _generalTask = generalTask;
            var userNames = _generalTask.CollectionTask.ResearchedUsers
                .Select(researchedUser => researchedUser.NickName).ToList();
            _generalTask.CollectionTask.InParserId = await ApiRequestService
                .AddFollowersTaskAPIAsync(apiKey, userNames);
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
            _generalTask.FiltrationTask = new ParserTaskEntity
            {
                InParserId = await ApiRequestService
                .AddFollowingTagsTaskAPIAsync(apiKey, taskId, userNames)
            };
            DataRepository.AddParserTask(_generalTask.FiltrationTask);
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
                var tagsTaskContent = await ApiRequestService.GetTagsTaskContentAPIAsync(apiKey, _generalTask.FiltrationTask.InParserId);
                var tagFreq = ParserDataService.RedoFiles(tagsTaskContent);
                ValidateTagFreq(tagFreq);
                ParserDataService.RareFreqTagsRemove(tagFreq);
                var hashtags = await ProcessHashtagsAsync(tagFreq);
                await SaveHashtagsAsync(hashtags);
                var areaHashtags = await GoogleApiRequestService.GetDataAsync(_generalTask.HashtagArea);
                var areaHashtagsEntities = await ProcessHashtagsAsync(areaHashtags.ToDictionary(x => x, x => 50));
                hashtags.AddRange(areaHashtagsEntities);
                await SaveHashtagsAsync(areaHashtagsEntities);
                var funnel = new FunnelEntity(minTagMediaCount, maxTagMediaCount, minMediaCountInterval, hashtagsNumber);
                funnel.FunnelText =  ParserDataService.CreateFunnels(funnel, hashtags).Item1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                ProcessLogger.Log(ex.Message);
            }
        }

        private void ValidateTagFreq(Dictionary<string, int> tagFreq)
        {
            if (tagFreq.Count < 100)
            {
                _generalTask.FiltrationTask.Status = StatusParserTaskEnum.Error;
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
            var interval = TimeSpan.FromMinutes(minutes);
            timer = new Timer(async state =>
            {
                await timerAction(parserTask);
            }, null, interval, interval);
        }

        private async Task CheckFollowersTaskStatusAsync(ParserTaskEntity parserTask)
        {
            try
            {
                var taskStatus = await ApiRequestService.GetTaskStatusAsync(apiKey, parserTask.InParserId);
                Console.WriteLine(taskStatus.tid_status);
                if (taskStatus.tid_status == "completed")
                {
                    await followersTimer.DisposeAsync();
                    Console.WriteLine("Приступаем к парсингу подписок подпищиков");
                    await AddFollowingTagsTaskAsync();
                    _generalTask.CollectionTask.Status = StatusParserTaskEnum.Completed;
                    DataRepository.AddParserTask(_generalTask.CollectionTask);
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
                var taskStatus = await ApiRequestService.GetTaskStatusAsync(apiKey, parserTask.InParserId);
                Console.WriteLine(taskStatus.tid_status);
                if (taskStatus.tid_status == "completed")
                {
                    await followingTagsTimer.DisposeAsync();
                    Console.WriteLine("Приступаем к созданию воронки");
                    await FunnelCreateAsync();
                    _generalTask.FiltrationTask.Status = StatusParserTaskEnum.Completed;
                    DataRepository.AddParserTask(_generalTask.FiltrationTask);
                    await DataRepository.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                ProcessLogger.Log(ex.Message);

            }
        }
    }
}
