using System;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers.ViewModels.Home
{
    public class AdultVolunteerFormViewModel : VolunteerFormViewModel
    {
        public bool TeenFormAvailable { get; set; }
    }
}