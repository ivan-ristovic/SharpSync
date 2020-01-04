using Microsoft.EntityFrameworkCore;

namespace SharpSync.Database
{
    public class DatabaseContext : DbContext
    {
        public virtual DbSet<SyncRule>? SyncRules { get; set; }


        public DatabaseContext()
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
