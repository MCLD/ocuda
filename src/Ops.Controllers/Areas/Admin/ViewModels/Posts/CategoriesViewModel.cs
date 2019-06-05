using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Posts
{
    public class CategoriesViewModel
    {
        public IEnumerable<PostCategory> Categories { get; set; }
        public PostCategory Category { get; set; }
        public PaginateModel PaginateModel { get; set; }
        public int SectionId { get; set; }
    }
}
