using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.ViewModels.Profile
{
    public class SearchViewModel : PaginateModel
    {
        public string SearchText { get; set; }
        public ICollection<User> Users { get; set; }
    }
}
