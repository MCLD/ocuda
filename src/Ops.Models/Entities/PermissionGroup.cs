using System.ComponentModel.DataAnnotations;
using Ocuda.Ops.Models.Abstract;

namespace Ocuda.Ops.Models.Entities
{
    public class PermissionGroup : BaseEntity
    {
        [Required]
        [MaxLength(255)]
        [Display(Name = "Permission Group Name")]
        public string PermissionGroupName { get; set; }

        [Required]
        [MaxLength(255)]
        [Display(Name = "AD Group Name")]
        public string GroupName { get; set; }
    }
}
