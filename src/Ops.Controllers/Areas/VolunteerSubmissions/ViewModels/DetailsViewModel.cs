using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.VolunteerSubmissions.ViewModels
{
    public class DetailsViewModel : ViewModelBase
    {
        public DetailsViewModel()
        {
            VolunteerFormHistory = new List<VolunteerFormHistory>();
        }

        public int SelectedLocation { get; set; }
        public ICollection<VolunteerFormHistory> VolunteerFormHistory { get; }
        public VolunteerFormSubmission VolunteerFormSubmission { get; set; }
    }
}