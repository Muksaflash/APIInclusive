using HashtagHelp.Domain.Models;
using HashtagHelp.Domain.Enums;
using HashtagHelp.Services.Interfaces;
using HashtagHelp.Domain.RequestModels;

namespace HashtagHelp.Services.Implementations
{
    public class FunnelService : IFunnelService
    {
        public IApiRequestService InstaParserApiRequestService { get; set; }
        public IHashtagApiRequestService HashtagApiRequestService { get; set; }
        public IProcessLogger ProcessLogger { get; set; }
        public IParserDataService ParserDataService { get; set; }
        public IDataRepository DataRepository { get; set; }
        public IGoogleApiRequestService GoogleApiRequestService { get; set; }
        private Timer followersTimer;
        private Timer followingTagsTimer;
        private string instaParserKey;
        private string instaParserUrl;
        private string hashtagApiKey;
        private GeneralTaskEntity _generalTask;
        private double checkTimerMinutes;
        private long minTagMediaCount;
        private long maxTagMediaCount;
        private long minMediaCountInterval;
        private long hashtagsNumber;
        private TaskCompletionSource<bool> _funnelCompletionSource = new();

        public async Task SetConfigure(GeneralTaskEntity generalTask)
        {
            try
            {
                var configData = await GoogleApiRequestService.GetAllConfigSheetData();

                if (generalTask.Status == StatusTaskEnum.Initiated)
                {
                    GoogleApiRequestService.HashtagArea = generalTask.HashtagArea;
                    var hashtagAreas = await GoogleApiRequestService.GetAreasListAsync();
                    if (!hashtagAreas.Contains(generalTask.HashtagArea))
                    {
                        throw new Exception("Неправильно указана ниша");
                    }

                    _generalTask = generalTask;
                    _generalTask.MainParserApiKey = instaParserKey;
                    _generalTask.ParserUrl = instaParserUrl;
                    _generalTask.Status = StatusTaskEnum.Configured;
                }
                else
                {
                    instaParserKey = _generalTask.MainParserApiKey;
                    instaParserUrl = _generalTask.ParserUrl;
                }

                hashtagApiKey = configData[3];
                checkTimerMinutes = double.Parse(configData[5]);
                minTagMediaCount = long.Parse(configData[11]);
                maxTagMediaCount = long.Parse(configData[21]);
                minMediaCountInterval = long.Parse(configData[31]);
                hashtagsNumber = long.Parse(configData[41]);
            }
            catch (Exception ex)
            {
                ProcessLogger.Log(ex.ToString());
                throw;
            }
        }

        public async Task AddFollowersTaskAsync()
        {
            try
            {
                ProcessLogger.Log("App was started");
                var userNames = _generalTask.CollectionTask.ResearchedUsers
                    .Select(researchedUser => researchedUser.NickName).ToList();
                _generalTask.CollectionTask.InParserId = await InstaParserApiRequestService
                    .AddFollowersTaskAPIAsync(instaParserKey, userNames, instaParserUrl);
                DataRepository.UpdateParserTask(_generalTask.CollectionTask);
                await DataRepository.SaveChangesAsync();
                StartCheckingTimer(_generalTask.CollectionTask, ref followersTimer, CheckFollowersTaskStatusAsync);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                ProcessLogger.Log(ex.ToString());
                throw;
            }
        }

        public async Task AddFollowingTagsTaskAsync()
        {
            try
            {
                var userNames = _generalTask.CollectionTask.ResearchedUsers
                    .Select(researchedUser => researchedUser.NickName).ToList();
                var taskId = _generalTask.CollectionTask.InParserId;
                _generalTask.FiltrationTask.InParserId = await InstaParserApiRequestService
                    .AddFollowingTagsTaskAPIAsync(instaParserKey, taskId, userNames, instaParserUrl);
                DataRepository.UpdateParserTask(_generalTask.FiltrationTask);
                await DataRepository.SaveChangesAsync();
                StartCheckingTimer(_generalTask.FiltrationTask, ref followingTagsTimer, CheckFollowingTagsTaskStatusAsync);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                ProcessLogger.Log(ex.ToString());
                throw;
            }
        }

        private async Task<Domain.ExternalApiModels.RocketAPI.BodyData> GetHashtagInfoAsync(string requiredHashtagText)
        {
            try
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
            catch (Exception ex)
            {
                ProcessLogger.Log(ex.ToString());
                throw;
            }
        }

        public async Task FunnelCreateAsync()
        {
            try
            {
                var tagsTaskContent = await InstaParserApiRequestService
                    .GetTagsTaskContentAPIAsync(instaParserKey, _generalTask.FiltrationTask.InParserId, instaParserUrl);
                var tagFreq = ParserDataService.RedoFiles(tagsTaskContent);
                ValidateTagFreq(tagFreq);
                ParserDataService.RareFreqTagsRemove(tagFreq);
                var hashtags = await ProcessHashtagsAsync(tagFreq);
                await SaveHashtagsAsync(hashtags);
                var areaHashtags = await GoogleApiRequestService.GetAreaHashtags();
                var areaHashtagsEntities = await ProcessHashtagsAsync(areaHashtags.ToDictionary(x => x, x => 50));
                hashtags.AddRange(areaHashtagsEntities);
                await SaveHashtagsAsync(areaHashtagsEntities);
                var funnel = new FunnelEntity(minTagMediaCount, maxTagMediaCount, minMediaCountInterval, hashtagsNumber);
                var funelLines = ParserDataService.CreateFunnels(funnel, hashtags);
                funnel.FunnelText = string.Join("", funelLines);
                _generalTask.HashtagFunnel = funnel;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                ProcessLogger.Log(ex.ToString());
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
                throw new Exception("Слишком мало хештегов найдено у конкурентов");
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
                var taskStatus = await InstaParserApiRequestService
                    .GetTaskStatusAsync(instaParserKey, parserTask.InParserId, instaParserUrl);
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
                ProcessLogger.Log(ex.ToString());
            }
        }

        private async Task CheckFollowingTagsTaskStatusAsync(ParserTaskEntity parserTask)
        {
            try
            {
                var taskStatus = await InstaParserApiRequestService
                    .GetTaskStatusAsync(instaParserKey, parserTask.InParserId, instaParserUrl);
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
                ProcessLogger.Log(ex.ToString());
            }
        }
    }
}
