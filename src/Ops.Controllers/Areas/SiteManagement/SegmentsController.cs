using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Segment;
using Ocuda.Utility.Models;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Utility.Keys;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement
{
    [Area("SiteManagement")]
    [Authorize(Policy = nameof(ClaimType.SiteManager))]
    [Route("[area]/[controller]")]
    public class SegmentsController : BaseController<SegmentsController>
    {
        private readonly ILanguageService _languageService;
        private readonly ISegmentService _segmentService;
        public static string Name { get { return "Segments"; } }

        public SegmentsController(ServiceFacades.Controller<SegmentsController> context,
            ILanguageService languageService,
            ISegmentService segmentService) : base(context)
        {
            _languageService = languageService
                ?? throw new ArgumentNullException(nameof(languageService));
            _segmentService = segmentService
                ?? throw new ArgumentNullException(nameof(segmentService));
        }

        [Route("")]
        [Route("[action]/{page}")]
        public async Task<IActionResult> Index(int page = 1)
        {
            var filter = new BaseFilter(page);
            var segmentList = await _segmentService.GetPaginatedListAsync(filter);

            var paginateModel = new PaginateModel
            {
                ItemCount = segmentList.Count,
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

            foreach (var segment in segmentList.Data.ToList())
            {
                segment.SegmentLanguages = await _segmentService.GetSegmentLanguagesByIdAsync(segment.Id);
            }
            var languages = await _languageService.GetActiveAsync();
            var selectedLanguage = languages.Single(_ => _.IsDefault);
            var viewModel = new IndexViewModel
            {
                Segments = segmentList.Data,
                PaginateModel = paginateModel,
                LanguageList = new SelectList(languages, nameof(Language.Id),
                    nameof(Language.Description),selectedLanguage.Id),
                AvailableLanguages = languages.Select(_=>_.Name).ToList()
            };
            return View(viewModel);
        }

        [Route("[action]/{id}")]
        [RestoreModelState]
        public async Task<IActionResult> SegmentDetails(int id, string language)
        {
            var segment = await _segmentService.GetSegmentById(id);
            if (segment == null)
            {
                ShowAlertDanger($"Could not find Segment with ID: {id}");
                RedirectToAction(nameof(SegmentsController.Index));
            }
            var languages = await _languageService.GetActiveAsync();

            var selectedLanguage = languages
                .FirstOrDefault(_ => _.Name.Equals(language, StringComparison.OrdinalIgnoreCase))
                ?? languages.Single(_ => _.IsDefault);
            var segmentText = _segmentService.GetBySegmentIdAndLanguage(id, selectedLanguage.Id);
            var viewModel = new DetailsViewModel
            {
                SegmentText = segmentText,
                Segment = segment,
                SelectedLanguage = selectedLanguage,
                LanguageList = new SelectList(languages, nameof(Language.Name),
                    nameof(Language.Description), selectedLanguage.Name),
            };
            return View(viewModel);
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState]
        public async Task<IActionResult> CreateSegment(Segment segment)
        {
            try
            {
                await _segmentService.AddSegment(segment);

                ShowAlertSuccess($"Added Segment: {segment.Name}");
                return RedirectToAction(nameof(SegmentsController.Index));
            }
            catch (OcudaException ex)
            {
                ShowAlertDanger($"Unable to Create Segment: {ex.Message}");
                RedirectToAction(nameof(SegmentsController.Index));
            }
            return RedirectToAction(nameof(SegmentsController.Index));
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState]
        public async Task<IActionResult> EditSegment(Segment segment)
        {
            try
            {
                await _segmentService.EditSegment(segment);

                ShowAlertSuccess($"Updated Segment: {segment.Name}");
                return RedirectToAction(nameof(SegmentsController.Index));
            }
            catch (OcudaException ex)
            {
                ShowAlertDanger($"Unable to Update Segment: {ex.Message}");
                RedirectToAction(nameof(SegmentsController.Index));
            }
            return RedirectToAction(nameof(SegmentsController.Index));
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState]
        public async Task<IActionResult> CreateSegmentText(SegmentText segmentText)
        {
            try
            {
                await _segmentService.AddSegmentText(segmentText);

                ShowAlertSuccess($"Added SegmentText: {segmentText.Header}");
                return RedirectToAction(nameof(SegmentsController.SegmentDetails), new { id = segmentText.SegmentId });
            }
            catch (OcudaException ex)
            {
                ShowAlertDanger($"Unable to Create Segment: {ex.Message}");
                RedirectToAction(nameof(SegmentsController.Index));
            }
            return RedirectToAction(nameof(SegmentsController.Index));
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState]
        public async Task<IActionResult> EditSegmentText(SegmentText segmentText)
        {
            try
            {
                await _segmentService.EditSegmentText(segmentText);
                var updatedSegmentText = _segmentService.GetBySegmentIdAndLanguage(segmentText.SegmentId,segmentText.LanguageId);
                var language = await _languageService.GetActiveByIdAsync(updatedSegmentText.LanguageId);

                ShowAlertSuccess($"Updated Segment: {updatedSegmentText.Header}");
                return RedirectToAction(nameof(SegmentsController.SegmentDetails), new { id = updatedSegmentText.SegmentId, language = language.Name});
            }
            catch (OcudaException ex)
            {
                ShowAlertDanger($"Unable to Update Segment: {ex.Message}");
                RedirectToAction(nameof(SegmentsController.Index));
            }
            return RedirectToAction(nameof(SegmentsController.Index));
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState]
        public async Task<IActionResult> DeleteSegmentText(SegmentText segmentText)
        {
            var language = await _languageService.GetActiveByIdAsync(segmentText.LanguageId);
            var segment = await _segmentService.GetSegmentById(segmentText.SegmentId);
            try
            {
                await _segmentService.DeleteSegmentText(segmentText);
                ShowAlertSuccess($"Deleted {language.Name} SegmentText for {segment.Name}");
            }
            catch (OcudaException ex)
            {
                ShowAlertDanger($"Unable to Delete {language.Name} SegmentText for {segment.Name}: {ex.Message}");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
