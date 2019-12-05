namespace Ocuda.Ops.Models.Keys
{
    public struct SiteSetting
    {
        public struct CoverIssueReporting
        {
            public const string LeapBibUrl = "CoverIssueReporting.LeapBibUrl";
        }

        public struct Email
        {
            public const string FromName = "Email.FromName";
            public const string FromAddress = "Email.FromAddress";
            public const string OutgoingHost = "Email.OutgoingHost";
            public const string OutgoingLogin = "Email.OutgoingLogin";
            public const string OutgoingPassword = "Email.OutgoingPassword";
            public const string OutgoingPort = "Email.OutgoingPort";
        }

        public struct FileManagement
        {
            public const string MaxUploadBytes = "FileManagement.MaxFileSizeBytes";
            public const string MaxThumbnailCount = "FileManagement.MaxThumbnailCount";
            public const string ThumbnailTypes = "FileManagement.ThumbnailTypes";
        }

        public struct UserInterface
        {
            public const string ItemsPerPage = "UserInterface.ItemsPerPage";
            public const string ModelStateTimeoutMinutes = "UserInterface.TimeOutMinutes";
        }
    }
}
