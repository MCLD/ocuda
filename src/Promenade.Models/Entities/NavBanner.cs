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

        [MaxLength(255)]
        public string Name { get; set; }

        [NotMapped]
        public NavBannerImage NavBannerImage { get; set; }

        [NotMapped]
        public ICollection<NavBannerLink> NavBannerLinks { get; set; }
    }
}