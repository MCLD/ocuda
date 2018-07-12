using System;
using System.Collections.Generic;
using System.Text;

namespace Ocuda.Ops.Controllers
{
    public struct SiteSettingKey
    {
        public struct FileUpload
        {
            public const string MaxFileSize = "FileUpload.MaxFileSize";
        }

        public struct Pagination
        {
            public const string ItemsPerPage = "Pagination.ItemsPerPage";
        }
    }
}
