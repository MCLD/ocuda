using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Promenade.Controllers.ViewModels.Shared;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service;

namespace Ocuda.Promenade.Controllers.Abstract
{
    public abstract class BasePageController<T> : BaseController<T>
    {
        private readonly PageService _pageService;
        private readonly RedirectService _redirectService;
        private readonly SocialCardService _socialCardService;

        protected abstract PageType PageType { get; }

        protected PageService PageService { get { return _pageService; } }
        protected RedirectService RedirectService { get { return _redirectService; } }
        protected SocialCardService SocialCardService { get { return _socialCardService; } }

        protected BasePageController(ServiceFacades.Controller<T> context,
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

        protected async Task<IActionResult> ReturnPageAsync(string stub)
        {
            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

            var page = await _pageService.GetByStubAndType(stub, PageType, forceReload);

            if (page == null)
            {
                return NotFound();
            }

            var viewModel = new PageViewModel
            {
                Content = CommonMark.CommonMarkConverter.Convert(page.Content),
                CanonicalUrl = await GetCanonicalUrl()
            };

            if (page.SocialCardId.HasValue)
            {
                var card = await _socialCardService.GetByIdAsync(page.SocialCardId.Value,
                    forceReload);
                card.Url = viewModel.CanonicalUrl;
                viewModel.SocialCard = card;
            }

            PageTitle = page.Title;

            return View(viewModel);
        }
    }
}
