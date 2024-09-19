using System.ComponentModel.DataAnnotations;
using Ocuda.Ops.Models.Abstract;

namespace Ocuda.Ops.Models.Entities
{
    public class PermissionGroupIncidentLocation : PermissionGroupMappingBase
    {
        [Required]
        public int LocationId { get; set; }
    }
}