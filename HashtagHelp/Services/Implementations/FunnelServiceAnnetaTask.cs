
using HashtagHelp.Domain.Models;
using HashtagHelp.Domain.Enums;
using HashtagHelp.Services.Interfaces;

namespace HashtagHelp.Services.Implementations
{
    public class FunnelServiceAnnetaTask : IFunnelService
    {
        private readonly IApiRequestService _instaParserApiRequestService;
        private readonly IHashtagApiRequestService _hashtagApiRequestService;
        private readonly IProcessLogger _processLogger;
        private readonly IParserDataService _parserDataService;
        private readonly IDataRepository _dataRepository;
        private readonly IGoogleApiRequestService _googleApiRequestService;

        private Timer collectionTimer;
        private Timer filtrationTimer;
        private string instaParserKey;
        private string instaParserUrl;
        private string hashtagApiKey;
        private GeneralTaskEntity _generalTask;
        private double checkTimerMinutes;
        private long minTagMediaCount;
        private long maxTagMediaCount;
        private long minMediaCountInterval;
        private int minFollowerTagsCount = 500;
        private long hashtagsNumber;
        private TaskCompletionSource<bool> _funnelCompletionSource = new();
        private readonly SemaphoreSlim semaphore = new(1);

        public FunnelServiceAnnetaTask(IApiRequestService apiRequestService, IHashtagApiRequestService hashtagApiRequestService,
        IProcessLogger processLogger, IParserDataService parserDataService, IDataRepository dataRepository,
        IGoogleApiRequestService googleApiRequestService)
        {
            _instaParserApiRequestService = apiRequestService;
            _hashtagApiRequestService = hashtagApiRequestService;
            _processLogger = processLogger;
            _parserDataService = parserDataService;
            _googleApiRequestService = googleApiRequestService;
            _dataRepository = dataRepository;
        }

        public async Task SetConfigureAsync(GeneralTaskEntity generalTask)
        {
            try
            {
                var configData = await _googleApiRequestService.GetAllConfigSheetData();
                _generalTask = generalTask;
                if (generalTask.Status == StatusTaskEnum.Initiated)
                {
                    var hashtagAreas = await _googleApiRequestService.GetAreasListAsync();
                    if (!hashtagAreas.Contains(generalTask.HashtagArea))
                    {
                        throw new Exception("Неправильно указана ниша");
                    }
                    instaParserKey = configData[1];
                    instaParserUrl = configData[2];

                    _generalTask.MainParserApiKey = instaParserKey;
                    _generalTask.ParserUrl = instaParserUrl;
                    _generalTask.Status = StatusTaskEnum.Configured;
                    _dataRepository.UpdateGeneralTask(_generalTask);
                }
                else
                {
                    instaParserKey = _generalTask.MainParserApiKey;
                    instaParserUrl = _generalTask.ParserUrl;
                }

                _googleApiRequestService.HashtagArea = generalTask.HashtagArea;
                hashtagApiKey = configData[3];
                checkTimerMinutes = double.Parse(configData[5]);
                minTagMediaCount = long.Parse(configData[11]);
                maxTagMediaCount = long.Parse(configData[21]);
                minMediaCountInterval = long.Parse(configData[31]);
                hashtagsNumber = long.Parse(configData[41]);
            }
            catch (Exception ex)
            {
                _processLogger.Log(ex.ToString());
                throw;
            }
        }

        public async Task StartTaskChainAsync()
        {
            await FunnelCreateAsync();
        }

        private async Task AddCollectionTaskAsync()
        {
            try
            {
                if (_generalTask.Status == StatusTaskEnum.Configured)
                {
                    var userNames = _generalTask.CollectionTask.ResearchedUsers
                        .Select(researchedUser => researchedUser.NickName).ToList();
                    _generalTask.CollectionTask.InParserId = await _instaParserApiRequestService
                        .AddFollowersTaskAPIAsync(instaParserKey, userNames, instaParserUrl);
                    _processLogger.Log("Начата задача сбора с id: " + _generalTask.CollectionTask.InParserId);
                    _generalTask.Status = StatusTaskEnum.Collection;
                    _dataRepository.UpdateParserTask(_generalTask.CollectionTask);
                    _dataRepository.UpdateGeneralTask(_generalTask);
                    await _dataRepository.SaveChangesAsync();
                }
                StartCheckingTimer(_generalTask.CollectionTask, ref collectionTimer, CheckCollectionTaskStatusAsync);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _processLogger.Log(ex.ToString());
                throw;
            }
        }

