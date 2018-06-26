using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Pages
{
    public class DetailViewModel
    {
        public Page Page { get; set; }
        public string Action { get; set; }
        public int SectionId { get; set; }
    }
}
