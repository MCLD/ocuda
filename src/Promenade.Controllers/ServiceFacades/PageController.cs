using System;
using Ocuda.Promenade.Service;

namespace Ocuda.Promenade.Controllers.ServiceFacades
{
    public class PageController
    {
        public PageController(CarouselService carouselService,
            DeckService deckService,
            ImageFeatureService imageFeatureService,
            PageService pageService,
            RedirectService redirectService,
            SegmentService segmentService,
            SocialCardService socialCardService)
        {
            CarouselService = carouselService
                ?? throw new ArgumentNullException(nameof(carouselService));
            DeckService = deckService ?? throw new ArgumentNullException(nameof(deckService));
            ImageFeatureService = imageFeatureService
                ?? throw new ArgumentNullException(nameof(imageFeatureService));
            PageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
            RedirectService = redirectService
                ?? throw new ArgumentNullException(nameof(redirectService));
            SegmentService = segmentService
                ?? throw new ArgumentNullException(nameof(segmentService));
            SocialCardService = socialCardService
                ?? throw new ArgumentNullException(nameof(socialCardService));
        }

        public CarouselService CarouselService { get; }
        public DeckService DeckService { get; }
        public ImageFeatureService ImageFeatureService { get; }
        public PageService PageService { get; }
        public RedirectService RedirectService { get; }
        public SegmentService SegmentService { get; }
        public SocialCardService SocialCardService { get; }
    }
}