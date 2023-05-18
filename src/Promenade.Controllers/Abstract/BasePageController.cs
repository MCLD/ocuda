using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Controllers.ViewModels.Shared;
using Ocuda.Promenade.Models.Entities;

namespace Ocuda.Promenade.Controllers.Abstract
{
    public abstract class BasePageController<T> : BaseController<T>
    {
        protected BasePageController(ServiceFacades.Controller<T> context,
            ServiceFacades.PageController pageContext) : base(context)
        {
            PageContext = pageContext;
        }

        protected ServiceFacades.PageController PageContext { get; }
        protected abstract PageType PageType { get; }

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
            return await ReturnPageAsync(stub, HttpContext.Request.Form["PreviewId"].FirstOrDefault());
        }

        protected async Task<IActionResult> ReturnCarouselItemAsync(string stub, int id)
        {
            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

            var pageHeader = await PageContext.PageService
                .GetHeaderByStubAndTypeAsync(stub, PageType, forceReload);

            if (pageHeader?.IsLayoutPage != true)
            {
                return NotFound();
            }

            var carouselItem = await PageContext.CarouselService
                .GetItemForHeaderAsync(pageHeader.Id, id, forceReload);

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
            return await ReturnPageAsync(stub, null);
        }

        protected async Task<IActionResult> ReturnPageAsync(string stub, string previewId)
        {
            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

            var pageHeader = await PageContext.PageService
                .GetHeaderByStubAndTypeAsync(stub, PageType, forceReload);

            if (pageHeader == null)
            {
                return NotFound();
            }

            if (pageHeader.IsLayoutPage)
            {
                return await ReturnLayoutPageAsync(pageHeader.Id, stub, previewId);
            }
            else
            {
                return await ReturnContentPageAsync(stub);
            }
        }

        private async Task<IActionResult> ReturnContentPageAsync(string stub)
        {
            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

            var page = await PageContext.PageService.GetContentPageByStubAndType(stub,
                PageType,
                forceReload);

            if (page == null)
            {
                return NotFound();
            }

            var viewModel = new PageViewModel
            {
                Content = CommonMark.CommonMarkConverter.Convert(page.Content),
                CanonicalUrl = await GetCanonicalLinkAsync()
            };

            if (page.SocialCardId.HasValue)
            {
                var card = await PageContext.SocialCardService
                    .GetByIdAsync(page.SocialCardId.Value, forceReload);
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

            var pageLayout = await PageContext.PageService
                .GetLayoutPageByHeaderAsync(headerId, forceReload, previewId);

            if (pageLayout == null)
            {
                return NotFound();
            }

            foreach (var item in pageLayout?.Items)
            {
                if (item.BannerFeatureId.HasValue)
                {
                    item.BannerFeature = await PageContext.ImageFeatureService
                        .GetByIdAsync(item.BannerFeatureId.Value, forceReload);
                }
                else if (item.CarouselId.HasValue)
                {
                    item.Carousel = await PageContext.CarouselService
                        .GetByIdAsync(item.CarouselId.Value, forceReload);
                }
                else if (item.DeckId.HasValue)
                {
                    item.CardDetails = await PageContext.DeckService
                        .GetByIdAsync(item.DeckId.Value, forceReload);
                    foreach (var cardDetail in item.CardDetails)
                    {
                        cardDetail.Text = CommonMark.CommonMarkConverter.Convert(cardDetail.Text);
                    }
                }
                else if (item.PageFeatureId.HasValue)
                {
                    item.PageFeature = await PageContext.ImageFeatureService
                        .GetByIdAsync(item.PageFeatureId.Value, forceReload);
                }
                else if (item.SegmentId.HasValue)
                {
                    item.SegmentText = await PageContext.SegmentService
                        .GetSegmentTextBySegmentIdAsync(item.SegmentId.Value, forceReload);

                    if (item.SegmentText != null)
                    {
                        item.SegmentText.Text = FormatForDisplay(item.SegmentText);
                    }
                }
                else if (item.WebslideId.HasValue)
                {
                    item.Webslide = await PageContext.ImageFeatureService
                        .GetByIdAsync(item.WebslideId.Value, forceReload);
                }
            }

            var viewModel = new PageLayoutViewModel
            {
                CanonicalUrl = await GetCanonicalLinkAsync(),
                HasCarousels = pageLayout.Items.Any(_ => _.CarouselId.HasValue),
                Stub = stub?.Trim()
            };

            if (pageLayout.Items.Any(_ => _.PageFeatureId.HasValue))
            {
                viewModel.PageFeatureTemplate = await PageContext.ImageFeatureService
                    .GetTemplateForPageLayoutAsync(pageLayout.Id);
            }

            if (pageLayout.SocialCardId.HasValue)
            {
                pageLayout.SocialCard = await PageContext.SocialCardService
                    .GetByIdAsync(pageLayout.SocialCardId.Value, forceReload);
                pageLayout.SocialCard.Url = viewModel.CanonicalUrl;
            }

            viewModel.PageLayout = pageLayout;

            PageTitle = pageLayout.PageLayoutText?.Title;

            viewModel.PageHeaderClasses = pageLayout.PageLayoutText?.IsTitleHidden == true
                ? "oc-title sr-only"
                : "oc-title";

            return View("LayoutPage", viewModel);
        }
    }
}