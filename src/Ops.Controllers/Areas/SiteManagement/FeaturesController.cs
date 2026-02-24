using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Feature;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Email;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Extensions;
using Ocuda.Utility.Filters;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement
{
    [Area("SiteManagement")]
    [Route("[area]/[controller]")]
    public class FeaturesController : BaseController<FeaturesController>
    {
        private readonly IFeatureService _featureService;
        private readonly ILanguageService _languageService;
        private readonly IPermissionGroupService _permissionGroupService;
        private readonly ISegmentService _segmentService;

        public FeaturesController(ServiceFacades.Controller<FeaturesController> context,
            IFeatureService featureService,
            ILanguageService languageService,
            IPermissionGroupService permissionGroupService,
            ISegmentService segmentService) : base(context)
        {
            ArgumentNullException.ThrowIfNull(featureService);
            ArgumentNullException.ThrowIfNull(languageService);
            ArgumentNullException.ThrowIfNull(permissionGroupService);
            ArgumentNullException.ThrowIfNull(segmentService);

            _featureService = featureService;
            _languageService = languageService;
            _permissionGroupService = permissionGroupService;
            _segmentService = segmentService;
        }

        public static string Area
        { get { return "SiteManagement"; } }

        public static string Name
        { get { return "Features"; } }

        [HttpGet("[action]/{slug}")]
        public async Task<IActionResult> AddDescription(string slug)
        {
            var hasPermission = await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.FeatureManagement)
                && await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.WebPageContentManagement);
            if (!hasPermission) { return RedirectToUnauthorized(); }

            var feature = await _featureService.GetFeatureBySlugAsync(slug);
            if (feature == null) { return NotFound(); }

            if (feature.TextSegmentId.HasValue)
            {
                return RedirectToAction(nameof(SegmentsController.Detail),
                    SegmentsController.Name,
                    new
                    {
                        area = Areas.SiteManagement.SegmentsController.Area,
                        id = feature.TextSegmentId.Value
                    });
            }

            var segment = await _segmentService.CreateAsync(new Segment
            {
                IsActive = true,
                Name = $"Feature {feature.Name} description text"
            });

            if (segment == null)
            {
                _logger.LogError("Unable to create segment for feature {FeatureName}",
                    feature.Name);
                ShowAlertDanger("Unable to create segment. Please contact an administrator.");
                return RedirectToAction(nameof(Details), new { slug });
            }

            feature.TextSegmentId = segment.Id;
            await _featureService.EditAsync(feature);

            return RedirectToAction(nameof(SegmentsController.Detail),
                SegmentsController.Name,
                new
                {
                    area = Areas.SiteManagement.SegmentsController.Area,
                    id = segment.Id
                });
        }

        [HttpGet("[action]")]
        [RestoreModelState]
        public async Task<IActionResult> CreateFeature()
        {
            var hasPermission = await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.FeatureManagement);
            if (!hasPermission) { return RedirectToUnauthorized(); }

            return View();
        }

        [HttpPost("[action]")]
        [SaveModelState]
        public async Task<IActionResult> CreateFeature(CreateFeatureViewModel viewModel)
        {
            var hasPermission = await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.FeatureManagement);
            if (!hasPermission) { return RedirectToUnauthorized(); }

            if (viewModel == null) { return NotFound(); }

            var slugLookup = await _featureService.GetFeatureBySlugAsync(viewModel.Slug?.Trim());
            if (slugLookup != null)
            {
                ModelState.AddModelError("Slug", "That Slug is already in use.");
            }

            if (ModelState.IsValid)
            {
                var defaultLanguageId = await _languageService.GetDefaultLanguageId();
                var segment = await _segmentService.CreateAsync(new Segment
                {
                    IsActive = true,
                    Name = $"Feature {viewModel.Name?.Trim()}",
                });

                await _segmentService.CreateSegmentTextAsync(new SegmentText
                {
                    LanguageId = defaultLanguageId,
                    SegmentId = segment.Id,
                    Text = viewModel.Name.Trim()
                });

                var feature = new Feature
                {
                    Name = viewModel.Name?.Trim(),
                    Icon = viewModel.Icon,
                    NameSegmentId = segment.Id,
                    Stub = viewModel.Slug
                };

                await _featureService.AddFeatureAsync(feature);

                return RedirectToAction(nameof(Feature), new { viewModel.Slug });
            }
            else
            {
                var issues = new StringBuilder("Issues creating ").Append(viewModel.Name).Append(":<ul>");
                foreach (var model in ModelState.Values.SelectMany(v => v.Errors))
                {
                    issues.Append("<li>")
                        .Append(model.ErrorMessage)
                        .Append("</li>");
                }
                issues.Append("</ul>");
                ShowAlertDanger(issues.ToString());
                return RedirectToAction(nameof(CreateFeature));
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> DeleteFeature(string slug)
        {
            var hasPermission = await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.FeatureManagement);
            if (!hasPermission) { return RedirectToUnauthorized(); }

            var feature = await _featureService.GetFeatureBySlugAsync(slug);
            if (feature == null)
            {
                return NotFound();
            }

            try
            {
                await _featureService.DeleteAsync(feature.Id);
                ShowAlertSuccess($"Deleted Feature: {feature.Name}");
            }
            catch (OcudaException ex)
            {
                _logger.LogError(ex, "Problem deleting feature: {Message}", ex.Message);
                ShowAlertDanger($"Unable to Delete Feature {feature.Name}: {ex.Message}");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> DeleteText(string slug)
        {
            var hasPermission = await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.FeatureManagement);
            if (!hasPermission) { return RedirectToUnauthorized(); }

            var feature = await _featureService.GetFeatureBySlugAsync(slug);
            if (feature == null) { return NotFound(); }

            if (feature.TextSegmentId.HasValue)
            {
                await _segmentService
                    .DeleteWithTextsAlreadyVerifiedAsync(feature.TextSegmentId.Value);
                ShowAlertSuccess("Text deleted.");
            }
            else
            {
                ShowAlertSuccess("Unable to delete text.");
            }

            feature.TextSegmentId = null;
            await _featureService.EditAsync(feature);

            return RedirectToAction(nameof(FeaturesController.Feature), new
            {
                slug = feature.Stub
            });
        }

        [HttpPost("[action]")]
        [SaveModelState]
        public async Task<IActionResult> EditFeature(Feature feature)
        {
            var hasPermission = await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.FeatureManagement);
            if (!hasPermission) { return RedirectToUnauthorized(); }

            if (feature == null) { return NotFound(); }

            if (ModelState.IsValid)
            {
                try
                {
                    await _featureService.EditAsync(feature);
                    ShowAlertSuccess($"Updated Feature: {feature.Name}");
                    return RedirectToAction(nameof(Feature), new { slug = feature.Stub });
                }
                catch (OcudaException ex)
                {
                    ShowAlertDanger($"Unable to Update Feature: {feature.Name}");
                    _logger.LogError(ex, "Problem updating feature: {Message}", ex.Message);
                    return RedirectToAction(nameof(Feature), new { slug = feature.Stub });
                }
            }
            else
            {
                var issues = new StringBuilder("Issues saving ").Append(feature.Name).Append(":<ul>");
                foreach (var model in ModelState.Values.SelectMany(v => v.Errors))
                {
                    issues.Append("<li>")
                        .Append(model.ErrorMessage)
                        .Append("</li>");
                }
                issues.Append("</ul>");
                ShowAlertDanger(issues.ToString());
                return RedirectToAction(nameof(Feature), new { slug = feature.Stub });
            }
        }

        [HttpGet("{slug}")]
        [RestoreModelState]
        public async Task<IActionResult> Feature(string slug)
        {
            var feature = await _featureService.GetFeatureBySlugAsync(slug);
            if (feature == null) { return NotFound(); }

            var viewModel = new FeatureViewModel
            {
                CanEditSegments = await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.WebPageContentManagement),
                CanManageFeatures = await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.FeatureManagement),
                CanUpdateSlug = IsSiteManager(),
                Feature = feature
            };

            viewModel.AllLanguages.AddRange(await _languageService.GetActiveNamesAsync());

            var defaultLanguageId = await _languageService.GetDefaultLanguageId();

            var nameSegment = await _segmentService
                .GetBySegmentAndLanguageAsync(feature.NameSegmentId, defaultLanguageId);
            feature.DisplayName = nameSegment.Text;
            viewModel.FeatureNameLanguages.AddRange(await _segmentService
                .GetSegmentLanguagesByIdAsync(feature.NameSegmentId));

            if (feature.TextSegmentId.HasValue)
            {
                var featureText = await _segmentService
                    .GetBySegmentAndLanguageAsync(feature.TextSegmentId.Value, defaultLanguageId);
                feature.BodyText = CommonMark.CommonMarkConverter.Convert(featureText?.Text);
                viewModel.FeatureTextLanguages.AddRange(await _segmentService
                    .GetSegmentLanguagesByIdAsync(feature.TextSegmentId.Value));
            }

            return View("FeatureDetails", viewModel);
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(int page)
        {
            var itemsPerPage = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.UserInterface.ItemsPerPage);

            var filter = new BaseFilter(page == 0 ? 1 : page, itemsPerPage);

            var featureList = await _featureService.GetPaginatedListAsync(filter);

            var viewModel = new FeatureViewModel
            {
                ItemCount = featureList.Count,
                CurrentPage = filter.Page,
                ItemsPerPage = filter.Take.Value
            };

            if (viewModel.PastMaxPage)
            {
                return RedirectToRoute(new { page = viewModel.LastPage ?? 1 });
            }

            viewModel.CanManageFeatures = await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.FeatureManagement);

            viewModel.AllFeatures.AddRange(featureList.Data);

            return View(viewModel);
        }

        [HttpGet("[action]/{slug}")]
        [RestoreModelState]
        public async Task<IActionResult> UpdateSlug(string slug)
        {
            if (!IsSiteManager()) { return RedirectToUnauthorized(); }

            var feature = await _featureService.GetFeatureBySlugAsync(slug);
            if (feature == null) { return NotFound(); }

            return View(new SlugViewModel { PriorSlug = slug, Slug = slug });
        }

        [HttpPost("[action]/{slug}")]
        [SaveModelState]
        public async Task<IActionResult> UpdateSlug(SlugViewModel viewModel)
        {
            if (!IsSiteManager()) { return RedirectToUnauthorized(); }

            if (viewModel == null) { return NotFound(); }

            if (string.IsNullOrEmpty(viewModel.Slug)) { return NotFound(); }

            var feature = await _featureService.GetFeatureBySlugAsync(viewModel.PriorSlug);
            if (feature == null) { return NotFound(); }

            var slugLookup = await _featureService.GetFeatureBySlugAsync(viewModel.Slug?.Trim());
            if (slugLookup != null)
            {
                ModelState.AddModelError("Slug", "That Slug is already in use.");
            }

            if (ModelState.IsValid)
            {
                feature.Stub = viewModel.Slug;
                await _featureService.EditAsync(feature);
                return RedirectToAction(nameof(Feature), new { slug = viewModel.Slug });
            }

            return RedirectToAction(nameof(UpdateSlug), new { slug = viewModel.PriorSlug });
        }
    }
}