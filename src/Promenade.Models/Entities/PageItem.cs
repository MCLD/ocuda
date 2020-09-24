using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class PageItem
    {
        [Key]
        [Required]
        public int Id { get; set; }

        public int PageLayoutId { get; set; }
        public PageLayout PageLayout { get; set; }

        public int Order { get; set; }

        [DisplayName("Carousel")]
        public int? CarouselId { get; set; }
        public Carousel Carousel { get; set; }

        [DisplayName("Segment")]
        public int? SegmentId { get; set; }
        public Segment Segment { get; set; }

        [NotMapped]
        public SegmentText SegmentText { get; set; }
    }
}
