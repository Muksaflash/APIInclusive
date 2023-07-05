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
        private double Minutes = 1;

        public async Task AddFollowersTaskAsync(GeneralTaskEntity generalTask)
        {
            ProcessLogger.Log("App was started");
            _generalTask = generalTask;
            var userNames = _generalTask.CollectionTask.ResearchedUsers
                .Select(researchedUser => researchedUser.NickName).ToList();
            _generalTask.CollectionTask.InParserId = await ApiRequestService
                .AddFollowersTaskAPIAsync(apiKey, userNames);
                DataRepository.UpdateGeneralTask(_generalTask);
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
            Console.WriteLine("Задача", _generalTask.FiltrationTask.InParserId);
            StartCheckingTimer(_generalTask.FiltrationTask, ref followingTagsTimer, CheckFollowingTagsTaskStatusAsync);
            await Task.CompletedTask;
        }

        private async Task<Domain.ExternalApiModels.RocketAPI.BodyData> GetHashtagInfoAsync(string requiredHashtagText)
        {
            Domain.ExternalApiModels.RocketAPI.BodyData hashtagInfo = new();
            var exists = DataRepository.DoesFieldExist("Hashtags", requiredHashtagText);
            HashtagEntity hashtag;
            if(exists)
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
                var tagsTaskContent = await ApiRequestService
                    .GetTagsTaskContentAPIAsync(apiKey, _generalTask.FiltrationTask.InParserId);
                var tagFreq = ParserDataService.RedoFiles(tagsTaskContent);
                if (tagFreq.Count < 100)
                {
                    _generalTask.FiltrationTask.Status = StatusParserTaskEnum.Error;
                    throw new Exception("too small hashtags");
                }
                ParserDataService.RareFreqTagsRemove(tagFreq);
                var hashtags = new List<HashtagEntity>();
                foreach (var hashtag in tagFreq)
                {
                    var hashtagInfo = await GetHashtagInfoAsync(hashtag.Key);
                    var hashtagEntity = new HashtagEntity
                    {
                        Name = hashtag.Key,
                        MediaCount = hashtagInfo.media_count,
                        InstagramId = hashtagInfo.id
                    };
                    hashtags.Add(hashtagEntity);
                    DataRepository.AddHashtag(hashtagEntity);
                    await DataRepository.SaveChangesAsync();
                }
                var areaHashtags = await GoogleApiRequestService.GetDataAsync(_generalTask.HashtagArea);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                ProcessLogger.Log(ex.Message);
            }
        }

        private void StartCheckingTimer(ParserTaskEntity parserTask, ref Timer timer, Func<ParserTaskEntity, Task> timerAction)
        {
            var interval = TimeSpan.FromMinutes(Minutes);
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
