using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Ops.Models;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.ViewModels.Files
{
    public class FileListViewModel
    {
        public IEnumerable<File> Files { get; set; }
        public PaginateModel PaginateModel { get; set; }
    }
}
