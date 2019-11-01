using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Entities
{
    public class FileLibrary : Abstract.BaseEntity
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        public ICollection<FileLibraryFileType> FileTypes { get; set; }

        public ICollection<File> Files { get; set; }

        public string Stub { get; set; }
    }
}
