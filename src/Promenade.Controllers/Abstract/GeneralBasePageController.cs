using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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
            if (string.IsNullOrEmpty(stub))
            {
                return NotFound();
            }
            if (HttpContext.Request.ContentType == "application/x-www-form-urlencoded")
            {
                return await ReturnPageAsync(stub,
                    HttpContext.Request.Form["PreviewId"].FirstOrDefault());
            }
            else
            {
                _logger.LogWarning("Bad page preview data submitted as {ContentType}: {Data}",
                    HttpContext.Request.ContentType,
                    HttpContext.Request.Body);
                return NotFound();
            }
        }
    }
}