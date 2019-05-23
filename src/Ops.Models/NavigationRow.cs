using System.Collections.Generic;

namespace Ocuda.Ops.Models
{
    public class NavigationRow
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public List<NavigationRow> Rows { get; set; }
    }
}
