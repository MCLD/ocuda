using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.ViewModels.Home
{
    public class IndexViewModel
    {
        public ICollection<Post> Posts { get; set; }
        public PaginateModel PaginateModel { get; set; }
    }
}
