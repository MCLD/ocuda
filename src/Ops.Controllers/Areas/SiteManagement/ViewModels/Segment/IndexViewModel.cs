using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Segment
{
    public class IndexViewModel
    {
        public ICollection<Promenade.Models.Entities.Segment> Segments { get; set; }
        public PaginateModel PaginateModel { get; set; }
        public Promenade.Models.Entities.Segment Segment { get; set; }
        public int SelectedLanguageId { get; set; }

        [DisplayName("Language")]
        public Language SelectedLanguage { get; set; }

        public SegmentText SegmentText { get; internal set; }
        public SelectList LanguageList { get; set; }
        public List<string> AvailableLanguages { get; set; }
    }
}
