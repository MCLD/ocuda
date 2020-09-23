using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Carousels;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement
{
    [Area("SiteManagement")]
    [Route("[area]/[controller]")]
    public class CarouselsController : BaseController<CarouselsController>
    {
        private const string DetailModelStateKey = "Carousels.Detail";
        private const string ItemErrorMessageKey = "Carousels.ItemErrorMessage";

        private readonly ICarouselService _carouselService;
        private readonly ILanguageService _languageService;
        private readonly IPermissionGroupService _permissionGroupService;

        public static string Area { get { return "SiteManagement"; } }
        public static string Name { get { return "Carousels"; } }

        public CarouselsController(ServiceFacades.Controller<CarouselsController> context,
            ICarouselService carouselService,
            ILanguageService languageService,
            IPermissionGroupService permissionGroupService)
            : base(context)
        {
            _carouselService = carouselService
                ?? throw new ArgumentNullException(nameof(carouselService));
            _languageService = languageService
                ?? throw new ArgumentNullException(nameof(languageService));
            _permissionGroupService = permissionGroupService
                ?? throw new ArgumentNullException(nameof(permissionGroupService));
        }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [Route("")]
        [Route("[action]/{page}")]
        public async Task<IActionResult> Index(int page = 1)
        {
            var filter = new BaseFilter(page);

            var carouselList = await _carouselService.GetPaginatedListAsync(filter);

            var paginateModel = new PaginateModel
            {
                ItemCount = carouselList.Count,
                CurrentPage = page,
                ItemsPerPage = filter.Take.Value
            };
            if (paginateModel.PastMaxPage)
            {
                return RedirectToRoute(
                    new
                    {
                        page = paginateModel.LastPage ?? 1
                    });
            }

            foreach (var carousel in carouselList.Data)
            {
                carousel.Name = await _carouselService.GetDefaultNameForCarouselAsync(
                    carousel.Id);
            }

            var viewModel = new IndexViewModel
            {
                Carousels = carouselList.Data,
                PaginateModel = paginateModel
            };

            return View(viewModel);
        }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Create(IndexViewModel model)
        {
            JsonResponse response;

            if (ModelState.IsValid)
            {
                try
                {
                    var carousel = await _carouselService.CreateAsync(model.Carousel);
                    response = new JsonResponse
                    {
                        Success = true,
                        Url = Url.Action(nameof(Detail), new { id = carousel.Id })
                    };
                    ShowAlertSuccess($"Created carousel: {carousel.CarouselText.Title}");
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

            return Json(response);
        }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Delete(IndexViewModel model)
        {
            try
            {
                await _carouselService.DeleteAsync(model.Carousel.Id);
                ShowAlertSuccess($"Deleted carousel: {model.Carousel.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting carousel: {Message}", ex.Message);
                ShowAlertDanger($"Error deleting carousel: {model.Carousel.Name}");
            }

            return RedirectToAction(nameof(Index), new { page = model.PaginateModel.CurrentPage });
        }

        [Route("[action]/{id}")]
        [RestoreModelState(Key = DetailModelStateKey)]
        public async Task<IActionResult> Detail(int id, string language, int? item)
        {
            if (!await HasCaroseulPermissionAsync(id))
            {
                return RedirectToUnauthorized();
            }

            var languages = await _languageService.GetActiveAsync();

            var selectedLanguage = languages
                .FirstOrDefault(_ => _.Name.Equals(language, StringComparison.OrdinalIgnoreCase))
                ?? languages.Single(_ => _.IsDefault);

            var carousel = await _carouselService.GetCarouselDetailsAsync(id, selectedLanguage.Id);

            foreach (var carouselItem in carousel.Items)
            {
                carouselItem.Name = await _carouselService.GetDefaultNameForItemAsync(
                    carouselItem.Id);
            }

            var viewModel = new DetailViewModel
            {
                Carousel = carousel,
                CarouselId = carousel.Id,
                FocusItemId = item,
                ItemErrorMessage = TempData[ItemErrorMessageKey] as string,
                LanguageId = selectedLanguage.Id,
                LanguageList = new SelectList(languages, nameof(Language.Name),
                    nameof(Language.Description), selectedLanguage.Name),
                LabelList = new SelectList(await _carouselService.GetButtonLabelsAsync(),
                    nameof(CarouselButtonLabel.Id), nameof(CarouselButtonLabel.Name)),
                AllowedImageDomains = (await _siteSettingService.GetSettingStringAsync(
                    Models.Keys.SiteSetting.Carousel.ImageRestrictToDomains))
                    .Replace(",", ", "),
                PageLayoutId = await _carouselService.GetPageLayoutIdForCarouselAsync(carousel.Id)
            };

            if (viewModel.PageLayoutId.HasValue)
            {
                viewModel.CarouselTemplate = await _carouselService
                    .GetTemplateForPageLayoutAsync(viewModel.PageLayoutId.Value);

                if (!string.IsNullOrWhiteSpace(viewModel.CarouselTemplate?.ButtonUrlTemplate))
                {
                    viewModel.ButtonUrlInfoMessage = viewModel.CarouselTemplate.ButtonUrlInfo;
                }
            }

            if (string.IsNullOrWhiteSpace(viewModel.CarouselTemplate?.ButtonUrlTemplate))
            {
                var AllowedLinkDomains = (await _siteSettingService.GetSettingStringAsync(
                    Models.Keys.SiteSetting.Carousel.LinkRestrictToDomains))
                    .Replace(",", ", ");

                if (!string.IsNullOrWhiteSpace(AllowedLinkDomains))
                {
                    viewModel.ButtonUrlInfoMessage
                        = $"Urls are restricted to the following domains: {AllowedLinkDomains}";
                }
                    
            }

            return View(viewModel);
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState(Key = DetailModelStateKey)]
        public async Task<IActionResult> EditCarouselText(DetailViewModel model)
        {
            if (!await HasCaroseulPermissionAsync(model.CarouselText.CarouselId))
            {
                return RedirectToUnauthorized();
            }

            if (ModelState.IsValid)
            {
                await _carouselService.SetCarouselTextAsync(model.CarouselText);
                ShowAlertSuccess("Updated carousel text.");
            }

            var language = await _languageService.GetActiveByIdAsync(model.CarouselText.LanguageId);

            return RedirectToAction(nameof(Detail), new
            {
                id = model.CarouselText.CarouselId,
                language = language.IsDefault ? null : language.Name
            });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> AddCarouselItem(DetailViewModel model)
        {
            JsonResponse response;

            if (await HasCaroseulPermissionAsync(model.CarouselItem.CarouselId))
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        var carouselItem = await _carouselService.CreateItemAsync(
                            model.CarouselItem);

                        var language = await _languageService.GetActiveByIdAsync(model.LanguageId);

                        response = new JsonResponse
                        {
                            Success = true,
                            Url = Url.Action(nameof(Detail), new
                            {
                                id = carouselItem.CarouselId,
                                language = language.IsDefault ? null : language.Name,
                                item = carouselItem.Id
                            })
                        };

                        ShowAlertSuccess($"Created item: {carouselItem.CarouselItemText.Label}");
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
        public async Task<IActionResult> DeleteCarouselItem(DetailViewModel model)
        {
            var carouselItem = await _carouselService.GetItemByIdAsync(model.CarouselItem.Id);

            if (!await HasCaroseulPermissionAsync(carouselItem.CarouselId))
            {
                return RedirectToUnauthorized();
            }

            try
            {
                await _carouselService.DeleteItemAsync(model.CarouselItem.Id);
                ShowAlertSuccess($"Deleted item: {model.CarouselItem.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting carousel item: {Message}", ex.Message);
                ShowAlertDanger($"Error deleting item: {model.CarouselItem.Name}");
            }

            var language = await _languageService.GetActiveByIdAsync(model.LanguageId);

            return RedirectToAction(nameof(Detail), new
            {
                id = model.CarouselId,
                language = language.IsDefault ? null : language.Name
            });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<JsonResult> ChangeSort(int id, bool increase, bool item)
        {
            JsonResponse response;

            int carouselId;
            if (item)
            {
                carouselId = (await _carouselService.GetItemByIdAsync(id)).CarouselId;
            }
            else
            {
                carouselId = await _carouselService.GetCarouselIdForButtonAsync(id);
            }

            if (await HasCaroseulPermissionAsync(carouselId))
            {
                try
                {
                    if (item)
                    {
                        await _carouselService.UpdateItemSortOrder(id, increase);
                    }
                    else
                    {
                        await _carouselService.UpdateButtonSortOrder(id, increase);
                    }
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
        public async Task<IActionResult> EditCarouselItemText(DetailViewModel model)
        {
            var carouselItem = await _carouselService
                .GetItemByIdAsync(model.CarouselItemText.CarouselItemId);

            if (!await HasCaroseulPermissionAsync(carouselItem.CarouselId))
            {
                return RedirectToUnauthorized();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _carouselService.SetItemTextAsync(model.CarouselItemText);
                    ShowAlertSuccess("Updated item text");
                }
                catch (OcudaException ex)
                {
                    TempData[ItemErrorMessageKey] = $"Unable to update item: {ex.Message}";
                }
            }

            var language = await _languageService.GetActiveByIdAsync(
                model.CarouselItemText.LanguageId);

            return RedirectToAction(nameof(Detail), new
            {
                id = model.CarouselId,
                language = language.IsDefault ? null : language.Name,
                item = model.CarouselItemText.CarouselItemId
            });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> AddItemButton(DetailViewModel model)
        {
            JsonResponse response;

            var carouselItem = await _carouselService
                .GetItemByIdAsync(model.CarouselButton.CarouselItemId);

            if (await HasCaroseulPermissionAsync(carouselItem.CarouselId))
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        var carouselButton = await _carouselService.CreateButtonAsync(
                            model.CarouselButton);

                        var language = await _languageService.GetActiveByIdAsync(model.LanguageId);

                        response = new JsonResponse
                        {
                            Success = true,
                            Url = Url.Action(nameof(Detail), new
                            {
                                id = model.CarouselId,
                                language = language.IsDefault ? null : language.Name,
                                item = carouselButton.CarouselItemId
                            })
                        };

                        ShowAlertSuccess("Added button");
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
        public async Task<IActionResult> EditItemButton(DetailViewModel model)
        {
            JsonResponse response;

            var carouselId = await _carouselService
                .GetCarouselIdForButtonAsync(model.CarouselButton.Id);

            if (await HasCaroseulPermissionAsync(carouselId))
            {

                if (ModelState.IsValid)
                {
                    try
                    {
                        var carouselButton = await _carouselService.EditButtonAsync(
                            model.CarouselButton);

                        var language = await _languageService.GetActiveByIdAsync(model.LanguageId);

                        response = new JsonResponse
                        {
                            Success = true,
                            Url = Url.Action(nameof(Detail), new
                            {
                                id = model.CarouselId,
                                language = language.IsDefault ? null : language.Name,
                                item = carouselButton.CarouselItemId
                            })
                        };

                        ShowAlertSuccess("Updated button");
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
        public async Task<IActionResult> DeleteItemButton(DetailViewModel model)
        {
            var carouselId = await _carouselService
                .GetCarouselIdForButtonAsync(model.CarouselButton.Id);

            if (!await HasCaroseulPermissionAsync(carouselId))
            {
                return RedirectToUnauthorized();
            }

            try
            {
                await _carouselService.DeleteButtonAsync(model.CarouselButton.Id);

                ShowAlertSuccess("Deleted button");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting carousel button: {Message}", ex.Message);
                ShowAlertDanger("Error deleting button.");
            }

            var language = await _languageService.GetActiveByIdAsync(model.LanguageId);

            return RedirectToAction(nameof(Detail), new
            {
                id = model.CarouselId,
                language = language.IsDefault ? null : language.Name,
                item = model.CarouselButton.CarouselItemId
            });
        }

        private async Task<bool> HasCaroseulPermissionAsync(int carouselId)
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
                    var pageHeaderId = await _carouselService.GetPageHeaderIdForCarouselAsync(
                        carouselId);
                    if (!pageHeaderId.HasValue)
                    {
                        return false;
                    }
                    var permissionGroups = await _permissionGroupService
                        .GetPagePermissionsAsync(pageHeaderId.Value);
                    var permissionGroupsStrings = permissionGroups
                        .Select(_ => _.PermissionGroupId.ToString(CultureInfo.InvariantCulture));

                    return permissionClaims.Any(_ => permissionGroupsStrings.Contains(_));
                }
                return false;
            }
        }
    }
}
