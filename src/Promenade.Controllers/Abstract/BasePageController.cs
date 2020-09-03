using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using Ocuda.Promenade.Controllers.ViewModels.Shared;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service;

namespace Ocuda.Promenade.Controllers.Abstract
{
    public abstract class BasePageController<T> : BaseController<T>
    {
        private readonly CarouselService _carouselService;
        private readonly PageService _pageService;
        private readonly RedirectService _redirectService;
        private readonly SegmentService _segmentService;
        private readonly SocialCardService _socialCardService;

        protected abstract PageType PageType { get; }

        protected CarouselService CarouselService { get { return _carouselService; } }
        protected PageService PageService { get { return _pageService; } }
        protected RedirectService RedirectService { get { return _redirectService; } }
        protected SegmentService SegmentService { get { return _segmentService; } }
        protected SocialCardService SocialCardService { get { return _socialCardService; } }

        protected BasePageController(ServiceFacades.Controller<T> context,
            CarouselService carouselService,
            PageService pageService,
            RedirectService redirectService,
            SegmentService segmentService,
            SocialCardService socialCardService) : base(context)
        {
            _carouselService = carouselService
                ?? throw new ArgumentNullException(nameof(carouselService));
            _pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
            _redirectService = redirectService
                ?? throw new ArgumentNullException(nameof(redirectService));
            _segmentService = segmentService
                ?? throw new ArgumentNullException(nameof(segmentService));
            _socialCardService = socialCardService
                ?? throw new ArgumentNullException(nameof(socialCardService));
        }

        protected async Task<IActionResult> ReturnPageAsync(string stub)
        {
            var pageHeader = await _pageService.GetHeaderByStubAndTypeAsync(stub, PageType);

            if (pageHeader == null)
            {
                return NotFound();
            }

            if (pageHeader.IsLayoutPage)
            {
                return await ReturnLayoutPageAsync(pageHeader.Id);
            }
            else
            {
                return await ReturnContentPageAsync(stub);
            }
        }

        private async Task<IActionResult> ReturnContentPageAsync(string stub)
        {
            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

            var page = await _pageService.GetContentPageByStubAndType(stub, PageType, forceReload);

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

        private async Task<IActionResult> ReturnLayoutPageAsync(int headerId)
        {
            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

            var pageLayout = await _pageService.GetLayoutPageByHeaderAsync(headerId, forceReload);

            if (pageLayout == null)
            {
                return NotFound();
            }

            foreach (var item in pageLayout.Items)
            {
                if (item.CarouselId.HasValue)
                {
                    item.Carousel = await _carouselService.GetByIdAsync(item.CarouselId.Value,
                        forceReload);
                }
                else if (item.SegmentId.HasValue)
                {
                    item.SegmentText = await _segmentService.GetSegmentTextBySegmentIdAsync(
                        item.SegmentId.Value,
                        forceReload);

                    if (item.SegmentText != null)
                    {
                        item.SegmentText.Text = CommonMark.CommonMarkConverter.Convert(
                            item.SegmentText.Text);
                    }
                }
            }

            var viewModel = new PageLayoutViewModel
            {
                CanonicalUrl = await GetCanonicalUrl(),
                HasCarousels = pageLayout.Items.Any(_ => _.CarouselId.HasValue)
            };

            if (pageLayout.SocialCardId.HasValue)
            {
                pageLayout.SocialCard = await _socialCardService.GetByIdAsync(
                    pageLayout.SocialCardId.Value,
                    forceReload);
                pageLayout.SocialCard.Url = viewModel.CanonicalUrl;
            }

            viewModel.PageLayout = pageLayout;

            PageTitle = pageLayout.PageLayoutText?.Title;

            return View("LayoutPage", viewModel);
        }
    }
}
