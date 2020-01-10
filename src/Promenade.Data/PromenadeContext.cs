using Microsoft.EntityFrameworkCore;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Data
{
    public abstract class PromenadeContext : Utility.Data.DbContextBase
    {
        protected PromenadeContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // configure composite keys
            // https://docs.microsoft.com/en-us/ef/core/modeling/keys
            modelBuilder.Entity<EmediaCategory>()
                .HasKey(_ => new { _.CategoryId, _.EmediaId });
            modelBuilder.Entity<LocationFeature>()
                .HasKey(_ => new { _.FeatureId, _.LocationId });
            modelBuilder.Entity<LocationGroup>()
                .HasKey(_ => new { _.GroupId, _.LocationId });
            modelBuilder.Entity<LocationHours>()
                .HasKey(_ => new { _.DayOfWeek, _.LocationId });
            modelBuilder.Entity<NavigationText>()
                .HasKey(_ => new { _.Id, _.LanguageId });
            modelBuilder.Entity<Page>()
                .HasKey(_ => new { _.LanguageId, _.PageHeaderId });
            modelBuilder.Entity<SegmentText>()
                .HasKey(_ => new { _.SegmentId, _.LanguageId });
        }

        // Read-Only
        public DbSet<Category> Categories { get; }
        public DbSet<Emedia> Emedia { get; set; }
        public DbSet<EmediaCategory> EmediaCategories { get; set; }
        public DbSet<ExternalResource> ExternalResources { get; set; }
        public DbSet<Feature> Features { get; }
        public DbSet<Group> Groups { get; }
        public DbSet<Location> Locations { get; }
        public DbSet<LocationFeature> LocationFeatures { get; }
        public DbSet<LocationGroup> LocationGroups { get; }
        public DbSet<LocationHours> LocationHours { get; }
        public DbSet<LocationHoursOverride> LocationHoursOverrides { get; }
        public DbSet<Navigation> Navigations { get; }
        public DbSet<NavigationText> NavigationTexts { get; }
        public DbSet<Page> Pages { get; }
        public DbSet<Segment> Segments { get; }
        public DbSet<SegmentText> SegmentTexts { get; }
        public DbSet<SiteSetting> SiteSettings { get; }
        public DbSet<SocialCard> SocialCards { get; }
        public DbSet<UrlRedirect> UrlRedirects { get; }

        // Read/Write 
        public DbSet<Language> Languages { get; set; }
        public DbSet<UrlRedirectAccess> UrlRedirectAccesses { get; set; }
        public DbSet<PageHeader> PageHeaders { get; set; }
    }
}
