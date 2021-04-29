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
    public class PageFeatureService : BaseService<PageFeatureService>, IPageFeatureService
    {
        private readonly IPageFeatureItemRepository _pageFeatureItemRepository;
        private readonly IPageFeatureItemTextRepository _pageFeatureItemTextRepository;
        private readonly IPageFeatureRepository _pageFeatureRepository;
        private readonly IPageFeatureTemplateRepository _pageFeatureTemplateRepository;

        public PageFeatureService(ILogger<PageFeatureService> logger,
            IHttpContextAccessor httpContextAccessor,
            IPageFeatureItemRepository pageFeatureItemRepository,
            IPageFeatureItemTextRepository pageFeatureItemTextRepository,
            IPageFeatureRepository pageFeatureRepository,
            IPageFeatureTemplateRepository pageFeatureTemplateRepository)
            : base(logger, httpContextAccessor)
        {
            _pageFeatureItemRepository = pageFeatureItemRepository
                ?? throw new ArgumentNullException(nameof(pageFeatureItemRepository));
            _pageFeatureItemTextRepository = pageFeatureItemTextRepository
                ?? throw new ArgumentNullException(nameof(pageFeatureItemTextRepository));
            _pageFeatureRepository = pageFeatureRepository
                ?? throw new ArgumentNullException(nameof(pageFeatureRepository));
            _pageFeatureTemplateRepository = pageFeatureTemplateRepository
                ?? throw new ArgumentNullException(nameof(pageFeatureTemplateRepository));
        }

        public async Task<PageFeatureItem> CreateItemAsync(PageFeatureItem featureItem)
        {
            featureItem.Name = featureItem.Name?.Trim();

            var maxSortOrder = await _pageFeatureItemRepository
                .GetMaxSortOrderForPageFeatureAsync(featureItem.PageFeatureId);
            if (maxSortOrder.HasValue)
            {
                featureItem.Order = maxSortOrder.Value + 1;
            }

            await _pageFeatureItemRepository.AddAsync(featureItem);
            await _pageFeatureItemRepository.SaveAsync();
            return featureItem;
        }

        public async Task<PageFeature> CreateNoSaveAsync(PageFeature feature)
        {
            feature.Name = feature.Name?.Trim();

            await _pageFeatureRepository.AddAsync(feature);
            return feature;
        }

        public async Task DeleteItemAsync(int featureItemId)
        {
            var featureItem = await _pageFeatureItemRepository.FindAsync(featureItemId);

            if (featureItem == null)
            {
                throw new OcudaException("Feature item does not exist.");
            }

            var subsequentItems = await _pageFeatureItemRepository.GetPageFeatureSubsequentAsync(
                featureItem.PageFeatureId, featureItem.Order);

            if (subsequentItems.Count > 0)
            {
                subsequentItems.ForEach(_ => _.Order--);
                _pageFeatureItemRepository.UpdateRange(subsequentItems);
            }

            var featureItemTexts = await _pageFeatureItemTextRepository
                .GetAllForPageFeatureItemAsync(featureItem.Id);
            _pageFeatureItemTextRepository.RemoveRange(featureItemTexts);

            _pageFeatureItemRepository.Remove(featureItem);
            await _pageFeatureItemRepository.SaveAsync();
        }

        public async Task<PageFeature> EditAsync(PageFeature feature)
        {
            var currentFeature = await _pageFeatureRepository.FindAsync(feature.Id);

            currentFeature.Name = feature.Name?.Trim();

            _pageFeatureRepository.Update(currentFeature);
            await _pageFeatureRepository.SaveAsync();
            return currentFeature;
        }

        public async Task<PageFeatureItem> EditItemAsync(PageFeatureItem featureItem)
        {
            var currentFeatureItem = await _pageFeatureItemRepository.FindAsync(featureItem.Id);

            currentFeatureItem.Name = featureItem.Name?.Trim();
            currentFeatureItem.StartDate = featureItem.StartDate;
            currentFeatureItem.EndDate = featureItem.EndDate;

            _pageFeatureItemRepository.Update(currentFeatureItem);
            await _pageFeatureItemRepository.SaveAsync();
            return currentFeatureItem;
        }

        public async Task<ICollection<PageFeatureTemplate>> GetAllTemplatesAsync()
        {
            return await _pageFeatureTemplateRepository.GetAllAsync();
        }

        public async Task<PageFeatureItem> GetItemByIdAsync(int id)
        {
            return await _pageFeatureItemRepository.FindAsync(id);
        }

        public async Task<PageFeatureItemText> GetItemTextByIdsAsync(int featureItemId,
            int languageId)
        {
            return await _pageFeatureItemTextRepository.GetByPageFeatureItemAndLanguageAsync(
                featureItemId, languageId);
        }

        public async Task<PageFeature> GetPageFeatureDetailsAsync(int id, int languageId)
        {
            var feature = await _pageFeatureRepository.GetIncludingChildrenAsync(id);

            feature.Items = feature.Items.OrderBy(_ => _.Order).ToList();
            foreach (var item in feature.Items)
            {
                item.PageFeatureItemText = await _pageFeatureItemTextRepository
                    .GetByPageFeatureItemAndLanguageAsync(item.Id, languageId);
            }

            return feature;
        }

        public async Task<int?> GetPageHeaderIdForPageFeatureAsync(int id)
        {
            return await _pageFeatureRepository.GetPageHeaderIdForPageFeatureAsync(id);
        }

        public async Task<int> GetPageLayoutIdForPageFeatureAsync(int id)
        {
            return await _pageFeatureRepository.GetPageLayoutIdForPageFeatureAsync(id);
        }

        public async Task<PageFeatureTemplate> GetTemplateForPageFeatureAsync(int id)
        {
            return await _pageFeatureTemplateRepository.GetForPageFeatureAsync(id);
        }

        public async Task<PageFeatureItemText> SetItemTextAsync(PageFeatureItemText itemText)
        {
            var currentText = await _pageFeatureItemTextRepository
                .GetByPageFeatureItemAndLanguageAsync(itemText.PageFeatureItemId,
                    itemText.LanguageId);

            if (currentText == null)
            {
                itemText.AltText = itemText.AltText?.Trim();
                itemText.Url = itemText.Url?.Trim();

                await _pageFeatureItemTextRepository.AddAsync(itemText);
                await _pageFeatureItemTextRepository.SaveAsync();
                _pageFeatureItemTextRepository.DetachEntity(itemText);

                return itemText;
            }
            else
            {
                currentText.AltText = itemText.AltText?.Trim();
                currentText.Url = itemText.Url?.Trim();

                if (!string.IsNullOrWhiteSpace(itemText.Filename))
                {
                    currentText.Filename = itemText.Filename?.Trim();
                }

                _pageFeatureItemTextRepository.Update(currentText);
                await _pageFeatureItemTextRepository.SaveAsync();
                _pageFeatureItemTextRepository.DetachEntity(currentText);

                return currentText;
            }
        }

        public async Task UpdateItemSortOrder(int id, bool increase)
        {
            var item = await _pageFeatureItemRepository.FindAsync(id);

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

            var itemInPosition = await _pageFeatureItemRepository.GetByFeatureAndOrderAsync(
                item.PageFeatureId, newSortOrder);

            if (itemInPosition == null)
            {
                throw new OcudaException("Item is already in the last position.");
            }

            itemInPosition.Order = item.Order;
            item.Order = newSortOrder;

            _pageFeatureItemRepository.Update(item);
            _pageFeatureItemRepository.Update(itemInPosition);
            await _pageFeatureItemRepository.SaveAsync();
        }
    }
}