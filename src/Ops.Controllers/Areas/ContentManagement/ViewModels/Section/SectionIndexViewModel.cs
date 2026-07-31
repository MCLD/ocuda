using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Section
{
    public class SectionIndexViewModel
    {
        [Display(Name = "Icon")]
        [MaxLength(255)]
        [Required]

        public string SectionIcon { get; set; }

        [Display(Name = "Name")]
        [MaxLength(255)]
        [Required]

        public string SectionName { get; set; }

        public ICollection<Models.Entities.Section> Sections { get; set; }

        [Display(Name = "Slug")]
        [MaxLength(255)]
        [Required]
        public string SectionSlug { get; set; }

        [Display(Name = "Supervisors only")]
        [Required]
        public bool SupervisorsOnly { get; set; }
    }
}
