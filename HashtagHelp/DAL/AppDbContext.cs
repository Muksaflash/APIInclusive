﻿using HashtagHelp.Domain.Models;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace HashtagHelp.DAL
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
        {
            Database.EnsureCreated();//обработать тут ошибки! и вокруг вообще
        }

        public DbSet<FollowerEntity> Followers { get; set; } = null!;
        public DbSet<HashtagEntity> Hashtags { get; set; } = null!;
        public DbSet<ResearchedUserEntity> ResearchedUsers { get; set; } = null!;
        public DbSet<ParserTaskEntity> Tasks { get; set; } = null!;
        public DbSet<GeneralTaskEntity> GeneralTasks { get; set; } = null!;
        public DbSet<UserEntity> Users { get; set; } = null!;
        public DbSet<FunnelServiceInfoEntity> FunnelServiceInfos { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GeneralTaskEntity>()
                .Property(p => p.Status)
                .HasConversion<string>();
            modelBuilder.Entity<GeneralTaskEntity>()
                .Property(p => p.ErrorInfo)
                .HasColumnType("LONGTEXT");
            modelBuilder.Entity<GeneralTaskEntity>()
                .Property(p => p.HashtagSemiAreas)
                .HasColumnType("LONGTEXT");
            base.OnModelCreating(modelBuilder);
        }
    }
}

