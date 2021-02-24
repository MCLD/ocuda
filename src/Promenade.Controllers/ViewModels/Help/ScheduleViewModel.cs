using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers.ViewModels.Help
{
    public class ScheduleViewModel
    {
        public SegmentText SegmentText { get; set; }
        public IEnumerable<SelectListItem> Subjects { get; set; }

        [Required]
        [Display(Name = "Requested date")]
        public System.DateTime RequestedDate { get; set; }

        [Required]
        [Display(Name = "Requested time")]
        public System.DateTime RequestedTime { get; set; }

        [Required]
        [Display(Name = "Subject")]
        public int SubjectId { get; set; }

        public string WarningText { get; set; }

        public IEnumerable<SelectListItem> TimeBlocks { get; set; }
    }
}
