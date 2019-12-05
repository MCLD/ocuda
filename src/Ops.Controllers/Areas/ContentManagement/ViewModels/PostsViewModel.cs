using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels
{
    public class PostsViewModel
    {
        public PaginateModel PaginateModel { get; set; }

        public string SectionStub { get; set; }

        public string SectionName { get; set; }

        public List<Category> SectionCategories { get; set; }

        public List<Post> SectionsPosts { get; set; }

        public List<PostCategory> PostCategories { get; set; }

        public ICollection<Post> AllCategoryPosts { get; set; }

    }
}
