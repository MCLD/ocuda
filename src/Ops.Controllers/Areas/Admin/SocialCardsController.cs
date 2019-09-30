using System;
using System.Collections.Generic;
using System.Text;
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
        public  IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState]
        public async Task<IActionResult> Create(IndexViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var card = await _socialCardService.CreateAsync(model.SocialCard);
                    ShowAlertSuccess($"Added social card: {card.Title}");
                }
                catch (OcudaException ex)
                {
                    _logger.LogError("Error adding social card: {ex}", ex.Message);
                    ShowAlertDanger("Unable to update social card: ", ex.Message);
                }
            }

            return RedirectToAction(nameof(Create));
        }
    }
}
