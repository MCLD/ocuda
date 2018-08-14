using System;
using System.Collections.Generic;
using System.Text;

namespace Ocuda.Ops.Models
{
    public class NavigationRow
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public List<NavigationRow> Rows { get; set; }
    }
}
