using System;
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
    public class SubjectController : BaseController<SubjectController>
    {
        private readonly PageService _pageService;
        private readonly RedirectService _redirectService;
        private readonly SocialCardService _socialCardService;

        public SubjectController(ServiceFacades.Controller<SubjectController> context,
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
            var page = await _pageService.GetByStubAndType(stub, PageType.Subject);

            if (page == null)
            {
                var redirect = await _redirectService.GetUrlRedirectByPathAsync(Request.Path);

                if (redirect != null)
                {
                    var redirectUrl = redirect.Url;

                    if (Request.QueryString.HasValue)
                    {
                        redirectUrl += Request.QueryString;
                    }

                    if (redirect.IsPermanent)
                    {
                        return RedirectPermanent(redirectUrl);
                    }
                    else
                    {
                        return Redirect(redirectUrl);
                    }
                }

                _logger.LogWarning($"No Subject page or redirect found for stub \"{stub}\": {Request.Path}");
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

            PageTitle = page.Title;

            return View(viewModel);
        }
    }
}
