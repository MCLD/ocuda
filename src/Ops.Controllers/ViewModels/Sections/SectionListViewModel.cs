using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Ops.Models;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.ViewModels.Sections
{
    public class SectionListViewModel
    {
        public IEnumerable<Section> Sections { get; set; }
        public PaginateModel PaginateModel { get; set; }
    }
}
