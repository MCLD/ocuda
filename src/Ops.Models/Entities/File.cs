using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Entities
{
    public class File : Abstract.BaseEntity
    {
        public int FileLibraryId { get; set; }
        public FileLibrary FileLibrary { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        public int FileTypeId { get; set; }
        public FileType FileType { get; set; }
    }
}
