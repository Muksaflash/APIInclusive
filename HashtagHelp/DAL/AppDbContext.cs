using HashtagHelp.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace HashtagHelp.DAL
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();
            
        }
        public DbSet<FollowerEntity> Followers { get; set; } = null!;
        public DbSet<HashtagEntity> Hashtags { get; set; } = null!;
        public DbSet<ResearchedUserEntity> ResearchedUsers { get; set; } = null!;
        public DbSet<FunnelEntity> Funnels { get; set; } = null!;
        public DbSet<ParserTaskEntity> Tasks { get; set; } = null!;
        public DbSet<TelegramUserEntity> TelegramUsers { get; set; } = null!;
    }
}
