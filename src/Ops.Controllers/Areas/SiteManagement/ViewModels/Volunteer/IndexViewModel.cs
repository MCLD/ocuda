using System.Collections.Generic;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Volunteer
{
    public class IndexViewModel : PaginateModel
    {
        public IndexViewModel()
        {
            VolunteerForms = new List<DetailsViewModel>();
        }

        public ICollection<DetailsViewModel> VolunteerForms { get; }
    }
}