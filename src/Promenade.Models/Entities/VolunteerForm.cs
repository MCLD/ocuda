using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class VolunteerForm
    {
        [NotMapped]
        public SegmentText HeaderSegment { get; set; }

        public int? HeaderSegmentId { get; set; }

        [Key]
        public int Id { get; set; }

        public bool IsDisabled { get; set; }

        [DisplayName("Volunteer Type")]
        public VolunteerFormType VolunteerFormType { get; set; }
    }
}