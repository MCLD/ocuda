using System.ComponentModel.DataAnnotations;
using Ocuda.Ops.Models.Abstract;

namespace Ocuda.Ops.Models.Entities
{
    public class SectionManagerGroup : BaseEntity
    {
        [Required]
        [MaxLength(255)]
        public string SectionName { get; set; }

        [Required]
        [MaxLength(255)]
        public string GroupName { get; set; }

    }
}
