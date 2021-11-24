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
                Category = "Social",
                Description = "Id of a social card for the eMedia page",
                Id = Keys.SiteSetting.Social.EmediaCardId,
                Name = "eMedia social card id",
                Type = SiteSettingType.Int,
                Value = "-1"
            },
            new SiteSetting
            {
                Id = Keys.SiteSetting.Social.FacebookUrl,
                Name = "Facebook URL",
                Description = "The URL to a Facebook profile",
                Category = "Social",
                Value = string.Empty,
                Type = SiteSettingType.StringNullable
            },
            new SiteSetting
            {
                Id = Keys.SiteSetting.Social.InstagramUrl,
                Name = "Instagram URL",
                Description = "The URL to an Instagram profile",
                Category = "Social",
                Value = string.Empty,
                Type = SiteSettingType.StringNullable
            },
            new SiteSetting
            {
                Id = Keys.SiteSetting.Social.TwitterUsername,
                Name = "Twitter Username",
                Description = "The Twitter @username a Twitter Card should be attributed to",
                Category = "Social",
                Value = string.Empty,
                Type = SiteSettingType.StringNullable
            },
            new SiteSetting
            {
                Id = Keys.SiteSetting.Social.TwitterUrl,
                Name = "Twitter URL",
                Description = "The URL to a Twitter profile",
                Category = "Social",
                Value = string.Empty,
                Type = SiteSettingType.StringNullable
            },
            new SiteSetting
            {
                Id = Keys.SiteSetting.Social.YoutubeUrl,
                Name = "Youtube URL",
                Description = "The URL to a Youtube page",
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
                Type = SiteSettingType.Int,
                Value = "-1"
            },
            new SiteSetting
            {
                Id = Keys.SiteSetting.Site.NavigationIdLeft,
                Name = "Left Navigation Id",
                Description = "Id of the Navigation object to use on the left",
                Category = "Site",
                Type = SiteSettingType.Int,
                Value = "-1"
            },
            new SiteSetting
            {
                Id = Keys.SiteSetting.Site.NavigationIdMiddle,
                Name = "Middle Navigation Id",
                Description = "Id of the Navigation object to use in the middle",
                Category = "Site",
                Type = SiteSettingType.Int,
                Value = "-1"
            },
            new SiteSetting
            {
                Id = Keys.SiteSetting.Site.NavigationIdTop,
                Name = "Top Navigation Id",
                Description = "Id of the Navigation object to use on the top",
                Category = "Site",
                Type = SiteSettingType.Int,
                Value = "-1"
            },
            new SiteSetting
            {
                Id = Keys.SiteSetting.Site.PageTitleSuffix,
                Name = "Page Title Suffix",
                Description = "Name to append to the end of page titles",
                Category = "Site",
                Type = SiteSettingType.StringNullable
            },
            #endregion
            #region Scheduling
            new SiteSetting
            {
                Category = "Scheduling",
                Description = "Number of hours scheduling is available (decimals allowed)",
                Id = Keys.SiteSetting.Scheduling.AvailableHours,
                Name = "Available hours",
                Type = SiteSettingType.Double,
                Value = "7.5"
            },
            new SiteSetting
            {
                Category = "Scheduling",
                Description = "Limit scheduling to this many hours after the current time (decimals allowed)",
                Id = Keys.SiteSetting.Scheduling.BufferHours,
                Name = "Buffer hours",
                Type = SiteSettingType.Double,
                Value = "4"
            },
            new SiteSetting
            {
                Category = "Scheduling",
                Description = "Set to enable scheduling",
                Id = Keys.SiteSetting.Scheduling.Enable,
                Name = "Scheduling enabled",
                Type = SiteSettingType.Bool,
                Value = "False"
            },
            new SiteSetting
            {
                Category = "Scheduling",
                Description = "Segment to show if scheduling is enabled",
                Id = Keys.SiteSetting.Scheduling.EnabledSegment,
                Name = "Enabled scheduling segment",
                Type = SiteSettingType.Int,
                Value = "-1"
            },
            new SiteSetting
            {
                Category = "Scheduling",
                Description = "Segment to show if scheduling is disabled",
                Id = Keys.SiteSetting.Scheduling.DisabledSegment,
                Name = "Disabled scheduling segment",
                Type = SiteSettingType.Int,
                Value = "-1"
            },
            new SiteSetting
            {
                Category = "Scheduling",
                Description = "Segment to show if selected timeslot is over the limit",
                Id = Keys.SiteSetting.Scheduling.OverLimitSegment,
                Name = "Over Limit scheduling segment",
                Type = SiteSettingType.Int,
                Value = "-1"
            },
            new SiteSetting
            {
                Category = "Scheduling",
                Description = "Segment to show if scheduling is successful",
                Id = Keys.SiteSetting.Scheduling.ScheduledSegment,
                Name = "Scheduled explanation segment",
                Type = SiteSettingType.Int,
                Value = "-1"
            },
            new SiteSetting
            {
                Category = "Scheduling",
                Description = "Starting hour scheduling is available, decimals allowed",
                Id = Keys.SiteSetting.Scheduling.StartHour,
                Name = "Start hour",
                Type = SiteSettingType.Double,
                Value = "9"
            },
            new SiteSetting
            {
                Category = "Scheduling",
                Description = "Days of the week scheduling is unavailable, comma delimited",
                Id = Keys.SiteSetting.Scheduling.UnavailableDays,
                Name = "Unavailable days",
                Type = SiteSettingType.StringNullable,
                Value = "Sunday,Saturday"
            }
            #endregion
        };
    }
}
