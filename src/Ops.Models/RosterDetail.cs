using System;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models
{
    public class RosterDetail : Abstract.BaseEntity
    {
        public int RosterHeaderId { get; set; }
        public RosterHeader RosterHeader { get; set; }

        [MaxLength(255)]
        [Required]
        public string Name { get; set; }

        [MaxLength(255)]
        [Required]
        public string EmailAddress { get; set; }

        [MaxLength(255)]
        [Required]
        public string JobTitle { get; set; }

        public int EmployeeId { get; set; }
        public int PositionNum { get; set; }
        public int? ReportsToId { get; set; }
        public int? ReportsToPos { get; set; }

        public DateTime AsOf { get; set; }
    }
}
