using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.ViewModels.Files
{
    public class LibraryViewModel
    {
        public IEnumerable<File> Files { get; set; }
        public PaginateModel PaginateModel { get; set; }
        public FileLibrary Library { get; set; }
    }
}
