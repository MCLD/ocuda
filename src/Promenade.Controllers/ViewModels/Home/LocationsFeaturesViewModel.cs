using System.ComponentModel.DataAnnotations;
using CommonMark;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers.ViewModels.Home
{
    public class LocationsFeaturesViewModel
    {
        public LocationsFeaturesViewModel(LocationFeature locationsFeatures)
        {
            BodyText = CommonMarkConverter.Convert(locationsFeatures?.Feature?.BodyText);
            Icon = string.IsNullOrEmpty(locationsFeatures?.Feature?.IconText)
                ? locationsFeatures?.Feature?.Icon
                : $"{locationsFeatures?.Feature?.Icon} prom-location-icon-text";
            ImagePath = locationsFeatures?.Feature?.ImagePath;
            Name = locationsFeatures?.Feature?.Name;
            RedirectLink = locationsFeatures?.RedirectUrl;
            NewTab = locationsFeatures?.NewTab;
            Stub = locationsFeatures?.Feature?.Stub;
            Text = CommonMarkConverter.Convert(locationsFeatures?.Text);
            IconText = locationsFeatures?.Feature?.IconText;
        }

        [Required]
        public string BodyText { get; set; }

        [MaxLength(48)]
        public string Icon { get; set; }

        [MaxLength(5)]
        public string IconText { get; set; }

        [MaxLength(255)]
        public string ImagePath { get; set; }

        [MaxLength(255)]
        [Required]
        public string Name { get; set; }

        public bool? NewTab { get; set; }

        [MaxLength(255)]
        public string RedirectLink { get; set; }

        [MaxLength(80)]
        [Required]
        public string Stub { get; set; }

        public string Text { get; set; }
    }
}