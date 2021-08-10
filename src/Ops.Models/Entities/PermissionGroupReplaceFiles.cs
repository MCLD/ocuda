using System.ComponentModel.DataAnnotations;
using Ocuda.Ops.Models.Abstract;

namespace Ocuda.Ops.Models.Entities
{
    public class PermissionGroupReplaceFiles : PermissionGroupMappingBase
    {
        public FileLibrary FileLibrary { get; set; }

        [Required]
        public int FileLibraryId { get; set; }
    }
}