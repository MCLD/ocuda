using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service;

namespace Ocuda.Promenade.Controllers
{
    [Route("[Controller]")]
    public class AboutController : BaseController
    {
        private readonly ILogger<AboutController> _logger;
        private readonly PageService _pageService;
        private readonly RedirectService _redirectService;

        public AboutController(ILogger<AboutController> logger,
            PageService pageService,
            RedirectService redirectService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
            _redirectService = redirectService 
                ?? throw new ArgumentNullException(nameof(redirectService));
        }

        [Route("{stub?}")]
        public async Task<IActionResult> Page(string stub)
        {
            var page = await _pageService.GetByStubAndType(stub, PageType.About);

            if (page == null)
            {
                var redirect = await _redirectService.GetUrlRedirectByPathAsync(
                    HttpContext.Request.Path);

                if (redirect != null)
                {
                    if (redirect.IsPermanent)
                    {
                        return RedirectPermanent(redirect.Url);
                    }
                    else
                    {
                        return Redirect(redirect.Url);
                    }
                }
                else
                {
                    _logger.LogWarning($"No About page or redirect found for stub {stub}: {HttpContext.Request.Path}");
                    return View("PageNotFound");
                }
            }

            var pageContent = CommonMark.CommonMarkConverter.Convert(page.Content);

            return View("Page", pageContent);
        }
    }
}
