using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.VolunteerSubmissions.ViewModels
{
    public class ViewModelBase : PaginateModel
    {
        public ViewModelBase()
        {
            Heading = "Volunteer Submissions";
        }

        public string BackLink { get; set; }
        public string Heading { get; set; }
        public string SecondaryHeading { get; set; }
    }
}