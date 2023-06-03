using HashtagHelp.Domain.Models;
using HashtagHelp.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace HashtagHelp.Services.Implementations
{
    public class FunnelCreatorService : IFunnelCreatorService
    {
        public async Task CreateFunnelAsync(ResearchedUserEntity researchedUser)
        {
            await researchedUser.GetFollowersAsync();
            // Реализуйте необходимую логику, связанную с созданием воронки
            Console.WriteLine("Funnel created for Researched User: " + researchedUser.Id);
            // Выполните необходимые операции
            await Task.CompletedTask;

        }
    }
}
