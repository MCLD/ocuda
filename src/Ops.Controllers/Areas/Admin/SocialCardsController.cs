using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Admin.ViewModels.SocialCards;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Admin
{
    [Area("Admin")]
    [Authorize(Policy = nameof(ClaimType.SiteManager))]
    [Route("[area]/[controller]")]
    public class SocialCardsController : BaseController<SocialCardsController>
    {
        private readonly ISocialCardService _socialCardService;

        public static string Name { get { return "SocialCards"; } }
        public static string Area { get { return "Admin"; } }

        public SocialCardsController(ServiceFacades.Controller<SocialCardsController> context,
            ISocialCardService socialCardService) : base(context)
        {
            _socialCardService = socialCardService
                ?? throw new ArgumentNullException(nameof(socialCardService));
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
                return RedirectToRoute(
                    new
                    {
                        page = paginateModel.LastPage ?? 1
                    });
            }

            var viewModel = new IndexViewModel
            {
                PaginateModel = paginateModel,
                SocialCards = socialCardList.Data
            };

            return View(viewModel);
        }

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
            if (ModelState.IsValid)
            {
                try
                {
                    var card = await _socialCardService.CreateAsync(model.SocialCard);
                    ShowAlertSuccess($"Added social card: {card.Title}");
                    return RedirectToAction(nameof(Edit), new { id = card.Id });
                }
                catch (OcudaException ex)
                {
                    _logger.LogError(ex, "Error adding social card: {Message}", ex.Message);
                    ShowAlertDanger("Unable to add social card: ", ex.Message);
                }
            }

            return RedirectToAction(nameof(Create));
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

            return RedirectToAction(nameof(Edit), new { id = model.SocialCard.Id });
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
    }
}
