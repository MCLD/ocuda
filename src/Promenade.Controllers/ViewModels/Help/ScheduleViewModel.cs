using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers.ViewModels.Help
{
    public class ScheduleViewModel
    {
        [Required]
        public System.DateTime FirstAvailable { get; set; }

        [Required]
        [Display(Name = i18n.Keys.Promenade.PromptRequestedDate)]
        public System.DateTime RequestedDate { get; set; }

        [Required]
        [Display(Name = i18n.Keys.Promenade.PromptRequestedTime)]
        public System.DateTime RequestedTime { get; set; }

        public SegmentText SegmentText { get; set; }
        [Required]
        [Display(Name = i18n.Keys.Promenade.PromptSubject)]
        public int SubjectId { get; set; }

        public IEnumerable<SelectListItem> Subjects { get; set; }
        public IEnumerable<SelectListItem> TimeBlocks { get; set; }
        public string WarningText { get; set; }
    }
}