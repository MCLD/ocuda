using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Ops.Models;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.ViewModels.Links
{
    public class IndexViewModel
    {
        public IEnumerable<Link> Links { get; set; }
        public IEnumerable<Category> Categories { get; set; }
        public PaginateModel PaginateModel { get; set; }
        public string CategoryName { get; set; }
    }
}
