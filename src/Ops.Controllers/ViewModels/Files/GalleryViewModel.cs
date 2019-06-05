using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.ViewModels.Files
{
    // TODO
    public class GalleryViewModel
    {
        public IEnumerable<File> Files { get; set; }
        //public IEnumerable<Category> Categories { get; set; }
        public PaginateModel PaginateModel { get; set; }
        public string CategoryName { get; set; }
    }
}
