using HashtagHelp.Domain.Models;
using HashtagHelp.Services.Interfaces;
using System.Threading;

namespace HashtagHelp.Services.Implementations
{
    public class FunnelServiceStab : IFunnelService
    {
        public IApiRequestService ApiRequestService { get; set; }
        private Timer followersTimer;
        private Timer followingTagsTimer;
        private string apiKey = "eMjDt55n11RuhCa7";
        private ParserTaskEntity? _followersParserTask;
        private ParserTaskEntity _followingTagsParserTask = new ParserTaskEntity();
        private double Minutes = 1;

        public async Task AddFollowersTaskAsync(ParserTaskEntity parserTask)
        {
            _followersParserTask = parserTask;
            _followersParserTask.InParserId = "";
            StartCheckingTimer(_followersParserTask, ref followersTimer, CheckFollowersTaskStatus);
            await Task.CompletedTask;
        }

        public async Task AddFollowingTagsTaskAsync()
        {
            var userNames = _followersParserTask.ResearchedUsers
                .Select(researchedUser => researchedUser.NickName).ToList();
            var taskId = _followersParserTask.InParserId;
            _followingTagsParserTask.InParserId = await ApiRequestService
                .AddFollowingTagsTaskAPIAsync(apiKey, taskId, userNames);
            StartCheckingTimer(_followingTagsParserTask, ref followingTagsTimer, CheckFollowingTagsTaskStatus);
            await Task.CompletedTask;
        }

        private void StartCheckingTimer(ParserTaskEntity parserTask, ref Timer timer, Func<ParserTaskEntity, Task> timerAction)
        {
            var interval = TimeSpan.FromMinutes(Minutes);
            timer = new Timer(async state =>
            {
                await timerAction(parserTask);
            }, null, interval, interval);
        }

        private async Task CheckFollowersTaskStatus(ParserTaskEntity parserTask)
        {
            try
            {
                var taskStatus = await ApiRequestService.GetTaskStatusAsync(apiKey, parserTask.InParserId);
                Console.WriteLine(taskStatus.tid_status + " " + parserTask.InParserId + " "+ taskStatus.AddTime);
                if (taskStatus.tid_status == "completed")
                {
                    await followersTimer.DisposeAsync();
                    await AddFollowingTagsTaskAsync();
                }
            }
            catch (Exception ex) { }
        }

        private async Task CheckFollowingTagsTaskStatus(ParserTaskEntity parserTask)
        {
            try
            {
                var taskStatus = await ApiRequestService.GetTaskStatusAsync(apiKey, parserTask.InParserId);
                Console.WriteLine(taskStatus.tid_status);
                if (taskStatus.tid_status == "completed")
                {
                    await followingTagsTimer.DisposeAsync();
                    Console.WriteLine("Акукарача");
                }
            }
            catch (Exception ex) { }
        }
    }
}