using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class Group
    {
        [Required]
        public int Id { get; set; }

        public string GroupType { get; set; }

        public bool IsLocationRegion { get; set; }

        public string SubscriptionUrl { get; set; }
    }
}
