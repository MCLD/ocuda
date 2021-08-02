namespace Ocuda.Ops.Models.Keys
{
    namespace SiteSetting
    {
        public static class Carousel
        {
            public const string ImageRestrictToDomains = "Carousel.ImageRestricToDomains";
            public const string LinkRestrictToDomains = "Carousel.LinkRestrictToDomains";
        }

        public static class CoverIssueReporting
        {
            public const string LeapBibUrl = "CoverIssueReporting.LeapBibUrl";
        }

        public static class Email
        {
            public const string AdminAddress = "Email.AdminAddress";
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

        public static class FileManagement
        {
            public const string MaxThumbnailCount = "FileManagement.MaxThumbnailCount";
            public const string MaxUploadBytes = "FileManagement.MaxFileSizeBytes";
            public const string ThumbnailTypes = "FileManagement.ThumbnailTypes";
        }

        public static class SiteManagement
        {
            public const string PromenadePublicPath = "SiteManagement.PromenadePublicPath";
            public const string PromenadeUrl = "SiteManagement.PromenadeUrl";
        }

        public static class UserInterface
        {
            public const string ItemsPerPage = "UserInterface.ItemsPerPage";
            public const string ModelStateTimeoutMinutes = "UserInterface.TimeOutMinutes";
        }
    }
}