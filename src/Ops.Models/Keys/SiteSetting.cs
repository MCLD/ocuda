namespace Ocuda.Ops.Models.Keys
{
    public struct SiteSetting
    {
        public struct Email
        {
            public const string FromAddress = "Email.FromAddress";
        }

        public struct FileManagement
        {
            public const string MaxUploadBytes = "FileManagement.MaxFileSizeBytes";
        }

        public struct UserInterface
        {
            public const string ItemsPerPage = "UserInterface.ItemsPerPage";
            public const string ModelStateTimeoutMinutes = "UserInterface.TimeOutMinutes";
        }
    }
}
