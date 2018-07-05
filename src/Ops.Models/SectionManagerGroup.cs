using System.ComponentModel.DataAnnotations;
using Ocuda.Ops.Models.Abstract;

namespace Ocuda.Ops.Models
{
    public class SectionManagerGroup : BaseEntity
    {
        [Required]
        public string SectionName { get; set; }
        [Required]
        public string GroupName { get; set; }

    }
}
