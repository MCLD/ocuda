using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Location
{
    public class LocationVolunteerMappingViewModel
    {
        public LocationVolunteerMappingViewModel(VolunteerFormUserMapping model)
        {
            Name = model.User.Name;
            UserId = model.UserId;
        }

        public string Name { get; set; }
        public int UserId { get; set; }
    }
}