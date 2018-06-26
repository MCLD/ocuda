using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Links
{
    public class DetailViewModel
    {
        public Link Link { get; set; }
        public IEnumerable<Category> Categories { get; set; }
        public string Action { get; set; }
        public int SectionId { get; set; }
    }
}
