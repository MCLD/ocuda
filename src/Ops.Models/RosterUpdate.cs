using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Models
{
    public class RosterUpdate
    {
        public IList<User> New { get; set; }
        public IList<User> Deactivated { get; set; }
        public RosterHeader RosterDetail { get; set; }
        public int TotalRows { get; set; }
        public int VacantCount { get; set; }
        public IList<User> Verified { get; set; }
        public string NewClass
        {
            get
            {
                return New?.Count > 0
                    ? "table-success"
                    : null;
            }
        }
        public string DeactivatedClass
        {
            get
            {
                return Deactivated?.Count > 0
                    ? "table-success"
                    : null;
            }
        }

        public string VerifiedClass
        {
            get
            {
                return Verified?.Count > 0
                    ? "table-success"
                    : null;
            }
        }

        public string VacantClass
        {
            get
            {
                return VacantCount > 0
                    ? "table-success"
                    : null;
            }
        }

    }
}
