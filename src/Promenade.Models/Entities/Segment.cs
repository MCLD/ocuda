using System;
using System.ComponentModel.DataAnnotations.Schema;
using Ocuda.Promenade.Models.Abstract;

namespace Ocuda.Promenade.Models.Entities
{
    public class Segment : BaseEntity
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsActive { get; set; }

        [NotMapped]
        public SegmentText SegmentText { get; set; }
    }
}
