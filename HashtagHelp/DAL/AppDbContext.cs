using HashtagHelp.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace HashtagHelp.DAL
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<FollowerEntity> Followers { get; set; } = null!;
        public DbSet<HashtagEntity> Hashtags { get; set; } = null!;
        public DbSet<ResearchedUserEntity> ResearchedUsers { get; set; } = null!;
        public DbSet<FunnelEntity> Funnels { get; set; } = null!;
        public DbSet<ParserTaskEntity> Tasks { get; set; } = null!;
        public DbSet<GeneralTaskEntity> GeneralTasks { get; set; } = null!;
        public DbSet<UserEntity> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GeneralTaskEntity>()
                .Property(p => p.Status)
                .HasConversion<string>();

            base.OnModelCreating(modelBuilder);
        }
    }
}

