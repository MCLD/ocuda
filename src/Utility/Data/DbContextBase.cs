using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Ocuda.Utility.Data
{
    public abstract class DbContextBase : DbContext
    {
        public DbContextBase(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var relationship in modelBuilder.Model
                .GetEntityTypes()
                .SelectMany(_ => _.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }

        protected void Migrate() => Database.Migrate();

        protected async Task MigrateAsync() => await Database.MigrateAsync();

        protected IEnumerable<string> GetPendingMigrationList() => Database.GetPendingMigrations();

        protected async Task<IEnumerable<string>> GetPendingMigrationListAsync() 
            => await Database.GetPendingMigrationsAsync();

        protected string GetCurrentMigration() => Database.GetAppliedMigrations().Last();

        protected async Task<string> GetCurrentMigrationAsync() 
            => (await Database.GetAppliedMigrationsAsync()).Last();
    }
}
