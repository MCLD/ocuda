using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class NavBannerLink
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [MaxLength(60)]
        public string Icon { get; set; }
        public NavBanner NavBanner { get; set; }
        public int NavBannerId { get; set; }
        public NavBannerLinkText Text { get; set; }
        public int Order {  get; set; }
    }
}
