
using HashtagHelp.Domain.Models;
using HashtagHelp.Domain.Enums;
using HashtagHelp.Services.Interfaces;
using System.Text.RegularExpressions;

namespace HashtagHelp.Services.Implementations
{
    public class FunnelService : IFunnelService
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
        private FunnelServiceInfoEntity _funnelServiceInfo;
        private GoogleSheetsFunnelTaskEntity _googleSheetsFunnelTask;
        private double checkTimerMinutes;
        private long minTagMediaCount;
        private long maxTagMediaCount;
        private long minMediaCountInterval;
        private int minTagFollowersNumber;
        private int minFollowerTagsCount;
        private long hashtagsNumber;
        private int hashtagsReadyNumber;
        private TaskCompletionSource<bool> _funnelCompletionSource = new();
        private readonly SemaphoreSlim semaphore = new(1);
        private readonly string _hashtagEntityAdress = "HashtagHelp.Domain.Models.HashtagEntity";
        
        public FunnelService(IApiRequestService apiRequestService, IHashtagApiRequestService hashtagApiRequestService,
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

        public async Task SetGoogleSheetsFunnelConfigureAsync(GoogleSheetsFunnelTaskEntity googleSheetsFunnelTask, FunnelServiceInfoEntity funnelServiceInfo)
        {
            try
            {
                var configData = await _googleApiRequestService.GetAllConfigSheetDataAsync();
                _googleSheetsFunnelTask = googleSheetsFunnelTask;
                _funnelServiceInfo = funnelServiceInfo;
                if (googleSheetsFunnelTask.Status == StatusTaskEnum.Initiated)
                {
                    _googleSheetsFunnelTask.Status = StatusTaskEnum.Configured;
                }
                hashtagApiKey = configData[3];
                checkTimerMinutes = double.Parse(configData[5]);
                minTagMediaCount = long.Parse(configData[11]);
                maxTagMediaCount = long.Parse(configData[21]);
                minMediaCountInterval = long.Parse(configData[31]);
                hashtagsNumber = long.Parse(configData[41]);
                minFollowerTagsCount = int.Parse(configData[51]);
                _googleApiRequestService.UserParsedHashtagsSheetName = _googleSheetsFunnelTask.ParsedSheetName;
                _googleApiRequestService.UserAreaHashtagsSheetName = _googleSheetsFunnelTask.AreaSheetName;
                _googleApiRequestService.UserSemiAreasHashtagsSheetName = _googleSheetsFunnelTask.SemiAreasSheetName;
                _googleApiRequestService.UserTable = _googleSheetsFunnelTask.TableId;
                _googleApiRequestService.UserOutputSheet = _googleSheetsFunnelTask.OutputGoogleSheet;
                if (_googleSheetsFunnelTask.MinHashtagFollowers == string.Empty)
                {
                    minTagFollowersNumber = int.Parse(configData[61]);
                }
                else
                {
                    minTagFollowersNumber = int.Parse(_googleSheetsFunnelTask.MinHashtagFollowers);
                }
            }
            catch (Exception ex)
            {
                _processLogger.Log(ex.ToString());
                _googleSheetsFunnelTask.Status = StatusTaskEnum.Error;
                _googleSheetsFunnelTask.ErrorInfo = ex.Message;
                throw;
            }
        }

        public async Task SetConfigureAsync(GeneralTaskEntity generalTask)
        {
            try
            {
                var configData = await _googleApiRequestService.GetAllConfigSheetDataAsync();
                _generalTask = generalTask;
                if (generalTask.Status == StatusTaskEnum.Initiated)
                {
                    var hashtagAreas = await _googleApiRequestService.GetAreasListAsync();
                    if (!hashtagAreas.Contains(generalTask.HashtagArea))
                    {
                        throw new Exception("Incorrect hashtags area specified.");
                    }
                    foreach (var semiArea in _generalTask.HashtagSemiAreas.Split(", "))
                        if (!hashtagAreas.Contains(semiArea))
                        {
                            throw new Exception("Incorrect hashtags semi areas specified.");
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
                _googleApiRequestService.HashtagSemiAreas = generalTask.HashtagSemiAreas.Split(", ").ToList();
                hashtagApiKey = configData[3];
                checkTimerMinutes = double.Parse(configData[5]);
                minTagMediaCount = long.Parse(configData[11]);
                maxTagMediaCount = long.Parse(configData[21]);
                minMediaCountInterval = long.Parse(configData[31]);
                hashtagsNumber = long.Parse(configData[41]);
                minFollowerTagsCount = int.Parse(configData[51]);
                minTagFollowersNumber = int.Parse(configData[61]);
            }
            catch (Exception ex)
            {
                _processLogger.Log(ex.ToString());
                _generalTask.Status = StatusTaskEnum.Error;
                _generalTask.ErrorInfo = ex.Message;
                _dataRepository.UpdateGeneralTask(_generalTask);
                await _dataRepository.SaveChangesAsync();
                throw;
            }
        }

        public async Task StartTaskChainAsync()
        {
            if (_googleSheetsFunnelTask.Status != null)
            {
                await GoogleSheetsFunnelCreateAsync();
            }
            if (_generalTask == null) return;
            if (_generalTask.Status == StatusTaskEnum.Configured || _generalTask.Status == StatusTaskEnum.Collection)
            {
                await AddCollectionTaskAsync();
            }
            if (_generalTask.Status == StatusTaskEnum.Collected || _generalTask.Status == StatusTaskEnum.Filtration)
            {
                await AddFiltrationTaskAsync();
            }
            if (_generalTask.Status == StatusTaskEnum.Filtrated)
            {
                await FunnelCreateAsync();
            }
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
                        .AddCollectionTaskApiAsync(instaParserKey, userNames, instaParserUrl);
                    _processLogger.Log("Started task with collection ID: " + _generalTask.CollectionTask.InParserId);
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
                        .AddFiltrationTaskApiAsync(instaParserKey, taskId, userNames, instaParserUrl);
                    _processLogger.Log("Started filtration task with ID: " + _generalTask.FiltrationTask.InParserId);
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
                    _funnelServiceInfo.IsRequestToHashtagParser = "false";
                    _dataRepository.UpdateFunnelServiceInfo(_funnelServiceInfo); // если фуннел сервайс инфо не нал
                    await _dataRepository.SaveChangesAsync();
                    hashtag = await _dataRepository.GetEntityByFieldValueAsync<HashtagEntity>(
                        _hashtagEntityAdress, "Name", requiredHashtagText);
                    hashtagInfo.id = hashtag.InstagramId;
                    hashtagInfo.media_count = hashtag.MediaCount;
                    hashtagInfo.name = hashtag.Name;
                    return hashtagInfo;
                }
                else
                {
                    _funnelServiceInfo.IsRequestToHashtagParser = "true";
                    _dataRepository.UpdateFunnelServiceInfo(_funnelServiceInfo);
                    await _dataRepository.SaveChangesAsync();
                    hashtagInfo = await _hashtagApiRequestService.GetHashtagInfoAsync(hashtagApiKey, requiredHashtagText);
                    _funnelServiceInfo.IsRequestToHashtagParser = "false";
                    _dataRepository.UpdateFunnelServiceInfo(_funnelServiceInfo);
                    await _dataRepository.SaveChangesAsync();
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
                /*                 var tagsTaskContent = await _instaParserApiRequestService
                                    .GetTagsTaskContentAPIAsync(instaParserKey, _generalTask.FiltrationTask.InParserId, instaParserUrl); */

                var parserFileName1 = "Фильтрация_аккаунтов_-_рушан_косвенные.txt";
                var filePath1 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Parser", parserFileName1);
                var parserFileName2 = "Фильтрация_аккаунтов_-_рушан_собствеенный.txt";
                var filePath2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Parser", parserFileName2);
                var parserFileName3 = "Фильтрация_аккаунтов_-прямые_рушан — копия.txt";
                var filePath3 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Parser", parserFileName3);

                var tagsTaskContent = File.ReadAllText(filePath1);
                tagsTaskContent += File.ReadAllText(filePath2);
                tagsTaskContent += File.ReadAllText(filePath3);


                var tagFreq = _parserDataService.RedoFiles(tagsTaskContent);
                ValidateTagFreq(tagFreq);
                tagFreq = _parserDataService.RareFreqTagsRemove(tagFreq, minFollowerTagsCount, minTagFollowersNumber);
                ValidateTagFreq(tagFreq);
                var parsedHashtagEntities = await ProcessHashtagsAsync(tagFreq);
                //var areaHashtags = await _googleApiRequestService.GetAreaHashtags();

                var targetFileName1 = "целевые.txt";
                var targetFilePath1 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Parser", targetFileName1);
                /*  var targetFileName2 = "конкурентов.txt";
                 var targetFilePath2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Parser", targetFileName2); */
                var targetTagsTaskContent = File.ReadAllText(targetFilePath1);
                /* targetTagsTaskContent += File.ReadAllText(targetFilePath2); */
                var areaHashtags = _parserDataService.RedoFiles(targetTagsTaskContent).Keys.ToList();

                //var semiAreaHashtags = await _googleApiRequestService.GetSemiAreaHashtags();

                var adjacentFileName = "смежные.txt";
                var adjacentFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Parser", adjacentFileName);
                var adjacentTagsTaskContent = File.ReadAllText(adjacentFilePath);
                var semiAreaHashtags = _parserDataService.RedoFiles(adjacentTagsTaskContent).Keys.ToList();

                areaHashtags = areaHashtags
                    .Select(word => word.TrimStart('#').Trim())
                    .Where(word => !ContainsWrongSymbols(word))
                    .ToList();
                semiAreaHashtags = semiAreaHashtags
                    .Select(word => word.TrimStart('#').Trim())
                    .Where(word => !ContainsWrongSymbols(word))
                    .ToList();
                var areaHashtagsEntities = await ProcessHashtagsAsync(areaHashtags
                    .GroupBy(x => x)
                    .ToDictionary(group => group.Key, group => 50));
                var semiAreaHashtagEntities = await ProcessHashtagsAsync(semiAreaHashtags
                    .GroupBy(x => x)
                    .ToDictionary(group => group.Key, group => 50));
                var funnel = new FunnelEntity(minTagMediaCount, maxTagMediaCount, minMediaCountInterval, hashtagsNumber, parsedHashtagEntities, areaHashtagsEntities, semiAreaHashtagEntities);
                var funelLines = _parserDataService.CreateFunnels(funnel);
                funnel.FunnelText = string.Join("", funelLines);
                var directoryOutFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "funnefreqOutInv.txt");
                File.AppendAllText(directoryOutFile, funnel.FunnelText);
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

        public async Task GoogleSheetsFunnelCreateAsync()
        {
            try
            {
                var tagsTaskContent = string.Join('\n', await _googleApiRequestService.GetUserParsedContentAsync());
                var tagFreq = _parserDataService.RedoFiles(tagsTaskContent);

                _funnelServiceInfo.ParsedHashtagNumber = tagFreq.Count.ToString();

                ValidateTagFreq(tagFreq); // переписать метод для гугл таблиц пользователя?
                tagFreq = _parserDataService.RareFreqTagsRemove(tagFreq, minFollowerTagsCount, minTagFollowersNumber);
                ValidateTagFreq(tagFreq);
                
                _funnelServiceInfo.FilteredHashtagNumber = tagFreq.Count.ToString();

                var areaHashtagsString = string.Join('\n', await _googleApiRequestService.GetUserAreaHashtagsAsync());
                var areaHashtags = _parserDataService.RedoFiles(areaHashtagsString).Keys.ToList();

                var semiAreaHashtagsString = string.Join('\n', await _googleApiRequestService.GetUserSemiAreaHashtagsAsync());
                var semiAreaHashtags = _parserDataService.RedoFiles(semiAreaHashtagsString).Keys.ToList();

                areaHashtags = areaHashtags
                    .Select(word => word.TrimStart('#').Trim())
                    .Where(word => !ContainsWrongSymbols(word))
                    .ToList();
                semiAreaHashtags = semiAreaHashtags
                    .Select(word => word.TrimStart('#').Trim())
                    .Where(word => !ContainsWrongSymbols(word))
                    .ToList();

                _funnelServiceInfo.AreaHashtagNumber = areaHashtags.Count.ToString();
                _funnelServiceInfo.SemiAreaHashtagNumber = semiAreaHashtags.Count.ToString();
                _dataRepository.UpdateFunnelServiceInfo(_funnelServiceInfo);
                await _dataRepository.SaveChangesAsync();

                var parsedHashtagEntities = await ProcessHashtagsAsync(tagFreq);

                var areaHashtagsEntities = await ProcessHashtagsAsync(areaHashtags
                    .GroupBy(x => x)
                    .ToDictionary(group => group.Key, group => 50));
                var semiAreaHashtagEntities = await ProcessHashtagsAsync(semiAreaHashtags
                    .GroupBy(x => x)
                    .ToDictionary(group => group.Key, group => 50));
                var funnel = new FunnelEntity(minTagMediaCount, maxTagMediaCount, minMediaCountInterval, hashtagsNumber, parsedHashtagEntities, areaHashtagsEntities, semiAreaHashtagEntities);
                var funelLines = _parserDataService.CreateFunnels(funnel);
                await _googleApiRequestService.PublicListAsync(funelLines);
                _funnelServiceInfo.Status = "Completed";
                _dataRepository.UpdateFunnelServiceInfo(_funnelServiceInfo);
                await _dataRepository.SaveChangesAsync();
                //_dataRepository.DeleteFunnelServiceInfo(_funnelServiceInfo); //читстить их в  Hosted Service?
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
            var value = Regex.IsMatch(text, @"[^a-zA-Zа-яА-Я0-9_]");
            return value;
        }


        public async Task WaitCompletionGeneralTaskAsync()
        {
            await _funnelCompletionSource.Task;
        }

        private async void ValidateTagFreq(Dictionary<string, int> tagFreq)
        {
            if (tagFreq.Count < minFollowerTagsCount)
            {
                _generalTask.Status = StatusTaskEnum.Error;
                _dataRepository.UpdateGeneralTask(_generalTask);
                await _dataRepository.SaveChangesAsync();
                throw new Exception("Too few hashtags found among followers.");
            }
        }

        private async Task<List<HashtagEntity>> ProcessHashtagsAsync(Dictionary<string, int> tagFreq)
        {
            var hashtags = new List<HashtagEntity>();

            foreach (var tag in tagFreq)
            {
                _funnelServiceInfo.Hashtag = tag.Key;

                var hashtagInfo = await GetHashtagInfoAsync(tag.Key);
                var hashtagEntity = new HashtagEntity
                {
                    Name = tag.Key,
                    MediaCount = hashtagInfo.media_count,
                    InstagramId = hashtagInfo.id
                };
                hashtags.Add(hashtagEntity);
                hashtagsReadyNumber ++;
                _funnelServiceInfo.HashtagsReadyNumber = hashtagsReadyNumber.ToString();
                _dataRepository.UpdateFunnelServiceInfo(_funnelServiceInfo);
                await SaveHashtagAsync(hashtagEntity);
            }
            return hashtags;
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
                    Console.WriteLine("Error while executing the timer: " + ex.Message);
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
                    Console.WriteLine("Starting the parsing of subscriber subscriptions.");
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
                    _generalTask.Status = StatusTaskEnum.Filtrated;
                    _dataRepository.UpdateGeneralTask(_generalTask);
                    await _dataRepository.SaveChangesAsync();
                    await FunnelCreateAsync();
                    await filtrationTimer.DisposeAsync();
                }
            }
            catch (Exception ex)
            {

                _processLogger.Log(ex.ToString());
                _funnelCompletionSource.SetException(ex);
                await filtrationTimer.DisposeAsync();
            }
        }
    }
}
