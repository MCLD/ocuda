using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Entities
{
    public class PermissionGroupPageContent
    {
        [Required]
        public int PermissionGroupId { get; set; }

        [Required]
        public int PageHeaderId { get; set; }
    }
}
