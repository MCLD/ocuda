using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Entities
{
    public class File : Abstract.BaseEntity
    {
        public int? FileLibraryId { get; set; }
        public FileLibrary FileLibrary { get; set; }

        public int? PageId { get; set; }
        public Page Page { get; set; }

        public int? PostId { get; set; }
        public Post Post { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Description { get; set; }

        public int FileTypeId { get; set; }
        public FileType FileType { get; set; }

        public ICollection<Thumbnail> Thumbnails { get; set; }
    }
}
