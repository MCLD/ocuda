using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class PageHeader
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        [DisplayName("Page Name")]
        public string PageName { get; set; }

        [Required]
        [MaxLength(255)]
        public string Stub { get; set; }

        public PageType Type { get; set; }

        [DisplayName("Layout Page?")]
        public bool IsLayoutPage { get; set; }

        [DisplayName("Carousel Template")]
        public int? LayoutCarouselTemplateId { get; set; }
        public CarouselTemplate LayoutCarouselTemplate { get; set; }

        [NotMapped]
        public ICollection<string> PageLanguages { get; set; }

        [NotMapped]
        public IEnumerable<string> PermissionGroupIds { get; set; }
    }

    public enum PageType
    {
        About,
        Home,
        News,
        Subject
    }
}
