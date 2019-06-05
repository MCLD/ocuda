using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Posts
{
    public class IndexViewModel
    {
        public IEnumerable<Post> Posts { get; set; }
        public PaginateModel PaginateModel { get; set; }
        public int SectionId { get; set; }
        public int? CategoryId { get; set; }
        public Post Post { get; set; }
        public PostCategory Category { get; set; }

        public SelectList CategoryList { get; set; }
    }
}
