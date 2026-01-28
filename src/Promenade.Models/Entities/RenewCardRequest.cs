using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ocuda.Models;

namespace Ocuda.Promenade.Models.Entities
{
    public class RenewCardRequest
    {
        [NotMapped]
        public bool Accepted { get; set; }

        [NotMapped]
        public IEnumerable<CustomerAddress> Addresses { get; set; }

        [Required]
        public string Barcode { get; set; }

        public int CustomerId { get; set; }

        [Required]
        public string Email { get; set; }

        public string GuardianBarcode { get; set; }
        public string GuardianName { get; set; }

        [Key]
        [Required]
        public int Id { get; set; }

        public bool IsDiscarded { get; set; }

        [NotMapped]
        public bool IsJuvenile { get; set; }

        public int LanguageId { get; set; }
        public Language Language { get; set; }

        public DateTime? ProcessedAt { get; set; }
        public bool SameAddress { get; set; }
        public DateTime SubmittedAt { get; set; }
    }
}
