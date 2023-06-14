//using HashtagHelp.Domain.Models;
//using HashtagHelp.Services.Interfaces;
//using System;
//using System.Threading.Tasks;

//namespace HashtagHelp.Services.Implementations
//{
//    public class FunnelCreatorService : IFunnelCreatorService
//    {
//        public IFollowersTaskService followersTaskService { get; set; }

//        public async Task CreateFunnelAsync(ResearchedUserEntity researchedUser)
//        {
//            await researchedUser.GetIdAsync();
//            await researchedUser.GetFollowersAsync();
//            foreach (var follower in researchedUser.Followers) 
//            {
//                await follower.GetFollowingTagsAsync();
//            }
//            // Реализуйте необходимую логику, связанную с созданием воронки
//            Console.WriteLine("Funnel created for Researched User: " + researchedUser.Id);
//            // Выполните необходимые операции
//            await Task.CompletedTask;
//        }
//    }
//}
