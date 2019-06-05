using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.ViewModels.Posts
{
    public class IndexViewModel
    {
        public IEnumerable<Post> Posts { get; set; }
        public PaginateModel PaginateModel { get; set; }
    }
}
