using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels
{
    public class PostDetailsViewModel
    {
        public string SectionStub { get; set; }

        public string SectionName { get; set; }

        public Post Post { get; set; }

        public List<PostCategory> PostCategories { get; set; }

        public List<Category> SectionCategories { get; set; }

        public List<Post> SectionsPosts { get; set; }
    }
}
