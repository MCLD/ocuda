using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Models.Defaults
{
    public static class SiteSettings
    {
        public static IEnumerable<SiteSetting> Get { get; } = new[]
        {
            #region Carousel

            new SiteSetting
            {
                Id = Keys.SiteSetting.Carousel.ImageRestrictToDomains,
                Name = "Image restrict to domain",
                Description = "Restrict carousel images to only these domains, comma delimited",
                Category = "Carousel",
                Value = "",
                Type = SiteSettingType.StringNullable
            },
            new SiteSetting
            {
                Id = Keys.SiteSetting.Carousel.LinkRestrictToDomains,
                Name = "Link restrict to domain",
                Description = "Restrict carousel links to only these domains, comma delimited",
                Category = "Carousel",
                Value = "",
                Type = SiteSettingType.StringNullable
            },

            #endregion Carousel

            #region CoverIssueReporting

            new SiteSetting
            {
                Id = Keys.SiteSetting.CoverIssueReporting.LeapBibUrl,
                Name = "Leap bib records url",
                Description = "Leap bib records url with scheme, host and path",
                Category = "Cover Issue Reporting",
                Value = "",
                Type = SiteSettingType.StringNullable
            },

            #endregion CoverIssueReporting

            #region Email

            new SiteSetting{
                Id = Keys.SiteSetting.Email.AdminAddress,
                Name = "Email address of the intranet site administrator",
                Description = "Email address in case staff has questions/poblems",
                Category = "Email",
                Value = "",
                Type = SiteSettingType.StringNullable
            },
            new SiteSetting
            {
                Id = Keys.SiteSetting.Email.FromAddress,
                Name = "Email from address",
                Description = "Email address that outgoing system mails are from",
                Category = "Email",
                Value = "",
                Type = SiteSettingType.StringNullable
            },
            new SiteSetting
            {
                Id = Keys.SiteSetting.Email.FromName,
                Name = "Email from name",
                Description = "Name that outgoing system mails are from",
                Category = "Email",
                Value = "",
                Type = SiteSettingType.StringNullable
            },
            new SiteSetting
            {
                Id = Keys.SiteSetting.Email.OutgoingHost,
                Name = "Email outgoing host",
                Description = "Outgoing host name for emails",
                Category = "Email",
                Value = "",
                Type = SiteSettingType.StringNullable
            },
            new SiteSetting
            {
                Id = Keys.SiteSetting.Email.OutgoingLogin,
                Name = "Email outgoing login",
                Description = "Login name for the outgoing host",
                Category = "Email",
                Value = "",
                Type = SiteSettingType.StringNullable
            },
            new SiteSetting
            {
                Id = Keys.SiteSetting.Email.OutgoingPassword,
                Name = "Email outgoing password",
                Description = "Password for the outgoing host",
                Category = "Email",
                Value = "",
                Type = SiteSettingType.StringNullable
            },
            new SiteSetting
            {
                Id = Keys.SiteSetting.Email.OutgoingPort,
                Name = "Email outgoing port",
                Description = "Port used for outgoing emails",
                Category = "Email",
                Value = "25",
                Type = SiteSettingType.Int
            },
            new SiteSetting
            {
                Id = Keys.SiteSetting.Email.OverrideToAddress,
                Name = "Override to email",
                Description = "Override all outgoing emails and send to this address",
                Category = "Email",
                Value = "",
                Type = SiteSettingType.StringNullable
            },
            new SiteSetting
            {
                Id = Keys.SiteSetting.Email.BccAddress,
                Name = "BCC address",
                Description = "BCC all outgoing emails to this address",
                Category = "Email",
                Value = "",
                Type = SiteSettingType.StringNullable
            },
            new SiteSetting
            {
                Id = Keys.SiteSetting.Email.RestrictToDomain,
                Name = "Restrict to domain",
                Description = "Restrict all outgoing emails to only addresses @ this domain",
                Category = "Email",
                Value = "",
                Type = SiteSettingType.StringNullable
            },

            #endregion Email

            #region FileManagement

            new SiteSetting
            {
                Id = Keys.SiteSetting.FileManagement.MaxUploadBytes,
                Name = "Maximum file size",
                Description = "Maximum file upload size (in bytes)",
                Category = "File Management",
                Value = "2097152",
                Type = SiteSettingType.Int
            },

            #endregion FileManagement

            #region SiteManagement

            new SiteSetting
            {
                Id = Keys.SiteSetting.SiteManagement.PromenadePublicPath,
                Name = "Promenade Public Path",
                Description = "Drive path to the Promenade 'public' folder",
                Category = "Site Management",
                Value = "",
                Type = SiteSettingType.StringNullable
            },
            new SiteSetting
            {
                Id = Keys.SiteSetting.SiteManagement.PromenadeUrl,
                Name = "Promenade Url",
                Description = "Promenade url with scheme and host",
                Category = "Site Management",
                Value = "",
                Type = SiteSettingType.StringNullable
            },

            #endregion SiteManagement

            #region UserInterface

            new SiteSetting
            {
                Id = Keys.SiteSetting.FileManagement.MaxThumbnailCount,
                Name = "Maximum Thumbnail Count",
                Description = "Maximum number of thumbnails that can be attached to a file",
                Category = "File Management",
                Value = "4",
                Type = SiteSettingType.Int
            },
            new SiteSetting
            {
                Id = Keys.SiteSetting.FileManagement.ThumbnailTypes,
                Name = "Thumbnail Types",
                Description = "Comma separated list of acceptable file type extensions for thumbnails",
                Category = "File Management",
                Value = ".jpg,.png",
                Type = SiteSettingType.String
            },
            new SiteSetting
            {
                Id = Keys.SiteSetting.UserInterface.ModelStateTimeoutMinutes,
                Name = "Web request validation timeout",
                Description = "Timeout for submitted pages to perform validation",
                Category = "User Interface",
                Value = "2",
                Type =  SiteSettingType.Int
            },
            new SiteSetting
            {
                Id = Keys.SiteSetting.UserInterface.ItemsPerPage,
                Name = "Items per page",
                Description = "Items shown on each page for pagination",
                Category = "User Interface",
                Value = "10",
                Type = SiteSettingType.Int
            }

            #endregion UserInterface
        };
    }
}