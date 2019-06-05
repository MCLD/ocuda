using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Models.Defaults
{
    public static class SiteSettings
    {
        public static IEnumerable<SiteSetting> Get { get; } = new[]
        {
            #region Email
            new SiteSetting
            {
                Key = Keys.SiteSetting.Email.FromAddress,
                Name = "Email from address",
                Description = "Email address that outgoing system mails are from",
                Category = "Email",
                Value = "ocuda",
                Type = SiteSettingType.String
            },
            new SiteSetting
            {
                Key = Keys.SiteSetting.Email.FromName,
                Name = "Email from name",
                Description = "Name that outgoing system mails are from",
                Category = "Email",
                Value = "ocuda",
                Type = SiteSettingType.String
            },
            new SiteSetting
            {
                Key = Keys.SiteSetting.Email.OutgoingHost,
                Name = "Email outgoing host",
                Description = "Outgoing host name for emails",
                Category = "Email",
                Value = "ocuda",
                Type = SiteSettingType.String
            },
            new SiteSetting
            {
                Key = Keys.SiteSetting.Email.OutgoingLogin,
                Name = "Email outgoing login",
                Description = "Login name for the outgoing host",
                Category = "Email",
                Value = "",
                Type = SiteSettingType.String_Emptiable
            },
            new SiteSetting
            {
                Key = Keys.SiteSetting.Email.OutgoingPassword,
                Name = "Email outgoing password",
                Description = "Password for the outgoing host",
                Category = "Email",
                Value = "",
                Type = SiteSettingType.String_Emptiable
            },
            new SiteSetting
            {
                Key = Keys.SiteSetting.Email.OutgoingPort,
                Name = "Email outgoing port",
                Description = "Port used for outgoing emails",
                Category = "Email",
                Value = "25",
                Type = SiteSettingType.Int
            },
            #endregion
            #region FileManagement
            new SiteSetting
            {
                Key = Keys.SiteSetting.FileManagement.MaxUploadBytes,
                Name = "Maximum file size",
                Description = "Maximum file upload size (in bytes)",
                Category = "File Management",
                Value = "2097152",
                Type = SiteSettingType.Int
            },
            #endregion
            #region UserInterface
            new SiteSetting
            {
                Key = Keys.SiteSetting.FileManagement.MaxThumbnailCount,
                Name = "Maximum Thumbnail Count",
                Description = "Maximum number of thumbnails that can be attached to a file",
                Category = "File Management",
                Value = "4",
                Type = SiteSettingType.Int
            },
            new SiteSetting
            {
                Key = Keys.SiteSetting.FileManagement.ThumbnailTypes,
                Name = "Thumbnail Types",
                Description = "Comma separated list of acceptable file type extensions for thumbnails",
                Category = "File Management",
                Value = ".jpg,.png",
                Type = SiteSettingType.String
            },
            new SiteSetting
            {
                Key = Keys.SiteSetting.UserInterface.ModelStateTimeoutMinutes,
                Name = "Web request validation timeout",
                Description = "Timeout for submitted pages to perform validation",
                Category = "User Interface",
                Value = "2",
                Type =  SiteSettingType.Int
            },
            new SiteSetting
            {
                Key = Keys.SiteSetting.UserInterface.ItemsPerPage,
                Name = "Items per page",
                Description = "Items shown on each page for pagination",
                Category = "User Interface",
                Value = "10",
                Type = SiteSettingType.Int
            }
            #endregion
        };
    }
}
