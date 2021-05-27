using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Segment;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;
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
    public class SegmentsController : BaseController<SegmentsController>
    {
        private readonly ILanguageService _languageService;
        private readonly ILocationService _locationService;
        private readonly IPermissionGroupService _permissionGroupService;
        private readonly ISegmentService _segmentService;

        public SegmentsController(ServiceFacades.Controller<SegmentsController> context,
            ILanguageService languageService,
            ILocationService locationService,
            IPermissionGroupService permissionGroupService,
            ISegmentService segmentService) : base(context)
        {
            _languageService = languageService
                ?? throw new ArgumentNullException(nameof(languageService));
            _locationService = locationService
                ?? throw new ArgumentNullException(nameof(locationService));
            _permissionGroupService = permissionGroupService
                ?? throw new ArgumentNullException(nameof(permissionGroupService));
            _segmentService = segmentService
                ?? throw new ArgumentNullException(nameof(segmentService));
        }

        public static string Area { get { return "SiteManagement"; } }
        public static string Name { get { return "Segments"; } }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> Create(IndexViewModel model)
        {
            JsonResponse response;

            if (model.Segment.StartDate.HasValue && model.Segment.EndDate.HasValue
                && model.Segment.StartDate > model.Segment.EndDate)
            {
                ModelState.AddModelError("Segment.StartDate", "Start Date cannot be after the End Date.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var segment = await _segmentService.CreateAsync(model.Segment);
                    response = new JsonResponse
                    {
                        Success = true,
                        Url = Url.Action(nameof(Detail), new { id = segment.Id })
                    };

                    ShowAlertSuccess($"Created segment: {segment.Name}");
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
                await _segmentService.DeleteAsync(model.Segment.Id);
                ShowAlertSuccess($"Deleted segment: {model.Segment.Name}");
            }
            catch (OcudaException ex)
            {
                var alertMessage = $"Unable to delete segment \"{model.Segment.Name}\": {ex.Message}";

                var inUseByList = (List<string>)ex.Data[OcudaExceptionData.SegmentInUseBy];
                if (inUseByList != null)
                {
                    var inUseByTag = new TagBuilder("ul");
                    foreach (var usedBy in inUseByList)
                    {
                        var inUseByItem = new TagBuilder("li");
                        inUseByItem.InnerHtml.SetContent(usedBy);
                        inUseByTag.InnerHtml.AppendHtml(inUseByItem);
                    }

                    using var inUseByHtml = new StringWriter();
                    inUseByTag.WriteTo(inUseByHtml, HtmlEncoder.Default);
                    ShowAlertDanger($"{alertMessage} {inUseByHtml}");
                }
                else
                {
                    ShowAlertDanger(alertMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting segment: {Message}", ex.Message);
                ShowAlertDanger("Unable to delete segment: ", ex.Message);
            }

            return RedirectToAction(nameof(Index), new { page = model.PaginateModel.CurrentPage });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> DeleteText(DetailViewModel model)
        {
            if (!await HasSegmentPermissionAsync(model.SegmentId))
            {
                return RedirectToUnauthorized();
            }

            var segmentText = await _segmentService.GetBySegmentAndLanguageAsync(model.SegmentId,
                model.LanguageId);

            await _segmentService.DeleteSegmentTextAsync(segmentText);

            var language = await _languageService.GetActiveByIdAsync(model.LanguageId);

            ShowAlertSuccess($"Deleted Segment {language.Description} text!");

            return RedirectToAction(nameof(Detail),
                new
                {
                    id = model.SegmentId,
                    language = language.Name
                });
        }

        [Route("[action]/{id}")]
        [RestoreModelState]
        public async Task<IActionResult> Detail(int id, string language)
        {
            if (!await HasSegmentPermissionAsync(id))
            {
                return RedirectToUnauthorized();
            }

            var segment = await _segmentService.GetByIdAsync(id);
            if (segment == null)
            {
                ShowAlertDanger($"Could not find Segment with ID: {id}");
                return RedirectToAction(nameof(SegmentsController.Index));
            }

            var languages = await _languageService.GetActiveAsync();

            var selectedLanguage = languages
                .FirstOrDefault(_ => _.Name.Equals(language, StringComparison.OrdinalIgnoreCase))
                ?? languages.Single(_ => _.IsDefault);

            var segmentText = await _segmentService
                .GetBySegmentAndLanguageAsync(id, selectedLanguage.Id);

            string backLink = null;
            string relationship = null;

            var pageLayoutId
                = await _segmentService.GetPageLayoutIdForSegmentAsync(segment.Id);

            if (pageLayoutId.HasValue)
            {
                backLink = Url.Action(nameof(PagesController.LayoutDetail),
                    PagesController.Name,
                    new
                    {
                        id = pageLayoutId.Value
                    });
                relationship = "This segment is used page layout ID: " + pageLayoutId.Value;
            }
            else
            {
                var locations = await _locationService.GetLocationsBySegment(segment.Id);

                if (locations?.Count == 1)
                {
                    backLink = Url.Action(nameof(LocationsController.Location),
                        LocationsController.Name,
                        new
                        {
                            locationStub = locations.First().Stub
                        });
                    relationship = "This segment is used for location: " + locations.First().Name;
                }
                if (locations?.Count > 1)
                {
                    relationship = string.Format(CultureInfo.InvariantCulture,
                        "This segment is used for multiple locations: {0}",
                        string.Join(", ", locations.Select(_ => _.Name)));
                }
            }

            var viewModel = new DetailViewModel
            {
                BackLink = backLink,
                LanguageDescription = selectedLanguage.Description,
                LanguageId = selectedLanguage.Id,
                LanguageList = new SelectList(languages,
                    nameof(Language.Name),
                    nameof(Language.Description),
                    selectedLanguage.Name),
                NewSegmentText = segmentText == null,
                Relationship = relationship,
                SegmentEndDate = segment.EndDate,
                SegmentId = segment.Id,
                SegmentName = segment.Name,
                SegmentStartDate = segment.StartDate,
                SegmentText = segmentText
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route("[action]/{id}")]
        [SaveModelState]
        public async Task<IActionResult> Detail(DetailViewModel model)
        {
            if (!await HasSegmentPermissionAsync(model.SegmentId))
            {
                return RedirectToUnauthorized();
            }

            var language = await _languageService.GetActiveByIdAsync(model.LanguageId);

            if (ModelState.IsValid)
            {
                var segmentText = model.SegmentText;
                segmentText.LanguageId = language.Id;
                segmentText.SegmentId = model.SegmentId;

                var currentSegmentText = await _segmentService.GetBySegmentAndLanguageAsync(
                    model.SegmentId, language.Id);

                if (currentSegmentText == null)
                {
                    await _segmentService.CreateSegmentTextAsync(segmentText);

                    ShowAlertSuccess("Added segment text!");
                }
                else
                {
                    await _segmentService.EditSegmentTextAsync(segmentText);

                    ShowAlertSuccess("Updated segment text!");
                }
            }

            return RedirectToAction(nameof(Detail), new
            {
                id = model.SegmentId,
                language = language.Name
            });
        }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpPost]
        [Route("[action]")]
        [SaveModelState]
        public async Task<IActionResult> Edit(IndexViewModel model)
        {
            JsonResponse response;

            if (model.Segment.StartDate.HasValue && model.Segment.EndDate.HasValue
                && model.Segment.StartDate > model.Segment.EndDate)
            {
                ModelState.AddModelError("Segment.StartDate", "Start Date cannot be after the End Date.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var segment = await _segmentService.EditAsync(model.Segment);
                    response = new JsonResponse
                    {
                        Success = true
                    };
                    ShowAlertSuccess($"Updated segment: {segment.Name}");
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
                    nameof(Language.Description), selectedLanguage.Id),
                AvailableLanguages = languages.Select(_ => _.Name).ToList()
            };
            return View(viewModel);
        }

        private async Task<bool> HasSegmentPermissionAsync(int segmentId)
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
                    var pageHeaderId = await _segmentService.GetPageHeaderIdForSegmentAsync(
                        segmentId);
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