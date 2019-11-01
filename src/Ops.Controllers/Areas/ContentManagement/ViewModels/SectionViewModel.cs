using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels
{
    public class SectionViewModel
    {
        public Section Section { get; set; }

        public Post Post { get; set; }

        public Category Category { get; set; }

        public List<Post> SectionsPosts { get; set; }

        public List<Section> UserSections { get; set; }

        public List<FileLibrary> FileLibraries { get; set; }

        public List<LinkLibrary> LinkLibraries { get; set; }

        public FileLibrary FileLibrary { get; set; }

        public LinkLibrary LinkLibrary { get; set; }

        public File File { get; set; }

        public Link Link { get; set; }

        public List<Category> SectionCategories { get; set; }

        public IFormFile UploadFile { get; set; }

        public PaginateModel PaginateModel { get; set; }

        public ICollection<Post> AllCategoryPosts { get; set; }

        public ICollection<Post> AllPosts { get; set; }

        public ICollection<Category> AllCategories { get; set; }

        public ICollection<File> Files { get; set; }

        public ICollection<FileType> FileTypes { get; set; }

        public ICollection<Link> Links { get; set; }

        public List<int> TypeIds { get; set; }

        public string Action { get; set; }
    }
}
