using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Section
{
    public class FileLibraryViewModel
    {
        public PaginateModel PaginateModel { get; set; }

        [Required]
        public string SectionStub { get; set; }

        [Required]
        public string SectionName { get; set; }

        [Required]
        [DisplayName("FileLibrary Stub")]
        public string FileLibraryStub { get; set; }

        [Required]
        [DisplayName("FileLibrary Name")]
        public string FileLibraryName { get; set; }

        [Required]
        public int FileLibraryId { get; set; }

        public ICollection<FileType> FileTypes { get; set; }

        public ICollection<File> Files { get; set; }

        public File File { get; set; }

        [Required]
        public IFormFile UploadFile { get; set; }
    }
}
