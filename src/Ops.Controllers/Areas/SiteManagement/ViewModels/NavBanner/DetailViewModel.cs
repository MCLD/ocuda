using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.NavBannerViewModels
{
    public class DetailViewModel
    {
        public string Name { get; set; }
        public IFormFile Image { get; set; }
        public NavBannerImage NavBannerImage { get; set; }
        public ICollection<NavBannerLink> Links { get; set; }
    }
}
