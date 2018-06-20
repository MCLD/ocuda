using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Ops.Models;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.ViewModels.Pages
{
    public class IndexViewModel
    {
        public IEnumerable<Page> Pages { get; set; }
        public PaginateModel PaginateModel { get; set; }
    }
}
