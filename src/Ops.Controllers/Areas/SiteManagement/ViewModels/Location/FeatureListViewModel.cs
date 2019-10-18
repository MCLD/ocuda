using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Location
{
    public class FeatureListViewModel
    {
        public IEnumerable<Promenade.Models.Entities.Feature> Features { get; set; }
        public PaginateModel PaginateModel { get; set; }
    }
}
