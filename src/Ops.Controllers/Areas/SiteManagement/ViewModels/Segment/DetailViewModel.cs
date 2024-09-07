using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Segment
{
    public class DetailViewModel
    {
        public DetailViewModel()
        {
            TemplateFields = new List<KeyWithDescription>();
        }

        [DisplayName("Displayed header")]
        public string AutomatedHeaderMarkup { get; set; }

        public string BackLink { get; set; }
        public bool IsShowNotes { get; set; }
        public string LanguageDescription { get; set; }
        public int LanguageId { get; set; }
        public SelectList LanguageList { get; set; }
        public bool NewSegmentText { get; set; }
        public string Relationship { get; set; }

        [DisplayName("End Date")]
        public DateTime? SegmentEndDate { get; set; }

        public int SegmentId { get; set; }

        [DisplayName("Segment Name")]
        public string SegmentName { get; set; }

        [DisplayName("Start Date")]
        public DateTime? SegmentStartDate { get; set; }

        public SegmentText SegmentText { get; set; }

        [DisplayName("Segment Wrap (all languages)")]
        public int? SegmentWrapId { get; set; }

        public SelectList SegmentWrapList { get; set; }

        [DisplayName("Language")]
        public Language SelectedLanguage { get; set; }

        [DisplayName("Available tags")]
        public ICollection<KeyWithDescription> TemplateFields { get; }
    }
}