        public async Task AddFiltrationTaskAsync()
        {
            try
            {
                if (_generalTask.Status == StatusTaskEnum.Collected)
                {
                    var userNames = _generalTask.CollectionTask.ResearchedUsers
                        .Select(researchedUser => researchedUser.NickName).ToList();
                    var taskId = _generalTask.CollectionTask.InParserId;
                    _generalTask.FiltrationTask.InParserId = await _instaParserApiRequestService
                        .AddFollowingTagsTaskAPIAsync(instaParserKey, taskId, userNames, instaParserUrl);
                    _processLogger.Log("Начата задача фильтрации с id: " + _generalTask.FiltrationTask.InParserId);
                    _generalTask.Status = StatusTaskEnum.Filtration;
                    _dataRepository.UpdateParserTask(_generalTask.FiltrationTask);
                    _dataRepository.UpdateGeneralTask(_generalTask);
                    await _dataRepository.SaveChangesAsync();
                }
                StartCheckingTimer(_generalTask.FiltrationTask, ref filtrationTimer, CheckFiltrationTaskStatusAsync);
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _processLogger.Log(ex.ToString());
                throw;
            }
        }

        private async Task<Domain.ExternalApiModels.RocketAPI.BodyData> GetHashtagInfoAsync(string requiredHashtagText)
        {
            try
            {
                Domain.ExternalApiModels.RocketAPI.BodyData hashtagInfo = new();
                HashtagEntity hashtag;
                var exists = _dataRepository.DoesHashtagExist(requiredHashtagText);
                if (exists)
                {
                    hashtag = await _dataRepository.GetEntityByFieldValueAsync<HashtagEntity>(
                        "HashtagHelp.Domain.Models.HashtagEntity", "Name", requiredHashtagText);
                    hashtagInfo.id = hashtag.InstagramId;
                    hashtagInfo.media_count = hashtag.MediaCount;
                    hashtagInfo.name = hashtag.Name;
                    return hashtagInfo;
                }
                else
                {
                    hashtagInfo = await _hashtagApiRequestService.GetHashtagInfoAsync(hashtagApiKey, requiredHashtagText);
                    return hashtagInfo;
                }
            }
            catch (Exception ex)
            {
                _processLogger.Log(ex.ToString());
                throw;
            }
        }

        public async Task FunnelCreateAsync()
        {
            try
            {
                /* var tagsTaskContent = await _instaParserApiRequestService
                    .GetTagsTaskContentAPIAsync(instaParserKey, _generalTask.FiltrationTask.InParserId, instaParserUrl);
                   */
                var directoryParserFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Subscribers_Tags_filtration_of-__vuhlandes_justinbaessler_baryshovv_roslichenkos_basechkaa_mistifika.txt");
                var tagsTaskContent = await File.ReadAllTextAsync(directoryParserFile);
                var tagFreq = _parserDataService.RedoFiles(tagsTaskContent);
                ValidateTagFreq(tagFreq);
                tagFreq = _parserDataService.RareFreqTagsRemove(tagFreq, minFollowerTagsCount);
                ValidateTagFreq(tagFreq);
                var hashtags = await ProcessHashtagsAsync(tagFreq);
                //await SaveHashtagsAsync(hashtags);
                var areaHashtags = await _googleApiRequestService.GetAreaHashtags();
                areaHashtags = areaHashtags
                    .Select(word => word.TrimStart('#'))
                    .Where(word => !ContainsWrongSymbols(word))
                    .ToList();
                var areaHashtagsEntities = await ProcessHashtagsAsync(areaHashtags.ToDictionary(x => x, x => 50));
                //!!! если хэштеги поввторяются, то возникает ошибка!
                hashtags.AddRange(areaHashtagsEntities);
                //await SaveHashtagsAsync(areaHashtagsEntities);
                var funnel = new FunnelEntity(minTagMediaCount, maxTagMediaCount, minMediaCountInterval, hashtagsNumber);
                var funelLines = _parserDataService.CreateFunnels(funnel, hashtags);
                funnel.FunnelText = string.Join("", funelLines);
                var directoryOutFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "funneOutAnn.txt");
                /* foreach (var hashtag in hashtags)
                {
                    File.AppendAllText(directoryOutFile, hashtag.Name + '\t' + hashtag.MediaCount + Environment.NewLine);
                } */
                await File.WriteAllTextAsync(directoryOutFile, funnel.FunnelText);
                _generalTask.HashtagFunnel = funnel;
                _funnelCompletionSource.SetResult(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                _processLogger.Log(ex.ToString());
                _funnelCompletionSource.SetException(ex);
            }
        }
        static bool ContainsWrongSymbols(string text)
        {
            foreach (char c in text)
            {
                if (char.IsSurrogate(c) || char.IsPunctuation(c))
                {
                    return true;//добавить все плохие символы!
                }
            }
            return false;
        }//придумать куда пихнуть этот функционал!!! он уже есть в ParserDataService
        public async Task WaitCompletionGeneralTaskAsync()
        {
            await _funnelCompletionSource.Task;
        }

