using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.ViewModels.Locations
{
    public class UpdateMapImageViewModel
    {
        public Location Location { get; set; }

        public string MapApiKey { get; set; }
    }
}