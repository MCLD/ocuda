using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class SegmentText
    {
        [MaxLength(255)]
        [DisplayName("Header (optional)")]
        public string Header { get; set; }

        public Language Language { get; set; }

        [Key]
        [Required]
        [DisplayName("Language")]
        public int LanguageId { get; set; }

        public Segment Segment { get; set; }

        [Key]
        [Required]
        public int SegmentId { get; set; }

        public string Text { get; set; }
    }
}