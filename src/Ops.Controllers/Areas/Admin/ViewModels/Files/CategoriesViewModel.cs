using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Ops.Models;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Files
{
    public class CategoriesViewModel
    {
        public IEnumerable<Category> Categories { get; set; }
        public Category Category { get; set; }
        public PaginateModel PaginateModel { get; set; }
    }
}
