using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service;

namespace Ocuda.Promenade.Controllers
{
    [Route("[Controller]")]
    public class NewsController : BasePageController<NewsController>
    {
        protected override PageType PageType { get { return PageType.News; } }

        public NewsController(ServiceFacades.Controller<NewsController> context,
            CarouselService carouselService,
            PageService pageService,
            RedirectService redirectService,
            SegmentService segmentService,
            SocialCardService socialCardService,
            WebslideService webslideService)
            : base(context, carouselService, pageService, redirectService, segmentService,
                  socialCardService, webslideService)
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
            return await ReturnPreviewPageAsync(stub,
                HttpContext.Request.Query["PreviewId"]);
        }

        [HttpGet("{stub?}/item/{id}")]
        public async Task<IActionResult> CarouselItem(string stub, int id)
        {
            return await ReturnCarouselItemAsync(stub, id);
        }
    }
}
