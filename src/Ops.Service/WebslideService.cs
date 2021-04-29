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
    public class WebslideService : BaseService<WebslideService>, IWebslideService
    {
        private readonly IWebslideItemRepository _webslideItemRepository;
        private readonly IWebslideItemTextRepository _webslideItemTextRepository;
        private readonly IWebslideRepository _webslideRepository;
        private readonly IWebslideTemplateRepository _webslideTemplateRepository;

        public WebslideService(ILogger<WebslideService> logger,
            IHttpContextAccessor httpContextAccessor,
            IWebslideItemRepository webslideItemRepository,
            IWebslideItemTextRepository webslideItemTextRepository,
            IWebslideRepository webslideRepository,
            IWebslideTemplateRepository webslideTemplateRepository)
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

        public async Task<WebslideItem> CreateItemAsync(WebslideItem webslideItem)
        {
            webslideItem.Name = webslideItem.Name?.Trim();

            var maxSortOrder = await _webslideItemRepository
                .GetMaxSortOrderForWebslideAsync(webslideItem.WebslideId);
            if (maxSortOrder.HasValue)
            {
                webslideItem.Order = maxSortOrder.Value + 1;
            }

            await _webslideItemRepository.AddAsync(webslideItem);
            await _webslideItemRepository.SaveAsync();
            return webslideItem;
        }

        public async Task<Webslide> CreateNoSaveAsync(Webslide webslide)
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

            var subsequentItems = await _webslideItemRepository.GetWebslideSubsequentAsync(
                webslideItem.WebslideId, webslideItem.Order);

            if (subsequentItems.Count > 0)
            {
                subsequentItems.ForEach(_ => _.Order--);
                _webslideItemRepository.UpdateRange(subsequentItems);
            }

            var webslideItemTexts = await _webslideItemTextRepository
                .GetAllForWebslideItemAsync(webslideItem.Id);
            _webslideItemTextRepository.RemoveRange(webslideItemTexts);

            _webslideItemRepository.Remove(webslideItem);
            await _webslideItemRepository.SaveAsync();
        }

        public async Task<Webslide> EditAsync(Webslide webslide)
        {
            var currentWebslide = await _webslideRepository.FindAsync(webslide.Id);

            currentWebslide.Name = webslide.Name?.Trim();

            _webslideRepository.Update(currentWebslide);
            await _webslideRepository.SaveAsync();
            return currentWebslide;
        }

        public async Task<WebslideItem> EditItemAsync(WebslideItem webslideItem)
        {
            var currentWebslideItem = await _webslideItemRepository.FindAsync(webslideItem.Id);

            currentWebslideItem.Name = webslideItem.Name?.Trim();
            currentWebslideItem.StartDate = webslideItem.StartDate;
            currentWebslideItem.EndDate = webslideItem.EndDate;

            _webslideItemRepository.Update(currentWebslideItem);
            await _webslideItemRepository.SaveAsync();
            return currentWebslideItem;
        }

        public async Task<ICollection<WebslideTemplate>> GetAllTemplatesAsync()
        {
            return await _webslideTemplateRepository.GetAllAsync();
        }

        public async Task<WebslideItem> GetItemByIdAsync(int id)
        {
            return await _webslideItemRepository.FindAsync(id);
        }

        public async Task<WebslideItemText> GetItemTextByIdsAsync(int webslideItemId, 
            int languageId)
        {
            return await _webslideItemTextRepository.GetByWebslideItemAndLanguageAsync(
                webslideItemId, languageId);
        }

        public async Task<int?> GetPageHeaderIdForWebslideAsync(int id)
        {
            return await _webslideRepository.GetPageHeaderIdForWebslideAsync(id);
        }

        public async Task<int> GetPageLayoutIdForWebslideAsync(int id)
        {
            return await _webslideRepository.GetPageLayoutIdForWebslideAsync(id);
        }

        public async Task<WebslideTemplate> GetTemplateForPageLayoutAsync(int id)
        {
            return await _webslideTemplateRepository.GetForPageLayoutAsync(id);
        }

        public async Task<WebslideTemplate> GetTemplateForWebslideAsync(int id)
        {
            return await _webslideTemplateRepository.GetForWebslideAsync(id);
        }

        public async Task<Webslide> GetWebslideDetailsAsync(int id, int languageId)
        {
            var webslide = await _webslideRepository.GetIncludingChildrenAsync(id);

            webslide.Items = webslide.Items.OrderBy(_ => _.Order).ToList();
            foreach (var item in webslide.Items)
            {
                item.WebslideItemText = await _webslideItemTextRepository
                    .GetByWebslideItemAndLanguageAsync(item.Id, languageId);
            }

            return webslide;
        }

        public async Task<WebslideItemText> SetItemTextAsync(WebslideItemText itemText)
        {
            var currentText = await _webslideItemTextRepository.GetByWebslideItemAndLanguageAsync(
                itemText.WebslideItemId, itemText.LanguageId);

            if (currentText == null)
            {
                itemText.AltText = itemText.AltText?.Trim();
                itemText.Url = itemText.Url?.Trim();

                await _webslideItemTextRepository.AddAsync(itemText);
                await _webslideItemTextRepository.SaveAsync();
                _webslideItemTextRepository.DetachEntity(itemText);

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

            var itemInPosition = await _webslideItemRepository.GetByWebslideAndOrderAsync(
                item.WebslideId, newSortOrder);

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
