using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class NavBannerLinkText
    {
        public Language Language { get; set; }

        [Key]
        [Required]
        public int LanguageId { get; set; }

        public NavBannerLink NavBannerLink { get; set; }

        [Key]
        [Required]
        public int NavBannerLinkId { get; set; }

        [Required]
        [MaxLength(60)]
        public string Text { get; set; }
    }
}