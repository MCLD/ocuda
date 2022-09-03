using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public enum PageType
    {
        About,
        Home,
        News,
        Subject
    }

    public class PageHeader
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [DisplayName("Layout Page?")]
        public bool IsLayoutPage { get; set; }

        public ImageFeatureTemplate LayoutBannerTemplate { get; set; }

        [DisplayName("Banner Template")]
        public int? LayoutBannerTemplateId { get; set; }

        public CarouselTemplate LayoutCarouselTemplate { get; set; }

        [DisplayName("Carousel Template")]
        public int? LayoutCarouselTemplateId { get; set; }

        public ImageFeatureTemplate LayoutFeatureTemplate { get; set; }

        [DisplayName("Feature Template")]
        public int? LayoutFeatureTemplateId { get; set; }

        public ImageFeatureTemplate LayoutWebslideTemplate { get; set; }

        [DisplayName("Webslide Template")]
        public int? LayoutWebslideTemplateId { get; set; }

        [Required]
        [MaxLength(255)]
        [DisplayName("Page Name")]
        public string PageName { get; set; }

        [NotMapped]
        public IEnumerable<string> PermissionGroupIds { get; set; }

        [Required]
        [MaxLength(255)]
        public string Stub { get; set; }

        public PageType Type { get; set; }
    }
}