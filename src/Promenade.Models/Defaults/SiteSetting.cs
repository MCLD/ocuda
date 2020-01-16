using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Promenade.Models.Defaults
{
    public static class SiteSettings
    {
        public static IEnumerable<SiteSetting> Get { get; } = new[]
        {
            #region Social
            new SiteSetting
            {
                Id = Keys.SiteSetting.Social.TwitterUsername,
                Name = "Twitter Username",
                Description = "The Twitter @username a Twitter Card should be attributed to",
                Category = "Social",
                Value = string.Empty,
                Type = SiteSettingType.StringNullable
            },
            #endregion
            #region Site
            new SiteSetting
            {
                Id = Keys.SiteSetting.Site.BannerImage,
                Name = "Banner Image",
                Description = "URL to the banner image at the top of the page",
                Category="Site",
                Type = SiteSettingType.StringNullable
            },
            new SiteSetting
            {
                Id = Keys.SiteSetting.Site.BannerImageAlt,
                Name = "Banner Image Alt",
                Description = "Alt text when a banner image is displayed",
                Category = "Site",
                Type = SiteSettingType.StringNullable
            },
             new SiteSetting
            {
                Id = Keys.SiteSetting.Site.GoogleTrackingCode,
                Name = "Google Analytics Tracking Code",
                Description = "Google Analytics Tracking Code",
                Category = "Site",
                Type = SiteSettingType.String
            },
            new SiteSetting
            {
                Id = Keys.SiteSetting.Site.IsTLS,
                Name = "TLS Enabled",
                Description = "Is the website enabled to use TLS?",
                Category = "Site",
                Value = "False",
                Type = SiteSettingType.Bool
            },
            new SiteSetting
            {
                Id = Keys.SiteSetting.Site.NavigationIdFooter,
                Name = "Footer Navigation Id",
                Description = "Id of the Navigation object to use in the footer",
                Category = "Site",
                Type = SiteSettingType.Int
            },
            new SiteSetting
            {
                Id = Keys.SiteSetting.Site.NavigationIdLeft,
                Name = "Left Navigation Id",
                Description = "Id of the Navigation object to use on the left",
                Category = "Site",
                Type = SiteSettingType.Int
            },
            new SiteSetting
            {
                Id = Keys.SiteSetting.Site.NavigationIdMiddle,
                Name = "Middle Navigation Id",
                Description = "Id of the Navigation object to use in the middle",
                Category = "Site",
                Type = SiteSettingType.Int
            },
            new SiteSetting
            {
                Id = Keys.SiteSetting.Site.NavigationIdTop,
                Name = "Top Navigation Id",
                Description = "Id of the Navigation object to use on the top",
                Category = "Site",
                Type = SiteSettingType.Int
            },
            new SiteSetting
            {
                Id = Keys.SiteSetting.Site.PageTitleSuffix,
                Name = "Page Title Suffix",
                Description = "Name to append to the end of page titles",
                Category = "Site",
                Type = SiteSettingType.StringNullable
            }
            #endregion
        };
    }
}
