using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class NavBannerLink
    {
        [MaxLength(60)]
        public string Icon { get; set; }

        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Link { get; set; }

        [NotMapped]
        public string LocalizedText { get; set; }

        public NavBanner NavBanner { get; set; }

        public int NavBannerId { get; set; }

        [NotMapped]
        public NavBannerLinkText NavBannerLinkText { get; set; }

        public int Order { get; set; }
    }
}