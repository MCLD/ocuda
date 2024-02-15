using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class NavBanner
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string ImagePath {  get; set; }

        [NotMapped]
        public ICollection<NavBannerLink> NavBannerLinks { get; set; }

        [NotMapped]
        public NavBannerText NavBannerText { get; set; }

        [NotMapped]
        public string Name { get; set; }
    }
}
