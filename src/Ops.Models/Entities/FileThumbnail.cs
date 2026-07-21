using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Ops.Models.Entities
{
    public class FileThumbnail : Abstract.BaseEntity
    {
        public File File { get; set; }

        public int FileId { get; set; }

        [MaxLength(255)]
        [Required]
        public string ThumbnailFile { get; set; }

        [NotMapped]
        public string Link { get; set; }
    }
}
