using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Pages
{
    public class IndexViewModel
    {
        public ICollection<Page> Pages { get; set; }
        public PaginateModel PaginateModel { get; set; }
        public Page Page { get; set; }
    }
}
