//using HashtagHelp.Services.Interfaces;

//namespace HashtagHelp.Services.Implementations.InstaParser
//{
//    public class ParserTaskChecker
//    {
//        private readonly IFunnelService _funnelService;

//        public ParserTaskChecker(IFunnelService funnelService)
//        {
//            _funnelService = funnelService;
//        }

//        public async Task StartCheckingTasks(TimeSpan interval)
//        {
//            while (true)
//            {
//                // Проверить статус задания парсера
//                var taskStatus = await _funnelService.CheckFollowersTaskStatus();

//                // Обработать статус задания (например, выполнить действия по его завершению)

//                // Подождать указанный интервал перед следующей проверкой
//                await Task.Delay(interval);
//            }
//        }
//    }
//}
