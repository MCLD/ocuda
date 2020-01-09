using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class LocationGroup
    {
        [Key]
        [Required]
        public int LocationId { get; set; }

        [Key]
        [Required]
        public int GroupId { get; set; }

        public bool HasSubscription { get; set; }
    }
}
