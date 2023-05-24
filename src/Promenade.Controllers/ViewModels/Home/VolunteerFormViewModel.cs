using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers.ViewModels.Home
{
    public class VolunteerFormViewModel
    {
        [DisplayName(i18n.Keys.Promenade.WeeklyAvailability)]
        [MaxLength(255)]
        public string Availability { get; set; }

        [Required]
        [DisplayName(i18n.Keys.Promenade.Email)]
        [MaxLength(255)]
        public string Email { get; set; }

        [DisplayName(i18n.Keys.Promenade.Experience)]
        [MaxLength(255)]
        public string Experience { get; set; }

        public int FormId { get; set; }

        public int LocationId { get; set; }
        public string LocationSlug { get; set; }

        [Required]
        [DisplayName(i18n.Keys.Promenade.Name)]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        [DisplayName(i18n.Keys.Promenade.PromptPhone)]
        [MaxLength(255)]
        public string Phone { get; set; }

        [DisplayName(i18n.Keys.Promenade.VolunteerRegularity)]
        [MaxLength(255)]
        public string Regularity { get; set; }

        public string SegmentHeader { get; set; }
        public string SegmentText { get; set; }
        public string WarningText { get; set; }

        public virtual VolunteerFormSubmission ToFormSubmission()
        {
            return new VolunteerFormSubmission
            {
                Name = Name,
                Phone = Phone,
                Email = Email,
                Availability = Availability,
                Experience = Experience,
                Regularity = Regularity,
                FormId = FormId,
                LocationId = LocationId,
            };
        }
    }
}