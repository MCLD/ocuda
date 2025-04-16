using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ocuda.Ops.Models.Abstract;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Models.Entities
{
    public class SiteSetting : BaseEntity
    {
        [Key]
        [Required]
        [MaxLength(255)]
        public new string Id { get; set; }

        [Column(TypeName = "int")]
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