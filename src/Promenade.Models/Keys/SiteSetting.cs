namespace Ocuda.Promenade.Models.Keys
{
    public struct SiteSetting
    {
        public struct Social
        {
            public const string FacebookUrl = "Social.FacebookUrl";
            public const string InstagramUrl = "Social.InstagramUrl";
            public const string TwitterUrl = "Social.TwitterUrl";
            public const string TwitterUsername = "Social.TwitterUsername";
            public const string YoutubeUrl = "Social.YoutubeUrl";
        }

        public struct Site
        {
            public const string BannerImage = "Site.BannerImage";
            public const string BannerImageAlt = "Site.BannerImageAlt";
            public const string GoogleTrackingCode = "Site.GoogleTrackingCode";
            public const string IsTLS = "Site.IsTLS";
            public const string NavigationIdFooter = "Site.NavigationIdFooter";
            public const string NavigationIdLeft = "Site.NavigationIdLeft";
            public const string NavigationIdMiddle = "Site.NavigationIdMiddle";
            public const string NavigationIdTop = "Site.NavigationIdTop";
            public const string PageTitleSuffix = "Site.PageTitleSuffix";
        }

        public struct Scheduling
        {
            public const string DisabledSegment = "Scheduling.DisabledSegment";
            public const string Enable = "Scheduling.Enable";
            public const string EnabledSegment = "Scheduling.EnabledSegment";
            public const string ScheduledSegment = "Scheduling.ScheduledSegment";
        }

        public struct Email
        {
            public const string HeaderSegment = "Email.HeaderSegment";
            public const string FooterSegment = "Email.FooterSegment";
        }
    }
}
