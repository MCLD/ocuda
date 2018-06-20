using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Controllers.ViewModels.Sections
{
    public class AdminDetailViewModel
    {
        public Section Section { get; set; }
        public string Action { get; set; }
        public string IsReadonly { get; set; }
    }
}
