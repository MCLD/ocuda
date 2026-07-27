using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Ops.Models.Entities
{
    public class FileLibrary : Abstract.BaseEntity
    {
        public FileLibrary()
        {
            Files = [];
            FileTypes = [];
        }

        public ICollection<File> Files { get; }

        public ICollection<FileLibraryFileType> FileTypes { get; }

        [Required]
        public bool IsFeatured { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        public Section Section { get; set; }

        public int SectionId { get; set; }

        [MaxLength(255)]
        public string Slug { get; set; }

        [Required]
        public FileLibrarySort SortOrder { get; set; }

        [NotMapped]
        public int TotalFilesInLibrary { get; set; }
    }
}
