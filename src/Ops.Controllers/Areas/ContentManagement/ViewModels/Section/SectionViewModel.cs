using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Section
{
    public class SectionViewModel
    {
        public SectionViewModel()
        {
            FileLibraries = [];
            LinkLibraries = [];
            Posts = [];
        }

        public bool CanBeDeleted { get; set; }

        public ICollection<FileLibrary> FileLibraries { get; }

        public FileLibrary FileLibrary { get; set; }

        public ICollection<LinkLibrary> LinkLibraries { get; }

        public LinkLibrary LinkLibrary { get; set; }

        public int PostCount { get; set; }

        public ICollection<Post> Posts { get; }

        public Models.Entities.Section Section { get; set; }
    }
}
