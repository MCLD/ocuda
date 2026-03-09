using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ocuda.Models;

namespace Ocuda.Ops.Models.Entities
{
    public class EmployeeCardResult : BaseEmployeeCard
    {
        public enum ResultType
        {
            CardCreated,
            Processed,
            ProcessedNoEmail
        }

        [NotMapped]
        public string DepartmentName { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
        public int EmployeeCardRequestId { get; set; }

        public DateTime? ProcessedAt { get; set; }

        [ForeignKey(nameof(ProcessedByUser))]
        public int? ProcessedBy { get; set; }

        public User ProcessedByUser { get; set; }

        public bool Renewal { get; set; }

        public ResultType Type { get; set; }
    }
}