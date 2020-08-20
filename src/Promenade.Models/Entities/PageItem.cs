using System.ComponentModel.DataAnnotations;

namespace Ocuda.Promenade.Models.Entities
{
    public class PageItem
    {
        [Key]
        [Required]
        public int Id { get; set; }

        public int PageDetailId { get; set; }
        public PageDetail PageDetail { get; set; }

        public int Order { get; set; }

        public int? CarouselId { get; set; }
        public Carousel Carousel { get; set; }

        public int? SegmentId { get; set; }
        public Segment Segment { get; set; }
    }
}
