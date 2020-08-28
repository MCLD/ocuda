using System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Pages
{
    public class DetailViewModel
    {
        public Page Page { get; set; }
        public int HeaderId { get; set; }

        [DisplayName("Page Name")]
        public string HeaderName { get; set; }

        [DisplayName("Stub")]
        public string HeaderStub { get; set; }

        [DisplayName("Type")]
        public PageType HeaderType { get; set; }

        public bool NewPage { get; set; }
        public string PageUrl { get; set; }
        public SelectList SocialCardList { get; set; }

        public int LanguageId { get; set; }
        public string LanguageDescription { get; set; }

        public SelectList LanguageList { get; set; }

        [DisplayName("Language")]
        public string SelectedLanguage { get; set; }
        public bool IsSiteManager { get; set; }
    }
}
