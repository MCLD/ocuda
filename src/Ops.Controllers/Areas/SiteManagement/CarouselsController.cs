﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Carousels;
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
                ShowAlertDanger("Unable to delete carousel: ", ex.Message);
            }

            return RedirectToAction(nameof(Index), new { page = model.PaginateModel.CurrentPage });
        }

        [Route("[action]/{id}")]
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
                FocusItemId = item,
                LanguageName = language,
                LanguageId = selectedLanguage.Id,
                LanguageList = new SelectList(languages, nameof(Language.Name),
                    nameof(Language.Description), selectedLanguage.Name),
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> EditCarouselText(DetailViewModel model)
        {
            return null;
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

                    response = new JsonResponse
                    {
                        Success = true,
                        Url = Url.Action(nameof(Detail), new { id = model.CarouselItem.CarouselId, item = carouselItem.Id })
                    };

                    ShowAlertSuccess($"Created carousel item: {carouselItem.Name}");
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
        public async Task<IActionResult> EditCarouselItemText(DetailViewModel model)
        {
            return null;
        }
    }
}
