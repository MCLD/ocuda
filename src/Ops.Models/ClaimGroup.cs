using System.ComponentModel.DataAnnotations;
using Ocuda.Ops.Models.Abstract;

namespace Ocuda.Ops.Models
{
    public class ClaimGroup : BaseEntity
    {
        [Required]
        [MaxLength(255)]
        public string ClaimType { get; set; }
        [Required]
        [MaxLength(255)]
        public string GroupName { get; set; }
    }
}
