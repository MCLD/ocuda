using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels
{
    public class SectionViewModel
    {
        public Section Section { get; set; }

        public List<Post> SectionsPosts { get; set; }

        public FileLibrary FileLibrary { get; set; }

        public LinkLibrary LinkLibrary { get; set; }

        public List<Post> AllPosts { get; set; }

        public PaginateModel PaginateModel { get; set; }

        public List<PostCategory> PostCategories { get; set; }

        public List<Category> SectionCategories { get; set; }

        public List<FileLibrary> FileLibraries { get; set; }

        public List<LinkLibrary> LinkLibraries { get; set; }
    }
}
