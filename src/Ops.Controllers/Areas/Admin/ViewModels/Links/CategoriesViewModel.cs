using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Ops.Models;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Links
{
    public class CategoriesViewModel
    {
        public IEnumerable<LinkCategory> Categories { get; set; }
        public LinkCategory Category { get; set; }
        public PaginateModel PaginateModel { get; set; }
    }
}
