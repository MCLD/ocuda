using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ocuda.Promenade.Models.Abstract;

namespace Ocuda.Promenade.Models.Entities
{
    public class Group : BaseEntity
    {
        [Required]
        public string Stub { get; set; }

        public string GroupType { get; set; }

        public bool IsLocationRegion { get; set; }

        public string SubscriptionUrl { get; set; }

        [NotMapped]
        public bool IsNewGroup { get; set; }
    }
}
