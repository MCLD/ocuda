using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Ops.Models;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.ViewModels.Posts
{
    public class AdminListViewModel
    {
        public IEnumerable<Post> Posts { get; set; }
        public Post Post { get; set; }
        public PaginateModel PaginateModel { get; set; }
    }
}
