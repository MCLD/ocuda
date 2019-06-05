using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Pages
{
    public class IndexViewModel
    {
        public IEnumerable<Page> Pages { get; set; }
        public Page Page { get; set; }
        public PaginateModel PaginateModel { get; set; }
        public int SectionId { get; set; }
    }
}
