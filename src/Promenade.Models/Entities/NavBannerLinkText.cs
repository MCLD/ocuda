using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Ocuda.Promenade.Models.Entities
{
    [PrimaryKey(nameof(LanguageId), nameof(NavBannerLinkId))]
    public class NavBannerLinkText
    {
        [Required]
        public int LanguageId { get; set; }
        public Language Language { get; set; }

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