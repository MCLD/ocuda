using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service;

namespace Ocuda.Promenade.Controllers
{
    [Route("[Controller]")]
    public class AboutController : BasePageController<AboutController>
    {
        protected override PageType PageType { get { return PageType.About; } }

        public AboutController(ServiceFacades.Controller<AboutController> context,
            CarouselService carouselService,
            PageService pageService,
            RedirectService redirectService,
            SegmentService segmentService,
            SocialCardService socialCardService,
            ImageFeatureService webslideService)
            : base(context, carouselService, pageService, redirectService,
                  segmentService, socialCardService, webslideService)
        {
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

        [HttpGet("{stub?}/item/{id}")]
        public async Task<IActionResult> CarouselItem(string stub, int id)
        {
            return await ReturnCarouselItemAsync(stub, id);
        }
    }
}