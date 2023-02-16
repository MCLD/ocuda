using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class VolunteerFormSubmission
    {
        [DisplayName(i18n.Keys.Promenade.WeeklyAvailability)]
        [MaxLength(255)]
        public string Availability { get; set; }

        public DateTime CreatedAt { get; set; }

        [Required]
        [DisplayName(i18n.Keys.Promenade.Email)]
        [MaxLength(255)]
        public string Email { get; set; }

        [DisplayName(i18n.Keys.Promenade.Experience)]
        [MaxLength(255)]
        public string Experience { get; set; }

        [Required]
        public int FormId { get; set; }

        [MaxLength(255)]
        public string GuardianEmail { get; set; }

        [MaxLength(255)]
        public string GuardianName { get; set; }

        [MaxLength(255)]
        public string GuardianPhone { get; set; }

        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public int LocationId { get; set; }

        [Required]
        [DisplayName(i18n.Keys.Promenade.Name)]
        [MaxLength(255)]
        public string Name { get; set; }

        [NotMapped]
        public int NotifyUserId { get; set; }

        [Required]
        [DisplayName(i18n.Keys.Promenade.PromptPhone)]
        [MaxLength(255)]
        public string Phone { get; set; }

        [DisplayName(i18n.Keys.Promenade.VolunteerRegularity)]
        [MaxLength(255)]
        public string Regularity { get; set; }

        public DateTime StaffNotifiedAt { get; set; }
    }
}