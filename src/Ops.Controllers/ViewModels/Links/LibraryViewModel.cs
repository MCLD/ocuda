using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.ViewModels.Links
{
    public class LibraryViewModel
    {
        public IEnumerable<Link> Links { get; set; }
        public PaginateModel PaginateModel { get; set; }
        public LinkLibrary Library { get; set; }
    }
}