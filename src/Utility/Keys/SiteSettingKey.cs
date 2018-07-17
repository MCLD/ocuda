using System;
using System.Collections.Generic;
using System.Text;

namespace Ocuda.Utility.Keys
{
    public struct SiteSettingKey
    {
        public struct EmailService
        {
            public const string FromAddress = "EmailService.FromAddress";
        }

        public struct FileUpload
        {
            public const string MaxFileSize = "FileUpload.MaxFileSize";
        }

        public struct ModelState
        {
            public const string TimeOutMinutes = "ModelState.TimeOutMinutes";
        }

        public struct Pagination
        {
            public const string ItemsPerPage = "Pagination.ItemsPerPage";
        }
    }
}
