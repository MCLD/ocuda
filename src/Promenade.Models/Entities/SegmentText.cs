using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [NotMapped]
        public string SegmentWrapPrefix { get; set; }

        [NotMapped]
        public string SegmentWrapSuffix { get; set; }

        public string Text { get; set; }
    }
}