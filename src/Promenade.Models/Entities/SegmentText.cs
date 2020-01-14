using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class SegmentText
    {
        [Key]
        [Required]
        public int SegmentId { get; set; }

        [NotMapped]
        public Segment Segment { get; set; }

        [Key]
        [Required]
        [DisplayName("Language")]
        public int LanguageId { get; set; }

        public Language Language { get; set; }

        [MaxLength(255)]
        public string Header { get; set; }

        [MaxLength(2000)]
        public string Text { get; set; }
    }
}
