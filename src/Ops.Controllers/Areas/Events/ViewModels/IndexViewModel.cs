using System.Collections.Generic;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Events.ViewModels
{
    public class IndexViewModel : PaginateModel
    {
        public IndexViewModel()
        {
            ScheduledEvents = new List<ScheduledEvent>();
            LocationNames = new Dictionary<int, string>();
        }

        public IDictionary<int, string> LocationNames { get; }
        public ICollection<ScheduledEvent> ScheduledEvents { get; }
    }
}