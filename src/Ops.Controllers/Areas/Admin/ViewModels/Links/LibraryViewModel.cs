using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Links
{
    public class LibraryViewModel
    {
        public IEnumerable<Link> Links { get; set; }
        public PaginateModel PaginateModel { get; set; }
        public LinkLibrary LinkLibrary { get; set; }
        public Link Link { get; set; }
    }
}
