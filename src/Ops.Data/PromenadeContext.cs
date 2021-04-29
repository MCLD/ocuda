﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Data
{
    public abstract class PromenadeContext
        : Utility.Data.DbContextBase, IMigratableContext, IDataProtectionKeyContext
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

            // configure composite keys
            // https://docs.microsoft.com/en-us/ef/core/modeling/keys
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
            modelBuilder.Entity<PageFeatureItemText>()
                .HasKey(_ => new { _.PageFeatureItemId, _.LanguageId });
            modelBuilder.Entity<PageLayoutText>()
                .HasKey(_ => new { _.PageLayoutId, _.LanguageId });
            modelBuilder.Entity<PodcastDirectoryInfo>()
                .HasKey(_ => new { _.PodcastId, _.PodcastDirectoryId });
            modelBuilder.Entity<ScheduleRequestLimit>()
                .HasKey(_ => new { _.DayOfWeek, _.Hour });
            modelBuilder.Entity<SegmentText>()
                .HasKey(_ => new { _.LanguageId, _.SegmentId });
            modelBuilder.Entity<WebslideItemText>()
                .HasKey(_ => new { _.LanguageId, _.WebslideItemId });
        }

        #region IMigratableContext
        public new void Migrate() => Database.Migrate();
        public new IEnumerable<string> GetPendingMigrationList() => Database.GetPendingMigrations();
        public new string GetCurrentMigration() => Database.GetAppliedMigrations().Last();
        #endregion IMigratableContext

        public DbSet<CarouselButtonLabel> CarouselButtonLabels { get; set; }
        public DbSet<CarouselButton> CarouselButtons { get; set; }
        public DbSet<CarouselButtonLabelText> CarouselButtonLabelTexts { get; set; }
        public DbSet<CarouselItem> CarouselItems { get; set; }
        public DbSet<CarouselItemText> CarouselItemTexts { get; set; }
        public DbSet<Carousel> Carousels { get; set; }
        public DbSet<CarouselTemplate> CarouselTemplates { get; set; }
        public DbSet<CarouselText> CarouselTexts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<CategoryText> CategoryTexts { get; set; }
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
        public DbSet<Emedia> Emedia { get; set; }
        public DbSet<EmediaCategory> EmediaCategories { get; set; }
        public DbSet<EmediaGroup> EmediaGroups { get; set; }
        public DbSet<EmediaText> EmediaTexts { get; set; }
        public DbSet<ExternalResource> ExternalResources { get; set; }
        public DbSet<Feature> Features { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<LocationFeature> LocationFeatures { get; set; }
        public DbSet<LocationGroup> LocationGroups { get; set; }
        public DbSet<LocationHours> LocationHours { get; set; }
        public DbSet<LocationHoursOverride> LocationHoursOverrides { get; set; }
        public DbSet<Navigation> Navigations { get; set; }
        public DbSet<NavigationText> NavigationTexts { get; set; }
        public DbSet<PageFeatureItem> PageFeatureItems { get; set; }
        public DbSet<PageFeatureItemText> PageFeatureItemTexts { get; set; }
        public DbSet<PageFeature> PageFeatures { get; set; }
        public DbSet<PageFeatureTemplate> PageFeatureTemplates { get; set; }
        public DbSet<PageItem> PageItems { get; set; }
        public DbSet<PageLayout> PageLayouts { get; set; }
        public DbSet<PageLayoutText> PageLayoutTexts { get; set; }
        public DbSet<Page> Pages { get; set; }
        public DbSet<PageHeader> PageHeaders { get; set; }
        public DbSet<PodcastDirectory> PodcastDirectories { get; set; }
        public DbSet<PodcastDirectoryInfo> PodcastDirectoryInfos { get; set; }
        public DbSet<PodcastItem> PodcastItems { get; set; }
        public DbSet<Podcast> Podcasts { get; set; }
        public DbSet<ScheduleRequest> ScheduleRequest { get; set; }
        public DbSet<ScheduleRequestLimit> ScheduleRequestLimits { get; set; }
        public DbSet<ScheduleRequestSubject> ScheduleRequestSubject { get; set; }
        public DbSet<Segment> Segments { get; set; }
        public DbSet<SegmentText> SegmentTexts { get; set; }
        public DbSet<SiteSetting> SiteSettings { get; set; }
        public DbSet<SocialCard> SocialCards { get; set; }
        public DbSet<UrlRedirect> UrlRedirects { get; set; }
        public DbSet<UrlRedirectAccess> UrlRedirectAccesses { get; set; }
        public DbSet<WebslideItem> WebslideItems { get; set; }
        public DbSet<WebslideItemText> WebslideItemTexts { get; set; }
        public DbSet<Webslide> Webslides { get; set; }
        public DbSet<WebslideTemplate> WebslideTemplates { get; set; }
    }
}
