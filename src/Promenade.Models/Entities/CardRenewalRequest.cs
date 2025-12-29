using System;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class CardRenewalRequest
    {
        [Required]
        public string Barcode { get; set; }

        [Required]
        public string Email { get; set; }

        public string GuardianBarcode { get; set; }
        public string GuardianName { get; set; }

        [Key]
        [Required]
        public int Id { get; set; }

        public bool IsDiscarded { get; set; }

        public int LanguageId { get; set; }
        public Language Language { get; set; }

        public int PatronId { get; set; }
        public DateTime? ProcessedAt {get;set;}
        public bool SameAddress { get; set; }
        public DateTime SubmittedAt { get; set; }
    }
}
