using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers.ViewModels.EmployeeSignup
{
    public class EmployeeSignupViewModel
    {
        public EmployeeCardRequest CardRequest { get; set; }
        public SegmentText SegmentText { get; set; }
        public SelectList Departments { get; set; }
    }
}
