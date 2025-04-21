using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.SocialCards;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement
{
    [Area("SiteManagement")]
    [Authorize(Policy = nameof(ClaimType.SiteManager))]
    [Route("[area]/[controller]")]
    public class SocialCardsController : BaseController<SocialCardsController>
    {
        private readonly ILocationService _locationService;
        private readonly ISocialCardService _socialCardService;

        public SocialCardsController(ServiceFacades.Controller<SocialCardsController> context,
            ILocationService locationService,
            ISocialCardService socialCardService) : base(context)
        {
            _locationService = locationService
                ?? throw new ArgumentNullException(nameof(locationService));
            _socialCardService = socialCardService
                ?? throw new ArgumentNullException(nameof(socialCardService));
        }

        public static string Area { get { return "SiteManagement"; } }
        public static string Name { get { return "SocialCards"; } }

        [Route("[action]")]
        [RestoreModelState]
        public IActionResult Create()
        {
            var viewModel = new DetailViewModel
            {
                Action = nameof(Create)
            };

            return View("Detail", viewModel);
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState]
        public async Task<IActionResult> Create(DetailViewModel model)
        {
            if (model != null && ModelState.IsValid)
            {
                try
                {
                    var card = await _socialCardService.CreateAsync(model.SocialCard);
                    if (!string.IsNullOrEmpty(model.LocationStub))
                    {
                        var location
                            = await _locationService.GetLocationByStubAsync(model.LocationStub);
                        if (location != null)
                        {
                            location.SocialCardId = card.Id;
                            await _locationService.EditAsync(location);
                            ShowAlertSuccess($"Added social card: {card.Title} and linked to location: {model.LocationStub}");
                            return RedirectToAction(nameof(LocationsController.Location),
                                LocationsController.Name,
                                new { locationStub = model.LocationStub });
                        }
                        else
                        {
                            ShowAlertWarning($"Added social card: {card.Title}, unable to link it to location: {model.LocationStub}");
                            return RedirectToAction(nameof(Edit), new { id = card.Id });
                        }
                    }
                    else
                    {
                        ShowAlertSuccess($"Added social card: {card.Title}");
                        return RedirectToAction(nameof(Edit), new { id = card.Id });
                    }
                }
                catch (OcudaException ex)
                {
                    _logger.LogError(ex, "Error adding social card: {Message}", ex.Message);
                    ShowAlertDanger("Unable to add social card: ", ex.Message);
                }
            }

            return string.IsNullOrEmpty(model?.LocationStub)
                ? RedirectToAction(nameof(Create))
                : RedirectToAction(nameof(EditForLocation),
                    new { locationStub = model.LocationStub });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Delete(IndexViewModel model)
        {
            try
            {
                await _socialCardService.DeleteAsync(model.SocialCard.Id);
                ShowAlertSuccess($"Deleted social card: {model.SocialCard.Title}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting social card: {Message}", ex.Message);
                ShowAlertDanger("Unable to delete social card: ", ex.Message);
            }

            return RedirectToAction(nameof(Index), new { page = model.PaginateModel.CurrentPage });
        }

        [Route("[action]/{id}")]
        [RestoreModelState]
        public async Task<IActionResult> Edit(int id)
        {
            var card = await _socialCardService.GetByIdAsync(id);

            var viewModel = new DetailViewModel
            {
                Action = nameof(Edit),
                SocialCard = card
            };

            return View("Detail", viewModel);
        }

        [HttpPost]
        [Route("[action]/{id?}")]
        [SaveModelState]
        public async Task<IActionResult> Edit(DetailViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var card = await _socialCardService.EditAsync(model.SocialCard);
                    ShowAlertSuccess($"Updated social card: {card.Title}");
                }
                catch (OcudaException ex)
                {
                    _logger.LogError(ex, "Error editing social card: {Message}", ex.Message);
                    ShowAlertDanger("Unable to update social card: ", ex.Message);
                }
            }

            return string.IsNullOrEmpty(model.LocationStub)
                ? RedirectToAction(nameof(Edit), new { id = model.SocialCard.Id })
                : RedirectToAction(nameof(LocationsController.Location),
                    LocationsController.Name,
                    new { locationStub = model.LocationStub });
        }

        [Route("[action]/{locationStub}")]
        [RestoreModelState]
        public async Task<IActionResult> EditForLocation(string locationStub)
        {
            Promenade.Models.Entities.Location location = null;

            if (!string.IsNullOrEmpty(locationStub))
            {
                location = await _locationService.GetLocationByStubAsync(locationStub);
            }

            if (location == null)
            {
                ShowAlertDanger($"Unknown location: {locationStub}");
                return RedirectToAction(nameof(Index));
            }

            if (!location.SocialCardId.HasValue)
            {
                return View("Detail", new DetailViewModel
                {
                    Action = nameof(Create),
                    LocationStub = locationStub
                });
            }

            var card = await _socialCardService.GetByIdAsync(location.SocialCardId.Value);

            if (card == null)
            {
                return View("Detail", new DetailViewModel
                {
                    Action = nameof(Create),
                    LocationStub = locationStub
                });
            }

            var viewModel = new DetailViewModel
            {
                Action = nameof(Edit),
                LocationStub = locationStub,
                SocialCard = card,
            };

            return View("Detail", viewModel);
        }

        [Route("")]
        [Route("{page}")]
        public async Task<IActionResult> Index(int page = 1)
        {
            var filter = new BaseFilter(page);

            var socialCardList = await _socialCardService.GetPaginatedListAsync(filter);

            var paginateModel = new PaginateModel
            {
                ItemCount = socialCardList.Count,
                CurrentPage = page,
                ItemsPerPage = filter.Take.Value
            };
            if (paginateModel.PastMaxPage)
            {
                return RedirectToRoute(new { page = paginateModel.LastPage ?? 1 });
            }

            var viewModel = new IndexViewModel
            {
                PaginateModel = paginateModel,
                SocialCards = socialCardList.Data
            };

            return View(viewModel);
        }
    }
}