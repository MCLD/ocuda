using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.CoverIssue.ViewModels.Management
{
    public class DetailViewModel
    {
        public CoverIssueHeader Header { get; set; }
        public ICollection<CoverIssueDetail> Details { get; set; }
        public string LeapPath { get; set; }
        public int HeaderId { get; set; }
        public bool CanEdit { get; set; }
    }
}