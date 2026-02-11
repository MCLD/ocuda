using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ocuda.Promenade.Models.Abstract;

namespace Ocuda.Ops.Models.Entities
{
    public class EmployeeCardResult : BaseEmployeeCard
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
        public int EmployeeCardRequestId { get; set; }

        [NotMapped]
        public string DepartmentName { get; set; }

        public DateTime? ProcessedAt { get; set; }

        [ForeignKey(nameof(ProcessedByUser))]
        public int? ProcessedBy { get; set; }
        public User ProcessedByUser { get; set; }

        public ResultType Type { get; set; }

        public enum ResultType
        {
            CardCreated,
            Processed,
            ProcessedNoEmail
        }
    }
}
