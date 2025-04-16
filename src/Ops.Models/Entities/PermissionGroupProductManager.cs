using System.ComponentModel.DataAnnotations;
using Ocuda.Ops.Models.Abstract;

namespace Ocuda.Ops.Models.Entities
{
    public class PermissionGroupProductManager : PermissionGroupMappingBase
    {
        [Required]
        public int ProductId { get; set; }
    }
}