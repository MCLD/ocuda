using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data
{
    public abstract class PromenadeContext : Utility.Data.DbContextBase, IMigratableContext
    {
        protected PromenadeContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // turn off cascading deletes
            foreach (var relationship in modelBuilder.Model
                .GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }

        #region IMigratableContext
        public new void Migrate() => Database.Migrate();
        public new IEnumerable<string> GetPendingMigrationList() => Database.GetPendingMigrations();
        public new string GetCurrentMigration() => Database.GetAppliedMigrations().Last();
        #endregion IMigratableContext

        public DbSet<LocationHours> LocationHours { get; set; }
        public DbSet<LocationHoursOverride> LocationHoursOverrides { get; set; }
    }
}
