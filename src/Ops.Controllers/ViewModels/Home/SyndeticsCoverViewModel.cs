using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Controllers.ViewModels.Home
{
    public class SyndeticsCoverViewModel
    {
        public int BibID { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string ISBN { get; set; }
        public string UPC { get; set; }
        public string OCLC { get; set; }
        public string ItemImagePath { get; set; }
        public string Format { get; set; }
        public string Summary { get; set; }
        public string LCCN { get; set; }
        public List<CoverIssueType> CoverIssueTypes { get; set; }
        public CoverIssueHeader Header { get; set; }
        public CoverIssueDetail Detail { get; set; }
        public CoverIssueType Type { get; set; }
    }
}
