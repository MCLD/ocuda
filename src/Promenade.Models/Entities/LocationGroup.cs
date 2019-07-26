using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class LocationGroup
    {
        [Required]
        public int Id { get; set; }

        public int LocationId { get; set; }

        public int GroupId { get; set; }

        public bool HasSubscription { get; set; }
    }
}
