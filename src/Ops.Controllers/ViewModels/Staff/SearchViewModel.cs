using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.ViewModels.Staff
{
    public class SearchViewModel : PaginateModel
    {
        public IDictionary<int, string> Locations { get; set; }
        public string SearchText { get; set; }
        public ICollection<User> Users { get; set; }
        public int AssociatedLocation { get; set; }
    }
}