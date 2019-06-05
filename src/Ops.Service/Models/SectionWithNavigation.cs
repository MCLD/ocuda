using System;
using System.Collections.Generic;
using System.Text;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Service.Models
{
    public class SectionWithNavigation
    {
        public Section Section { get; set; }
        public IEnumerable<Link> NavigationLinks { get; set; }
    }
}
