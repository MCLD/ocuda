using System.ComponentModel.DataAnnotations;

namespace Ocuda.Ops.Models.Entities
{
    public class Link : Abstract.BaseEntity
    {
        public int LinkLibraryId { get; set; }
        public LinkLibrary LinkLibrary { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        [MaxLength(255)]
        public string Url { get; set; }

        [MaxLength(32)]
        public string Icon { get; set; }
    }
}
