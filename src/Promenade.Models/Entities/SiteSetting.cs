using System.ComponentModel.DataAnnotations;
using Ocuda.Promenade.Models.Abstract;

namespace Ocuda.Promenade.Models.Entities
{
    public class SiteSetting : BaseEntity
    {
        [Required]
        [MaxLength(255)]
        public string Key { get; set; }

        public SiteSettingType Type { get; set; }

        // shown to user:
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Description { get; set; }

        [MaxLength(255)]
        public string Value { get; set; }

        [MaxLength(255)]
        public string Category { get; set; }
    }

    public enum SiteSettingType
    {
        Bool,
        Int,
        String,
        StringNullable
    }
}
