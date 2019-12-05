using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.CoverIssue.ViewModels.Management
{
    public class IndexViewModel
    {
        public ICollection<CoverIssueHeader> CoverIssueHeaders { get; set; }
        public PaginateModel PaginateModel { get; set; }
    }
}
