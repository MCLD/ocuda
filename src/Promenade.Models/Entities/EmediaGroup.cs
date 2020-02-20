using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class EmediaGroup
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [MaxLength(255)]
        [Required]
        public string Name { get; set; }

        public int SortOrder { get; set; }

        public int? SegmentId { get; set; }
        public Segment Segment { get; set; }

        public ICollection<Emedia> Emedias { get; set; }
    }
}
