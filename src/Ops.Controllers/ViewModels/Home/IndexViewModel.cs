using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.ViewModels.Home
{
    public class IndexViewModel : PaginateModel
    {
        public IndexViewModel()
        {
            FileLibraries = new List<FileLibrary>();
            LinkLibraries = new List<LinkLibrary>();
            Posts = new List<Post>();
        }

        public ICollection<FileLibrary> FileLibraries { get; }

        public string LibraryClasses
        {
            get
            {
                return Posts?.Count > 0
                    ? "col-lg-4"
                    : "col-sm-8 offset-sm-2";
            }
        }

        public ICollection<LinkLibrary> LinkLibraries { get; }

        public string PostClasses
        {
            get
            {
                return FileLibraries?.Count > 0 || LinkLibraries?.Count > 0
                    ? "col-lg-8"
                    : "col-sm-8 offset-sm-2";
            }
        }

        public ICollection<Post> Posts { get; }
        public bool SectionManager { get; set; }
        public string SectionName { get; set; }
        public string SectionSlug { get; set; }
        public bool SupervisorsOnly { get; set; }
    }
}