using System.Collections.Generic;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Controllers.ViewModels.Profile
{
    public class IndexViewModel
    {
        public User User { get; set; }
        public ICollection<User> DirectReports { get; set; }
        public bool CanEdit { get; set; }
    }
}
