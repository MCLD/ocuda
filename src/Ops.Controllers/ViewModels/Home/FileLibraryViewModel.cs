using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.ViewModels.Home
{
    public class FileLibraryViewModel : PaginateModel
    {
        private bool managementRights;
        private bool replaceRights;

        public IDictionary<int, List<File>> ByFileYear
        {
            get
            {
                return Files != null
                    ? Files.GroupBy(_ => _.FileDate?.Year ?? 0)
                        .ToDictionary(_ => _.Key, _ => _.ToList())
                    : [];
            }
        }

        public File File { get; set; }

        [Required]
        public int FileLibraryId { get; set; }

        [Required]
        [DisplayName("File Library Name")]
        public string FileLibraryName { get; set; }

        [Required]
        [DisplayName("File Library Slug")]
        public string FileLibrarySlug { get; set; }

        public FileLibrarySort FileLibrarySortOrder { get; set; }

        public ICollection<File> Files { get; set; }

        public ICollection<FileType> FileTypes { get; set; }

        public string GetFileDetailsLink { get; set; }

        public bool HasAdminRights { get; set; }

        public bool HasManagementRights
        {
            get
            {
                return HasAdminRights || managementRights;
            }

            set
            {
                managementRights = value;
            }
        }

        public bool HasReplaceRights
        {
            get
            {
                return HasAdminRights || HasManagementRights || replaceRights;
            }

            set
            {
                replaceRights = value;
            }
        }

        public long MaximumFileUploadBytes { get; set; }

        public int ReplaceFileId { get; set; }

        [Required]
        public string SectionName { get; set; }

        [Required]
        public string SectionSlug { get; set; }

        [Required]
        [DisplayName("file")]
        public IFormFile UploadFile { get; set; }

        public bool UseThumbnails { get; set; }

        public static IDictionary<int, FileThumbnail> GetThumbnails(File file)
        {
            int key = 0;
            return file.Thumbnails?.OrderBy(_ => _.File).ToDictionary(_ => key++, v => v);
        }

        public static string ValidClass(ModelValidationState state) =>
            state == ModelValidationState.Invalid ? "input-validation-error" : null;

        public string JoinFileExtensions(string separator)
        {
            return string.Join(separator, FileTypes.Select(_ => _.Extension).ToList());
        }

        public string JsArrayFileExtensions()
        {
            return $"[{string.Join(",", FileTypes.Select(_ => $"'{_.Extension}'"))}]";
        }
    }
}
