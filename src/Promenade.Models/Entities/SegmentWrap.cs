using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class SegmentWrap
    {
        [Required]
        [MaxLength(255)]
        public string Description { get; set; }

        [Key]
        [Required]
        public int Id { get; set; }

        public bool IsDeleted { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        public string Prefix { get; set; }
        public string Suffix { get; set; }

        [NotMapped]
        public int UsedByCount { get; set; }
    }
}