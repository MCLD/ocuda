using System;
using Ocuda.Promenade.Service;

namespace Ocuda.Promenade.Controllers.ServiceFacades
{
    public class PageController
    {
        public PageController(CarouselService carouselService,
            DeckService deckService,
            ImageFeatureService imageFeatureService,
            NavBannerService navBannerService,
            PageService pageService,
            RedirectService redirectService,
            SegmentService segmentService,
            SocialCardService socialCardService)
        {
            ArgumentNullException.ThrowIfNull(carouselService);
            ArgumentNullException.ThrowIfNull(deckService);
            ArgumentNullException.ThrowIfNull(imageFeatureService);
            ArgumentNullException.ThrowIfNull(navBannerService);
            ArgumentNullException.ThrowIfNull(pageService);
            ArgumentNullException.ThrowIfNull(redirectService);
            ArgumentNullException.ThrowIfNull(segmentService);
            ArgumentNullException.ThrowIfNull(socialCardService);

            CarouselService = carouselService;
            DeckService = deckService;
            ImageFeatureService = imageFeatureService;
            NavBannerService = navBannerService;
            PageService = pageService;
            RedirectService = redirectService;
            SegmentService = segmentService;
            SocialCardService = socialCardService;
        }

        public CarouselService CarouselService { get; }
        public DeckService DeckService { get; }
        public ImageFeatureService ImageFeatureService { get; }
        public NavBannerService NavBannerService { get; }
        public PageService PageService { get; }
        public RedirectService RedirectService { get; }
        public SegmentService SegmentService { get; }
        public SocialCardService SocialCardService { get; }
    }
}