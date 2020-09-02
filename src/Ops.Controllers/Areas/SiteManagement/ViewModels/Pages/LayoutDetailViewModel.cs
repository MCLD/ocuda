using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Pages
{
    public class LayoutDetailViewModel
    {
        public PageLayout PageLayout { get; set; }
        public int PageLayoutId { get; set; }

        public PageLayoutText PageLayoutText { get; set; }

        public PageItem PageItem { get; set; }

        public int LanguageId { get; set; }

        public SelectList LanguageList { get; set; }

        [DisplayName("Language")]
        public string SelectLanguage { get; set; }

        public SelectList CarouselList { get; set; }
        public SelectList SegmentList { get; set; }
    }
}
