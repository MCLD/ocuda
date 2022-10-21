using System.Collections.Generic;

namespace Ocuda.Ops.Models
{
    public class RosterUpdate
    {
        public RosterUpdate()
        {
            Issues = new List<string>();
        }

        public IList<string> Issues { get; }
        public int RosterHeaderId { get; set; }
        public int TotalRows { get; set; }
    }
}