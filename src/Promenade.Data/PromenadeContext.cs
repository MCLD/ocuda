using Microsoft.EntityFrameworkCore;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Data
{
    public abstract class PromenadeContext : Utility.Data.DbContextBase
    {
        protected PromenadeContext(DbContextOptions options) : base(options) { }

        // Read-Only
        public DbSet<Feature> Features { get; }
        public DbSet<Group> Groups { get; }
        public DbSet<Location> Locations { get; }
        public DbSet<LocationFeature> LocationFeatures { get; }
        public DbSet<LocationGroup> LocationGroups { get; }
        public DbSet<LocationHours> LocationHours { get; }
        public DbSet<LocationHoursOverride> LocationHoursOverrides { get; }
        public DbSet<Page> Pages { get; }
        public DbSet<SiteSetting> SiteSettings { get; }
        public DbSet<SocialCard> SocialCards { get; }
        public DbSet<UrlRedirect> UrlRedirects { get; }

        // Read/Write 
        public DbSet<Language> Languages { get; set; }
        public DbSet<PageHeader> PageHeaders { get; set; }
        public DbSet<UrlRedirectAccess> UrlRedirectAccesses { get; set; }
    }
}
