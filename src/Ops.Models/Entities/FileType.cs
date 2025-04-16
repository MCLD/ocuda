using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Entities
{
    public class FileType : Abstract.BaseEntity
    {
        [Required]
        [MaxLength(16)]
        public string Extension { get; set; }

        [MaxLength(32)]
        public string Icon { get; set; }
    }
}