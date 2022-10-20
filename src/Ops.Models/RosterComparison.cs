using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Models
{
    public class RosterComparison
    {
        public IDictionary<int, string> NewLocations { get; set; }
        public ICollection<User> NewUsers { get; set; }
        public IEnumerable<RosterLocation> RemovedLocations { get; set; }
        public ICollection<User> RemovedUsers { get; set; }
        public RosterHeader RosterHeader { get; set; }
        public int TotalRecords { get; set; }
        public ICollection<User> UpdatedUsers { get; set; }
    }
}