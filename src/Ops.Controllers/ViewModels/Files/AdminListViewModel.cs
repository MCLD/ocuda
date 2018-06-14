using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Ops.Models;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.ViewModels.Files
{
    public class AdminListViewModel
    {
        public IEnumerable<SectionFile> SectionFiles { get; set; }
        public SectionFile SectionFile { get; set; }
        public PaginateModel PaginateModel { get; set; }
    }
}
