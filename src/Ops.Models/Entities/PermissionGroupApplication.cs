using System.ComponentModel.DataAnnotations;
using Ocuda.Ops.Models.Abstract;

namespace Ocuda.Ops.Models.Entities
{
    public class PermissionGroupApplication : PermissionGroupMappingBase
    {
        [Required]
        [MaxLength(255)]
        public string ApplicationPermission { get; set; }

        public PermissionGroup PermissionGroup { get; set; }
    }
}