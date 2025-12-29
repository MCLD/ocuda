using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ocuda.Ops.Service.Models.CardRenewal
{
    public class ProcessResult
    {
        public bool APIRenew { get; set; }
        public bool EmailNotUpdated { get; set; }
    }
}
