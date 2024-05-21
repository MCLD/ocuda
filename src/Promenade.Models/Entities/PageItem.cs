using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ocuda.Promenade.Models.Entities
{
    public class PageItem
    {
        public ImageFeature BannerFeature { get; set; }

        [DisplayName("Banner")]
        public int? BannerFeatureId { get; set; }

        [NotMapped]
        public IEnumerable<CardDetail> CardDetails { get; set; }

        public Carousel Carousel { get; set; }

        [DisplayName("Carousel")]
        public int? CarouselId { get; set; }

        public Deck Deck { get; set; }
        public int? DeckId { get; set; }

        [Key]
        [Required]
        public int Id { get; set; }

        public NavBanner NavBanner { get; set; }
        public int? NavBannerId { get; set; }

        public int Order { get; set; }
        public ImageFeature PageFeature { get; set; }

        [DisplayName("Feature")]
        public int? PageFeatureId { get; set; }

        public PageLayout PageLayout { get; set; }
        public int PageLayoutId { get; set; }
        public Segment Segment { get; set; }

        [DisplayName("Segment")]
        public int? SegmentId { get; set; }

        [NotMapped]
        public SegmentText SegmentText { get; set; }

        public ImageFeature Webslide { get; set; }

        [DisplayName("Webslide")]
        public int? WebslideId { get; set; }
    }
}