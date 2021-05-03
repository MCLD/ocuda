using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Webslides;
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
    public class WebslidesController : BaseController<WebslidesController>
    {
        private const string DetailModelStateKey = "Webslides.Detail";
        private const string ItemErrorMessageKey = "Webslides.ItemErrorMessage";

        private const string ImagesFilePath = "images";
        private const string WebslidesFilePath = "Webslides";

        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ILanguageService _languageService;
        private readonly IPermissionGroupService _permissionGroupService;
        private readonly IWebslideService _webslideService;

        public static string Area { get { return "SiteManagement"; } }
        public static string Name { get { return "Webslides"; } }

        public WebslidesController(ServiceFacades.Controller<WebslidesController> context,
            IDateTimeProvider dateTimeProvider,
            ILanguageService languageService,
            IPermissionGroupService permissionGroupService,
            IWebslideService webslideService)
            : base(context)
        {
            _dateTimeProvider = dateTimeProvider
                ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _languageService = languageService
                ?? throw new ArgumentNullException(nameof(languageService));
            _permissionGroupService = permissionGroupService
                ?? throw new ArgumentNullException(nameof(permissionGroupService));
            _webslideService = webslideService
                ?? throw new ArgumentNullException(nameof(webslideService));
        }

        [Route("[action]/{id}")]
        [RestoreModelState(Key = DetailModelStateKey)]
        public async Task<IActionResult> Detail(int id, string language, int? item)
        {
            if (!await HasWebslidePermissionAsync(id))
            {
                return RedirectToUnauthorized();
            }

            var languages = await _languageService.GetActiveAsync();

            var selectedLanguage = languages
                .FirstOrDefault(_ => _.Name.Equals(language, StringComparison.OrdinalIgnoreCase))
                ?? languages.Single(_ => _.IsDefault);

            var webslide = await _webslideService.GetWebslideDetailsAsync(id, selectedLanguage.Id);

            var viewModel = new DetailViewModel
            {
                Webslide = webslide,
                WebslideId = webslide.Id,
                FocusItemId = item,
                ItemErrorMessage = TempData[ItemErrorMessageKey] as string,
                LanguageId = selectedLanguage.Id,
                LanguageList = new SelectList(languages, nameof(Language.Name),
                    nameof(Language.Description), selectedLanguage.Name),
                PageLayoutId = await _webslideService.GetPageLayoutIdForWebslideAsync(webslide.Id),
                CurrentDateTime = _dateTimeProvider.Now
            };

            viewModel.WebslideTemplate = await _webslideService
                .GetTemplateForPageLayoutAsync(viewModel.PageLayoutId);

            return View(viewModel);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> AddWebslideItem(DetailViewModel model)
        {
            JsonResponse response;

            if (await HasWebslidePermissionAsync(model.WebslideItem.WebslideId))
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        var webslideItem = await _webslideService.CreateItemAsync(
                            model.WebslideItem);

                        var language = await _languageService.GetActiveByIdAsync(model.LanguageId);

                        response = new JsonResponse
                        {
                            Success = true,
                            Url = Url.Action(nameof(Detail), new
                            {
                                id = webslideItem.WebslideId,
                                language = language.IsDefault ? null : language.Name,
                                item = webslideItem.Id
                            })
                        };

                        ShowAlertSuccess($"Created item: {webslideItem.Name}");
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

            return Json(response);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> EditWebslideItem(DetailViewModel model)
        {
            JsonResponse response;

            var webslideItem = await _webslideService.GetItemByIdAsync(model.WebslideItem.Id);

            if (await HasWebslidePermissionAsync(webslideItem.WebslideId))
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        webslideItem = await _webslideService.EditItemAsync(
                            model.WebslideItem);

                        var language = await _languageService.GetActiveByIdAsync(model.LanguageId);

                        response = new JsonResponse
                        {
                            Success = true,
                            Url = Url.Action(nameof(Detail), new
                            {
                                id = webslideItem.WebslideId,
                                language = language.IsDefault ? null : language.Name,
                                item = webslideItem.Id
                            })
                        };

                        ShowAlertSuccess($"Created item: {webslideItem.Name}");
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

            return Json(response);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> DeleteWebslideItem(DetailViewModel model)
        {
            var webslideItem = await _webslideService.GetItemByIdAsync(model.WebslideItem.Id);

            if (!await HasWebslidePermissionAsync(webslideItem.WebslideId))
            {
                return RedirectToUnauthorized();
            }

            try
            {
                await _webslideService.DeleteItemAsync(model.WebslideItem.Id);
                ShowAlertSuccess($"Deleted item: {model.WebslideItem.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting webslide item: {Message}", ex.Message);
                ShowAlertDanger($"Error deleting item: {model.WebslideItem.Name}");
            }

            var language = await _languageService.GetActiveByIdAsync(model.LanguageId);

            return RedirectToAction(nameof(Detail), new
            {
                id = model.WebslideId,
                language = language.IsDefault ? null : language.Name
            });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<JsonResult> ChangeSort(int id, bool increase)
        {
            JsonResponse response;

            var webslideId = (await _webslideService.GetItemByIdAsync(id)).WebslideId;

            if (await HasWebslidePermissionAsync(webslideId))
            {
                try
                {
                    await _webslideService.UpdateItemSortOrder(id, increase);

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
        [SaveModelState(Key = DetailModelStateKey)]
        public async Task<IActionResult> EditWebslideItemText(DetailViewModel model)
        {
            var webslideItem = await _webslideService
                .GetItemByIdAsync(model.WebslideItemText.WebslideItemId);

            if (!await HasWebslidePermissionAsync(webslideItem.WebslideId))
            {
                return RedirectToUnauthorized();
            }

            byte[] imageBytes = null;

            if (model.ItemImage != null)
            {
                var extension = Path.GetExtension(model.ItemImage.FileName);
                if (extension != ".jpg" && extension != ".png")
                {
                    ModelState.AddModelError("ItemImage", "Image must be a .jpg or .png file");
                }
                else
                {
                    using (var ms = new MemoryStream())
                    {
                        await model.ItemImage.CopyToAsync(ms);
                        imageBytes = ms.ToArray();
                    }

                    var template = await _webslideService
                        .GetTemplateForWebslideAsync(webslideItem.WebslideId);

                    if (template?.Height.HasValue == true || template?.Width.HasValue == true)
                    {
                        using (var image = Image.Load(imageBytes))
                        {
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
            }
            else
            {
                var currentItemText = await _webslideService.GetItemTextByIdsAsync(webslideItem.Id,
                    model.WebslideItemText.LanguageId);

                if (currentItemText == null)
                {
                    ModelState.AddModelError("ItemImage", "A webslide image is required");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var itemText = await _webslideService.SetItemTextAsync(model.WebslideItemText);

                    if (model.ItemImage != null)
                    {
                        string basePath = await _siteSettingService.GetSettingStringAsync(
                            Models.Keys.SiteSetting.SiteManagement.PromenadePublicPath);
                        var filePath = Path.Combine(basePath, ImagesFilePath, WebslidesFilePath);

                        if (!Directory.Exists(filePath))
                        {
                            _logger.LogInformation("Creating webslide directory: {Path}", filePath);
                            Directory.CreateDirectory(filePath);
                        }

                        var filename = string.Format(CultureInfo.InvariantCulture,
                            "{0}-{1}{2}",
                            itemText.WebslideItemId,
                            itemText.LanguageId,
                            Path.GetExtension(model.ItemImage.FileName));

                        filePath = Path.Combine(filePath, filename);

                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }

                        await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

                        if (string.IsNullOrWhiteSpace(itemText.Filename))
                        {
                            itemText.Filename = filename;

                            await _webslideService.SetItemTextAsync(itemText);
                        }
                    }

                    ShowAlertSuccess("Updated item text");
                }
                catch (OcudaException ex)
                {
                    TempData[ItemErrorMessageKey] = $"Unable to update item: {ex.Message}";
                }
            }

            var language = await _languageService.GetActiveByIdAsync(
                model.WebslideItemText.LanguageId);

            return RedirectToAction(nameof(Detail), new
            {
                id = model.WebslideId,
                language = language.IsDefault ? null : language.Name,
                item = model.WebslideItemText.WebslideItemId
            });
        }

        private async Task<bool> HasWebslidePermissionAsync(int webslideId)
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
                    var pageHeaderId = await _webslideService.GetPageHeaderIdForWebslideAsync(
                        webslideId);
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
