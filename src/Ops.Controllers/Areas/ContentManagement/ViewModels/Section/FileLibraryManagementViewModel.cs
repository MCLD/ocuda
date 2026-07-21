using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Section
{
    public class FileLibraryManagementViewModel
    {
        public string DisplayName
        {
            get
            {
                return IsNew ? "New File Library" : Name;
            }
        }

        public bool IsNew { get; set; }

        public string GetAvailableTypesLink { get; set; }

        public int FileCount { get; set; }

        public string SectionName { get; set; }

        public string SectionSlug { get; set; }

        public string FileLibrarySlug { get; set; }

        public IEnumerable<SelectListItem> SortOrderOptions { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        [MaxLength(255)]

        public string Slug { get; set; }

        [Display(Name = "Display on front page of section")]

        public bool IsFeatured { get; set; }

        [Display(Name = "Sort order of files")]

        public FileLibrarySort SortOrder { get; set; }

        public IEnumerable<FileType> AssignedFileTypes { get; set; }
    }
}
