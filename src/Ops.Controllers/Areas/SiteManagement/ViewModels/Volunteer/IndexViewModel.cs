using System.Collections.Generic;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Volunteer
{
    public class IndexViewModel
    {
        public PaginateModel PaginateModel { get; set; }
        public ICollection<DetailsViewModel> VolunteerForms { get; set; }
    }
}