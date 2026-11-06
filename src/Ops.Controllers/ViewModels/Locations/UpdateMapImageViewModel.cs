using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.ViewModels.Locations
{
    public class UpdateMapImageViewModel
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public Location Location { get; set; }

        public string MapApiKey { get; set; }
        public string ExistingMapImage { get; set; }
    }
}
