using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Controllers.ViewModels.Shared
{
    public class BaseViewModel
    {
        public IEnumerable<Section> Sections { get; set; }
    }
}
