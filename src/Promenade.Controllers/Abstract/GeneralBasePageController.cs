using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ocuda.Promenade.Controllers.Abstract
{
    /// <summary>
    /// This abstract class exposes the necessary methods with attributes for displaying a "page" on
    /// the site. The attributes provide the controller routing with the requisite information to
    /// display the appropriate page. This is broken out from BasePageController so that the
    /// ErrorController class is capable of displaying pages without being beholden to the
    /// attributes present in this class.
    /// </summary>
    /// <typeparam name="T">The type of the concrete page controller</typeparam>
    public abstract class GeneralBasePageController<T> : BasePageController<T>
    {
        protected GeneralBasePageController(ServiceFacades.Controller<T> context,
            ServiceFacades.PageController pageContext) : base(context, pageContext)
        {
        }

        [HttpGet("{slug?}/item/{id}")]
        public async Task<IActionResult> CarouselItem(string slug, int id)
        {
            return await ReturnCarouselItemAsync(slug, id);
        }

        [HttpGet("{slug?}")]
        public async Task<IActionResult> Page(string slug)
        {
            return await ReturnPageAsync(slug);
        }

        [HttpPost("{slug?}")]
        public async Task<IActionResult> PagePreview(string slug)
        {
            if (string.IsNullOrEmpty(slug)
                || HttpContext.Request.ContentType != "application/x-www-form-urlencoded")
            {
                return StatusCode(StatusCodes.Status405MethodNotAllowed);
            }

            var previewId = HttpContext.Request.Form["PreviewId"].FirstOrDefault();

            if (string.IsNullOrEmpty(previewId))
            {
                return StatusCode(StatusCodes.Status405MethodNotAllowed);
            }

            return await ReturnPageAsync(slug, previewId);
        }
    }
}