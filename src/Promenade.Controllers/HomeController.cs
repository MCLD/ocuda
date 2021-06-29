using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service;

namespace Ocuda.Promenade.Controllers
{
    [Route("")]
    [Route("{culture:cultureConstraint?}")]
    public class HomeController : BasePageController<HomeController>
    {
        public HomeController(ServiceFacades.Controller<HomeController> context,
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

        public static string Name { get { return "Home"; } }
        protected override PageType PageType { get { return PageType.Home; } }

        [HttpGet("{stub?}/item/{id}")]
        public async Task<IActionResult> CarouselItem(string stub, int id)
        {
            return await ReturnCarouselItemAsync(stub, id);
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            return await ReturnPageAsync(nameof(Index));
        }

        [HttpPost("")]
        public async Task<IActionResult> PagePreview()
        {
            return await ReturnPreviewPageAsync(nameof(Index),
                HttpContext.Request.Form["PreviewId"].FirstOrDefault());
        }
    }
}