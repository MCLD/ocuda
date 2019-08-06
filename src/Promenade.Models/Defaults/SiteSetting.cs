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
                Key = Keys.SiteSetting.Site.IsTLS,
                Name = "TLS Enabled",
                Description = "Is the website enabled to use TLS?",
                Category = "Site",
                Value = "False",
                Type = SiteSettingType.Bool
            }
            #endregion
        };
    }
}
