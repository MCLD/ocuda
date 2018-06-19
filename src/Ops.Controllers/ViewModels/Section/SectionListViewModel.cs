using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Ops.Models;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.ViewModels.Section
{
    public class SectionListViewModel
    {
        public IEnumerable<Models.Section> Sections { get; set; }
        public Models.Section Section { get; set; }
        public PaginateModel PaginateModel { get; set; }
    }
}
