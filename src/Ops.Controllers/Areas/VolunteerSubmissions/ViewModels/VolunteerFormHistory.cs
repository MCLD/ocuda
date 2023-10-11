using System;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.VolunteerSubmissions.ViewModels
{
    public class VolunteerFormHistory
    {
        public string Text { get; set; }
        public DateTime Timestamp { get; set; }
        public User User { get; set; }
    }
}