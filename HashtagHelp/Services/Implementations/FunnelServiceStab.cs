using HashtagHelp.Domain.Models;
using HashtagHelp.Services.Interfaces;

namespace HashtagHelp.Services.Implementations
{
    public class FunnelServiceStab : IFunnelService
    {
        public IApiRequestService ApiRequestService { get; set; }
        public IHashtagApiRequestService HashtagApiRequestService { get; set; }
        public IParserDataService ParserDataService { get; set; }
        public IProcessLogger ProcessLogger { get; set; }
        public IGoogleApiRequestService GoogleApiRequestService { get; set; }

        private Timer followersTimer;
        private Timer followingTagsTimer;
        private string apiKey = "eMjDt55n11RuhCa7";
        private string hashtagApiKey = "a8f3f7e68amsh2703987539fa87cp17165ajsn6d5c6feed1e9";
        private int bottomBorder = 2;
        private ParserTaskEntity? _followersParserTask;
        private ParserTaskEntity _followingTagsParserTask = new ParserTaskEntity();
        
        private double Minutes = 0.2;

        public async Task AddFollowersTaskAsync(ParserTaskEntity parserTask)
        {
            ProcessLogger.Log("Stab was started");
            var myString = await GoogleApiRequestService.GetDataAsync();
            _followersParserTask = parserTask;
            var userNames = _followersParserTask.ResearchedUsers
                .Select(researchedUser => researchedUser.NickName).ToList();
            _followersParserTask.InParserId = "2405075";
            StartCheckingTimer(_followersParserTask, ref followersTimer, CheckFollowersTaskStatusAsync);
            await Task.CompletedTask;
        }

        public async Task AddFollowingTagsTaskAsync()
        {
            var userNames = _followersParserTask.ResearchedUsers
                .Select(researchedUser => researchedUser.NickName).ToList();
            var taskId = _followersParserTask.InParserId;
            _followingTagsParserTask.InParserId = "2406109";
            Console.WriteLine(_followingTagsParserTask.InParserId);
            ProcessLogger.Log(_followingTagsParserTask.InParserId);
            StartCheckingTimer(_followingTagsParserTask, ref followingTagsTimer, CheckFollowingTagsTaskStatusAsync);
            await Task.CompletedTask;
        }

        public async Task FunnelCreateAsync()
        {
            try
            {
            var tagsTaskContent = await ApiRequestService
                .GetTagsTaskContentAPIAsync(apiKey,_followingTagsParserTask.InParserId);
            var tagFreq = ParserDataService.RedoFiles(tagsTaskContent);
            ParserDataService.RareFreqTagsRemove(tagFreq, bottomBorder);
            var hashtags = new List<HashtagEntity>();
            foreach (var hashtag in tagFreq)
            {
                var hashtagInfo = await HashtagApiRequestService.GetMediaCountAsync(hashtagApiKey, hashtag.Key);
                hashtags.Add(new HashtagEntity
                {
                    Name = hashtag.Key,
                    MediaCount = hashtagInfo.media_count,
                    InstagramId = hashtagInfo.id
                });
            }
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
                }
            }
            catch (Exception ex )
            { 
                Console.WriteLine(ex); 
                ProcessLogger.Log(ex.Message);
            }
        }
    }
}
