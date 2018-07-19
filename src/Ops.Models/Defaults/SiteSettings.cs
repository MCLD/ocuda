using System.Collections.Generic;
namespace Ocuda.Ops.Models.Defaults
{
    public static class SiteSettings
    {
        public static IEnumerable<SiteSetting> Get { get; } = new SiteSetting[]
        {
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
                Key = Keys.SiteSetting.FileManagement.MaxUploadBytes,
                Name = "Maximum file size",
                Description = "Maximum file upload size (in bytes)",
                Category = "File Management",
                Value = "2097152",
                Type = SiteSettingType.Int
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
        };
    }
}
