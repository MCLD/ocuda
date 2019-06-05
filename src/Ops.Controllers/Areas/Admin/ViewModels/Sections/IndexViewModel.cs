using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Sections
{
    public class IndexViewModel
    {
        public IEnumerable<Section> Sections { get; set; }
        public Section Section { get; set; }
        public PaginateModel PaginateModel { get; set; }
    }
}
