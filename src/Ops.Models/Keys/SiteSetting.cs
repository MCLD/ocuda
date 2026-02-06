namespace Ocuda.Ops.Models.Keys
{
    namespace SiteSetting
    {
        public static class Carousel
        {
            public static readonly string AltTextEnglish = "Carousel.AltTextEnglish";
            public static readonly string AltTextEspanol = "Carousel.AltTextEspanol";
            public static readonly string ImageRestrictToDomains = "Carousel.ImageRestricToDomains";
            public static readonly string LinkRestrictToDomains = "Carousel.LinkRestrictToDomains";
        }

        public static class CoverIssueReporting
        {
            public static readonly string LeapBibUrl = "CoverIssueReporting.LeapBibUrl";
        }

        public static class Email
        {
            public static readonly string AdminAddress = "Email.AdminAddress";
            public static readonly string BccAddress = "Email.BccAddress";
            public static readonly string FromAddress = "Email.FromAddress";
            public static readonly string FromName = "Email.FromName";
            public static readonly string OutgoingHost = "Email.OutgoingHost";
            public static readonly string OutgoingLogin = "Email.OutgoingLogin";
            public static readonly string OutgoingPassword = "Email.OutgoingPassword";
            public static readonly string OutgoingPort = "Email.OutgoingPort";
            public static readonly string OverrideToAddress = "Email.OverrideToAddress";
            public static readonly string RestrictToDomain = "Email.RestrictToDomain";
        }

        public static class FileManagement
        {
            public static readonly string MaxThumbnailCount = "FileManagement.MaxThumbnailCount";
            public static readonly string MaxUploadBytes = "FileManagement.MaxFileSizeBytes";
            public static readonly string ThumbnailTypes = "FileManagement.ThumbnailTypes";
        }

        public static class Incident
        {
            public static readonly string Documentation = "Incident.Documentation";
            public static readonly string EmailTemplateId = "Incident.EmailTemplateId";
            public static readonly string LawEnforcementAddresses = "Incident.LawEnforcementAddresses";
            public static readonly string NotifyTitleClassificationIds = "Incident.NotifyTitleClassificationIds";
            public static readonly string NotifyUserIds = "Incident.NotifyUserIds";
        }

        public static class Scheduling
        {
            public static readonly string Documentation = "Scheduling.Documentation";
        }

        public static class SiteManagement
        {
            public static readonly string PromenadePublicPath = "SiteManagement.PromenadePublicPath";
            public static readonly string PromenadeUrl = "SiteManagement.PromenadeUrl";
        }

        public static class UserInterface
        {
            public static readonly string BaseIntranetLink = "UserInterface.BaseIntranetUrl";
            public static readonly string ItemsPerPage = "UserInterface.ItemsPerPage";
            public static readonly string ModelStateTimeoutMinutes = "UserInterface.TimeOutMinutes";
            public static readonly string PageTitleBase = "UserInterface.PageTitleBase";
        }
    }
}