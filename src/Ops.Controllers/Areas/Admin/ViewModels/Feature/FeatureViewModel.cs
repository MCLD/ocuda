using System.Collections.Generic;
using Ocuda.Ops.Service.Models;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Feature
{
    public class FeatureViewModel
    {
        public ICollection<Promenade.Models.Entities.Feature> AllFeatures { get; set; }

        public Promenade.Models.Entities.Feature Feature { get; set; }

        public PaginateModel PaginateModel { get; set; }
    }
}