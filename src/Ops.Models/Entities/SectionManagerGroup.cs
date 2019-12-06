using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ocuda.Ops.Models.Abstract;

namespace Ocuda.Ops.Models.Entities
{
    public class SectionManagerGroup : BaseEntity
    {
        [Required]
        [ForeignKey("Section")]
        public int SectionId { get; set; }

        public Section Section { get; set; }

        [Required]
        [MaxLength(255)]
        public string GroupName { get; set; }

    }
}
