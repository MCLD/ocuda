using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Ocuda.Promenade.Models.Entities
{
    public class NavBannerLinkText
    {
        [Key]
        [Required]
        public int LanguageId { get; set; }
        public Language Language { get; set; }

        [Key]
        [Required]
        public int NavBannerLinkId { get; set; }
        public NavBannerLink NavBannerLink { get; set; }

        [Required]
        [MaxLength(255)]
        public string Link { get; set; }

        [Required]
        [MaxLength(60)]
        public string Text { get; set; }
    }
}