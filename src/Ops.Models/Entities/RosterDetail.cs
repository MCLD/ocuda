using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Entities
{
    public class RosterDetail : Abstract.BaseEntity
    {
        [Required]
        public int DivisionId { get; set; }

        [MaxLength(255)]
        [Required]
        public string DivisionName { get; set; }

        [MaxLength(255)]
        public string EmailAddress { get; set; }

        public int? EmployeeId { get; set; }

        [MaxLength(255)]
        public string JobTitle { get; set; }

        [Required]
        public int LocationId { get; set; }

        [MaxLength(255)]
        [Required]
        public string LocationName { get; set; }

        [MaxLength(255)]
        [Required]
        public string Name { get; set; }

        public int? PositionNum { get; set; }

        public int? ReportsToId { get; set; }

        [MaxLength(255)]
        public string ReportsToName { get; set; }

        public int? ReportsToPos { get; set; }
        public RosterHeader RosterHeader { get; set; }

        [Required]
        public int RosterHeaderId { get; set; }
    }
}