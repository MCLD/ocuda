using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers.ViewModels.Services
{
    public class EmployeeCardViewModel
    {
        public EmployeeCardRequest CardRequest { get; set; }
        public SegmentText Segment { get; set; }
    }
}
