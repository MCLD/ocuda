using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Segment
{
    public class DetailsViewModel
    {
        public SegmentText SegmentText { get; set; }

        public Promenade.Models.Entities.Segment Segment { get; set; }

        public int SelectedLanguageId { get; set; }

        [DisplayName("Language")]
        public Language SelectedLanguage { get; set; }

        public SelectList LanguageList { get; set; }
    }
}
