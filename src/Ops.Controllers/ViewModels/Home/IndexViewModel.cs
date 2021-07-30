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
        }

        public ICollection<FileLibrary> FileLibraries { get; }
        public ICollection<Post> Posts { get; set; }
    }
}