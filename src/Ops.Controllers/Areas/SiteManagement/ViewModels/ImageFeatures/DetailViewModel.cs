using System;
using System.ComponentModel;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.ImageFeatures
{
    public class DetailViewModel
    {
        public DateTime CurrentDateTime { get; set; }

        public string EditTemplateDisabled
        {
            get
            {
                return HasEditTemplatePermissions
                    ? null
                    : "disabled";
            }
        }

        public int? FocusItemId { get; set; }
        public bool HasEditTemplatePermissions { get; set; }
        public ImageFeature ImageFeature { get; set; }
        public DateTime? ImageFeatureEndDate { get; set; }
        public DateTime? ImageFeatureEndTime { get; set; }
        public int ImageFeatureId { get; set; }

        public ImageFeatureItem ImageFeatureItem { get; set; }
        public ImageFeatureItemText ImageFeatureItemText { get; set; }

        public DateTime? ImageFeatureStartDate { get; set; }
        public DateTime? ImageFeatureStartTime { get; set; }
        public ImageFeatureTemplate ImageFeatureTemplate { get; set; }

        public string ImageTemplateDescription
        {
            get
            {
                if (ImageFeatureTemplate == null)
                {
                    return null;
                }

                var sb = new StringBuilder();
                if (ImageFeatureTemplate.Width.HasValue)
                {
                    sb.Append(' ').Append(ImageFeatureTemplate.Width.Value).Append("px");
                }
                else
                {
                    sb.Append(" no width restriction,");
                }
                if (ImageFeatureTemplate.Height.HasValue)
                {
                    if (ImageFeatureTemplate.Width.HasValue)
                    {
                        sb.Append(" ×");
                    }
                    sb.Append(' ').Append(ImageFeatureTemplate.Height.Value).Append("px");
                }
                else
                {
                    sb.Append(" no height restriction,");
                }
                if (ImageFeatureTemplate.MaximumFileSizeBytes.HasValue)
                {
                    sb.Append(" @ <").Append(ImageFeatureTemplate.MaximumFileSizeBytes / 1024).Append(" KB");
                }
                if (ImageFeatureTemplate.ItemsToDisplay.HasValue)
                {
                    sb.Append(", ").Append(ImageFeatureTemplate.ItemsToDisplay).Append(" images display at a time");
                }
                return sb.ToString()?.Trim();
            }
        }

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
