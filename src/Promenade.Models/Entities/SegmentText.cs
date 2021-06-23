using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class SegmentText
    {
        [Key]
        [Required]
        public int SegmentId { get; set; }

        public Segment Segment { get; set; }

        [Key]
        [Required]
        [DisplayName("Language")]
        public int LanguageId { get; set; }

        public Language Language { get; set; }

        [MaxLength(255)]
        [DisplayName("Header (optional)")]
        public string Header { get; set; }

        [MaxLength(4000)]
        public string Text { get; set; }
    }
}
