using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Promenade.Models.Defaults
{
    public static class SiteSettings
    {
        public static IEnumerable<SiteSetting> Get { get; } = new[]
        {
            #region RenewCard

            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.RenewCard),
                Description = "Accepted counties for card renewal addresses, comma delimited",
                Id = Keys.SiteSetting.RenewCard.AcceptedCounties,
                Name = "Accepted counties",
                Type = SiteSettingType.StringNullable
            },
            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.RenewCard),
                Description = "Customer codes ids to check for becoming 18 years old, comma delimited",
                Id = Keys.SiteSetting.RenewCard.AgeCheckCustomerCodes,
                Name = "Age check customer codes",
                Type = SiteSettingType.StringNullable
            },
            new SiteSetting
            {
                Category = nameof (Keys.SiteSetting.RenewCard),
                Description = "Segment to show to age check customers who have become 18",
                Id = Keys.SiteSetting.RenewCard.AgeCheckSegment,
                Name = "Age check segment",
                Type = SiteSettingType.Int,
                Value = "-1"
            },
            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.RenewCard),
                Description = "Number of days before a cards expiration that it's eligible for online renewal",
                Id = Keys.SiteSetting.RenewCard.ExpirationCutoffDays,
                Name = "Card renewal expiration cutoff days",
                Type= SiteSettingType.Int,
                Value = "-1"
            },
            
            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.RenewCard),
                Description = "Segment to show on the card renewal home page",
                Id = Keys.SiteSetting.RenewCard.HomeSegment,
                Name = "Card renewal segment",
                Type = SiteSettingType.Int,
                Value = "-1"
            },
            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.RenewCard),
                Description = "Juvenile customer code ids, comma delimited",
                Id = Keys.SiteSetting.RenewCard.JuvenileCustomerCodes,
                Name = "Juvenile customer codes",
                Type = SiteSettingType.StringNullable
            },
            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.RenewCard),
                Description = "Segment to show on the juvenile page",
                Id = Keys.SiteSetting.RenewCard.JuvenileSegment,
                Name = "Juvenile segment",
                Type = SiteSettingType.Int,
                Value = "-1"
            },
            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.RenewCard),
                Description = "Segment to show on the verify address page when there's no valid addresses",
                Id = Keys.SiteSetting.RenewCard.NoAddressSegment,
                Name = "No address segment",
                Type = SiteSettingType.Int,
                Value = "-1"
            },
            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.RenewCard),
                Description = "Nonresident customer code ids, comma delimited",
                Id = Keys.SiteSetting.RenewCard.NonresidentCustomerCodes,
                Name = "Nonresident customer codes",
                Type = SiteSettingType.StringNullable
            },
            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.RenewCard),
                Description = "Segment to show when a customer can't renew their card due to being a nonresident",
                Id = Keys.SiteSetting.RenewCard.NonresidentSegment,
                Name = "Nonresident segment",
                Type = SiteSettingType.Int,
                Value = "-1"
            },
            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.RenewCard),
                Description = "Segment to show when api settings aren't configured",
                Id = Keys.SiteSetting.RenewCard.NotConfiguredSegment,
                Name = "Not configured segment",
                Type = SiteSettingType.Int,
                Value = "-1"
            },
            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.RenewCard),
                Description = "Staff customer code ids, comma delimited",
                Id = Keys.SiteSetting.RenewCard.StaffCustomerCodes,
                Name = "Staff customer codes",
                Type = SiteSettingType.StringNullable
            },
            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.RenewCard),
                Description = "Segment to show on the submitted page",
                Id = Keys.SiteSetting.RenewCard.SubmittedSegment,
                Name = "Submitted segment",
                Type = SiteSettingType.Int,
                Value = "-1"
            },
            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.RenewCard),
                Description = "Segment to show on the verify address page",
                Id = Keys.SiteSetting.RenewCard.VerifyAddressSegment,
                Name = "Verify address segment",
                Type = SiteSettingType.Int,
                Value = "-1"
            },

            #endregion RenewCard

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
                Description = "The stub of a link to perform a search in the Polaris ILS",
                Id = Keys.SiteSetting.Site.CatalogSearchLink,
                Name = "Catalog search link",
                Type = SiteSettingType.StringNullable
            },
            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.Site),
                Description = "URL to the footer image at the bottom of the page",
                Id = Keys.SiteSetting.Site.FooterImage,
                Name = "Footer Image",
                Type = SiteSettingType.StringNullable
            },
            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.Site),
                Description = "Alt text when a footer image is displayed",
                Id = Keys.SiteSetting.Site.FooterImageAlt,
                Name = "Footer Image Alt",
                Type = SiteSettingType.StringNullable
            },
            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.Site),
                Description = "Link to the forgot password page",
                Id = Keys.SiteSetting.Site.ForgotPasswordLink,
                Name = "Forgot Password link",
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
            new SiteSetting
            {
                Category = nameof(Keys.SiteSetting.Site),
                Description = "A link for 'Services at all locations' on location pages",
                Id = Keys.SiteSetting.Site.ServicesAtAllLink,
                Name = "Services at all link",
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
                Description = "The Twitter/X @username a Twitter Card should be attributed to",
                Id = Keys.SiteSetting.Social.TwitterUsername,
                Name = "Twitter/X Username",
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
                Description = "The URL to a Twitter/X profile",
                Id = Keys.SiteSetting.Social.TwitterUrl,
                Name = "Twitter/X URL",
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