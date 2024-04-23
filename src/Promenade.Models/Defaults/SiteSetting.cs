using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Promenade.Models.Defaults
{
    public static class SiteSettings
    {
        public static IEnumerable<SiteSetting> Get { get; } = new[]
        {
            #region Contact

            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.Contact),
                Description = "Link for 'Contact Us' text in the site footer",
                Id = Keys.SiteSetting.Contact.Link,
                Name = "Contact Us link",
                Type = SiteSettingType.StringNullable
            },
            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.Contact),
                Description = "Telephone number for the site footer",
                Id = Keys.SiteSetting.Contact.Telephone,
                Name = "Telephone number",
                Type = SiteSettingType.StringNullable
            },

            #endregion Contact

            #region Scheduling

            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.Scheduling),
                Description = "Number of hours scheduling is available (decimals allowed)",
                Id = Keys.SiteSetting.Scheduling.AvailableHours,
                Name = "Available hours",
                Type = SiteSettingType.Double,
                Value = "7.5"
            },
            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.Scheduling),
                Description = "Limit scheduling to this many hours after the current time (decimals allowed)",
                Id = Keys.SiteSetting.Scheduling.BufferHours,
                Name = "Buffer hours",
                Type = SiteSettingType.Double,
                Value = "4"
            },
            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.Scheduling),
                Description = "Set to enable scheduling",
                Id = Keys.SiteSetting.Scheduling.Enable,
                Name = "Scheduling enabled",
                Type = SiteSettingType.Bool,
                Value = "False"
            },
            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.Scheduling),
                Description = "Segment to show if scheduling is enabled",
                Id = Keys.SiteSetting.Scheduling.EnabledSegment,
                Name = "Enabled scheduling segment",
                Type = SiteSettingType.Int,
                Value = "-1"
            },
            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.Scheduling),
                Description = "Segment to show if scheduling is disabled",
                Id = Keys.SiteSetting.Scheduling.DisabledSegment,
                Name = "Disabled scheduling segment",
                Type = SiteSettingType.Int,
                Value = "-1"
            },
            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.Scheduling),
                Description = "Segment to show if selected timeslot is over the limit",
                Id = Keys.SiteSetting.Scheduling.OverLimitSegment,
                Name = "Over Limit scheduling segment",
                Type = SiteSettingType.Int,
                Value = "-1"
            },
            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.Scheduling),
                Description = "Segment to show if scheduling is successful",
                Id = Keys.SiteSetting.Scheduling.ScheduledSegment,
                Name = "Scheduled explanation segment",
                Type = SiteSettingType.Int,
                Value = "-1"
            },
            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.Scheduling),
                Description = "Starting hour scheduling is available, decimals allowed",
                Id = Keys.SiteSetting.Scheduling.StartHour,
                Name = "Start hour",
                Type = SiteSettingType.Double,
                Value = "9"
            },
            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.Scheduling),
                Description = "Days of the week scheduling is unavailable, comma delimited",
                Id = Keys.SiteSetting.Scheduling.UnavailableDays,
                Name = "Unavailable days",
                Type = SiteSettingType.StringNullable,
                Value = "Sunday,Saturday"
            },

            #endregion Scheduling

            #region Site

            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.Site),
                Description = "URL to the banner image at the top of the page",
                Id = Keys.SiteSetting.Site.BannerImage,
                Name = "Banner Image",
                Type = SiteSettingType.StringNullable
            },
            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.Site),
                Description = "Alt text when a banner image is displayed",
                Id = Keys.SiteSetting.Site.BannerImageAlt,
                Name = "Banner Image Alt",
                Type = SiteSettingType.StringNullable
            },
             new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.Site),
                Description = "Google Analytics Tracking Code",
                Id = Keys.SiteSetting.Site.GoogleTrackingCode,
                Name = "Google Analytics Tracking Code",
                Type = SiteSettingType.String
            },
            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.Site),
                Description = "Is the website enabled to use TLS?",
                Id = Keys.SiteSetting.Site.IsTLS,
                Name = "TLS Enabled",
                Type = SiteSettingType.Bool,
                Value = "False"
            },
            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.Site),
                Description = "Id of the Navigation object to use in the footer",
                Id = Keys.SiteSetting.Site.NavigationIdFooter,
                Name = "Footer Navigation Id",
                Type = SiteSettingType.Int,
                Value = "-1"
            },
            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.Site),
                Description = "Id of the Navigation object to use on the left",
                Id = Keys.SiteSetting.Site.NavigationIdLeft,
                Name = "Left Navigation Id",
                Type = SiteSettingType.Int,
                Value = "-1"
            },
            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.Site),
                Description = "Id of the Navigation object to use in the middle",
                Id = Keys.SiteSetting.Site.NavigationIdMiddle,
                Name = "Middle Navigation Id",
                Type = SiteSettingType.Int,
                Value = "-1"
            },
            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.Site),
                Description = "Id of the Navigation object to use on the top",
                Id = Keys.SiteSetting.Site.NavigationIdTop,
                Name = "Top Navigation Id",
                Type = SiteSettingType.Int,
                Value = "-1"
            },
            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.Site),
                Description = "Name to append to the end of page titles",
                Id = Keys.SiteSetting.Site.PageTitleSuffix,
                Name = "Page Title Suffix",
                Type = SiteSettingType.StringNullable
            },

            #endregion Site

            #region Social

            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.Social),
                Description = "Id of a social card for the eMedia page",
                Id = Keys.SiteSetting.Social.EmediaCardId,
                Name = "eMedia social card id",
                Type = SiteSettingType.Int,
                Value = "-1"
            },
            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.Social),
                Description = "The URL to a Facebook profile",
                Id = Keys.SiteSetting.Social.FacebookUrl,
                Name = "Facebook URL",
                Type = SiteSettingType.StringNullable
            },
            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.Social),
                Description = "The URL to an Instagram profile",
                Id = Keys.SiteSetting.Social.InstagramUrl,
                Name = "Instagram URL",
                Type = SiteSettingType.StringNullable
            },
            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.Social),
                Description = "The Twitter @username a Twitter Card should be attributed to",
                Id = Keys.SiteSetting.Social.TwitterUsername,
                Name = "Twitter Username",
                Type = SiteSettingType.StringNullable
            },
            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.Social),
                Description = "The URL to a TikTok profile",
                Id = Keys.SiteSetting.Social.TikTokUrl,
                Name = "TikTok URL",
                Type = SiteSettingType.StringNullable
            },
            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.Social),
                Description = "The URL to a Twitter profile",
                Id = Keys.SiteSetting.Social.TwitterUrl,
                Name = "Twitter URL",
                Type = SiteSettingType.StringNullable
            },
            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.Social),
                Description = "The URL to a Youtube page",
                Id = Keys.SiteSetting.Social.YoutubeUrl,
                Name = "Youtube URL",
                Type = SiteSettingType.StringNullable
            },

            #endregion Social
        };
    }
}