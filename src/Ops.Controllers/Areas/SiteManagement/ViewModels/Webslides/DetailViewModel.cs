using System;
using System.ComponentModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Webslides
{
    public class DetailViewModel
    {
        public Webslide Webslide { get; set; }
        public int WebslideId { get; set; }

        public WebslideItem WebslideItem { get; set; }
        public WebslideItemText WebslideItemText { get; set; }

        [DisplayName("Image")]
        public IFormFile ItemImage { get; set; }

        public WebslideTemplate WebslideTemplate { get; set; }

        public int LanguageId { get; set; }

        public SelectList LanguageList { get; set; }

        [DisplayName("Language")]
        public string SelectLanguage { get; set; }

        public int? FocusItemId { get; set; }
        public string ItemErrorMessage { get; set; }

        public int PageLayoutId { get; set; }

        public DateTime CurrentDateTime { get; set; }
    }
}
