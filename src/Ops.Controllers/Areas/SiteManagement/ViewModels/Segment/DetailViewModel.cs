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
            TemplateFields = [];
        }

        [DisplayName("Displayed header")]
        public string AutomatedHeaderMarkup { get; set; }

        public string BackLink { get; set; }

        /// <summary>
        /// Set to true to show the activation drop-down, otherwise segment is set to always active
        /// </summary>
        public bool CanBeDeactivated { get; set; }

        public bool CanBeScheduled
        {
            get
            {
                return IsSchedulable || SegmentStartDate.HasValue || SegmentEndDate.HasValue;
            }
        }

        /// <summary>
        /// Informational text to show below the wrap drop-down
        /// </summary>
        public string FlagWrap { get; set; }

        [DisplayName("Is active?")]
        public bool IsActive { get; set; }

        /// <summary>
        /// Set to true to show the schedule start and end dates, otherwise dates are not editable
        /// </summary>
        public bool IsSchedulable { get; set; }

        /// <summary>
        /// Set to true if this segment is podcast show notes
        /// </summary>
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

        /// <summary>
        /// Set to true to disable the segment header and keep it set to empty
        /// </summary>
        public bool SuppressHeader { get; set; }
        /// <summary>
        /// Set to true to disable the ability to select a segment wrap and keep it set to empty
        /// </summary>
        public bool SuppressWrap { get; set; }

        [DisplayName("Available tags")]
        public ICollection<KeyWithDescription> TemplateFields { get; }
    }
}