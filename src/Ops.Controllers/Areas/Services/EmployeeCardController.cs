using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Services.ViewModels.EmployeeCard;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;

namespace Ocuda.Ops.Controllers.Areas.Services
{
    [Area("Services")]
    [Route("[area]/[controller]")]
    public class EmployeeCardController : BaseController<EmployeeCardController>
    {
        public IEmployeeCardService _employeeCardService;

        public EmployeeCardController(ServiceFacades.Controller<EmployeeCardController> context,
            IEmployeeCardService employeeCardService)
            : base(context)
        {
            ArgumentNullException.ThrowIfNull(employeeCardService);

            _employeeCardService = employeeCardService;
        }

        public static string Name
        { get { return "EmployeeCard"; } }

        [HttpGet("[action]/{id}")]
        [RestoreModelState]
        public async Task<IActionResult> Details(int id)
        {
            var cardRequest = await _employeeCardService.GetRequestAsync(id);
            if (cardRequest == null)
            {
                ShowAlertWarning($"Unable to find card request {id}");
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new DetailsViewModel
            {
                CardRequest = cardRequest
            };

            return View(viewModel);
        }

        [HttpPost("[action]/{id}")]
        [SaveModelState]
        public async Task<IActionResult> Details(DetailsViewModel viewModel)
        {
            ArgumentNullException.ThrowIfNull(viewModel);
            if (viewModel.Action == DetailsViewModel.SubmitType.SaveNotes)
            {
                await _employeeCardService.UpdateNotesAsync(viewModel.CardRequest);
            }
            else
            {

            }
            return RedirectToAction(nameof(Details), new { id = viewModel.CardRequest.Id });
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? page, bool processed)
        {
            page ??= 1;

            var filter = new RequestFilter(page.Value)
            {
                IsProcessed = processed
            };

            var requests = await _employeeCardService.GetRequestsAsync(filter);

            var viewModel = new IndexViewModel
            {
                CardRequests = requests.Data,
                CurrentPage = page.Value,
                IsProcessed = processed,
                ItemCount = requests.Count,
                ItemsPerPage = filter.Take.Value,
            };

            if (processed)
            {
                viewModel.PendingCount = await _employeeCardService.GetRequestCountAsync(false);
                viewModel.ProcessedCount = viewModel.ItemCount;

            }
            else
            {
                viewModel.PendingCount = viewModel.ItemCount;
                viewModel.ProcessedCount = await _employeeCardService.GetRequestCountAsync(true);
            }

            return View(viewModel);
        }
    }
}
