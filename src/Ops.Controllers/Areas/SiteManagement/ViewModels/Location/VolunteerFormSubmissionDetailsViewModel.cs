using System;
using System.Globalization;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Location
{
    public class VolunteerFormSubmissionDetailsViewModel
    {
        public VolunteerFormSubmissionDetailsViewModel(VolunteerFormSubmission dm)
        {
            if (dm == null)
            {
                throw new ArgumentNullException(nameof(dm));
            }
            if (!string.IsNullOrWhiteSpace(dm.GuardianName))
            {
                GuardianName = dm.GuardianName;
            }
            if (!string.IsNullOrWhiteSpace(dm.GuardianEmail))
            {
                GuardianEmail = dm.GuardianEmail;
            }
            if (!string.IsNullOrWhiteSpace(dm.GuardianPhone))
            {
                GuardianPhone = dm.GuardianPhone;
            }
            if (!string.IsNullOrWhiteSpace(dm.Regularity))
            {
                Regularity = dm.Regularity;
            }
            CreatedAt = dm.CreatedAt.ToString("MMMM dd h:mm tt", new CultureInfo("en-US"));
            Name = dm.Name;
            Email = dm.Email;
            Phone = dm.Phone;
            Availability = dm.Availability;
            Experience = dm.Experience;
        }

        public string Availability { get; set; }
        public string CreatedAt { get; set; }
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