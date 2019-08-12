using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Location
{
    public class FeatureListViewModel
    {
        public IEnumerable<Feature> Features { get; set; }
        public PaginateModel PaginateModel { get; set; }
    }
}
