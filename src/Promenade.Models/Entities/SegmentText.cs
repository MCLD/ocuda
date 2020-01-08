using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Ocuda.Promenade.Models.Abstract;

namespace Ocuda.Promenade.Models.Entities
{
    public class SegmentText : BaseEntity
    {
        public int SegmentId { get; set; }

        [NotMapped]
        public Segment Segment { get; set; }

        public string Header { get; set; }

        public string Text { get; set; }

        [Required]
        [DisplayName("Language")]
        public int LanguageId { get; set; }

        public Language Language { get; set; }
    }
}
