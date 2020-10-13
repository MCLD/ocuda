using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Entities
{
    public class PermissionGroupApplication
    {
        [Required]
        [MaxLength(255)]
        public string ApplicationPermission { get; set; }

        [Required]
        public int PermissionGroupId { get; set; }
        public PermissionGroup PermissionGroup { get; set; }
    }
}