        private void ValidateTagFreq(Dictionary<string, int> tagFreq)
        {
            if (tagFreq.Count < minFollowerTagsCount)
            {
                _generalTask.Status = StatusTaskEnum.Error;
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
                await SaveHashtagAsync(hashtagEntity);
            }
            return hashtags;
        }

        private async Task SaveHashtagsAsync(List<HashtagEntity> hashtags)
        {
            foreach (var hashtag in hashtags)
            {
                if (!_dataRepository.DoesHashtagExist(hashtag.Name))
                {
                    _dataRepository.AddHashtag(hashtag);
                    await _dataRepository.SaveChangesAsync();
                }
            }
        }

        private async Task SaveHashtagAsync(HashtagEntity hashtag)
        {
            if (!_dataRepository.DoesHashtagExist(hashtag.Name))
            {
                _dataRepository.AddHashtag(hashtag);
                await _dataRepository.SaveChangesAsync();
            }
        }

        private void StartCheckingTimer(ParserTaskEntity parserTask, ref Timer timer, Func<ParserTaskEntity, Task> timerAction)
        {
            var interval = TimeSpan.FromMinutes(checkTimerMinutes);
            timer = new Timer(async state =>
            {
                try
                {
                    semaphore.Wait();
                    await timerAction(parserTask);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при выполнении таймера: {ex.Message}");
                    throw;
                }
                finally
                {
                    semaphore.Release();
                }
            }, null, interval, interval);
        }

        private async Task CheckCollectionTaskStatusAsync(ParserTaskEntity parserTask)
        {
            try
            {
                var taskStatus = await _instaParserApiRequestService
                    .GetTaskStatusAsync(instaParserKey, parserTask.InParserId, instaParserUrl);
                Console.WriteLine(taskStatus.tid_status);
                if (taskStatus.tid_status == "completed")
                {
                    await collectionTimer.DisposeAsync();
                    Console.WriteLine("Приступаем к парсингу подписок подпищиков");
                    _generalTask.Status = StatusTaskEnum.Collected;
                    _dataRepository.UpdateGeneralTask(_generalTask);
                    await _dataRepository.SaveChangesAsync();
                    await AddFiltrationTaskAsync();
                }
            }
            catch (Exception ex)
            {
                await collectionTimer.DisposeAsync();
                _processLogger.Log(ex.ToString());
                _generalTask.Status = StatusTaskEnum.Error;
                _dataRepository.UpdateGeneralTask(_generalTask);
                await _dataRepository.SaveChangesAsync();
                _funnelCompletionSource.SetException(ex);
            }
        }

        private async Task CheckFiltrationTaskStatusAsync(ParserTaskEntity parserTask)
        {
            try
            {
                var taskStatus = await _instaParserApiRequestService
                    .GetTaskStatusAsync(instaParserKey, parserTask.InParserId, instaParserUrl);
                Console.WriteLine(taskStatus.tid_status);
                if (taskStatus.tid_status == "completed")
                {
                    Console.WriteLine("Приступаем к созданию воронки");
                    /*  _generalTask.Status = StatusTaskEnum.Filtrated;
                      _dataRepository.UpdateGeneralTask(_generalTask);
                     await _dataRepository.SaveChangesAsync(); */ //раскомментить для работы
                    await FunnelCreateAsync();
                    await filtrationTimer.DisposeAsync();
                }
            }
            catch (Exception ex)
            {

                _processLogger.Log(ex.ToString());
                _generalTask.Status = StatusTaskEnum.Error;
                _dataRepository.UpdateGeneralTask(_generalTask);
                await _dataRepository.SaveChangesAsync();
                _funnelCompletionSource.SetException(ex);
                await filtrationTimer.DisposeAsync();
            }
        }
    }
}
