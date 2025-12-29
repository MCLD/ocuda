using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Data
{
    public abstract class PromenadeContext : Utility.Data.DbContextBase, IDataProtectionKeyContext
    {
        protected PromenadeContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<CardDetail> CardDetails { get; }
        public DbSet<CardRenewalRequest> CardRenewalRequests { get; set; }
        public DbSet<Card> Cards { get; }
        public DbSet<CarouselButtonLabel> CarouselButtonLabels { get; }
        public DbSet<CarouselButtonLabelText> CarouselButtonLabelTexts { get; }
        public DbSet<CarouselButton> CarouselButtons { get; }
        public DbSet<CarouselItem> CarouselItems { get; }
        public DbSet<CarouselItemText> CarouselItemTexts { get; }
        public DbSet<Carousel> Carousels { get; }
        public DbSet<CarouselTemplate> CarouselTemplates { get; }
        public DbSet<CarouselText> CarouselTexts { get; }
        public DbSet<Category> Categories { get; }
        public DbSet<CategoryText> CategoryTexts { get; }
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
        public DbSet<Deck> Decks { get; }
        public DbSet<Emedia> Emedia { get; set; }
        public DbSet<EmediaCategory> EmediaCategories { get; set; }
        public DbSet<EmediaGroup> EmediaGroups { get; }
        public DbSet<EmediaText> EmediaTexts { get; }
        public DbSet<EmployeeCardRequest> EmployeeCardRequests { get; }
        public DbSet<ExternalResource> ExternalResources { get; set; }
        public DbSet<Feature> Features { get; }
        public DbSet<Group> Groups { get; }
        public DbSet<ImageFeatureItem> ImageFeatureItems { get; }
        public DbSet<ImageFeatureItemText> ImageFeatureItemTexts { get; }
        public DbSet<ImageFeature> ImageFeatures { get; }
        public DbSet<ImageFeatureTemplate> ImageFeatureTemplates { get; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<LocationFeature> LocationFeatures { get; }
        public DbSet<LocationForm> LocationForms { get; }
        public DbSet<LocationGroup> LocationGroups { get; }
        public DbSet<LocationHours> LocationHours { get; }
        public DbSet<LocationHoursOverride> LocationHoursOverrides { get; }
        public DbSet<LocationInteriorImageAltText> LocationInteriorImageAltTexts { get; }
        public DbSet<LocationInteriorImage> LocationInteriorImages { get; }
        public DbSet<LocationProductMap> LocationProductMaps { get; }
        public DbSet<Location> Locations { get; }
        public DbSet<NavBannerImage> NavBannerImages { get; }
        public DbSet<NavBannerLink> NavBannerLinks { get; }
        public DbSet<NavBannerLinkText> NavBannerLinkTexts { get; }
        public DbSet<NavBanner> NavBanners { get; }
        public DbSet<Navigation> Navigations { get; }
        public DbSet<NavigationText> NavigationTexts { get; }
        public DbSet<PageHeader> PageHeaders { get; set; }
        public DbSet<PageItem> PageItems { get; set; }
        public DbSet<PageLayout> PageLayouts { get; set; }
        public DbSet<PageLayoutText> PageLayoutTexts { get; }
        public DbSet<Page> Pages { get; }
        public DbSet<PodcastDirectory> PodcastDirectories { get; }
        public DbSet<PodcastDirectoryInfo> PodcastDirectoryInfos { get; set; }
        public DbSet<PodcastItem> PodcastItems { get; }
        public DbSet<Podcast> Podcasts { get; }
        public DbSet<ProductLocationInventory> ProductLocationInventories { get; }
        public DbSet<Product> Products { get; }
        public DbSet<ScheduleRequest> ScheduleRequest { get; set; }
        public DbSet<ScheduleRequestLimit> ScheduleRequestLimits { get; }
        public DbSet<ScheduleRequestSubject> ScheduleRequestSubject { get; }
        public DbSet<ScheduleRequestSubjectText> ScheduleRequestSubjectTexts { get; }
        public DbSet<Segment> Segments { get; }
        public DbSet<SegmentText> SegmentTexts { get; }
        public DbSet<SiteSetting> SiteSettings { get; }
        public DbSet<SocialCard> SocialCards { get; }
        public DbSet<UrlRedirectAccess> UrlRedirectAccesses { get; set; }
        public DbSet<UrlRedirect> UrlRedirects { get; }
        public DbSet<VolunteerForm> VolunteerForms { get; }
        public DbSet<VolunteerFormSubmission> VolunteerFormSubmissions { get; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new System.ArgumentNullException(nameof(modelBuilder));
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
            modelBuilder.Entity<NavBannerImage>()
                .HasKey(_ => new { _.NavBannerId, _.LanguageId });
            modelBuilder.Entity<NavBannerLinkText>()
                .HasKey(_ => new { _.NavBannerLinkId, _.LanguageId });
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
                .HasKey(_ => new { _.LanguageId, _.ScheduleRequestSubjectId });
            modelBuilder.Entity<SegmentText>()
                .HasKey(_ => new { _.LanguageId, _.SegmentId });
        }
    }
}