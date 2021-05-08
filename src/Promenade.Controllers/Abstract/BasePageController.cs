using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Controllers.ViewModels.Shared;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service;

namespace Ocuda.Promenade.Controllers.Abstract
{
    public abstract class BasePageController<T> : BaseController<T>
    {
        protected BasePageController(ServiceFacades.Controller<T> context,
            CarouselService carouselService,
            PageFeatureService pageFeatureService,
            PageService pageService,
            RedirectService redirectService,
            SegmentService segmentService,
            SocialCardService socialCardService,
            WebslideService webslideService) : base(context)
        {
            CarouselService = carouselService
                ?? throw new ArgumentNullException(nameof(carouselService));
            PageFeatureService = pageFeatureService
                ?? throw new ArgumentNullException(nameof(pageFeatureService));
            PageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
            RedirectService = redirectService
                ?? throw new ArgumentNullException(nameof(redirectService));
            SegmentService = segmentService
                ?? throw new ArgumentNullException(nameof(segmentService));
            SocialCardService = socialCardService
                ?? throw new ArgumentNullException(nameof(socialCardService));
            WebslideService = webslideService
                ?? throw new ArgumentNullException(nameof(webslideService));
        }

        protected CarouselService CarouselService { get; }
        protected PageFeatureService PageFeatureService { get; }
        protected PageService PageService { get; }
        protected abstract PageType PageType { get; }
        protected RedirectService RedirectService { get; }
        protected SegmentService SegmentService { get; }
        protected SocialCardService SocialCardService { get; }
        protected WebslideService WebslideService { get; }

        protected async Task<IActionResult> ReturnCarouselItemAsync(string stub, int id)
        {
            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

            var pageHeader = await PageService.GetHeaderByStubAndTypeAsync(stub,
                PageType,
                forceReload);

            if (pageHeader?.IsLayoutPage != true)
            {
                return NotFound();
            }

            var carouselItem = await CarouselService.GetItemForHeaderAsync(pageHeader.Id, id,
                forceReload);

            if (carouselItem == null)
            {
                return NotFound();
            }

            var viewModel = new CarouselItemViewModel
            {
                Item = carouselItem,
                ReturnUrl = Url.Action(nameof(Page), PageTitle, new { stub })
            };

            return View("CarouselItem", viewModel);
        }

        protected async Task<IActionResult> ReturnPageAsync(string stub)
        {
            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

            var pageHeader = await PageService.GetHeaderByStubAndTypeAsync(stub,
                PageType,
                forceReload);

            if (pageHeader == null)
            {
                return NotFound();
            }

            if (pageHeader.IsLayoutPage)
            {
                return await ReturnLayoutPageAsync(pageHeader.Id, stub, null);
            }
            else
            {
                return await ReturnContentPageAsync(stub);
            }
        }

        protected async Task<IActionResult> ReturnPreviewPageAsync(string stub, string previewId)
        {
            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

            var pageHeader = await PageService.GetHeaderByStubAndTypeAsync(stub,
                PageType,
                forceReload);

            if (pageHeader == null)
            {
                return NotFound();
            }

            if (pageHeader.IsLayoutPage)
            {
                return await ReturnLayoutPageAsync(pageHeader.Id,
                    stub,
                    previewId);
            }
            else
            {
                return await ReturnContentPageAsync(stub);
            }
        }

        private async Task<IActionResult> ReturnContentPageAsync(string stub)
        {
            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

            var page = await PageService.GetContentPageByStubAndType(stub, PageType, forceReload);

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
                var card = await SocialCardService.GetByIdAsync(page.SocialCardId.Value,
                    forceReload);
                card.Url = viewModel.CanonicalUrl;
                viewModel.SocialCard = card;
            }

            PageTitle = page.Title;

            return View(viewModel);
        }

        private async Task<IActionResult> ReturnLayoutPageAsync(int headerId,
            string stub,
            string previewId)
        {
            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

            // always force reload previews
            if (!string.IsNullOrEmpty(previewId))
            {
                _logger.LogInformation("Showing preview for {Stub}: {PreviewId}",
                    stub,
                    previewId);
                forceReload = true;
            }

            var pageLayout = await PageService.GetLayoutPageByHeaderAsync(headerId,
                forceReload,
                previewId);

            if (pageLayout == null)
            {
                return NotFound();
            }

            foreach (var item in pageLayout?.Items)
            {
                if (item.CarouselId.HasValue)
                {
                    item.Carousel = await CarouselService.GetByIdAsync(item.CarouselId.Value,
                        forceReload);
                }
                else if (item.PageFeatureId.HasValue)
                {
                    item.PageFeature = await PageFeatureService.GetByIdAsync(
                        item.PageFeatureId.Value,
                        forceReload);

                    foreach (var featureItem in item.PageFeature.Items)
                    {
                        featureItem.PageFeatureItemText.Filename = PageFeatureService
                            .GetPageFeatureFilePath(featureItem.PageFeatureItemText.Filename);
                    }
                }
                else if (item.SegmentId.HasValue)
                {
                    item.SegmentText = await SegmentService.GetSegmentTextBySegmentIdAsync(
                        item.SegmentId.Value,
                        forceReload);

                    if (item.SegmentText != null)
                    {
                        item.SegmentText.Text = CommonMark.CommonMarkConverter.Convert(
                            item.SegmentText.Text);
                    }
                }
                else if (item.WebslideId.HasValue)
                {
                    item.Webslide = await WebslideService.GetByIdAsync(item.WebslideId.Value,
                        forceReload);
                }
            }

            var viewModel = new PageLayoutViewModel
            {
                CanonicalUrl = await GetCanonicalUrl(),
                HasCarousels = pageLayout.Items.Any(_ => _.CarouselId.HasValue),
                Stub = stub?.Trim()
            };

            if (pageLayout.Items.Any(_ => _.PageFeatureId.HasValue))
            {
                viewModel.PageFeatureTemplate = await PageFeatureService
                    .GetTemplateForPageLayoutAsync(pageLayout.Id);
            }

            if (pageLayout.SocialCardId.HasValue)
            {
                pageLayout.SocialCard = await SocialCardService.GetByIdAsync(
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