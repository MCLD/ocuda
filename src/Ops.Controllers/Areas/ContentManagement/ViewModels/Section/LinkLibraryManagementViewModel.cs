using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Section
{
    public class LinkLibraryManagementViewModel
    {
        public string DisplayName
        {
            get
            {
                return IsNew ? "New Link Library" : Name;
            }
        }

        public bool IsNew { get; set; }

        public int LinkCount { get; set; }

        public string SectionName { get; set; }

        public string SectionSlug { get; set; }

        public string LinkLibrarySlug { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        [MaxLength(255)]

        public string Slug { get; set; }

        [Display(Name = "Display on front page of section")]

        public bool IsFeatured { get; set; }
    }
}
