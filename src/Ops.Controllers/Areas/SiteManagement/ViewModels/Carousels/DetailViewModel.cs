using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Carousels
{
    public class DetailViewModel
    {
        public Carousel Carousel { get; set; }
        public int CarouselId { get; set; }

        public CarouselText CarouselText { get; set; }

        public CarouselItem CarouselItem { get; set; }
        public CarouselItemText CarouselItemText { get; set; }

        public CarouselButton CarouselButton { get; set; }

        public CarouselTemplate CarouselTemplate { get; set; }

        public int LanguageId { get; set; }

        public SelectList LanguageList { get; set; }

        [DisplayName("Language")]
        public string SelectLanguage { get; set; }

        public SelectList LabelList { get; set; }

        public string AllowedImageDomains { get; set; }

        public string ButtonUrlInfoMessage { get; set; }

        public int? FocusItemId { get; set; }
        public string ItemErrorMessage { get; set; }

        public int? PageLayoutId { get; set; }
    }
}
