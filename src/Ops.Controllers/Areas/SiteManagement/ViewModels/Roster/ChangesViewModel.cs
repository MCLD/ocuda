﻿using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Roster
{
    public class ChangesViewModel
    {
        public RosterHeader RosterDetail { get; set; }
        public IEnumerable<RosterDetail> NewEmployees { get; set; }
        public IEnumerable<RosterDetail> RemovedEmployees { get; set; }
    }
}
