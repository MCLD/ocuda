using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.NavBannerViewModels
{
    public class DetailViewModel
    {
        public string Name { get; set; }
        public IFormFile Image { get; set; }
        public Language Language { get; set; }
        public int NavBannerId { get; set; }
        public NavBannerImage NavBannerImage { get; set; }
        public List<NavBannerLink> Links { get; set; }
        public List<NavBannerLinkText> LinkTexts { get; set; }
        public int? PageLayoutId { get; set; }
        public SelectList LanguageList { get; set; }
    }
}
