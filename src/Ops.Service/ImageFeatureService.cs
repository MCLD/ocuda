using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Service
{
    public class ImageFeatureService : BaseService<ImageFeatureService>, IImageFeatureService
    {
        private readonly IImageFeatureItemRepository _webslideItemRepository;
        private readonly IImageFeatureItemTextRepository _webslideItemTextRepository;
        private readonly IImageFeatureRepository _webslideRepository;
        private readonly IImageFeatureTemplateRepository _webslideTemplateRepository;

        public ImageFeatureService(ILogger<ImageFeatureService> logger,
            IHttpContextAccessor httpContextAccessor,
            IImageFeatureItemRepository webslideItemRepository,
            IImageFeatureItemTextRepository webslideItemTextRepository,
            IImageFeatureRepository webslideRepository,
            IImageFeatureTemplateRepository webslideTemplateRepository)
            : base(logger, httpContextAccessor)
        {
            _webslideItemRepository = webslideItemRepository
                ?? throw new ArgumentNullException(nameof(webslideItemRepository));
            _webslideItemTextRepository = webslideItemTextRepository
                ?? throw new ArgumentNullException(nameof(webslideItemTextRepository));
            _webslideRepository = webslideRepository
                ?? throw new ArgumentNullException(nameof(webslideRepository));
            _webslideTemplateRepository = webslideTemplateRepository
                ?? throw new ArgumentNullException(nameof(webslideTemplateRepository));
        }

        public async Task<ImageFeatureItem> CreateItemAsync(ImageFeatureItem webslideItem)
        {
            webslideItem.Name = webslideItem.Name?.Trim();

            var maxSortOrder = await _webslideItemRepository
                .GetMaxSortOrderForImageFeatureAsync(webslideItem.ImageFeatureId);
            if (maxSortOrder.HasValue)
            {
                webslideItem.Order = maxSortOrder.Value + 1;
            }

            await _webslideItemRepository.AddAsync(webslideItem);
            await _webslideItemRepository.SaveAsync();
            return webslideItem;
        }

        public async Task<ImageFeature> CreateNoSaveAsync(ImageFeature webslide)
        {
            webslide.Name = webslide.Name?.Trim();

            await _webslideRepository.AddAsync(webslide);
            return webslide;
        }

        public async Task DeleteItemAsync(int webslideItemId)
        {
            var webslideItem = await _webslideItemRepository.FindAsync(webslideItemId);

            if (webslideItem == null)
            {
                throw new OcudaException("Webslide item does not exist.");
            }

            var subsequentItems = await _webslideItemRepository.GetImageFeatureSubsequentAsync(
                webslideItem.ImageFeatureId, webslideItem.Order);

            if (subsequentItems.Count > 0)
            {
                subsequentItems.ForEach(_ => _.Order--);
                _webslideItemRepository.UpdateRange(subsequentItems);
            }

            var webslideItemTexts = await _webslideItemTextRepository
                .GetAllForImageFeatureItemAsync(webslideItem.Id);
            _webslideItemTextRepository.RemoveRange(webslideItemTexts);

            _webslideItemRepository.Remove(webslideItem);
            await _webslideItemRepository.SaveAsync();
        }

        public async Task DeleteNoSaveAsync(int id)
        {
            var webslide = await _webslideRepository.FindAsync(id);
            if (webslide == null)
            {
                throw new OcudaException("Could not find that Web Slide");
            }
            var items = await _webslideItemRepository.GetByImageFeatureAsync(id);
            foreach (var item in items)
            {
                var itemTexts = await _webslideItemTextRepository
                    .GetAllForImageFeatureItemAsync(item.Id);
                _webslideItemTextRepository.RemoveRange(itemTexts);
            }
            _webslideItemRepository.RemoveRange(items);
            _webslideRepository.Remove(webslide);
        }

        public async Task<ImageFeature> EditAsync(ImageFeature webslide)
        {
            var currentWebslide = await _webslideRepository.FindAsync(webslide.Id);

            currentWebslide.Name = webslide.Name?.Trim();

            _webslideRepository.Update(currentWebslide);
            await _webslideRepository.SaveAsync();
            return currentWebslide;
        }

        public async Task<ImageFeatureItem> EditItemAsync(ImageFeatureItem webslideItem)
        {
            var currentWebslideItem = await _webslideItemRepository.FindAsync(webslideItem.Id);

            currentWebslideItem.Name = webslideItem.Name?.Trim();
            currentWebslideItem.StartDate = webslideItem.StartDate;
            currentWebslideItem.EndDate = webslideItem.EndDate;

            _webslideItemRepository.Update(currentWebslideItem);
            await _webslideItemRepository.SaveAsync();
            return currentWebslideItem;
        }

        public async Task<ICollection<ImageFeatureTemplate>> GetAllTemplatesAsync()
        {
            return await _webslideTemplateRepository.GetAllAsync();
        }

        public async Task<ImageFeature> GetImageFeatureDetailsAsync(int id, int languageId)
        {
            var webslide = await _webslideRepository.GetIncludingChildrenAsync(id);

            webslide.Items = webslide.Items.OrderBy(_ => _.Order).ToList();
            foreach (var item in webslide.Items)
            {
                item.ImageFeatureItemText = await _webslideItemTextRepository
                    .GetByImageFeatureItemAndLanguageAsync(item.Id, languageId);
            }

            return webslide;
        }

        public async Task<ImageFeatureItem> GetItemByIdAsync(int id)
        {
            return await _webslideItemRepository.FindAsync(id);
        }

        public async Task<ImageFeatureItemText> GetItemTextByIdsAsync(int webslideItemId,
            int languageId)
        {
            return await _webslideItemTextRepository.GetByImageFeatureItemAndLanguageAsync(
                webslideItemId, languageId);
        }

        public async Task<int?> GetPageHeaderIdForImageFeatureAsync(int id)
        {
            return await _webslideRepository.GetPageHeaderIdForImageFeatureAsync(id);
        }

        public async Task<int> GetPageLayoutIdForImageFeatureAsync(int id)
        {
            return await _webslideRepository.GetPageLayoutIdForImageFeatureAsync(id);
        }

        public async Task<ImageFeatureTemplate> GetTemplateForImageFeatureAsync(int id)
        {
            return await _webslideTemplateRepository.GetForImageFeatureAsync(id);
        }

        public async Task<ImageFeatureTemplate> GetTemplateForPageLayoutAsync(int id)
        {
            return await _webslideTemplateRepository.GetForPageLayoutAsync(id);
        }

        public async Task<ImageFeatureItemText> SetItemTextAsync(ImageFeatureItemText itemText)
        {
            var currentText = await _webslideItemTextRepository.GetByImageFeatureItemAndLanguageAsync(
                itemText.ImageFeatureItemId, itemText.LanguageId);

            if (currentText == null)
            {
                itemText.AltText = itemText.AltText?.Trim();
                itemText.Link = itemText.Link?.Trim();

                await _webslideItemTextRepository.AddAsync(itemText);
                await _webslideItemTextRepository.SaveAsync();
                _webslideItemTextRepository.DetachEntity(itemText);

                return itemText;
            }
            else
            {
                currentText.AltText = itemText.AltText?.Trim();
                currentText.Link = itemText.Link?.Trim();

                if (!string.IsNullOrWhiteSpace(itemText.Filename))
                {
                    currentText.Filename = itemText.Filename?.Trim();
                }

                _webslideItemTextRepository.Update(currentText);
                await _webslideItemTextRepository.SaveAsync();
                _webslideItemTextRepository.DetachEntity(currentText);

                return currentText;
            }
        }

        public async Task UpdateItemSortOrder(int id, bool increase)
        {
            var item = await _webslideItemRepository.FindAsync(id);

            int newSortOrder;
            if (increase)
            {
                newSortOrder = item.Order + 1;
            }
            else
            {
                if (item.Order == 0)
                {
                    throw new OcudaException("Item is already in the first position.");
                }
                newSortOrder = item.Order - 1;
            }

            var itemInPosition = await _webslideItemRepository.GetByImageFeatureAndOrderAsync(
                item.ImageFeatureId, newSortOrder);

            if (itemInPosition == null)
            {
                throw new OcudaException("Item is already in the last position.");
            }

            itemInPosition.Order = item.Order;
            item.Order = newSortOrder;

            _webslideItemRepository.Update(item);
            _webslideItemRepository.Update(itemInPosition);
            await _webslideItemRepository.SaveAsync();
        }
    }
}