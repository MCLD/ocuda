using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data
{
    public abstract class PromenadeContext
        : Utility.Data.DbContextBase, IMigratableContext, IDataProtectionKeyContext
    {
        protected PromenadeContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<CardDetail> CardDetails { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<CarouselButtonLabel> CarouselButtonLabels { get; set; }

        public DbSet<CarouselButtonLabelText> CarouselButtonLabelTexts { get; set; }

        public DbSet<CarouselButton> CarouselButtons { get; set; }

        public DbSet<CarouselItem> CarouselItems { get; set; }

        public DbSet<CarouselItemText> CarouselItemTexts { get; set; }

        public DbSet<Carousel> Carousels { get; set; }

        public DbSet<CarouselTemplate> CarouselTemplates { get; set; }

        public DbSet<CarouselText> CarouselTexts { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<CategoryText> CategoryTexts { get; set; }

        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

        public DbSet<Deck> Decks { get; set; }
        public DbSet<Emedia> Emedia { get; set; }

        public DbSet<EmediaCategory> EmediaCategories { get; set; }

        public DbSet<EmediaGroup> EmediaGroups { get; set; }

        public DbSet<EmediaText> EmediaTexts { get; set; }

        public DbSet<ExternalResource> ExternalResources { get; set; }

        public DbSet<Feature> Features { get; set; }

        public DbSet<Group> Groups { get; set; }
        public DbSet<ImageFeatureItem> ImageFeatureItems { get; set; }
        public DbSet<ImageFeatureItemText> ImageFeatureItemTexts { get; set; }
        public DbSet<ImageFeature> ImageFeatures { get; set; }
        public DbSet<ImageFeatureTemplate> ImageFeatureTemplates { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<LocationFeature> LocationFeatures { get; set; }
        public DbSet<LocationForm> LocationForms { get; set; }
        public DbSet<LocationGroup> LocationGroups { get; set; }
        public DbSet<LocationHours> LocationHours { get; set; }
        public DbSet<LocationHoursOverride> LocationHoursOverrides { get; set; }
        public DbSet<LocationInteriorImageAltText> LocationInteriorImageAltTexts { get; set; }
        public DbSet<LocationInteriorImage> LocationInteriorImages { get; set; }
        public DbSet<LocationProductMap> LocationProductMaps { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Navigation> Navigations { get; set; }
        public DbSet<NavigationText> NavigationTexts { get; set; }
        public DbSet<PageHeader> PageHeaders { get; set; }
        public DbSet<PageItem> PageItems { get; set; }
        public DbSet<PageLayout> PageLayouts { get; set; }
        public DbSet<PageLayoutText> PageLayoutTexts { get; set; }
        public DbSet<Page> Pages { get; set; }
        public DbSet<PodcastDirectory> PodcastDirectories { get; set; }
        public DbSet<PodcastDirectoryInfo> PodcastDirectoryInfos { get; set; }
        public DbSet<PodcastItem> PodcastItems { get; set; }
        public DbSet<Podcast> Podcasts { get; set; }
        public DbSet<ProductLocationInventory> ProductLocationInventories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ScheduleRequest> ScheduleRequest { get; set; }
        public DbSet<ScheduleRequestLimit> ScheduleRequestLimits { get; set; }
        public DbSet<ScheduleRequestSubject> ScheduleRequestSubject { get; set; }
        public DbSet<ScheduleRequestSubjectText> ScheduleRequestSubjectTexts { get; set; }
        public DbSet<Segment> Segments { get; set; }
        public DbSet<SegmentText> SegmentTexts { get; set; }
        public DbSet<SiteSetting> SiteSettings { get; set; }
        public DbSet<SocialCard> SocialCards { get; set; }
        public DbSet<UrlRedirectAccess> UrlRedirectAccesses { get; set; }
        public DbSet<UrlRedirect> UrlRedirects { get; set; }
        public DbSet<VolunteerForm> VolunteerForms { get; set; }
        public DbSet<VolunteerFormSubmission> VolunteerFormSubmissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new System.ArgumentNullException(nameof(modelBuilder));
            }

            // turn off cascading deletes
            foreach (var relationship in modelBuilder.Model
                .GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            // configure composite keys
            // https://docs.microsoft.com/en-us/ef/core/modeling/keys
            modelBuilder.Entity<CardDetail>()
                .HasKey(_ => new { _.CardId, _.LanguageId });
            modelBuilder.Entity<CarouselButtonLabelText>()
                .HasKey(_ => new { _.CarouselButtonLabelId, _.LanguageId });
            modelBuilder.Entity<CarouselItemText>()
                .HasKey(_ => new { _.CarouselItemId, _.LanguageId });
            modelBuilder.Entity<CarouselText>()
                .HasKey(_ => new { _.CarouselId, _.LanguageId });
            modelBuilder.Entity<CategoryText>()
                .HasKey(_ => new { _.CategoryId, _.LanguageId });
            modelBuilder.Entity<EmediaCategory>()
                .HasKey(_ => new { _.CategoryId, _.EmediaId });
            modelBuilder.Entity<EmediaText>()
                .HasKey(_ => new { _.EmediaId, _.LanguageId });
            modelBuilder.Entity<LocationInteriorImageAltText>()
                .HasKey(_ => new { _.LocationInteriorImageId, _.LanguageId });
            modelBuilder.Entity<ImageFeatureItemText>()
                .HasKey(_ => new { _.LanguageId, _.ImageFeatureItemId });
            modelBuilder.Entity<LocationFeature>()
                .HasKey(_ => new { _.FeatureId, _.LocationId });
            modelBuilder.Entity<LocationForm>()
                .HasKey(_ => new { _.LocationId, _.FormId });
            modelBuilder.Entity<LocationGroup>()
                .HasKey(_ => new { _.GroupId, _.LocationId });
            modelBuilder.Entity<LocationHours>()
                .HasKey(_ => new { _.DayOfWeek, _.LocationId });
            modelBuilder.Entity<NavigationText>()
                .HasKey(_ => new { _.NavigationId, _.LanguageId });
            modelBuilder.Entity<Page>()
                .HasKey(_ => new { _.LanguageId, _.PageHeaderId });
            modelBuilder.Entity<PageLayoutText>()
                .HasKey(_ => new { _.PageLayoutId, _.LanguageId });
            modelBuilder.Entity<PodcastDirectoryInfo>()
                .HasKey(_ => new { _.PodcastId, _.PodcastDirectoryId });
            modelBuilder.Entity<ProductLocationInventory>()
                .HasKey(_ => new { _.ProductId, _.LocationId });
            modelBuilder.Entity<ScheduleRequestLimit>()
                .HasKey(_ => new { _.DayOfWeek, _.Hour });
            modelBuilder.Entity<ScheduleRequestSubjectText>()
                .HasKey(_ => new { _.ScheduleRequestSubjectId, _.LanguageId });
            modelBuilder.Entity<SegmentText>()
                .HasKey(_ => new { _.LanguageId, _.SegmentId });

            modelBuilder.Entity<VolunteerForm>()
                .Property(_ => _.VolunteerFormType)
                .HasConversion<int>();
        }

        #region IMigratableContext

        public new string GetCurrentMigration() => Database.GetAppliedMigrations().Last();

        public new IEnumerable<string> GetPendingMigrationList() => Database.GetPendingMigrations();

        public new void Migrate() => Database.Migrate();

        #endregion IMigratableContext
    }
}