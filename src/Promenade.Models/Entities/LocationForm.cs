using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class LocationForm
    {
        [ForeignKey("FormId")]
        public VolunteerForm Form { get; set; }

        public int FormId { get; set; }

        [ForeignKey("LocationId")]
        public Location Location { get; set; }

        public int LocationId { get; set; }
    }
}