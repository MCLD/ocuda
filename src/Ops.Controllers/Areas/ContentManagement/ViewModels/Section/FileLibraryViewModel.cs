using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.ContentManagement.ViewModels.Section
{
    public class FileLibraryViewModel : PaginateModel
    {
        private bool _replaceRights;

        public File File { get; set; }

        [Required]
        public int FileLibraryId { get; set; }

        [Required]
        [DisplayName("File Library Name")]
        public string FileLibraryName { get; set; }

        [Required]
        [DisplayName("File Library Stub")]
        public string FileLibraryStub { get; set; }

        public ICollection<File> Files { get; set; }
        public ICollection<FileType> FileTypes { get; set; }
        public bool HasAdminRights { get; set; }

        public bool HasReplaceRights
        {
            set
            {
                _replaceRights = value;
            }
            get
            {
                return HasAdminRights || _replaceRights;
            }
        }

        public int ReplaceFileId { get; set; }

        [Required]
        public string SectionName { get; set; }

        [Required]
        public string SectionStub { get; set; }

        [Required]
        [DisplayName("file")]
        public IFormFile UploadFile { get; set; }
    }
}