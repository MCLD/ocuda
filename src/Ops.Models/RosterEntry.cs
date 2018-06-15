using System;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models
{
    public class RosterEntry
    {
        public int Id { get; set; }

        public int RosterDetailId { get; set; }
        public RosterDetail RosterDetail { get; set; }

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
