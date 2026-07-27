using System;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Entities
{
    public class EmployeeCardStats
    {
        [Required]
        public int Accepted { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Key]
        [Required]
        public int Month { get; set; }

        [Required]
        public int Processed { get; set; }

        [Required]
        public int ProcessedNoEmail { get; set; }

        [Required]
        public int Renewal { get; set; }

        [Required]
        public int Total { get; set; }

        [Key]
        [Required]
        public int Year { get; set; }
    }
}
