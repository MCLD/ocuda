using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class NavBannerLink
    {
        [Key]
        [Required]
        public int Id { get; set; }
        public NavBanner NavBanner { get; set; }
        public int NavBannerId { get; set; }
        public NavBannerLinkText Text { get; set; }
        public int NavBannerLinkTextId { get; set; }
    }
}
