using System.Collections.Generic;
using Ocuda.Ops.Models;

namespace Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Roster
{
    public class ChangesViewModel
    {
        public RosterDetail RosterDetail { get; set; }
        public IEnumerable<RosterEntry> NewEmployees { get; set; }
        public IEnumerable<RosterEntry> RemovedEmployees { get; set; }
    }
}
