using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Models.Defaults
{
    public static class DefaultSiteSettings
    {
        public static IEnumerable<SiteSetting> SiteSettings { get; } = new SiteSetting[]
        {
            new SiteSetting
            {
                Key = "EmailService.FromAddress",
                Name = "From Address",
                Description = "The email address displayed on outgoing emails.",
                Category = "Email Service",
                Value = "staff@mcldaz.org",
                Type = SiteSettingType.String
            },
            new SiteSetting
            {
                Key = "FileUpload.MaxFileSize",
                Name = "Maximum File Size",
                Description = "The maximum file size for uploading files.",
                Category = "File Uploads",
                Value = "2096000",
                Type = SiteSettingType.Int
            },
            new SiteSetting
            {
                Key = "ModelState.TimeOutMinutes",
                Name = "Time Out Minutes",
                Description = "ModelState time out for invalid POST requests",
                Category = "ModelState",
                Value = "2",
                Type =  SiteSettingType.Int
            },
            new SiteSetting
            {
                Key = "Pagination.ItemsPerPage",
                Name = "Items Per Page",
                Description = "The number of items displayed on each page.",
                Category = "Pagination",
                Value = "10",
                Type = SiteSettingType.Int
            }
        };
    }
}
