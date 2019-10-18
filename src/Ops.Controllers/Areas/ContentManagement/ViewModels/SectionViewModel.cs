using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels
{
    public class SectionViewModel
    {
        public Section Section { get; set; }

        public Post Post { get; set; }

        public PostCategory Category { get; set; }

        public List<Post> SectionsPosts { get; set; }

        public List<Section> UserSections { get; set; }

        public List<PostCategory> SectionCategories { get; set; }

        public PaginateModel PaginateModel { get; set; }

        public ICollection<Post> AllPostCategoryPosts { get; set; }

        public ICollection<Post> AllPosts { get; set; }

        public ICollection<PostCategory> AllPostCategories { get; set; }
    }
}
