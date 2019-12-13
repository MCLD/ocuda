using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Models.Defaults
{
    public static class SiteSettings
    {
        public static IEnumerable<SiteSetting> Get { get; } = new[]
        {
            #region Social
            new SiteSetting
            {
                Key = Keys.SiteSetting.Social.TwitterSite,
                Name = "Twitter Site",
                Description = "@username for the website used in the Twitter card footer.",
                Category = "Social",
                Value = string.Empty,
                Type = SiteSettingType.StringNullable
            },
            #endregion
            #region Site
            new SiteSetting
            {
                Key = Keys.SiteSetting.Site.BannerImage,
                Name = "Banner Image",
                Description = "URL to the banner image at the top of the page",
                Category="Site",
                Type = SiteSettingType.StringNullable
            },
            new SiteSetting
            {
                Key = Keys.SiteSetting.Site.BannerImageAlt,
                Name = "Banner Image Alt",
                Description = "Alt text when a banner image is displayed",
                Category = "Site",
                Type = SiteSettingType.StringNullable
            },
            new SiteSetting
            {
                Key = Keys.SiteSetting.Site.IsTLS,
                Name = "TLS Enabled",
                Description = "Is the website enabled to use TLS?",
                Category = "Site",
                Value = "False",
                Type = SiteSettingType.Bool
            },
            new SiteSetting
            {
                Key = Keys.SiteSetting.Site.NavigationIdFooter,
                Name = "Footer Navigation Id",
                Description = "Id of the Navigation object to use in the footer",
                Category = "Site",
                Type = SiteSettingType.Int
            },
            new SiteSetting
            {
                Key = Keys.SiteSetting.Site.NavigationIdLeft,
                Name = "Left Navigation Id",
                Description = "Id of the Navigation object to use on the left",
                Category = "Site",
                Type = SiteSettingType.Int
            },
            new SiteSetting
            {
                Key = Keys.SiteSetting.Site.NavigationIdMiddle,
                Name = "Middle Navigation Id",
                Description = "Id of the Navigation object to use in the middle",
                Category = "Site",
                Type = SiteSettingType.Int
            },
            new SiteSetting
            {
                Key = Keys.SiteSetting.Site.NavigationIdTop,
                Name = "Top Navigation Id",
                Description = "Id of the Navigation object to use on the top",
                Category = "Site",
                Type = SiteSettingType.Int
            }
            #endregion
        };
    }
}
