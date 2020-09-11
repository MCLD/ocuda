using System.ComponentModel.DataAnnotations;
using Ocuda.Ops.Models.Abstract;

namespace Ocuda.Ops.Models.Entities
{
    public class PermissionGroupPageContent : PermissionGroupMappingBase
    {
        [Required]
        public int PageHeaderId { get; set; }
    }
}
