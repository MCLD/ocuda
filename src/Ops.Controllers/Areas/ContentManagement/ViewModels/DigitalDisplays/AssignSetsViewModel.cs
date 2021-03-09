using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.DigitalDisplays
{
    public class AssignSetsViewModel
    {
        public DigitalDisplay DigitalDisplay { get; set; }
        public IEnumerable<int> DisplaySets { get; set; }
        public ICollection<DigitalDisplaySet> Sets { get; set; }
    }
}