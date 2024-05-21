using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Pages
{
    public class LayoutDetailViewModel
    {
        public ImageFeature BannerFeature { get; set; }
        public Carousel Carousel { get; set; }
        public Deck Deck { get; set; }
        public int LanguageId { get; set; }
        public SelectList LanguageList { get; set; }
        public ImageFeature PageFeature { get; set; }
        public NavBanner NavBanner { get; set; }
        public PageItem PageItem { get; set; }
        public PageLayout PageLayout { get; set; }
        public int PageLayoutId { get; set; }

        public PageLayoutText PageLayoutText { get; set; }
        public string PreviewLink { get; set; }

        public Promenade.Models.Entities.Segment Segment { get; set; }

        [DisplayName("Language")]
        public string SelectLanguage { get; set; }

        public ImageFeature Webslide { get; set; }
    }
}