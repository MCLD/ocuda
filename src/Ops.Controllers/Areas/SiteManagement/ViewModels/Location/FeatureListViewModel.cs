using System.Collections.Generic;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Location
{
    public class FeatureListViewModel
    {
        public IEnumerable<Promenade.Models.Entities.Feature> Features { get; set; }
        public PaginateModel PaginateModel { get; set; }
    }
}