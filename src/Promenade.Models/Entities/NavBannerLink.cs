using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class NavBannerLink
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [MaxLength(60)]
        public string Icon { get; set; }

        [Required]
        [MaxLength(255)]
        public string Link { get; set; }

        public NavBanner NavBanner { get; set; }
        public int NavBannerId { get; set; }

        [NotMapped]
        public NavBannerLinkText Text { get; set; }
        public int Order {  get; set; }
    }
}
