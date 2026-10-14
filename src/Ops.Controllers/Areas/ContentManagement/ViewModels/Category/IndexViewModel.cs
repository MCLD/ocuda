using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Category
{
    public class IndexViewModel : PaginateModel
    {
        public IEnumerable<Models.Entities.Category> Categories { get; set; }

        [Display(Name = "Category Name")]
        [MaxLength(255)]
        [Required]
        public string CategoryName { get; set; }

        public string CategorySectionLink { get; set; }

        public string CategorySectionMappingJson { get; set; }

        [Display(Name = "Category Slug")]
        [MaxLength(255)]
        [Required]
        public string CategorySlug { get; set; }

        public bool HasManagementPermission { get; set; }

        public int? Page { get; set; }
    }
}
