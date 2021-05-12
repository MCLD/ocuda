using System;
using System.ComponentModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.PageFeatures
{
    public class DetailViewModel
    {
        public ImageFeature PageFeature { get; set; }
        public int PageFeatureId { get; set; }

        public ImageFeatureItem PageFeatureItem { get; set; }
        public ImageFeatureItemText PageFeatureItemText { get; set; }

        [DisplayName("Image")]
        public IFormFile ItemImage { get; set; }

        public ImageFeatureTemplate PageFeatureTemplate { get; set; }

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
