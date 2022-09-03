using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service;

namespace Ocuda.Promenade.Controllers
{
    [Route("[Controller]")]
    [Route("{culture:cultureConstraint?}/[Controller]")]
    public class SubjectController : BasePageController<SubjectController>
    {
        public SubjectController(ServiceFacades.Controller<SubjectController> context,
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
        { get { return PageType.Subject; } }

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
            return await ReturnPreviewPageAsync(stub,
                HttpContext.Request.Form["PreviewId"].FirstOrDefault());
        }
    }
}