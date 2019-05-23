using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Entities
{
    public class FileLibrary : Abstract.BaseEntity
    {
        public int SectionId { get; set; }
        public Section Section { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        public ICollection<FileLibraryFileType> FileTypes { get; set; }

        public ICollection<File> Files { get; set; }
    }
}
