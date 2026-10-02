using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Category
{
    public class IndexViewModel
    {
        public IEnumerable<Models.Entities.Category> Categories { get; set; }

        public int? Page { get; set; }

        [Display(Name = "Category Name")]
        [MaxLength(255)]
        [Required]
        public string CategoryName { get; set; }

        [Display(Name = "Category Slug")]
        [MaxLength(255)]
        [Required]
        public string CategorySlug { get; set; }
    }
}
