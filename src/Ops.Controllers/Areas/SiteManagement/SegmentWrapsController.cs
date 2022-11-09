using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.SegmentWrap;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Controllers.ServiceFacades;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement
{
    [Area("SiteManagement")]
    [Route("[area]/[controller]")]
    public class SegmentWrapsController : BaseController<SegmentsController>
    {
        private readonly ISegmentWrapService _segmentWrapService;

        public SegmentWrapsController(Controller<SegmentsController> context,
            ISegmentWrapService segmentWrapService) : base(context)
        {
            _segmentWrapService = segmentWrapService
                ?? throw new ArgumentNullException(nameof(segmentWrapService));
        }

        public static string Area
        { get { return "SiteManagement"; } }

        public static string Name
        { get { return "SegmentWraps"; } }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpGet("[action]")]
        [RestoreModelState]
        public async Task<IActionResult> AddUpdate()
        {
            return View("Detail", new DetailViewModel
            {
                IsEdit = false
            });
        }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpGet("[action]/{segmentWrapId}")]
        [RestoreModelState]
        public async Task<IActionResult> AddUpdate(int segmentWrapId)
        {
            var segmentWrap = await _segmentWrapService.FindAsync(segmentWrapId);

            if (segmentWrap == null)
            {
                ShowAlertWarning($"Unable to find Segment Wrap Id {segmentWrapId}");
                return RedirectToAction(nameof(Index));
            }

            return View("Detail", new DetailViewModel
            {
                IsEdit = true,
                SegmentWrap = segmentWrap,
                SegmentWrapId = segmentWrap.Id
            });
        }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpPost("[action]")]
        [HttpPost("[action]/{segmentWrapId}")]
        [SaveModelState]
        public async Task<IActionResult> AddUpdate(DetailViewModel viewModel)
        {
            if (viewModel == null)
            {
                ShowAlertWarning("Unable to insert blank Segment Wrap.");
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                return viewModel.IsEdit
                    ? await UpdateAsync(viewModel)
                    : await AddAsync(viewModel);
            }
            else
            {
                return viewModel.IsEdit
                    ? RedirectToAction(nameof(AddUpdate))
                    : RedirectToAction(nameof(AddUpdate), new { segmentWrapId = viewModel.SegmentWrap.Id });
            }
        }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpPost("[action]")]
        public async Task<IActionResult> Disable(int segmentWrapId)
        {
            string description = await _segmentWrapService.DisableAsync(segmentWrapId)
                ? "deleted"
                : "disabled";
            ShowAlertWarning($"Segment Wrap ID {segmentWrapId} has been {description}.");
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpGet("")]
        [HttpGet("[action]/{page}")]
        public async Task<IActionResult> Index(int page)
        {
            if (page == 0)
            {
                page = 1;
            }

            var filter = new BaseFilter(page);
            var segmentWrapsList = await _segmentWrapService.GetPaginatedAsync(filter);

            var indexModel = new IndexViewModel
            {
                CurrentPage = page,
                ItemCount = segmentWrapsList.Count,
                ItemsPerPage = filter.Take.Value,
                SegmentWraps = segmentWrapsList.Data
            };

            return indexModel.PastMaxPage
                ? RedirectToRoute(new { page = indexModel.LastPage ?? 1 })
                : View(nameof(Index), indexModel);
        }

        private async Task<IActionResult> AddAsync(DetailViewModel viewModel)
        {
            await _segmentWrapService.AddAsync(viewModel.SegmentWrap);
            return RedirectToAction(nameof(Index));
        }

        private async Task<IActionResult> UpdateAsync(DetailViewModel viewModel)
        {
            viewModel.SegmentWrap.Id = viewModel.SegmentWrapId;
            await _segmentWrapService.UpdateAsync(viewModel.SegmentWrap);
            return RedirectToAction(nameof(Index));
        }
    }
}