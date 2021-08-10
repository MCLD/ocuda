using System.ComponentModel.DataAnnotations;
using Ocuda.Ops.Models.Abstract;

namespace Ocuda.Ops.Models.Entities
{
    public class PermissionGroupSectionManager : PermissionGroupMappingBase
    {
        public Section Section { get; set; }

        [Required]
        public int SectionId { get; set; }
    }
}