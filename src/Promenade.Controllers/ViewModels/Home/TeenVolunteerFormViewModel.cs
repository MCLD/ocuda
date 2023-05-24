using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers.ViewModels.Home
{
    public class TeenVolunteerFormViewModel : VolunteerFormViewModel
    {
        public bool AdultFormAvailable { get; set; }

        [Required]
        [DisplayName(i18n.Keys.Promenade.GuardianEmail)]
        [MaxLength(255)]
        public string GuardianEmail { get; set; }

        [Required]
        [DisplayName(i18n.Keys.Promenade.GuardianName)]
        [MaxLength(255)]
        public string GuardianName { get; set; }

        [Required]
        [DisplayName(i18n.Keys.Promenade.GuardianPhone)]
        [MaxLength(255)]
        public string GuardianPhone { get; set; }

        public override VolunteerFormSubmission ToFormSubmission()
        {
            var submission = base.ToFormSubmission();
            submission.GuardianEmail = GuardianEmail;
            submission.GuardianName = GuardianName;
            submission.GuardianPhone = GuardianPhone;
            return submission;
        }
    }
}