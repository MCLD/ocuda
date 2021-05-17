using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Service.Abstract;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Repositories;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Service
{
    public class ImageFeatureService : BaseService<ImageFeatureService>, IImageFeatureService
    {
        private const string FeaturesFilePath = "features";
        private const string ImagesFilePath = "images";
        private readonly IImageFeatureItemRepository _imageFeatureItemRepository;
        private readonly IImageFeatureItemTextRepository _imageFeatureItemTextRepository;
        private readonly IImageFeatureRepository _imageFeatureRepository;
        private readonly IImageFeatureTemplateRepository _imageFeatureTemplateRepository;
        private readonly ILanguageService _languageService;
        private readonly ISiteSettingService _siteSettingService;

        public ImageFeatureService(ILogger<ImageFeatureService> logger,
            IHttpContextAccessor httpContextAccessor,
            IImageFeatureItemRepository imageFeatureItemRepository,
            IImageFeatureItemTextRepository imageFeatureItemTextRepository,
            IImageFeatureRepository imageFeatureRepository,
            IImageFeatureTemplateRepository imageFeatureTemplateRepository,
            ILanguageService languageService,
            ISiteSettingService siteSettingService)
            : base(logger, httpContextAccessor)
        {
            _imageFeatureItemRepository = imageFeatureItemRepository
                ?? throw new ArgumentNullException(nameof(imageFeatureItemRepository));
            _imageFeatureItemTextRepository = imageFeatureItemTextRepository
                ?? throw new ArgumentNullException(nameof(imageFeatureItemTextRepository));
            _imageFeatureRepository = imageFeatureRepository
                ?? throw new ArgumentNullException(nameof(imageFeatureRepository));
            _imageFeatureTemplateRepository = imageFeatureTemplateRepository
                ?? throw new ArgumentNullException(nameof(imageFeatureTemplateRepository));
            _languageService = languageService
                ?? throw new ArgumentNullException(nameof(languageService));
            _siteSettingService = siteSettingService
                ?? throw new ArgumentNullException(nameof(siteSettingService));
        }

        public async Task<ImageFeatureItem> CreateItemAsync(ImageFeatureItem imageFeatureItem)
        {
            imageFeatureItem.Name = imageFeatureItem.Name?.Trim();

            var maxSortOrder = await _imageFeatureItemRepository
                .GetMaxSortOrderForImageFeatureAsync(imageFeatureItem.ImageFeatureId);
            if (maxSortOrder.HasValue)
            {
                imageFeatureItem.Order = maxSortOrder.Value + 1;
            }

            await _imageFeatureItemRepository.AddAsync(imageFeatureItem);
            await _imageFeatureItemRepository.SaveAsync();
            return imageFeatureItem;
        }

        public async Task<ImageFeature> CreateNoSaveAsync(ImageFeature imageFeature)
        {
            imageFeature.Name = imageFeature.Name?.Trim();

            await _imageFeatureRepository.AddAsync(imageFeature);
            return imageFeature;
        }

        public async Task DeleteItemAsync(int imageFeatureItemId)
        {
            var imageFeatureItem = await _imageFeatureItemRepository.FindAsync(imageFeatureItemId);

            if (imageFeatureItem == null)
            {
                throw new OcudaException("Image feature item does not exist.");
            }

            var subsequentItems = await _imageFeatureItemRepository.GetImageFeatureSubsequentAsync(
                imageFeatureItem.ImageFeatureId,
                imageFeatureItem.Order);

            if (subsequentItems.Count > 0)
            {
                subsequentItems.ForEach(_ => _.Order--);
                _imageFeatureItemRepository.UpdateRange(subsequentItems);
            }

            var imageFeatureItemTexts = await _imageFeatureItemTextRepository
                .GetAllForImageFeatureItemAsync(imageFeatureItem.Id);

            IDictionary<string, string> issues = null;

            if (imageFeatureItemTexts.Count > 0)
            {
                issues = await DeleteImages(imageFeatureItemTexts);
                _imageFeatureItemTextRepository.RemoveRange(imageFeatureItemTexts);
            }

            _imageFeatureItemRepository.Remove(imageFeatureItem);
            await _imageFeatureItemRepository.SaveAsync();

            if (issues?.Count > 0)
            {
                string issueTexts = string.Join(',', issues);
                var oex = new OcudaException($"File deletion error(s): {issueTexts}");
                foreach (var key in issues.Keys)
                {
                    oex.Data[key] = issues[key];
                }
                throw oex;
            }
        }

        public async Task DeleteNoSaveAsync(int imageFeatureId)
        {
            var imageFeature = await _imageFeatureRepository.FindAsync(imageFeatureId);
            if (imageFeature == null)
            {
                throw new OcudaException("Could not find that image feature");
            }
            var items = await _imageFeatureItemRepository.GetByImageFeatureAsync(imageFeatureId);

            IDictionary<string, string> issues = null;

            foreach (var item in items)
            {
                var itemTexts = await _imageFeatureItemTextRepository
                    .GetAllForImageFeatureItemAsync(item.Id);
                issues = await DeleteImages(itemTexts);
                _imageFeatureItemTextRepository.RemoveRange(itemTexts);
            }
            _imageFeatureItemRepository.RemoveRange(items);
            _imageFeatureRepository.Remove(imageFeature);

            if (issues?.Count > 0)
            {
                string issueTexts = string.Join(',', issues);
                var oex = new OcudaException($"File deletion error(s): {issueTexts}");
                foreach (var key in issues.Keys)
                {
                    oex.Data[key] = issues[key];
                }
                throw oex;
            }
        }

        public async Task<ImageFeature> EditAsync(ImageFeature imageFeature)
        {
            var currentImageFeature = await _imageFeatureRepository.FindAsync(imageFeature.Id);

            currentImageFeature.Name = imageFeature.Name?.Trim();

            _imageFeatureRepository.Update(currentImageFeature);
            await _imageFeatureRepository.SaveAsync();
            return currentImageFeature;
        }

        public async Task<ImageFeatureItem> EditItemAsync(ImageFeatureItem imageFeatureItem)
        {
            var currentItem = await _imageFeatureItemRepository.FindAsync(imageFeatureItem.Id);

            currentItem.Name = imageFeatureItem.Name?.Trim();
            currentItem.StartDate = imageFeatureItem.StartDate;
            currentItem.EndDate = imageFeatureItem.EndDate;

            _imageFeatureItemRepository.Update(currentItem);
            await _imageFeatureItemRepository.SaveAsync();
            return currentItem;
        }

        public async Task<ICollection<ImageFeatureTemplate>> GetAllTemplatesAsync()
        {
            return await _imageFeatureTemplateRepository.GetAllAsync();
        }

        public async Task<ImageFeature> GetImageFeatureDetailsAsync(int id, int languageId)
        {
            var imageFeature = await _imageFeatureRepository.GetIncludingChildrenAsync(id);

            imageFeature.Items = imageFeature.Items.OrderBy(_ => _.Order).ToList();
            foreach (var item in imageFeature.Items)
            {
                item.ImageFeatureItemText = await _imageFeatureItemTextRepository
                    .GetByImageFeatureItemAndLanguageAsync(item.Id, languageId);
            }

            return imageFeature;
        }

        public async Task<string> GetImageFeaturePathAsync(string languageName)
        {
            string basePath = await _siteSettingService.GetSettingStringAsync(
                Ops.Models.Keys.SiteSetting.SiteManagement.PromenadePublicPath);

            return Path.Combine(basePath,
                ImagesFilePath,
                languageName,
                FeaturesFilePath);
        }

        public async Task<ImageFeatureItem> GetItemByIdAsync(int id)
        {
            return await _imageFeatureItemRepository.FindAsync(id);
        }

        public async Task<ImageFeatureItemText> GetItemTextByIdsAsync(int imageFeatureItemId,
            int languageId)
        {
            return await _imageFeatureItemTextRepository.GetByImageFeatureItemAndLanguageAsync(
                imageFeatureItemId, languageId);
        }

        public async Task<int?> GetPageHeaderIdForImageFeatureAsync(int id)
        {
            return await _imageFeatureRepository.GetPageHeaderIdForImageFeatureAsync(id);
        }

        public async Task<int> GetPageLayoutIdForImageFeatureAsync(int id)
        {
            return await _imageFeatureRepository.GetPageLayoutIdForImageFeatureAsync(id);
        }

        public async Task<ImageFeatureTemplate> GetTemplateForImageFeatureAsync(int id)
        {
            return await _imageFeatureTemplateRepository.GetForImageFeatureAsync(id);
        }

        public async Task<ImageFeatureTemplate> GetTemplateForPageLayoutAsync(int id)
        {
            return await _imageFeatureTemplateRepository.GetForPageLayoutAsync(id);
        }

        public async Task<ImageFeatureItemText> SetItemTextAsync(ImageFeatureItemText itemText)
        {
            var currentText = await _imageFeatureItemTextRepository
                .GetByImageFeatureItemAndLanguageAsync(itemText.ImageFeatureItemId,
                    itemText.LanguageId);

            if (currentText == null)
            {
                itemText.AltText = itemText.AltText?.Trim();
                itemText.Link = itemText.Link?.Trim();

                await _imageFeatureItemTextRepository.AddAsync(itemText);
                await _imageFeatureItemTextRepository.SaveAsync();
                _imageFeatureItemTextRepository.DetachEntity(itemText);

                return itemText;
            }
            else
            {
                currentText.AltText = itemText.AltText?.Trim();
                currentText.Link = itemText.Link?.Trim();


                if (!string.IsNullOrWhiteSpace(itemText.Filename))
                {
                    if (currentText.Filename != itemText.Filename)
                    {
                        // new filename means delete the old file
                        await DeleteImage(currentText);
                    }
                    currentText.Filename = itemText.Filename?.Trim();
                }

                _imageFeatureItemTextRepository.Update(currentText);
                await _imageFeatureItemTextRepository.SaveAsync();
                _imageFeatureItemTextRepository.DetachEntity(currentText);

                return currentText;
            }
        }

        public async Task UpdateItemSortOrder(int id, bool increase)
        {
            var item = await _imageFeatureItemRepository.FindAsync(id);

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

            var itemInPosition = await _imageFeatureItemRepository.GetByImageFeatureAndOrderAsync(
                item.ImageFeatureId, newSortOrder);

            if (itemInPosition == null)
            {
                throw new OcudaException("Item is already in the last position.");
            }

            itemInPosition.Order = item.Order;
            item.Order = newSortOrder;

            _imageFeatureItemRepository.Update(item);
            _imageFeatureItemRepository.Update(itemInPosition);
            await _imageFeatureItemRepository.SaveAsync();
        }
        private async Task<IDictionary<string, string>> DeleteImage(ImageFeatureItemText itemTexts)
        {
            return await DeleteImages(new[] { itemTexts });
        }

        private async Task<IDictionary<string, string>>
            DeleteImages(ICollection<ImageFeatureItemText> itemTexts)
        {
            var issues = new Dictionary<string, string>();
            if (itemTexts.Count > 0)
            {
                var languages = await _languageService.GetActiveAsync();

                foreach (var itemText in itemTexts)
                {
                    var languageName = languages.SingleOrDefault(_ => _.Id == itemText.LanguageId);

                    if (languageName != null)
                    {
                        var path = await GetImageFeaturePathAsync(languageName.Name);

                        try
                        {
                            File.Delete(Path.Combine(path, itemText.Filename));
                        }
                        catch (IOException ioex)
                        {
                            _logger.LogError(ioex,
                                "Problem deleting file {FilePath}: {ErrorMessage}",
                                path,
                                ioex.Message);
                            issues.Add(path, ioex.Message);
                        }
                    }
                }
            }
            return issues;
        }
    }
}