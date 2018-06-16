using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Ops.Models;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.ViewModels.Links
{
    public class AdminListViewModel
    {
        public IEnumerable<Link> Links { get; set; }
        public Link Link { get; set; }
        public PaginateModel PaginateModel { get; set; }
    }
}
