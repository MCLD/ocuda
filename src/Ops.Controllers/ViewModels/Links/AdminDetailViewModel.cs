using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Controllers.ViewModels.Links
{
    public class AdminDetailViewModel
    {
        public Link Link { get; set; }
        public string Action { get; set; }
    }
}
