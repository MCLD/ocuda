using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Ocuda.Ops.Models.Entities;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Section
{
    public class ManageThumbnailsViewModel
    {
        public int DeleteThumbnailId { get; set; }

        public string FileName { get; set; }

        public IEnumerable<FileThumbnail> Thumbnails { get; set; }

        public string SectionName { get; set; }

        public string FileLibraryName { get; set; }

        public string SectionSlug { get; set; }

        public string FileLibrarySlug { get; set; }

        public int FileId { get; set; }

        public IEnumerable<FileType> FileTypes { get; set; }

        [Required]
        [Display(Name = "Thumbnail file")]
        public IFormFile UploadFile { get; set; }

        public static string ValidClass(ModelValidationState state) =>
            state == ModelValidationState.Invalid ? "input-validation-error" : null;

        public string JoinFileExtensions(string separator)
        {
            return string.Join(separator, FileTypes.Select(_ => _.Extension).ToList());
        }
    }
}
