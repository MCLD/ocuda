using System;
using System.Collections.Generic;
using System.Text;

namespace Ocuda.Ops.Controllers.ViewModels.Section
{
    public class AdminDetailViewModel
    {
        public Models.Section Section { get; set; }
        public string Action { get; set; }
        public string IsReadonly { get; set; }
    }
}
