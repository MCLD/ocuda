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
        public DbSet<Location> Location { get; set; }
        public DbSet<Feature> Feature { get; set; }
        public DbSet<Group> Group { get; set; }
        public DbSet<LocationFeature> LocationFeature { get; set; }
        public DbSet<LocationGroup> LocationGroup { get; set; }
        public DbSet<Page> Pages { get; set; }
        public DbSet<SiteSetting> SiteSettings { get; set; }
        public DbSet<SocialCard> SocialCards { get; set; }
        public DbSet<UrlRedirectAccess> UrlRedirectAccesses { get; set; }
        public DbSet<UrlRedirect> UrlRedirects { get; set; }
    }
}
