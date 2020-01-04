using Microsoft.EntityFrameworkCore;

namespace SharpSync.Database
{
    public class DatabaseContext : DbContext
    {
        public virtual DbSet<SyncRule> SyncRules { get; set; }


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
    }
}
