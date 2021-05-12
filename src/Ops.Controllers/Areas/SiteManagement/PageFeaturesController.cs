﻿using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.PageFeatures;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;
using SixLabors.ImageSharp;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement
{
    [Area("SiteManagement")]
    [Route("[area]/[controller]")]
    public class PageFeaturesController : BaseController<PageFeaturesController>
    {
        private const string DetailModelStateKey = "PageFeatures.Detail";
        private const string ImagesFilePath = "images";
        private const string ItemErrorMessageKey = "PageFeatures.ItemErrorMessage";
        private const string PageFeaturesFilePath = "features";

        private static readonly string[] ValidImageExtensions = new[] { ".jpg", ".png" };

        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILanguageService _languageService;
        private readonly IPageFeatureService _pageFeatureService;
        private readonly IPermissionGroupService _permissionGroupService;

        public PageFeaturesController(ServiceFacades.Controller<PageFeaturesController> context,
            IDateTimeProvider dateTimeProvider,
            ILanguageService languageService,
            IPageFeatureService pageFeatureService,
            IPermissionGroupService permissionGroupService)
            : base(context)
        {
            _dateTimeProvider = dateTimeProvider
                ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _languageService = languageService
                ?? throw new ArgumentNullException(nameof(languageService));
            _pageFeatureService = pageFeatureService
                ?? throw new ArgumentNullException(nameof(pageFeatureService));
            _permissionGroupService = permissionGroupService
                ?? throw new ArgumentNullException(nameof(permissionGroupService));
        }

        public static string Area { get { return "SiteManagement"; } }
        public static string Name { get { return "PageFeatures"; } }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> AddPageFeatureItem(DetailViewModel model)
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
                if (await HasPageFeaturePermissionAsync(model.PageFeatureItem.PageFeatureId))
                {
                    if (ModelState.IsValid)
                    {
                        try
                        {
                            var featureItem = await _pageFeatureService.CreateItemAsync(
                                model.PageFeatureItem);

                            var language = await _languageService.GetActiveByIdAsync(model.LanguageId);

                            response = new JsonResponse
                            {
                                Success = true,
                                Url = Url.Action(nameof(Detail), new
                                {
                                    id = featureItem.PageFeatureId,
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

            var featureId = (await _pageFeatureService.GetItemByIdAsync(id)).PageFeatureId;

            if (await HasPageFeaturePermissionAsync(featureId))
            {
                try
                {
                    await _pageFeatureService.UpdateItemSortOrder(id, increase);

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
        public async Task<IActionResult> DeletePageFeatureItem(DetailViewModel model)
        {
            var featureItem = await _pageFeatureService.GetItemByIdAsync(model.PageFeatureItem.Id);
            var featureItemText = await _pageFeatureService
                .GetItemTextByIdsAsync(model.PageFeatureItem.Id, model.LanguageId);

            if (!await HasPageFeaturePermissionAsync(featureItem.PageFeatureId))
            {
                return RedirectToUnauthorized();
            }

            var language = await _languageService.GetActiveByIdAsync(model.LanguageId);

            try
            {
                var filePath = await GetPageFeaturePathAsync(language.Name);

                await _pageFeatureService.DeleteItemAsync(model.PageFeatureItem.Id);

                System.IO.File.Delete(Path.Combine(filePath, featureItemText.Filename));

                ShowAlertSuccess($"Deleted item: {model.PageFeatureItem.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting feature item: {Message}", ex.Message);
                ShowAlertDanger($"Error deleting item: {model.PageFeatureItem.Name}");
            }


            return RedirectToAction(nameof(Detail), new
            {
                id = model.PageFeatureId,
                language = language.IsDefault ? null : language.Name
            });
        }

        [Route("[action]/{id}")]
        [RestoreModelState(Key = DetailModelStateKey)]
        public async Task<IActionResult> Detail(int id, string language, int? item)
        {
            if (!await HasPageFeaturePermissionAsync(id))
            {
                return RedirectToUnauthorized();
            }

            var languages = await _languageService.GetActiveAsync();

            var selectedLanguage = languages
                .FirstOrDefault(_ => _.Name.Equals(language, StringComparison.OrdinalIgnoreCase))
                ?? languages.Single(_ => _.IsDefault);

            var feature = await _pageFeatureService.GetPageFeatureDetailsAsync(id,
                selectedLanguage.Id);

            var viewModel = new DetailViewModel
            {
                PageFeature = feature,
                PageFeatureId = feature.Id,
                FocusItemId = item,
                ItemErrorMessage = TempData[ItemErrorMessageKey] as string,
                LanguageId = selectedLanguage.Id,
                LanguageList = new SelectList(languages, nameof(Language.Name),
                    nameof(Language.Description), selectedLanguage.Name),
                PageLayoutId = await _pageFeatureService
                    .GetPageLayoutIdForPageFeatureAsync(feature.Id),
                CurrentDateTime = _dateTimeProvider.Now
            };

            viewModel.PageFeatureTemplate = await _pageFeatureService
                .GetTemplateForPageFeatureAsync(feature.Id);

            return View(viewModel);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> EditPageFeatureItem(DetailViewModel model)
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
                var featureItem = await _pageFeatureService.GetItemByIdAsync(model.PageFeatureItem.Id);

                if (await HasPageFeaturePermissionAsync(featureItem.PageFeatureId))
                {
                    if (ModelState.IsValid)
                    {
                        try
                        {
                            featureItem = await _pageFeatureService.EditItemAsync(
                                model.PageFeatureItem);

                            var language = await _languageService.GetActiveByIdAsync(model.LanguageId);

                            response = new JsonResponse
                            {
                                Success = true,
                                Url = Url.Action(nameof(Detail), new
                                {
                                    id = featureItem.PageFeatureId,
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
        public async Task<IActionResult> EditPageFeatureItemText(DetailViewModel model)
        {
            var featureItem = await _pageFeatureService
                .GetItemByIdAsync(model.PageFeatureItemText.PageFeatureItemId);

            if (!await HasPageFeaturePermissionAsync(featureItem.PageFeatureId))
            {
                return RedirectToUnauthorized();
            }

            byte[] imageBytes = null;

            if (model.ItemImage != null)
            {
                var extension = Path.GetExtension(model.ItemImage.FileName);
                if (!ValidImageExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("ItemImage",
                        $"Image type must be one of: {ValidImageExtensions}");
                }
                else
                {
                    using var ms = new MemoryStream();
                    await model.ItemImage.CopyToAsync(ms);
                    imageBytes = ms.ToArray();

                    var template = await _pageFeatureService
                        .GetTemplateForPageFeatureAsync(featureItem.PageFeatureId);

                    if (template?.Height.HasValue == true || template?.Width.HasValue == true)
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
                var currentItemText = await _pageFeatureService.GetItemTextByIdsAsync(
                    featureItem.Id,
                    model.PageFeatureItemText.LanguageId);

                if (currentItemText == null)
                {
                    ModelState.AddModelError("ItemImage", "A feature image is required");
                }
            }

            var language = await _languageService
                .GetActiveByIdAsync(model.PageFeatureItemText.LanguageId);

            if (ModelState.IsValid)
            {
                try
                {
                    var itemText = await _pageFeatureService.SetItemTextAsync(
                        model.PageFeatureItemText);

                    if (model.ItemImage != null)
                    {
                        var filePath = await GetPageFeaturePathAsync(language.Name);

                        if (!Directory.Exists(filePath))
                        {
                            _logger.LogInformation("Creating page feature directory: {Path}",
                                filePath);
                            Directory.CreateDirectory(filePath);
                        }

                        var fullFilePath = Path.Combine(filePath, model.ItemImage.FileName);

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

                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }

                        await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

                        if (string.IsNullOrWhiteSpace(itemText.Filename))
                        {
                            itemText.Filename = Path.GetFileName(fullFilePath);

                            await _pageFeatureService.SetItemTextAsync(itemText);
                        }
                    }

                    ShowAlertSuccess("Updated item text");
                }
                catch (OcudaException ex)
                {
                    TempData[ItemErrorMessageKey] = $"Unable to update item: {ex.Message}";
                }
            }

            return RedirectToAction(nameof(Detail), new
            {
                id = model.PageFeatureId,
                language = language.IsDefault ? null : language.Name,
                item = model.PageFeatureItemText.PageFeatureItemId
            });
        }

        private async Task<string> GetPageFeaturePathAsync(string languageName)
        {
            string basePath = await _siteSettingService.GetSettingStringAsync(
                Models.Keys.SiteSetting.SiteManagement.PromenadePublicPath);

            return Path.Combine(basePath,
                ImagesFilePath,
                languageName,
                PageFeaturesFilePath);
        }

        private async Task<bool> HasPageFeaturePermissionAsync(int featureId)
        {
            if (!string.IsNullOrEmpty(UserClaim(ClaimType.SiteManager)))
            {
                return true;
            }
            else
            {
                var permissionClaims = UserClaims(ClaimType.PermissionId);
                if (permissionClaims.Count > 0)
                {
                    var pageHeaderId = await _pageFeatureService.GetPageHeaderIdForPageFeatureAsync(
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