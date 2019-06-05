using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Links
{
    public class IndexViewModel
    {
        public IEnumerable<LinkLibrary> Libraries { get; set; }
        public LinkLibrary LinkLibrary { get; set; }
        public PaginateModel PaginateModel { get; set; }
        public int SectionId { get; set; }
    }
}
