using System;
using System.ComponentModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.ImageFeatures
{
    public class DetailViewModel
    {
        public DateTime CurrentDateTime { get; set; }
        public int? FocusItemId { get; set; }
        public ImageFeature ImageFeature { get; set; }
        public int ImageFeatureId { get; set; }

        public ImageFeatureItem ImageFeatureItem { get; set; }
        public ImageFeatureItemText ImageFeatureItemText { get; set; }

        public ImageFeatureTemplate ImageFeatureTemplate { get; set; }

        public string ItemErrorMessage { get; set; }

        [DisplayName("Image")]
        public IFormFile ItemImage { get; set; }

        public int LanguageId { get; set; }

        public SelectList LanguageList { get; set; }

        public int PageLayoutId { get; set; }

        [DisplayName("Language")]
        public string SelectLanguage { get; set; }
    }
}