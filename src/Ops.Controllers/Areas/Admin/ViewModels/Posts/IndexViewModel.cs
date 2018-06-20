using System.Collections.Generic;
using Ocuda.Ops.Models;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Posts
{
    public class IndexViewModel
    {
        public IEnumerable<Post> Posts { get; set; }
        public Post Post { get; set; }
        public PaginateModel PaginateModel { get; set; }
    }
}
