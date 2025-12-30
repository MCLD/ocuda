using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ocuda.Ops.Models.Entities;
using Category = Ocuda.Ops.Models.Entities.Category;

namespace Ocuda.Ops.Data
{
    public abstract class OpsContext
        : Utility.Data.DbContextBase, IMigratableContext, IDataProtectionKeyContext
    {
        protected OpsContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<ApiKey> ApiKeys { get; set; }
        public DbSet<CardRenewalResponse> CardRenewalResponses { get; set; }
        public DbSet<CardRenewalResult> CardRenewalResults { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ClaimGroup> ClaimGroups { get; set; }
        public DbSet<CoverIssueDetail> CoverIssueDetails { get; set; }
        public DbSet<CoverIssueHeader> CoverIssueHeaders { get; set; }
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
        public DbSet<DigitalDisplayAsset> DigitalDisplayAssets { get; set; }
        public DbSet<DigitalDisplayAssetSet> DigitalDisplayAssetSets { get; set; }
        public DbSet<DigitalDisplayDisplaySet> DigitalDisplayDisplaySets { get; set; }
        public DbSet<DigitalDisplayItem> DigitalDisplayItems { get; set; }
        public DbSet<DigitalDisplay> DigitalDisplays { get; set; }
        public DbSet<DigitalDisplaySet> DigitalDisplaySets { get; set; }
        public DbSet<EmailRecord> EmailRecords { get; set; }
        public DbSet<EmailSetup> EmailSetups { get; set; }
        public DbSet<EmailSetupText> EmailSetupTexts { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }
        public DbSet<EmailTemplateText> EmailTemplateTexts { get; set; }
        public DbSet<ExternalResource> ExternalResources { get; set; }
        public DbSet<FileLibrary> FileLibraries { get; set; }
        public DbSet<FileLibraryFileType> FileLibraryFileTypes { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<FileType> FileTypes { get; set; }
        public DbSet<HistoricalIncident> HistoricalIncidents { get; set; }
        public DbSet<IdentityProvider> IdentityProviders { get; set; }
        public DbSet<IncidentFollowup> IncidentFollowups { get; set; }
        public DbSet<IncidentParticipant> IncidentParticipants { get; set; }
        public DbSet<IncidentRelationship> IncidentRelationships { get; set; }
        public DbSet<Incident> Incidents { get; set; }
        public DbSet<IncidentStaff> IncidentStaffs { get; set; }
        public DbSet<IncidentType> IncidentTypes { get; set; }
        public DbSet<LinkLibrary> LinkLibraries { get; set; }
        public DbSet<Link> Links { get; set; }
        public DbSet<PermissionGroupApplication> PermissionGroupApplication { get; set; }
        public DbSet<PermissionGroupIncidentLocation> PermissionGroupIncidentLocations { get; set; }
        public DbSet<PermissionGroupPageContent> PermissionGroupPageContents { get; set; }
        public DbSet<PermissionGroupPodcastItem> PermissionGroupPodcastItems { get; set; }
        public DbSet<PermissionGroupProductManager> PermissionGroupProductManager { get; set; }
        public DbSet<PermissionGroupReplaceFiles> PermissionGroupReplaceFiles { get; set; }
        public DbSet<PermissionGroup> PermissionGroups { get; set; }
        public DbSet<PermissionGroupSectionManager> PermissionGroupSectionManager { get; set; }
        public DbSet<PostCategory> PostCategories { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<RosterDetail> RosterDetails { get; set; }
        public DbSet<RosterDivision> RosterDivisions { get; set; }
        public DbSet<RosterHeader> RosterHeaders { get; set; }
        public DbSet<RosterLocation> RosterLocations { get; set; }
        public DbSet<ScheduleClaim> ScheduleClaims { get; set; }
        public DbSet<ScheduleLogCallDisposition> ScheduleLogCallDispositions { get; set; }
        public DbSet<ScheduleLog> ScheduleLogs { get; set; }
        public DbSet<SectionCategory> SectionCategories { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<SiteSetting> SiteSettings { get; set; }
        public DbSet<TitleClass> TitleClasses { get; set; }
        public DbSet<TitleClassMapping> TitleClassMappings { get; set; }
        public DbSet<UserMetadata> UserMetadata { get; set; }
        public DbSet<UserMetadataType> UserMetadataTypes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserSyncHistory> UserSyncHistories { get; set; }
        public DbSet<UserSyncLocation> UserSyncLocations { get; set; }
        public DbSet<VolunteerFormSubmissionEmailRecord> VolunteerFormSubmissionEmailRecords { get; set; }
        public DbSet<VolunteerFormUserMapping> VolunteerFormUserMappings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);

            base.OnModelCreating(modelBuilder);

            // configure composite keys
            // https://docs.microsoft.com/en-us/ef/core/modeling/keys
            modelBuilder.Entity<DigitalDisplayAssetSet>()
                .HasKey(_ => new { _.DigitalDisplayAssetId, _.DigitalDisplaySetId });
            modelBuilder.Entity<DigitalDisplayDisplaySet>()
                .HasKey(_ => new { _.DigitalDisplayId, _.DigitalDisplaySetId });
            modelBuilder.Entity<DigitalDisplayItem>()
                .HasKey(_ => new { _.DigitalDisplayAssetId, _.DigitalDisplayId });
            modelBuilder.Entity<EmailSetupText>()
                .HasKey(_ => new { _.EmailSetupId, _.PromenadeLanguageName });
            modelBuilder.Entity<EmailTemplateText>()
                .HasKey(_ => new { _.EmailTemplateId, _.PromenadeLanguageName });
            modelBuilder.Entity<FileLibraryFileType>()
                .HasKey(_ => new { _.FileLibraryId, _.FileTypeId });
            modelBuilder.Entity<IncidentRelationship>()
                .HasKey(_ => new { _.IncidentId, _.RelatedIncidentId });
            modelBuilder.Entity<PermissionGroupApplication>()
                .HasKey(_ => new { _.PermissionGroupId, _.ApplicationPermission });
            modelBuilder.Entity<PermissionGroupIncidentLocation>()
                .HasKey(_ => new { _.PermissionGroupId, _.LocationId });
            modelBuilder.Entity<PermissionGroupPageContent>()
                .HasKey(_ => new { _.PermissionGroupId, _.PageHeaderId });
            modelBuilder.Entity<PermissionGroupPodcastItem>()
                .HasKey(_ => new { _.PermissionGroupId, _.PodcastId });
            modelBuilder.Entity<PermissionGroupReplaceFiles>()
                .HasKey(_ => new { _.PermissionGroupId, _.FileLibraryId });
            modelBuilder.Entity<PermissionGroupProductManager>()
               .HasKey(_ => new { _.PermissionGroupId, _.ProductId });
            modelBuilder.Entity<PermissionGroupSectionManager>()
                .HasKey(_ => new { _.PermissionGroupId, _.SectionId });
            modelBuilder.Entity<PostCategory>()
                .HasKey(_ => new { _.PostId, _.CategoryId });
            modelBuilder.Entity<SectionCategory>()
                .HasKey(_ => new { _.SectionId, _.CategoryId });
            modelBuilder.Entity<TitleClassMapping>()
                .HasKey(_ => new { _.TitleClassId, _.UserTitle });
            modelBuilder.Entity<UserMetadata>()
                .HasKey(_ => new { _.UserId, _.UserMetadataTypeId });
            modelBuilder.Entity<VolunteerFormSubmissionEmailRecord>()
                .HasKey(_ => new { _.VolunterFormSubmissionId, _.EmailRecordId });
            modelBuilder.Entity<VolunteerFormUserMapping>()
                .HasKey(_ => new { _.VolunteerFormId, _.LocationId, _.UserId });
        }

        #region IMigratableContext

        public new string GetCurrentMigration() => Database.GetAppliedMigrations().Last();

        public new IEnumerable<string> GetPendingMigrationList() => Database.GetPendingMigrations();

        public new void Migrate() => Database.Migrate();

        #endregion IMigratableContext
    }
}