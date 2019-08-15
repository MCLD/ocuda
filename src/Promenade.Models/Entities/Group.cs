using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class Group
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string Stub { get; set; }

        public string GroupType { get; set; }

        public bool IsLocationRegion { get; set; }

        public string SubscriptionUrl { get; set; }

        [NotMapped]
        public bool IsNewGroup { get; set; }
    }
}
