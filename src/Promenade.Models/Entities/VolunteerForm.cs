using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class VolunteerForm
    {
        public int? HeaderSegmentId { get; set; }

        [Key]
        public int Id { get; set; }

        public bool IsDisabled { get; set; }

        [DisplayName("Volunteer Type")]
        public VolunteerFormType VolunteerFormType { get; set; }
    }
}