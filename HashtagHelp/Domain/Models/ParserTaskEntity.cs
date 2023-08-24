namespace HashtagHelp.Domain.Models
{
    public class ParserTaskEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string InParserId { get; set; } = string.Empty;
        public List<ResearchedUserEntity> ResearchedUsers { get; set; } = new List<ResearchedUserEntity>();  
        
        private SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        public async Task<bool> TryAcquireSemaphoreAsync()
        {
            return await _semaphore.WaitAsync(0);
        }

        public void ReleaseSemaphore()
        {
            _semaphore.Release();
        }
    }
}

