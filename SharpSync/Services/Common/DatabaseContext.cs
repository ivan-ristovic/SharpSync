using Microsoft.EntityFrameworkCore;
using SharpSync.Database;

namespace SharpSync.Services.Common
{
    public class DatabaseContext : DbContext
    {
        public virtual DbSet<SyncRule> SyncRules { get; set; }
        public virtual DbSet<SourcePath> SourcePaths { get; set; }
        public virtual DbSet<DestinationPath> DestinationPaths { get; set; }


#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public DatabaseContext()
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        {

        }


        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
                return;

            optionsBuilder.UseSqlite($"Data Source=SharpSync.db;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SyncRule>()
                .HasOne(r => r.Source)
                .WithMany(s => s.SyncRules)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<SyncRule>()
                .HasOne(r => r.Destination)
                .WithMany(s => s.SyncRules)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
