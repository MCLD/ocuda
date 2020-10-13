namespace Ocuda.Ops.Models.Keys
{
    public struct SiteSetting
    {
        public struct Carousel
        {
            public const string ImageRestrictToDomains = "Carousel.ImageRestricToDomains";
            public const string LinkRestrictToDomains = "Carousel.LinkRestrictToDomains";
        }

        public struct CoverIssueReporting
        {
            public const string LeapBibUrl = "CoverIssueReporting.LeapBibUrl";
        }

        public struct Email
        {
            public const string BccAddress = "Email.BccAddress";
            public const string FromAddress = "Email.FromAddress";
            public const string FromName = "Email.FromName";
            public const string OutgoingHost = "Email.OutgoingHost";
            public const string OutgoingLogin = "Email.OutgoingLogin";
            public const string OutgoingPassword = "Email.OutgoingPassword";
            public const string OutgoingPort = "Email.OutgoingPort";
            public const string OverrideToAddress = "Email.OverrideToAddress";
            public const string RestrictToDomain = "Email.RestrictToDomain";
        }

        public struct FileManagement
        {
            public const string MaxUploadBytes = "FileManagement.MaxFileSizeBytes";
            public const string MaxThumbnailCount = "FileManagement.MaxThumbnailCount";
            public const string ThumbnailTypes = "FileManagement.ThumbnailTypes";
        }

        public struct SiteManagement
        {
            public const string PromenadePublicPath = "SiteManagement.PromenadePublicPath";
            public const string PromenadeUrl = "SiteManagement.PromenadeUrl";
        }

        public struct UserInterface
        {
            public const string ItemsPerPage = "UserInterface.ItemsPerPage";
            public const string ModelStateTimeoutMinutes = "UserInterface.TimeOutMinutes";
        }
    }
}
