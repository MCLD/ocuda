using System.ComponentModel.DataAnnotations;
using Ocuda.Utility.Models;

namespace Ocuda.Promenade.Models.Entities
{
    public class SiteSetting
    {
        [Key]
        [Required]
        [MaxLength(255)]
        public string Id { get; set; }

        public SiteSettingType Type { get; set; }

        // shown to user:
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        [MaxLength(255)]
        public string Description { get; set; }

        [MaxLength(255)]
        public string Value { get; set; }

        [MaxLength(255)]
        public string Category { get; set; }
    }
}
