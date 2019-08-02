using Microsoft.EntityFrameworkCore;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Data
{
    public abstract class PromenadeContext : Utility.Data.DbContextBase
    {
        protected PromenadeContext(DbContextOptions options) : base(options) { }

        // Read-Only
        public DbSet<Feature> Feature { get; }
        public DbSet<Group> Group { get; }
        public DbSet<Location> Location { get; }
        public DbSet<LocationFeature> LocationFeature { get; }
        public DbSet<LocationGroup> LocationGroup { get; }
        public DbSet<LocationHours> LocationHours { get; }
        public DbSet<LocationHoursOverride> LocationHoursOverrides { get; }
        public DbSet<Page> Pages { get; }
        public DbSet<SiteSetting> SiteSettings { get; }
        public DbSet<SocialCard> SocialCards { get; }
        public DbSet<UrlRedirect> UrlRedirects { get; }

        // Read/Write 
        public DbSet<UrlRedirectAccess> UrlRedirectAccesses { get; set; }
    }
}
