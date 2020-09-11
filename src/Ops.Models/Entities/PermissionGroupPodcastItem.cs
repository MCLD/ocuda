using System.ComponentModel.DataAnnotations;
using Ocuda.Ops.Models.Abstract;

namespace Ocuda.Ops.Models.Entities
{
    public class PermissionGroupPodcastItem : PermissionGroupMappingBase
    {
        [Required]
        public int PodcastId { get; set; }
    }
}
