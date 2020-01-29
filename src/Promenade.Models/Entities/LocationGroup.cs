using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class LocationGroup
    {
        [Key]
        [Required]
        public int LocationId { get; set; }

        public Location Location { get; set; }

        [Key]
        [Required]
        public int GroupId { get; set; }

        public Group Group { get; set; }

        public bool HasSubscription { get; set; }

        public int DisplayOrder { get; set; }
    }
}
