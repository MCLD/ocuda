using System;
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
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement
{
    [Area("SiteManagement")]
    [Authorize(Policy = nameof(ClaimType.SiteManager))]
    [Route("[area]/[controller]")]
    public class CarouselsController : BaseController<CarouselsController>
    {
        private const string DetailModelStateKey = "Carousels.Detail";
        private const string ItemErrorMessageKey = "Carousels.ItemErrorMessage";

        private readonly ICarouselService _carouselService;
        private readonly ILanguageService _languageService;

        public static string Area { get { return "SiteManagement"; } }
        public static string Name { get { return "Carousels"; } }

        public CarouselsController(ServiceFacades.Controller<CarouselsController> context,
            ICarouselService carouselService,
            ILanguageService languageService)
            : base(context)
        {
            _carouselService = carouselService
                ?? throw new ArgumentNullException(nameof(carouselService));
            _languageService = languageService
                ?? throw new ArgumentNullException(nameof(languageService));
        }

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

            var viewModel = new IndexViewModel
            {
                Carousels = carouselList.Data,
                PaginateModel = paginateModel
            };

            return View(viewModel);
        }

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
                    ShowAlertSuccess($"Created carousel: {carousel.Name}");
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

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Edit(IndexViewModel model)
        {
            JsonResponse response;

            if (ModelState.IsValid)
            {
                try
                {
                    var carousel = await _carouselService.EditAsync(model.Carousel);
                    response = new JsonResponse
                    {
                        Success = true
                    };
                    ShowAlertSuccess($"Updated carousel: {carousel.Name}");
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
            var languages = await _languageService.GetActiveAsync();

            var selectedLanguage = languages
                .FirstOrDefault(_ => _.Name.Equals(language, StringComparison.OrdinalIgnoreCase))
                ?? languages.Single(_ => _.IsDefault);

            var carousel = await _carouselService.GetCarouselDetailsAsync(id, selectedLanguage.Id);

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
                    nameof(CarouselButtonLabel.Id), nameof(Carousel.Name)),
                AllowedImageDomains = (await _siteSettingService.GetSettingStringAsync(
                    Models.Keys.SiteSetting.Carousel.ImageRestrictToDomains))
                    .Replace(",", ", "),
                AllowedLinkDomains = (await _siteSettingService.GetSettingStringAsync(
                    Models.Keys.SiteSetting.Carousel.LinkRestrictToDomains))
                    .Replace(",", ", "),
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState(Key = DetailModelStateKey)]
        public async Task<IActionResult> EditCarouselText(DetailViewModel model)
        {
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

            if (ModelState.IsValid)
            {
                try
                {
                    var carouselItem = await _carouselService.CreateItemAsync(model.CarouselItem);

                    var language = await _languageService.GetActiveByIdAsync(model.LanguageId);

                    response = new JsonResponse
                    {
                        Success = true,
                        Url = Url.Action(nameof(Detail), new
                        {
                            id = model.CarouselItem.CarouselId,
                            language = language.IsDefault ? null : language.Name,
                            item = carouselItem.Id
                        })
                    };

                    ShowAlertSuccess($"Created item: {carouselItem.Name}");
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

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> EditCarouselItem(DetailViewModel model)
        {
            JsonResponse response;

            if (ModelState.IsValid)
            {
                try
                {
                    var carouselItem = await _carouselService.EditItemAsync(
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

                    ShowAlertSuccess($"Updated item: {carouselItem.Name}");
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

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> DeleteCarouselItem(DetailViewModel model)
        {
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
            var success = false;
            var message = string.Empty;

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
                success = true;
            }
            catch (OcudaException ex)
            {
                message = ex.Message;
            }

            return Json(new { success, message });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> EditCarouselItemText(DetailViewModel model)
        {
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

            return Json(response);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> EditItemButton(DetailViewModel model)
        {
            JsonResponse response;

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

            return Json(response);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> DeleteItemButton(DetailViewModel model)
        {
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
    }
}
