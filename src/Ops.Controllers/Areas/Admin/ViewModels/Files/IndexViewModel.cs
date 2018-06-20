using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Ops.Models;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Files
{
    public class IndexViewModel
    {
        public IEnumerable<File> Files { get; set; }
        public File File { get; set; }
        public PaginateModel PaginateModel { get; set; }
    }
}
