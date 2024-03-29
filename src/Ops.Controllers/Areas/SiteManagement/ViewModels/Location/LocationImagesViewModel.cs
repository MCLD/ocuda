using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Location
{
    public class LocationImagesViewModel : LocationPartialViewModel
    {
        public Promenade.Models.Entities.Location Location { get; set; }
        public IFormFile Image { get; set; }
        public List<Language> Languages { get; set; }
        public List<LocationInteriorImage> InteriorImages { get; set; }
        public LocationInteriorImage NewInteriorImage { get; set; }
        public LocationInteriorImage UpdatedInteriorImage { get; set; }
    }
}
