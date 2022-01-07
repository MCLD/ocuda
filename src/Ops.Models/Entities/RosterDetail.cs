using System;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Entities
{
    public class RosterDetail : Abstract.BaseEntity
    {
        public DateTime AsOf { get; set; }

        [MaxLength(255)]
        public string EmailAddress { get; set; }

        public int EmployeeId { get; set; }
        public DateTime? HireDate { get; set; }
        public bool IsVacant { get; set; }

        [MaxLength(255)]
        public string JobTitle { get; set; }

        [MaxLength(255)]
        [Required]
        public string Name { get; set; }

        public int PositionNum { get; set; }
        public DateTime? RehireDate { get; set; }
        public int? ReportsToId { get; set; }
        public int? ReportsToPos { get; set; }
        public RosterHeader RosterHeader { get; set; }
        public int RosterHeaderId { get; set; }
        public int? Unit { get; set; }
    }
}
