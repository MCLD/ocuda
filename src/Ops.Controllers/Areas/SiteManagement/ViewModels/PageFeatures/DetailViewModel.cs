using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.PageFeatures
{
    public class DetailViewModel
    {
        public PageFeature PageFeature { get; set; }
        public int PageFeatureId { get; set; }

        public PageFeatureItem PageFeatureItem { get; set; }
        public PageFeatureItemText PageFeatureItemText { get; set; }

        [DisplayName("Image")]
        public IFormFile ItemImage { get; set; }

        public PageFeatureTemplate PageFeatureTemplate { get; set; }

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
