using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Ops.Models.Entities
{
    public class RenewCardResult
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Required]
        public int RenewCardRequestId { get; set; }

        public int? RenewCardResponseId { get; set; }
        public RenewCardResponse RenewCardResponse { get; set; }

        public DateTime CreatedAt { get; set; }

        [ForeignKey(nameof(CreatedByUser))]
        public int CreatedBy { get; set; }
        public User CreatedByUser { get; set; }

        public bool IsDiscarded { get; set; }

        [DisplayName("Email Text")]
        public string ResponseText { get; set; }
    }
}
