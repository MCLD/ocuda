using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Ops.Models;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.ViewModels.Files
{
    public class AdminCategoriesViewModel
    {
        public IEnumerable<FileCategory> Categories { get; set; }
        public FileCategory Category { get; set; }
        public PaginateModel PaginateModel { get; set; }
    }
}
