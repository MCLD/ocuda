using System.Collections.Generic;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Incident.ViewModel
{
    public class IndexViewModel : PaginateModel
    {
        public bool CanViewAll { get; set; }
        public ICollection<dynamic> Incidents { get; set; }
        public int Page { get; set; }
        public string SearchText { get; set; }
        public bool ViewingAll { get; set; }
    }
}
