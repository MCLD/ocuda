using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Admin.ViewModels.CoverIssueManagement
{
    public class CoverIssueManagementViewModel
    {
        public ICollection<CoverIssueHeader> AllCoverIssues { get; set; }
        public PaginateModel PaginateModel { get; set; }
        public CoverIssueHeader Header { get; set; }
        public List<CoverIssueDetail> Details { get; set; }
        public List<CoverIssueType> Types { get; set; }
        public int OrderBy { get; set; }
        public bool OrderDesc { get; set; }
        public string Search { get; set; }
        public int? SearchCount { get; set; }
        public string CoverIssueImgPath { get; set; }
        public string CoverIssueLeapPath { get; set; }
    }
}
