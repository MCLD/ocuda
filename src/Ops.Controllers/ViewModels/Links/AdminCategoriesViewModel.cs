using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Ops.Models;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.ViewModels.Links
{
    public class AdminCategoriesViewModel
    {
        public IEnumerable<LinkCategory> Categories { get; set; }
        public LinkCategory Category { get; set; }
        public PaginateModel PaginateModel { get; set; }
    }
}
