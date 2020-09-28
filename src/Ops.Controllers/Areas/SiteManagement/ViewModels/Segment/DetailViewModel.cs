using System;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Segment
{
    public class DetailViewModel
    {
        public SegmentText SegmentText { get; set; }
        public int SegmentId { get; set; }

        [DisplayName("Segment Name")]
        public string SegmentName { get; set; }

        [DisplayName("Start Date")]
        public DateTime? SegmentStartDate { get; set; }

        [DisplayName("End Date")]
        public DateTime? SegmentEndDate { get; set; }

        public bool NewSegmentText { get; set; }

        public int LanguageId { get; set; }
        public string LanguageDescription { get; set; }

        public SelectList LanguageList { get; set; }

        [DisplayName("Language")]
        public Language SelectedLanguage { get; set; }

        public int? PageLayoutId { get; set; }
    }
}
