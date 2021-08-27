using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Segment
{
    public class IndexViewModel
    {
        public List<string> AvailableLanguages { get; set; }
        public SelectList LanguageList { get; set; }
        public PaginateModel PaginateModel { get; set; }
        public Promenade.Models.Entities.Segment Segment { get; set; }
        public DateTime? SegmentEndDate { get; set; }
        public DateTime? SegmentEndTime { get; set; }
        public ICollection<Promenade.Models.Entities.Segment> Segments { get; set; }
        public DateTime? SegmentStartDate { get; set; }
        public DateTime? SegmentStartTime { get; set; }
        public SegmentText SegmentText { get; internal set; }

        [DisplayName("Language")]
        public Language SelectedLanguage { get; set; }

        public int SelectedLanguageId { get; set; }

        public static string RowClass(Promenade.Models.Entities.Segment segment)
        {
            if (segment == null)
            {
                return null;
            }
            if (!segment.IsActive
                || (segment.StartDate.HasValue && DateTime.Now < segment.StartDate.Value)
                || (segment.EndDate.HasValue && DateTime.Now > segment.EndDate.Value))
            {
                return "table-danger";
            }

            return "table-success";
        }

        public static string ActiveDescription(Promenade.Models.Entities.Segment segment)
        {
            if (segment == null)
            {
                return null;
            }

            if (segment.StartDate.HasValue || segment.EndDate.HasValue)
            {
                string result = segment.StartDate.HasValue
                    ? segment.StartDate.Value.ToString(CultureInfo.CurrentCulture)
                    : "<em>none</em>";
                result += " &ndash; ";
                result += segment.EndDate.HasValue
                   ? segment.EndDate.Value.ToString(CultureInfo.CurrentCulture)
                   : "<em>none</em>";

                return result;
            }
            return "always active";
        }
    }
}