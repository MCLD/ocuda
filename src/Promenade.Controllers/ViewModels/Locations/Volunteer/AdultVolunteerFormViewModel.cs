using System;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers.ViewModels.Locations.Volunteer
{
    public class AdultVolunteerFormViewModel : VolunteerFormViewModel
    {
        public bool TeenFormAvailable { get; set; }
    }
}