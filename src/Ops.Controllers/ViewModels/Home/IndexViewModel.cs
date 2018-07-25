using System.Collections.Generic;
using Ocuda.Ops.Models;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.ViewModels.Home
{
    public class IndexViewModel
    {
        public IEnumerable<Post> Posts { get; set; }
        public IEnumerable<Link> Links { get; set; }
        public IEnumerable<File> Files { get; set; }
        public IEnumerable<Calendar> Calendars { get; set; }
        public PaginateModel PaginateModel { get; set; }
        public Section Section { get; set; }
    }
}
