using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers
{
    [Route("[Controller]")]
    [Route("{culture:cultureConstraint?}/[Controller]")]
    public class AboutController : BasePageController<AboutController>
    {
        public AboutController(ServiceFacades.Controller<AboutController> context,
            ServiceFacades.PageController pageContext)
            : base(context, pageContext)
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