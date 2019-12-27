using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ocuda.Promenade.Models.Abstract;

namespace Ocuda.Promenade.Models.Entities
{
    public class Group : BaseEntity
    {
        [Required]
        [MaxLength(255)]
        public string Stub { get; set; }

        [MaxLength(255)]
        public string GroupType { get; set; }

        public bool IsLocationRegion { get; set; }

        [MaxLength(255)]
        public string SubscriptionUrl { get; set; }

        [MaxLength(255)]
        public string MapImage { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        [NotMapped]
        public bool IsNewGroup { get; set; }
    }
}
