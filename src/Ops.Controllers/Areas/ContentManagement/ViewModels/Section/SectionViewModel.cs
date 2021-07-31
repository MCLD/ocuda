using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Section
{
    public class SectionViewModel
    {
        public List<Post> AllPosts { get; set; }
        public ICollection<FileLibrary> FileLibraries { get; set; }
        public FileLibrary FileLibrary { get; set; }
        public List<LinkLibrary> LinkLibraries { get; set; }
        public LinkLibrary LinkLibrary { get; set; }
        public PaginateModel PaginateModel { get; set; }
        public List<PostCategory> PostCategories { get; set; }
        public Models.Entities.Section Section { get; set; }

        public List<Category> SectionCategories { get; set; }
        public List<Post> SectionsPosts { get; set; }
    }
}