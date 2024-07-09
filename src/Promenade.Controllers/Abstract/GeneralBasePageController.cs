using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
                if (!HttpContext.Request.Body.CanSeek)
                {
                    HttpContext.Request.EnableBuffering();
                }
                HttpContext.Request.Body.Position = 0;
                using var reader = new StreamReader(HttpContext.Request.Body, Encoding.UTF8);
                var body = await reader.ReadToEndAsync();
                HttpContext.Request.Body.Position = 0;

                _logger.LogWarning("Bad preview submission to {Slug} as {ContentType}: {Data}",
                    stub,
                    HttpContext.Request.ContentType,
                    body);
                return NotFound();
            }
        }
    }
}