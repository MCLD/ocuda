using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.NavBannerViewModels
{
    public class DetailViewModel
    {
        public static long TimeStamp
        {
            get
            {
                return DateTime.Now.Ticks;
            }
        }

        public NavBannerLink BottomLeft { get; set; }

        public NavBannerLink BottomRight { get; set; }

        public IFormFile Image { get; set; }

        public Language Language { get; set; }

        public SelectList LanguageList { get; set; }

        public string Name { get; set; }

        public int NavBannerId { get; set; }

        public NavBannerImage NavBannerImage { get; set; }

        public string NavBannerLink { get; set; }

        public string NavBannerLinkPath
        {
            get
            {
                return NavBannerImage == null
                    ? null
                    : $"{NavBannerLink}?navBannerID={NavBannerId}&languageName={Language.Name}";
            }
        }

        public int? PageLayoutId { get; set; }
        public NavBannerLink TopLeft { get; set; }
        public NavBannerLink TopRight { get; set; }
    }
}