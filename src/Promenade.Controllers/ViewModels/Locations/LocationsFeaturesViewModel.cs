using System.ComponentModel.DataAnnotations;
using CommonMark;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers.ViewModels.Locations
{
    public class LocationsFeaturesViewModel
    {
        [MaxLength(255)]
        [Required]
        public string Name { get; set; }

        [MaxLength(48)]
        public string Icon { get; set; }

        [MaxLength(255)]
        public string ImagePath { get; set; }

        [MaxLength(80)]
        [Required]
        public string Stub { get; set; }

        [Required]
        public string BodyText { get; set; }

        public string Text { get; set; }

        [MaxLength(255)]
        public string RedirectUrl { get; set; }

        [MaxLength(5)]
        public string IconText { get; set; }

        public LocationsFeaturesViewModel(LocationFeature locationsFeatures)
        {
            BodyText = CommonMarkConverter.Convert(locationsFeatures?.Feature?.BodyText);
            Icon = string.IsNullOrEmpty(locationsFeatures?.Feature?.IconText)
                ? locationsFeatures?.Feature?.Icon
                : $"{locationsFeatures?.Feature?.Icon} prom-location-icon-text";
            ImagePath = locationsFeatures?.Feature?.ImagePath;
            Name = locationsFeatures?.Feature?.Name;
            RedirectUrl = locationsFeatures?.RedirectUrl;
            Stub = locationsFeatures?.Feature?.Stub;
            Text = CommonMarkConverter.Convert(locationsFeatures?.Text);
            IconText = locationsFeatures?.Feature?.IconText;
        }
    }
}
