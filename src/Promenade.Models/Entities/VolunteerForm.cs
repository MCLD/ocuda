using System.Collections.Generic;
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

        [DisplayName("Email setup for multiple staff notifications")]
        public int? NotfiyStaffOverflowEmailSetupId { get; set; }

        [DisplayName("Email setup used to notify staff")]
        public int? NotifyStaffEmailSetupId { get; set; }

        public PageHeader ThanksPageHeader { get; set; }

        public int? ThanksPageHeaderId { get; set; }
        public ICollection<VolunteerFormSubmission> VolunteerFormSubmissions { get; }

        [DisplayName("Volunteer Type")]
        public VolunteerFormType VolunteerFormType { get; set; }
    }
}