using System;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Location
{
    public class VolunteerFormSubmissionDetailsViewModel
    {
        public VolunteerFormSubmissionDetailsViewModel(VolunteerFormSubmission volunteerFormSubmission)
        {
            ArgumentNullException.ThrowIfNull(volunteerFormSubmission);

            Availability = volunteerFormSubmission.Availability;
            CreatedAt = volunteerFormSubmission.CreatedAt;
            Email = volunteerFormSubmission.Email;
            Experience = volunteerFormSubmission.Experience;
            GuardianEmail = volunteerFormSubmission?.GuardianEmail;
            GuardianName = volunteerFormSubmission?.GuardianName;
            GuardianPhone = volunteerFormSubmission?.GuardianPhone;
            Name = volunteerFormSubmission.Name;
            Phone = volunteerFormSubmission.Phone;
            Regularity = volunteerFormSubmission?.Regularity;
        }

        public string Availability { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Email { get; set; }
        public string Experience { get; set; }
        public string GuardianEmail { get; set; }
        public string GuardianName { get; set; }
        public string GuardianPhone { get; set; }
        public bool IsTeen { get; set; }
        public string LocationStub { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Regularity { get; set; }
    }
}