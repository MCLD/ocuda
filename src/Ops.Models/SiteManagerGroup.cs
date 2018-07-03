using System.ComponentModel.DataAnnotations;
using Ocuda.Ops.Models.Abstract;

namespace Ocuda.Ops.Models
{
    public class SiteManagerGroup : BaseEntity
    {
        [Required]
        public string GroupName { get; set; }
    }
}
