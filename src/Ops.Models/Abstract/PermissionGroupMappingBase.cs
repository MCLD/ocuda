using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Abstract
{
    public abstract class PermissionGroupMappingBase
    {
        [Required]
        public int PermissionGroupId { get; set; }
    }
}