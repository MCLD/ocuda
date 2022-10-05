using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service;

namespace Ocuda.Promenade.Controllers
{
    [Route("[Controller]")]
    [Route("{culture:cultureConstraint?}/[Controller]")]
    public class AboutController : BasePageController<AboutController>
    {
        public AboutController(ServiceFacades.Controller<AboutController> context,
            CarouselService carouselService,
            DeckService deckService,
            ImageFeatureService imageFeatureService,
            PageService pageService,
            RedirectService redirectService,
            SegmentService segmentService,
            SocialCardService socialCardService)
            : base(context, carouselService, deckService, imageFeatureService,
                  pageService, redirectService, segmentService, socialCardService)
        {
        }

        protected override PageType PageType
        { get { return PageType.About; } }

        [HttpGet("{stub?}/item/{id}")]
        public async Task<IActionResult> CarouselItem(string stub, int id)
        {
            return await ReturnCarouselItemAsync(stub, id);
        }

        [HttpGet("{stub?}")]
        public async Task<IActionResult> Page(string stub)
        {
            return await ReturnPageAsync(stub);
        }

        [HttpPost("{stub?}")]
        public async Task<IActionResult> PagePreview(string stub)
        {
            var pagePreview = await ReturnPreviewPageAsync(stub,
                HttpContext.Request.Query["PreviewId"]);

            return pagePreview;
        }
    }
}