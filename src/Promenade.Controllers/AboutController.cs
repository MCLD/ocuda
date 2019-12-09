using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Controllers.ViewModels.Shared;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service;

namespace Ocuda.Promenade.Controllers
{
    [Route("[Controller]")]
    [Route("{culture:cultureConstraint}/[Controller]")]
    public class AboutController : BaseController<AboutController>
    {
        private readonly PageService _pageService;
        private readonly RedirectService _redirectService;
        private readonly SocialCardService _socialCardService;

        public AboutController(ServiceFacades.Controller<AboutController> context,
            PageService pageService,
            RedirectService redirectService,
            SocialCardService socialCardService) : base(context)
        {
            _pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
            _redirectService = redirectService
                    ?? throw new ArgumentNullException(nameof(redirectService));
            _socialCardService = socialCardService
                    ?? throw new ArgumentNullException(nameof(socialCardService));
        }

        [Route("{stub?}")]
        public async Task<IActionResult> Page(string stub)
        {
            var page = await _pageService.GetByStubAndType(stub, PageType.About);

            if (page == null)
            {
                var queryParams = Request.Query.ToDictionary(_ => _.Key, _ => _.Value.ToString());

                var redirect = await _redirectService.GetUrlRedirectByPathAsync(Request.Path,
                    queryParams);

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

                _logger.LogWarning($"No About page or redirect found for stub \"{stub}\": {Request.Path}");
                return View("PageNotFound");
            }

            var viewModel = new PageViewModel
            {
                Content = CommonMark.CommonMarkConverter.Convert(page.Content)
            };

            if (page.SocialCardId.HasValue)
            {
                var card = await _socialCardService.GetByIdAsync(page.SocialCardId.Value);
                card.Url = await GetCanonicalUrl();
                viewModel.SocialCard = card;
            }

            return View(viewModel);
        }
    }
}
