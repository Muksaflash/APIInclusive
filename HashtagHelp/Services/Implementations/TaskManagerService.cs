using HashtagHelp.Domain.Models;
using HashtagHelp.Services.Interfaces;

namespace HashtagHelp.Services.Implementations
{
    class TaskManagerService : IHostedService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IProcessLogger _processLogger;


        public TaskManagerService(IServiceScopeFactory serviceScopeFactory, IProcessLogger processLogger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _processLogger = processLogger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                Console.WriteLine("Hosted service works");
                IQueryable<GeneralTaskEntity>? notCompletedGeneralTasks;

                using (var scope1 = _serviceScopeFactory.CreateScope())
                {
                    var serviceProvider = scope1.ServiceProvider;
                    var dataRepository = serviceProvider.GetRequiredService<IDataRepository>();

                    notCompletedGeneralTasks = dataRepository.GetNotCompletedGeneralTasks();

                    Console.WriteLine("Not completed tasks: " + notCompletedGeneralTasks.Count().ToString());

                    var resumeTasks = notCompletedGeneralTasks.Select(ResumeTask);
                    Task.WhenAll(resumeTasks);

                    async Task ResumeTask(GeneralTaskEntity generalTask)
                    {
                        Console.WriteLine("start of task reworker");
                        using IServiceScope scope2 = _serviceScopeFactory.CreateScope();
                        var dataRepository = scope2.ServiceProvider.GetRequiredService<IDataRepository>();
                        var funnelService = scope2.ServiceProvider.GetRequiredService<IFunnelService>();
                        await funnelService.SetConfigureAsync(generalTask);
                        await funnelService.StartTaskChainAsync();
                        await funnelService.WaitCompletionGeneralTaskAsync();
                    }

                }
                cancellationToken.ThrowIfCancellationRequested();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                _processLogger.Log(ex.ToString());
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
