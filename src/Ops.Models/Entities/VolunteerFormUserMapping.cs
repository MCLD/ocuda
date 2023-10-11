using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Ops.Models.Entities
{
    public class VolunteerFormUserMapping
    {
        public int VolunteerFormId { get; set; }
        public int LocationId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        public int UserId { get; set; }
    }
}