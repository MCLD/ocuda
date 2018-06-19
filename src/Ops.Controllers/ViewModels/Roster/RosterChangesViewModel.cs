using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Controllers.ViewModels.Roster
{
    public class RosterChangesViewModel
    {
        public RosterDetail RosterDetail { get; set; }
        public IEnumerable<RosterEntry> NewEmployees { get; set; }
        public IEnumerable<RosterEntry> RemovedEmployees { get; set; }
    }
}
