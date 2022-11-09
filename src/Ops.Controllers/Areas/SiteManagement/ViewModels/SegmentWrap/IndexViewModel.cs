using System.Collections.Generic;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.SegmentWrap
{
    public class IndexViewModel : PaginateModel
    {
        public ICollection<Promenade.Models.Entities.SegmentWrap> SegmentWraps { get; set; }
    }
}