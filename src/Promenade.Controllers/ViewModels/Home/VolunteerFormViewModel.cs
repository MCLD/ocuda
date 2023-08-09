using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers.ViewModels.Home
{
    public class VolunteerFormViewModel
    {
        [DisplayName(i18n.Keys.Promenade.PromptWeeklyAvailability)]
        [MaxLength(255)]
        public string Availability { get; set; }

        [Required]
        [DisplayName(i18n.Keys.Promenade.PromptEmail)]
        [MaxLength(255)]
        public string Email { get; set; }

        [DisplayName(i18n.Keys.Promenade.PromptExperience)]
        [MaxLength(255)]
        public string Experience { get; set; }

        public int FormId { get; set; }

        public int LocationId { get; set; }
        public string LocationSlug { get; set; }

        [Required]
        [DisplayName(i18n.Keys.Promenade.PromptName)]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        [DisplayName(i18n.Keys.Promenade.PromptPhone)]
        [MaxLength(255)]
        public string Phone { get; set; }

        [DisplayName(i18n.Keys.Promenade.PromptVolunteerRegularity)]
        [MaxLength(255)]
        public string Regularity { get; set; }

        public string SegmentHeader { get; set; }
        public string SegmentText { get; set; }
        public string WarningText { get; set; }

        public virtual VolunteerFormSubmission ToFormSubmission()
        {
            return new VolunteerFormSubmission
            {
                Availability = Availability,
                Email = Email,
                Experience = Experience,
                VolunteerFormId = FormId,
                LocationId = LocationId,
                Name = Name,
                Phone = Phone,
                Regularity = Regularity
            };
        }
    }
}