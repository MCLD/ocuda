using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Ops.Models.Entities
{
    public class Thumbnail : Abstract.BaseEntity
    {
        public int FileId { get; set; }
        public File File { get; set; }

        public int FileTypeId { get; set; }
        public FileType FileType { get; set; }

        [NotMapped]
        public string Url { get; set; }
    }
}
