using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Service
{
    public class CarouselService : BaseService<CarouselService>, ICarouselService
    {
        private readonly ICarouselButtonLabelRepository _carouselButtonLabelRepository;
        private readonly ICarouselButtonRepository _carouselButtonRepository;
        private readonly ICarouselItemRepository _carouselItemRepository;
        private readonly ICarouselItemTextRepository _carouselItemTextRepository;
        private readonly ICarouselRepository _carouselRepository;
        private readonly ICarouselTemplateRepository _carouselTemplateRepository;
        private readonly ICarouselTextRepository _carouselTextRepository;
        private readonly ILanguageRepository _languageRepository;
        private readonly ISiteSettingService _siteSettingService;

        public CarouselService(ILogger<CarouselService> logger,
            IHttpContextAccessor httpContextAccessor,
            ICarouselButtonLabelRepository carouselButtonLabelRepository,
            ICarouselButtonRepository carouselButtonRepository,
            ICarouselItemRepository carouselItemRepository,
            ICarouselItemTextRepository carouselItemTextRepository,
            ICarouselRepository carouselRepository,
            ICarouselTemplateRepository carouselTemplateRepository,
            ICarouselTextRepository carouselTextRepository,
            ILanguageRepository languageRepository,
            ISiteSettingService siteSettingService)
            : base(logger, httpContextAccessor)
        {
            _carouselButtonLabelRepository = carouselButtonLabelRepository
                ?? throw new ArgumentNullException(nameof(carouselButtonLabelRepository));
            _carouselButtonRepository = carouselButtonRepository
                ?? throw new ArgumentNullException(nameof(carouselButtonRepository));
            _carouselItemRepository = carouselItemRepository
                ?? throw new ArgumentNullException(nameof(carouselItemRepository));
            _carouselItemTextRepository = carouselItemTextRepository
                ?? throw new ArgumentNullException(nameof(carouselItemTextRepository));
            _carouselRepository = carouselRepository
                ?? throw new ArgumentNullException(nameof(carouselRepository));
            _carouselTemplateRepository = carouselTemplateRepository
                ?? throw new ArgumentNullException(nameof(carouselTemplateRepository));
            _carouselTextRepository = carouselTextRepository
                ?? throw new ArgumentNullException(nameof(carouselTextRepository));
            _languageRepository = languageRepository
                ?? throw new ArgumentNullException(nameof(languageRepository));
            _siteSettingService = siteSettingService
                ?? throw new ArgumentNullException(nameof(siteSettingService));
        }

        public async Task<Carousel> CloneAsync(int currentCarouselId)
        {
            var currentCarousel = await _carouselRepository.FindAsync(currentCarouselId);

            var newCarousel = new Carousel
            {
                Name = currentCarousel.Name
            };

            await _carouselRepository.AddAsync(newCarousel);
            await _carouselRepository.SaveAsync();

            var carouselTexts = await _carouselTextRepository.GetForCarouselAsync(currentCarouselId);

            foreach (var text in carouselTexts)
            {
                var newText = new CarouselText
                {
                    CarouselId = newCarousel.Id,
                    LanguageId = text.LanguageId,
                    Title = text.Title
                };

                await _carouselTextRepository.AddAsync(newText);
            }

            await _carouselTextRepository.SaveAsync();

            var carouselItems = await GetItemListByIdAsync(currentCarouselId);

            foreach (var item in carouselItems)
            {
                var newItem = new CarouselItem
                {
                    CarouselId = newCarousel.Id,
                    Order = item.Order,
                    Name = item.Name,
                };

                await _carouselItemRepository.AddAsync(newItem);
                await _carouselItemRepository.SaveAsync();

                var itemTexts = await _carouselItemTextRepository.GetAllForCarouselItemAsync(item.Id);

                foreach (var itemText in itemTexts)
                {
                    await _carouselItemTextRepository.AddAsync(new CarouselItemText
                    {
                        CarouselItemId = newItem.Id,
                        LanguageId = itemText.LanguageId,
                        Label = itemText.Label,
                        ImageUrl = itemText.ImageUrl,
                        Title = itemText.Title,
                        Description = itemText.Description,
                    });
                }

                await _carouselItemTextRepository.SaveAsync();

                foreach (var button in item.Buttons)
                {
                    await _carouselButtonRepository.AddAsync(new CarouselButton
                    {
                        CarouselItemId = newItem.Id,
                        Order = button.Order,
                        Url = button.Url,
                        LabelId = button.LabelId,
                    });
                }

                await _carouselButtonRepository.SaveAsync();
            }

            return newCarousel;
        }

        public async Task<Carousel> CreateAsync(Carousel carousel)
        {
            carousel = await CreateNoSaveAsync(carousel);

            await _carouselRepository.SaveAsync();
            return carousel;
        }

        public async Task<CarouselButton> CreateButtonAsync(CarouselButton button)
        {
            button.Url = button.Url?.Trim();

            var carouselTemplate = await _carouselTemplateRepository
                .GetByCarouselItemAsync(button.CarouselItemId);

            if (string.IsNullOrWhiteSpace(carouselTemplate?.ButtonUrlTemplate))
            {
                var delimitedLinkDomains = await _siteSettingService.GetSettingStringAsync(
                    Ops.Models.Keys.SiteSetting.Carousel.LinkRestrictToDomains);

                if (!string.IsNullOrWhiteSpace(delimitedLinkDomains))
                {
                    var allowedLinkDomains = delimitedLinkDomains.Split(',').ToList();

                    string linkDomain;
                    try
                    {
                        linkDomain = new Uri(button.Url).Host.Split('.', 2)[1];
                    }
                    catch (Exception)
                    {
                        throw new OcudaException("Invalid URL");
                    }

                    if (!allowedLinkDomains.Any(_ => _
                        .IndexOf(linkDomain, StringComparison.OrdinalIgnoreCase) != -1))
                    {
                        throw new OcudaException("URL is not from an accepted domain.");
                    }
                }
            }

            var maxSortOrder = await _carouselButtonRepository
                .GetMaxSortOrderForItemAsync(button.CarouselItemId);
            if (maxSortOrder.HasValue)
            {
                button.Order = maxSortOrder.Value + 1;
            }

            await _carouselButtonRepository.AddAsync(button);
            await _carouselButtonRepository.SaveAsync();
            return button;
        }

        public async Task<CarouselItem> CreateItemAsync(CarouselItem carouselItem)
        {
            var itemText = carouselItem.CarouselItemText;

            var imageUrl = itemText.ImageUrl?.Trim();
            await ValidateItemImageUrl(imageUrl);

            itemText.LanguageId = await _languageRepository.GetDefaultLanguageId();
            itemText.CarouselItem = carouselItem;
            itemText.Description = itemText.Description?.Trim();
            itemText.ImageUrl = imageUrl;
            itemText.Label = itemText.Label?.Trim();
            itemText.Title = itemText.Title?.Trim();

            var maxSortOrder = await _carouselItemRepository
                .GetMaxSortOrderForCarouselAsync(carouselItem.CarouselId);
            if (maxSortOrder.HasValue)
            {
                carouselItem.Order = maxSortOrder.Value + 1;
            }

            await _carouselItemRepository.AddAsync(carouselItem);
            await _carouselItemTextRepository.AddAsync(itemText);
            await _carouselItemRepository.SaveAsync();
            return carouselItem;
        }

        public async Task<Carousel> CreateNoSaveAsync(Carousel carousel)
        {
            var carouselText = carousel.CarouselText;
            carouselText.LanguageId = await _languageRepository.GetDefaultLanguageId();
            carouselText.Carousel = carousel;
            carouselText.Title = carouselText.Title?.Trim();
            carouselText.Footer = carouselText.Footer?.Trim();

            await _carouselRepository.AddAsync(carousel);
            await _carouselTextRepository.AddAsync(carouselText);
            return carousel;
        }

        public async Task DeleteAsync(int carouselId)
        {
            await DeleteNoSaveAsync(carouselId);
            await _carouselRepository.SaveAsync();
        }

        public async Task DeleteButtonAsync(int carouselButtonId)
        {
            var carouselButton = await _carouselButtonRepository.FindAsync(carouselButtonId);

            var subsequentButtons = await _carouselButtonRepository.GetItemSubsequentAsync(
                carouselButton.CarouselItemId, carouselButton.Order);

            if (subsequentButtons.Count > 0)
            {
                subsequentButtons.ForEach(_ => _.Order--);
                _carouselButtonRepository.UpdateRange(subsequentButtons);
            }

            _carouselButtonRepository.Remove(carouselButton);
            await _carouselButtonRepository.SaveAsync();
        }

        public async Task DeleteItemAsync(int carouselItemId)
        {
            var carouselItem = await _carouselItemRepository
                .GetIncludingChildrenAsync(carouselItemId);

            if (carouselItem == null)
            {
                throw new OcudaException("Carousel item does not exist.");
            }

            var subsequentItems = await _carouselItemRepository.GetCarouselSubsequentAsync(
                carouselItem.CarouselId, carouselItem.Order);

            if (subsequentItems.Count > 0)
            {
                subsequentItems.ForEach(_ => _.Order--);
                _carouselItemRepository.UpdateRange(subsequentItems);
            }

            var carouselItemTexts = await _carouselItemTextRepository
                .GetAllForCarouselItemAsync(carouselItem.Id);
            _carouselItemTextRepository.RemoveRange(carouselItemTexts);

            _carouselButtonRepository.RemoveRange(carouselItem.Buttons);
            _carouselItemRepository.Remove(carouselItem);
            await _carouselItemRepository.SaveAsync();
        }

        public async Task DeleteNoSaveAsync(int carouselId)
        {
            var carousel = await _carouselRepository.GetIncludingChildrenAsync(carouselId);

            if (carousel == null)
            {
                throw new OcudaException("Carousel does not exist.");
            }

            var carouselTexts = await _carouselTextRepository.GetForCarouselAsync(carousel.Id);
            _carouselTextRepository.RemoveRange(carouselTexts);

            var carouselItemTexts = await _carouselItemTextRepository
                .GetAllForCarouselAsync(carousel.Id);
            _carouselItemTextRepository.RemoveRange(carouselItemTexts);

            var carouselButtons = carousel.Items.SelectMany(_ => _.Buttons).ToList();
            _carouselButtonRepository.RemoveRange(carouselButtons);

            _carouselItemRepository.RemoveRange(carousel.Items);

            _carouselRepository.Remove(carousel);
        }

        public async Task<CarouselButton> EditButtonAsync(CarouselButton carouselButton)
        {
            var currentCarouselButton = await _carouselButtonRepository.FindAsync(carouselButton.Id);

            var linkUrl = carouselButton.Url?.Trim();

            var carouselTemplate = await _carouselTemplateRepository
                .GetByCarouselItemAsync(currentCarouselButton.CarouselItemId);

            if (string.IsNullOrWhiteSpace(carouselTemplate?.ButtonUrlTemplate))
            {
                var delimitedLinkDomains = await _siteSettingService.GetSettingStringAsync(
                    Ops.Models.Keys.SiteSetting.Carousel.LinkRestrictToDomains);

                if (!string.IsNullOrWhiteSpace(delimitedLinkDomains))
                {
                    var allowedLinkDomains = delimitedLinkDomains.Split(',').ToList();

                    string linkDomain;
                    try
                    {
                        linkDomain = new Uri(linkUrl).Host.Split('.', 2)[1];
                    }
                    catch (Exception)
                    {
                        throw new OcudaException("Invalid URL");
                    }

                    if (!allowedLinkDomains.Any(_ => _
                        .IndexOf(linkDomain, StringComparison.OrdinalIgnoreCase) != -1))
                    {
                        throw new OcudaException("URL is not from an accepted domain.");
                    }
                }
            }

            currentCarouselButton.LabelId = carouselButton.LabelId;
            currentCarouselButton.Url = carouselButton.Url?.Trim();

            _carouselButtonRepository.Update(currentCarouselButton);
            await _carouselButtonRepository.SaveAsync();
            return currentCarouselButton;
        }

        public async Task<ICollection<CarouselTemplate>> GetAllTemplatesAsync()
        {
            return await _carouselTemplateRepository.GetAllAsync();
        }

        public async Task<ICollection<CarouselButtonLabel>> GetButtonLabelsAsync()
        {
            return await _carouselButtonLabelRepository.GetAllAsync();
        }

        public async Task<Carousel> GetCarouselDetailsAsync(int id, int languageId)
        {
            var carousel = await _carouselRepository.GetIncludingChildrenWithLabelsAsync(id);

            carousel.CarouselText = await _carouselTextRepository
                .GetByCarouselAndLanguageAsync(id, languageId);

            carousel.Items = carousel.Items.OrderBy(_ => _.Order).ToList();
            foreach (var item in carousel.Items)
            {
                item.CarouselItemText = await _carouselItemTextRepository
                    .GetByCarouselItemAndLanguageAsync(item.Id, languageId);

                item.Buttons = item.Buttons.OrderBy(_ => _.Order).ToList();
            }

            return carousel;
        }

        public async Task<int> GetCarouselIdForButtonAsync(int id)
        {
            return await _carouselButtonRepository.GetCarouselIdForButtonAsync(id);
        }

        public async Task<string> GetDefaultNameForCarouselAsync(int carouselId)
        {
            var defaultLanguageId = await _languageRepository.GetDefaultLanguageId();
            return (await _carouselTextRepository.GetByCarouselAndLanguageAsync(carouselId,
                defaultLanguageId))
                .Title;
        }

        public async Task<string> GetDefaultNameForItemAsync(int itemId)
        {
            var defaultLanguageId = await _languageRepository.GetDefaultLanguageId();
            return (await _carouselItemTextRepository.GetByCarouselItemAndLanguageAsync(itemId,
                defaultLanguageId))
                .Label;
        }

        public async Task<CarouselItem> GetItemByIdAsync(int id)
        {
            return await _carouselItemRepository.FindAsync(id);
        }

        public async Task<List<CarouselItem>> GetItemListByIdAsync(int carouselId)
        {
            return await _carouselItemRepository.GetAllByCarouselIdAsync(carouselId);
        }

        public async Task<int?> GetPageHeaderIdForCarouselAsync(int id)
        {
            return await _carouselRepository.GetPageHeaderIdForCarouselAsync(id);
        }

        public async Task<int?> GetPageLayoutIdForCarouselAsync(int id)
        {
            return await _carouselRepository.GetPageLayoutIdForCarouselAsync(id);
        }

        public async Task<DataWithCount<ICollection<Carousel>>> GetPaginatedListAsync(
                                                                                                                                                            BaseFilter filter)
        {
            return await _carouselRepository.GetPaginatedListAsync(filter);
        }

        public async Task<CarouselTemplate> GetTemplateForPageLayoutAsync(int id)
        {
            return await _carouselTemplateRepository.GetForPageLayoutAsync(id);
        }

        public async Task<CarouselText> SetCarouselTextAsync(CarouselText carouselText)
        {
            var currentText = await _carouselTextRepository
                .GetByCarouselAndLanguageAsync(carouselText.CarouselId, carouselText.LanguageId);

            if (currentText == null)
            {
                carouselText.Title = carouselText.Title?.Trim();
                carouselText.Footer = carouselText.Footer?.Trim();

                await _carouselTextRepository.AddAsync(carouselText);
                await _carouselTextRepository.SaveAsync();
                return carouselText;
            }
            else
            {
                currentText.Title = carouselText.Title?.Trim();
                currentText.Footer = carouselText.Footer?.Trim();

                _carouselTextRepository.Update(currentText);
                await _carouselTextRepository.SaveAsync();
                return currentText;
            }
        }

        public async Task<CarouselItemText> SetItemTextAsync(CarouselItemText itemText)
        {
            var imageUrl = itemText.ImageUrl?.Trim();
            await ValidateItemImageUrl(imageUrl);

            var currentText = await _carouselItemTextRepository.GetByCarouselItemAndLanguageAsync(
                itemText.CarouselItemId, itemText.LanguageId);

            if (currentText == null)
            {
                itemText.Description = itemText.Description?.Trim();
                itemText.ImageUrl = imageUrl;
                itemText.Label = itemText.Label?.Trim();
                itemText.Title = itemText.Title?.Trim();

                await _carouselItemTextRepository.AddAsync(itemText);
                await _carouselItemTextRepository.SaveAsync();
                return itemText;
            }
            else
            {
                currentText.Description = itemText.Description?.Trim();
                currentText.ImageUrl = imageUrl;
                currentText.Label = itemText.Label?.Trim();
                currentText.Title = itemText.Title?.Trim();

                _carouselItemTextRepository.Update(currentText);
                await _carouselItemTextRepository.SaveAsync();
                return currentText;
            }
        }

        public async Task UpdateButtonSortOrder(int id, bool increase)
        {
            var button = await _carouselButtonRepository.FindAsync(id);

            int newSortOrder;
            if (increase)
            {
                newSortOrder = button.Order + 1;
            }
            else
            {
                if (button.Order == 0)
                {
                    throw new OcudaException("Button is already in the first position.");
                }
                newSortOrder = button.Order - 1;
            }

            var buttonInPosition = await _carouselButtonRepository.GetByItemAndOrderAsync(
                button.CarouselItemId, newSortOrder);

            if (buttonInPosition == null)
            {
                throw new OcudaException("Button is already in the last position.");
            }

            buttonInPosition.Order = button.Order;
            button.Order = newSortOrder;

            _carouselButtonRepository.Update(button);
            _carouselButtonRepository.Update(buttonInPosition);
            await _carouselButtonRepository.SaveAsync();
        }

        public async Task UpdateItemSortOrder(int id, bool increase)
        {
            var item = await _carouselItemRepository.FindAsync(id);

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

            var itemInPosition = await _carouselItemRepository.GetByCarouselAndOrderAsync(
                item.CarouselId, newSortOrder);

            if (itemInPosition == null)
            {
                throw new OcudaException("Item is already in the last position.");
            }

            itemInPosition.Order = item.Order;
            item.Order = newSortOrder;

            _carouselItemRepository.Update(item);
            _carouselItemRepository.Update(itemInPosition);
            await _carouselItemRepository.SaveAsync();
        }

        private async Task ValidateItemImageUrl(string imageUrl)
        {
            var delimitedImageDomains = await _siteSettingService.GetSettingStringAsync(
                    Ops.Models.Keys.SiteSetting.Carousel.ImageRestrictToDomains);

            if (!string.IsNullOrWhiteSpace(delimitedImageDomains))
            {
                var allowedImageDomains = delimitedImageDomains.Split(',').ToList();

                string imageDomain;
                try
                {
                    imageDomain = new Uri(imageUrl).Host.Split('.', 2)[1];
                }
                catch (Exception)
                {
                    throw new OcudaException("Invalid Image URL");
                }

                if (!allowedImageDomains.Any(_ => _
                    .IndexOf(imageDomain, StringComparison.OrdinalIgnoreCase) != -1))
                {
                    throw new OcudaException("Image URL is not from an accepted domain.");
                }
            }
        }
    }
}