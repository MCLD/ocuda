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
        protected OpsContext(DbContextOptions options) : base(options) { }

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
            modelBuilder.Entity<EmailSetupText>()
                .HasKey(_ => new { _.EmailSetupId, _.PromenadeLanguageName });
            modelBuilder.Entity<EmailTemplateText>()
                .HasKey(_ => new { _.EmailTemplateId, _.PromenadeLanguageName });
            modelBuilder.Entity<FileLibraryFileType>()
                .HasKey(_ => new { _.FileLibraryId, _.FileTypeId });
            modelBuilder.Entity<UserMetadata>()
                .HasKey(_ => new { _.UserId, _.UserMetadataTypeId });
            modelBuilder.Entity<SectionCategory>()
                .HasKey(_ => new { _.SectionId, _.CategoryId });
            modelBuilder.Entity<PostCategory>()
                .HasKey(_ => new { _.PostId, _.CategoryId });
        }

        #region IMigratableContext
        public new void Migrate() => Database.Migrate();
        public new IEnumerable<string> GetPendingMigrationList() => Database.GetPendingMigrations();
        public new string GetCurrentMigration() => Database.GetAppliedMigrations().Last();
        #endregion IMigratableContext

        public DbSet<ClaimGroup> ClaimGroups { get; set; }
        public DbSet<CoverIssueDetail> CoverIssueDetails { get; set; }
        public DbSet<CoverIssueHeader> CoverIssueHeaders { get; set; }
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
        public DbSet<EmailRecord> EmailRecords { get; set; }
        public DbSet<EmailSetup> EmailSetups { get; set; }
        public DbSet<EmailSetupText> EmailSetupTexts { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }
        public DbSet<EmailTemplateText> EmailTemplateTexts { get; set; }
        public DbSet<ExternalResource> ExternalResources { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<FileLibrary> FileLibraries { get; set; }
        public DbSet<FileLibraryFileType> FileLibraryFileTypes { get; set; }
        public DbSet<FileType> FileTypes { get; set; }
        public DbSet<Link> Links { get; set; }
        public DbSet<LinkLibrary> LinkLibraries { get; set; }
        public DbSet<PostCategory> PostCategories { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<RosterHeader> RosterHeaders { get; set; }
        public DbSet<RosterDetail> RosterDetails { get; set; }
        public DbSet<ScheduleClaim> ScheduleClaims { get; set; }
        public DbSet<ScheduleLog> ScheduleLogs { get; set; }
        public DbSet<ScheduleLogCallDisposition> ScheduleLogCallDispositions { get; set; }
        public DbSet<SectionCategory> SectionCategories { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<SectionManagerGroup> SectionManagerGroups { get; set; }
        public DbSet<SiteSetting> SiteSettings { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserMetadata> UserMetadata { get; set; }
        public DbSet<UserMetadataType> UserMetadataTypes { get; set; }
    }
}
