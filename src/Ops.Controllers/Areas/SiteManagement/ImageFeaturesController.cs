﻿using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageOptimApi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.ImageFeatures;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Extensions;
using Ocuda.Utility.Keys;
using SixLabors.ImageSharp;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement
{
    [Area("SiteManagement")]
    [Route("[area]/[controller]")]
    public class ImageFeaturesController : BaseController<ImageFeaturesController>
    {
        private const string DetailModelStateKey = "ImageFeatures.Detail";
        private const string ItemErrorMessageKey = "ImageFeatures.ItemErrorMessage";
        private const int MaximumFileSizeBytes = 200 * 1024;
        private static readonly string[] ValidImageExtensions = new[] { ".jpg", ".png", ".svg" };

        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IImageFeatureService _imageFeatureService;
        private readonly IImageService _imageService;
        private readonly ILanguageService _languageService;
        private readonly IPermissionGroupService _permissionGroupService;

        public ImageFeaturesController(ServiceFacades.Controller<ImageFeaturesController> context,
            IDateTimeProvider dateTimeProvider,
            ILanguageService languageService,
            IImageFeatureService imageFeatureService,
            IImageService imageService,
            IPermissionGroupService permissionGroupService)
            : base(context)
        {
            _dateTimeProvider = dateTimeProvider
                ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _languageService = languageService
                ?? throw new ArgumentNullException(nameof(languageService));
            _imageFeatureService = imageFeatureService
                ?? throw new ArgumentNullException(nameof(imageFeatureService));
            _imageService = imageService
                ?? throw new ArgumentNullException(nameof(imageService));
            _permissionGroupService = permissionGroupService
                ?? throw new ArgumentNullException(nameof(permissionGroupService));
        }

        public static string Area
        { get { return "SiteManagement"; } }

        public static string Name
        { get { return "ImageFeatures"; } }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> AddImageFeatureItem(DetailViewModel model)
        {
            JsonResponse response;

            if (model == null)
            {
                response = new JsonResponse
                {
                    Message = "Invalid request",
                    Success = false
                };
            }
            else
            {
                if (await HasPageContentPermissionAsync(model.ImageFeatureItem.ImageFeatureId))
                {
                    if (ModelState.IsValid)
                    {
                        try
                        {
                            if (model.ImageFeatureStartDate.HasValue
                                && model.ImageFeatureStartTime.HasValue)
                            {
                                model.ImageFeatureItem.StartDate =
                                    model.ImageFeatureStartDate.Value
                                        .CombineWithTime(model.ImageFeatureStartTime.Value);
                            }

                            if (model.ImageFeatureEndDate.HasValue
                                && model.ImageFeatureEndTime.HasValue)
                            {
                                model.ImageFeatureItem.EndDate =
                                    model.ImageFeatureEndDate.Value
                                        .CombineWithTime(model.ImageFeatureEndTime.Value);
                            }

                            var featureItem = await _imageFeatureService.CreateItemAsync(
                                model.ImageFeatureItem);

                            var language = await _languageService.GetActiveByIdAsync(model.LanguageId);

                            response = new JsonResponse
                            {
                                Success = true,
                                Url = Url.Action(nameof(Detail), new
                                {
                                    id = featureItem.ImageFeatureId,
                                    language = language.IsDefault ? null : language.Name,
                                    item = featureItem.Id
                                })
                            };

                            ShowAlertSuccess($"Created item: {featureItem.Name}");
                        }
                        catch (OcudaException ex)
                        {
                            response = new JsonResponse
                            {
                                Success = false,
                                Message = ex.Message
                            };
                        }
                    }
                    else
                    {
                        var errors = ModelState.Values
                            .SelectMany(_ => _.Errors)
                            .Select(_ => _.ErrorMessage);

                        response = new JsonResponse
                        {
                            Success = false,
                            Message = string.Join(Environment.NewLine, errors)
                        };
                    }
                }
                else
                {
                    response = new JsonResponse
                    {
                        Message = "Unauthorized",
                        Success = false
                    };
                }
            }

            return Json(response);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<JsonResult> ChangeSort(int id, bool increase)
        {
            JsonResponse response;

            var featureId = (await _imageFeatureService.GetItemByIdAsync(id)).ImageFeatureId;

            if (await HasPageContentPermissionAsync(featureId))
            {
                try
                {
                    await _imageFeatureService.UpdateItemSortOrder(id, increase);

                    response = new JsonResponse
                    {
                        Success = true
                    };
                }
                catch (OcudaException ex)
                {
                    response = new JsonResponse
                    {
                        Message = ex.Message,
                        Success = false
                    };
                }
            }
            else
            {
                response = new JsonResponse
                {
                    Message = "Unauthorized",
                    Success = false
                };
            }

            return Json(response);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> DeleteImageFeatureItem(DetailViewModel model)
        {
            var featureItem = await _imageFeatureService
                .GetItemByIdAsync(model.ImageFeatureItem.Id);

            if (!await HasPageContentPermissionAsync(featureItem.ImageFeatureId))
            {
                return RedirectToUnauthorized();
            }

            var language = await _languageService.GetActiveByIdAsync(model.LanguageId);

            try
            {
                await _imageFeatureService.DeleteItemAsync(model.ImageFeatureItem.Id);
                ShowAlertSuccess($"Deleted item: {model.ImageFeatureItem.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting feature item: {Message}", ex.Message);
                ShowAlertDanger($"Error deleting item: {model.ImageFeatureItem.Name}");
            }

            return RedirectToAction(nameof(Detail), new
            {
                id = model.ImageFeatureId,
                language = language.IsDefault ? null : language.Name
            });
        }

        [Route("[action]/{id}")]
        [Route("[action]/feature/{id}/item/{item}")]
        [Route("[action]/layout/{pageLayoutId}/feature/{id}")]
        [Route("[action]/layout/{pageLayoutId}/feature/{id}/item/{item}")]
        [RestoreModelState(Key = DetailModelStateKey)]
        public async Task<IActionResult> Detail(int id, string language, int? item, int? pageLayoutId)
        {
            if (!await HasPageContentPermissionAsync(id))
            {
                return RedirectToUnauthorized();
            }

            var languages = await _languageService.GetActiveAsync();

            var selectedLanguage = languages
                .FirstOrDefault(_ => _.Name.Equals(language, StringComparison.OrdinalIgnoreCase))
                ?? languages.Single(_ => _.IsDefault);

            var feature = await _imageFeatureService.GetImageFeatureDetailsAsync(id,
                selectedLanguage.Id);

            var viewModel = new DetailViewModel
            {
                AcceptImageExtensions = string.Join(',', ValidImageExtensions),
                CurrentDateTime = _dateTimeProvider.Now,
                FocusItemId = item,
                HasEditTemplatePermissions = IsSiteManager(),
                ImageFeature = feature,
                ImageFeatureId = feature.Id,
                ImageFeatureTemplate = await _imageFeatureService
                    .GetTemplateForImageFeatureAsync(feature.Id),
                ItemErrorMessage = TempData[ItemErrorMessageKey] as string,
                LanguageId = selectedLanguage.Id,
                LanguageList = new SelectList(languages,
                    nameof(Language.Name),
                    nameof(Language.Description),
                    selectedLanguage.Name),
                PageLayoutId = pageLayoutId ?? await _imageFeatureService
                    .GetPageLayoutIdForImageFeatureAsync(feature.Id),
            };

            var promBaseLink = await _siteSettingService
                .GetSettingStringAsync(Models.Keys.SiteSetting.SiteManagement.PromenadeUrl);

            if (!string.IsNullOrEmpty(promBaseLink))
            {
                viewModel.PublicLinkBase = $"{promBaseLink}assets/images/{selectedLanguage.Name}/features";
            }

            viewModel.ImageFeatureTemplate ??= new ImageFeatureTemplate();
            viewModel.ImageFeatureTemplate.MaximumFileSizeBytes ??= MaximumFileSizeBytes;

            return View(viewModel);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> EditImageFeatureItem(DetailViewModel model)
        {
            JsonResponse response;

            if (model == null)
            {
                response = new JsonResponse
                {
                    Message = "Invalid request",
                    Success = false
                };
            }
            else
            {
                var featureItem = await _imageFeatureService
                    .GetItemByIdAsync(model.ImageFeatureItem.Id);

                if (await HasPageContentPermissionAsync(featureItem.ImageFeatureId))
                {
                    if (ModelState.IsValid)
                    {
                        try
                        {
                            if (model.ImageFeatureStartDate.HasValue
                                && model.ImageFeatureStartTime.HasValue)
                            {
                                model.ImageFeatureItem.StartDate =
                                    model.ImageFeatureStartDate.Value
                                        .CombineWithTime(model.ImageFeatureStartTime.Value);
                            }

                            if (model.ImageFeatureEndDate.HasValue
                                && model.ImageFeatureEndTime.HasValue)
                            {
                                model.ImageFeatureItem.EndDate =
                                    model.ImageFeatureEndDate.Value
                                        .CombineWithTime(model.ImageFeatureEndTime.Value);
                            }

                            featureItem = await _imageFeatureService.EditItemAsync(
                                model.ImageFeatureItem);

                            var language = await _languageService
                                .GetActiveByIdAsync(model.LanguageId);

                            response = new JsonResponse
                            {
                                Success = true,
                                Url = Url.Action(nameof(Detail), new
                                {
                                    id = featureItem.ImageFeatureId,
                                    language = language.IsDefault ? null : language.Name,
                                    item = featureItem.Id
                                })
                            };

                            ShowAlertSuccess($"Created item: {featureItem.Name}");
                        }
                        catch (OcudaException ex)
                        {
                            response = new JsonResponse
                            {
                                Success = false,
                                Message = ex.Message
                            };
                        }
                    }
                    else
                    {
                        var errors = ModelState.Values
                            .SelectMany(_ => _.Errors)
                            .Select(_ => _.ErrorMessage);

                        response = new JsonResponse
                        {
                            Success = false,
                            Message = string.Join(Environment.NewLine, errors)
                        };
                    }
                }
                else
                {
                    response = new JsonResponse
                    {
                        Message = "Unauthorized",
                        Success = false
                    };
                }
            }

            return Json(response);
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState(Key = DetailModelStateKey)]
        public async Task<IActionResult> EditImageFeatureItemText(DetailViewModel model)
        {
            var featureItem = await _imageFeatureService
                .GetItemByIdAsync(model.ImageFeatureItemText.ImageFeatureItemId);

            if (!await HasPageContentPermissionAsync(featureItem.ImageFeatureId))
            {
                return RedirectToUnauthorized();
            }

            byte[] imageBytes = null;

            OptimizedImageResult optimized;

            if (model.ItemImage != null)
            {
                var extension = Path.GetExtension(model.ItemImage.FileName);
                if (!ValidImageExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("ItemImage",
                        $"Image type must be one of: {string.Join(", ", ValidImageExtensions)}");
                }
                else
                {
                    try
                    {
                        optimized = await _imageService.OptimizeAsync(model.ItemImage);
                        imageBytes = optimized.File;
                    }
                    catch (ParameterException pex)
                    {
                        TempData[ItemErrorMessageKey] = $"Error optimizing uploaded image: {pex.Message}";
                        ModelState.AddModelError("ItemImage",
                            $"Error optimizing uploaded image: {pex.Message}");
                    }
                    
                    var template = await _imageFeatureService
                        .GetTemplateForImageFeatureAsync(featureItem.ImageFeatureId);

                    var maxFileSize = template?.MaximumFileSizeBytes ?? MaximumFileSizeBytes;

                    if (imageBytes?.Length > maxFileSize)
                    {
                        ModelState.AddModelError("ItemImage", $"Image must be smaller than {maxFileSize / 1024} KB");
                    }

                    if (imageBytes != null && (template?.Height.HasValue == true || template?.Width.HasValue == true))
                    {
                        using var image = Image.Load(imageBytes);
                        if (template.Height.HasValue && image.Height != template.Height)
                        {
                            ModelState.AddModelError("ItemImage",
                                $"Image height needs to be {template.Height}px");
                        }
                        if (template.Width.HasValue && image.Width != template.Width)
                        {
                            ModelState.AddModelError("ItemImage",
                                $"Image width needs to be {template.Width}px");
                        }
                    }
                }
            }
            else
            {
                var currentItemText = await _imageFeatureService.GetItemTextByIdsAsync(
                    featureItem.Id,
                    model.ImageFeatureItemText.LanguageId);

                if (currentItemText == null)
                {
                    ModelState.AddModelError("ItemImage", "A feature image is required");
                }
            }

            var language = await _languageService
                .GetActiveByIdAsync(model.ImageFeatureItemText.LanguageId);

            if (ModelState.IsValid)
            {
                try
                {
                    if (model.ItemImage != null)
                    {
                        var filePath = await _imageFeatureService
                            .GetImageFeaturePathAsync(language.Name);

                        if (!Directory.Exists(filePath))
                        {
                            _logger.LogInformation("Creating image feature directory: {Path}",
                                filePath);
                            Directory.CreateDirectory(filePath);
                        }

                        // check if it's the same filename and if os, allow it
                        var currentItem = await _imageFeatureService.GetItemTextByIdsAsync(
                            model.ImageFeatureItemText.ImageFeatureItemId,
                            model.ImageFeatureItemText.LanguageId);

                        var fullFilePath = Path.Combine(filePath, model.ItemImage.FileName);

                        if (currentItem?.Filename != model.ItemImage.FileName)
                        {
                            int renameCounter = 1;
                            while (System.IO.File.Exists(fullFilePath))
                            {
                                fullFilePath = Path.Combine(filePath, string.Format(
                                    CultureInfo.InvariantCulture,
                                    "{0}-{1}{2}",
                                    Path.GetFileNameWithoutExtension(model.ItemImage.FileName),
                                    renameCounter++,
                                    Path.GetExtension(model.ItemImage.FileName)));
                            }
                        }

                        if (System.IO.File.Exists(fullFilePath))
                        {
                            System.IO.File.Delete(fullFilePath);
                        }

                        await System.IO.File.WriteAllBytesAsync(fullFilePath, imageBytes);

                        model.ImageFeatureItemText.Filename = Path.GetFileName(fullFilePath);
                    }

                    await _imageFeatureService.SetItemTextAsync(model.ImageFeatureItemText);

                    ShowAlertSuccess("Updated item text");
                }
                catch (OcudaException ex)
                {
                    TempData[ItemErrorMessageKey] = $"Unable to update item: {ex.Message}";
                }
            }

            return RedirectToAction(nameof(Detail), new
            {
                id = model.ImageFeatureId,
                language = language.IsDefault ? null : language.Name,
                item = model.ImageFeatureItemText.ImageFeatureItemId
            });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> UpdateTemplate(DetailViewModel viewModel)
        {
            if (viewModel?.ImageFeatureTemplate == null)
            {
                ShowAlertWarning("Could not find template to update.");
            }
            else
            {
                if (viewModel.ImageFeatureTemplate.Id == default)
                {
                    if (viewModel.ImageFeatureTemplate.Height == null
                        && viewModel.ImageFeatureTemplate.ItemsToDisplay == null
                        && viewModel.ImageFeatureTemplate.MaximumFileSizeBytes == null
                        && viewModel.ImageFeatureTemplate.Width == null)
                    {
                        // nothing to update
                        ShowAlertWarning("No template restrictions to add.");
                    }
                    else
                    {
                        try
                        {
                            await _imageFeatureService.AddTemplateAsync(viewModel.ImageFeatureId,
                                viewModel.PageLayoutId,
                                viewModel.ImageFeatureTemplate);
                        }
                        catch (OcudaException oex)
                        {
                            _logger.LogError(oex, "Error updating image feature template: {Message}", oex.Message);
                            ShowAlertDanger($"Error updating record: {oex.Message}");
                        }
                    }
                }
                else
                {
                    if (viewModel.ImageFeatureTemplate.Height == null
                        && viewModel.ImageFeatureTemplate.ItemsToDisplay == null
                        && viewModel.ImageFeatureTemplate.MaximumFileSizeBytes == null
                        && viewModel.ImageFeatureTemplate.Width == null)
                    {
                        await _imageFeatureService
                            .ClearTemplateForImageFeatureAsync(viewModel.ImageFeatureTemplate.Id);
                    }
                    else
                    {
                        await _imageFeatureService
                            .UpdateTemplateAsync(viewModel.ImageFeatureTemplate);
                    }
                }
            }

            return RedirectToAction(nameof(Detail), new { id = viewModel?.ImageFeatureId });
        }

        private async Task<bool> HasPageContentPermissionAsync(int featureId)
        {
            if (!string.IsNullOrEmpty(UserClaim(ClaimType.SiteManager))
                || await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.WebPageContentManagement))
            {
                return true;
            }
            else
            {
                var permissionClaims = UserClaims(ClaimType.PermissionId);
                if (permissionClaims.Count > 0)
                {
                    var pageHeaderId = await _imageFeatureService.GetPageHeaderIdForImageFeatureAsync(
                        featureId);
                    if (!pageHeaderId.HasValue)
                    {
                        return false;
                    }
                    var permissionGroups = await _permissionGroupService
                        .GetPermissionsAsync<PermissionGroupPageContent>(pageHeaderId.Value);
                    var permissionGroupsStrings = permissionGroups
                        .Select(_ => _.PermissionGroupId.ToString(CultureInfo.InvariantCulture));

                    return permissionClaims.Any(_ => permissionGroupsStrings.Contains(_));
                }
                return false;
            }
        }
    }
}