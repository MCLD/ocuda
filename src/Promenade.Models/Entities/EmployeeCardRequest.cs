using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class EmployeeCardRequest
    {
        [Required]
        [DisplayName(i18n.Keys.Promenade.PromptBirthDate)]
        public DateTime BirthDate { get; set; }

        [DisplayName(i18n.Keys.Promenade.PromptLibraryCardNumber)]
        [MaxLength(16)]
        public string CardNumber { get; set; }

        [Required]
        [DisplayName(i18n.Keys.Promenade.PromptCity)]
        [MaxLength(255)]
        public string City { get; set; }

        [Required]
        [DisplayName(i18n.Keys.Promenade.PromptCountyDepartment)]
        [MaxLength(255)]
        public string Department { get; set; }

        [Required]
        [DisplayName(i18n.Keys.Promenade.PromptEmail)]
        [MaxLength(255)]
        public string Email { get; set; }

        [Required]
        [DisplayName(i18n.Keys.Promenade.PromptEmployeeNumber)]
        [MaxLength(16)]
        public string EmployeeNumber { get; set; }

        public bool ExistingAccount { get; set; }

        [Required]
        [DisplayName(i18n.Keys.Promenade.PromptFirstName)]
        [MaxLength(255)]
        public string FirstName { get; set; }

        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        [DisplayName(i18n.Keys.Promenade.PromptLastName)]
        [MaxLength(255)]
        public string LastName { get; set; }

        [MaxLength(2000)]
        public string Notes { get; set; }

        [Required]
        [DisplayName(i18n.Keys.Promenade.PromptPhone)]
        [MaxLength(16)]
        public string Phone { get; set; }

        public DateTime? ProcessedAt { get; set; }
        public int? ProcessedBy { get; set; }
        
        [Required]
        [DisplayName(i18n.Keys.Promenade.PromptStreetAddress)]
        [MaxLength(255)]
        public string StreetAddress { get; set; }

        public DateTime SubmittedAt { get; set; }

        [Required]
        [DisplayName(i18n.Keys.Promenade.PromptZipCode)]
        [MaxLength(16)]
        public string ZipCode { get; set; }
    }
}
