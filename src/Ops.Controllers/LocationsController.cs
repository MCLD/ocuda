using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.ServiceFacades;
using Ocuda.Ops.Controllers.ViewModels.Locations;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Extensions;

namespace Ocuda.Ops.Controllers
{
    [Route("[controller]")]
    public class LocationsController : BaseController<LocationsController>
    {
        private readonly IFeatureService _featureService;
        private readonly ILanguageService _languageService;
        private readonly ILocationFeatureService _locationFeatureService;
        private readonly ILocationService _locationService;
        private readonly IPermissionGroupService _permissionGroupService;
        private readonly ISegmentService _segmentService;

        public LocationsController(Controller<LocationsController> context,
            IFeatureService featureService,
            ILanguageService languageService,
            ILocationFeatureService locationFeatureService,
            ILocationService locationService,
            IPermissionGroupService permissionGroupService,
            ISegmentService segmentService) : base(context)
        {
            ArgumentNullException.ThrowIfNull(featureService);
            ArgumentNullException.ThrowIfNull(languageService);
            ArgumentNullException.ThrowIfNull(locationFeatureService);
            ArgumentNullException.ThrowIfNull(locationService);
            ArgumentNullException.ThrowIfNull(permissionGroupService);
            ArgumentNullException.ThrowIfNull(segmentService);

            _featureService = featureService;
            _languageService = languageService;
            _locationFeatureService = locationFeatureService;
            _locationService = locationService;
            _permissionGroupService = permissionGroupService;
            _segmentService = segmentService;
        }

        public static string Name
        { get { return "Locations"; } }

        [HttpGet("[action]/{slug}/{featureId}")]
        public async Task<IActionResult> AddDescription(string slug, int featureId)
        {
            var hasPermission = await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.LocationManagement)
                && await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.WebPageContentManagement);
            if (!hasPermission) { return RedirectToUnauthorized(); }

            Location location;
            Feature feature;
            try
            {
                (feature, location) = await GetFeatureLocation(featureId, slug);
            }
            catch (OcudaException)
            {
                return NotFound();
            }

            var locationFeature = await _locationFeatureService
                .GetByFeatureIdLocationIdAsync(featureId, location.Id);
            if (locationFeature == null) { return NotFound(); }

            if (locationFeature.SegmentId != null)
            {
                return RedirectToAction(nameof(Areas.SiteManagement.SegmentsController.Detail),
                    Areas.SiteManagement.SegmentsController.Name,
                    new
                    {
                        area = Areas.SiteManagement.SegmentsController.Area,
                        id = locationFeature.SegmentId
                    });
            }

            var segment = await _segmentService.CreateAsync(new Segment
            {
                IsActive = true,
                Name = $"Location {location.Name} feature {feature.Name} custom text"
            });

            if (segment == null)
            {
                _logger.LogError("Unable to create segment for {LocationName} feature {FeatureName}",
                    location.Name,
                    feature.Name);
                ShowAlertDanger("Unable to create segment. Please contact an administrator.");
                return RedirectToAction(nameof(LocationFeature), new { slug, featureId });
            }

            locationFeature.SegmentId = segment.Id;
            await _locationFeatureService.EditAsync(locationFeature);

            return RedirectToAction(nameof(Areas.SiteManagement.SegmentsController.Detail),
                Areas.SiteManagement.SegmentsController.Name,
                new
                {
                    area = Areas.SiteManagement.SegmentsController.Area,
                    id = segment.Id
                });
        }

        [HttpPost("[action]/{slug}")]
        public async Task<IActionResult> AddFeature(string slug, int featureId)
        {
            var hasPermission = await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission) { return RedirectToUnauthorized(); }

            var location = await _locationService.GetLocationByStubAsync(slug);
            if (location == null) { return NotFound(); }

            var locationFeature = await _locationFeatureService
                .GetByFeatureIdLocationIdAsync(featureId, location.Id);

            if (locationFeature == null)
            {
                await _locationFeatureService.AddLocationFeatureAsync(new LocationFeature
                {
                    FeatureId = featureId,
                    LocationId = location.Id
                });
            }
            else
            {
                ShowAlertDanger("Feature is already configured for that location.");
            }

            return RedirectToAction(nameof(LocationFeature), new { slug, featureId });
        }

        [HttpGet("[action]/{slug}")]
        public async Task<IActionResult> AddFeature(string slug)
        {
            var hasPermission = await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission) { return RedirectToUnauthorized(); }

            var location = await _locationService.GetLocationByStubAsync(slug);
            if (location == null) { return NotFound(); }

            var features = await _featureService.GetAllFeaturesAsync();

            var locationFeatures = await _locationFeatureService
                .GetLocationFeaturesByLocationAsync(location.Id);

            var locationHasFeatureIds = locationFeatures.Select(_ => _.FeatureId);

            var viewModel = new AddFeatureViewModel
            {
                Location = location
            };

            viewModel.AvailableFeatures.AddRange(features
                .Where(_ => !locationHasFeatureIds.Contains(_.Id))
                .ToList());

            return View(viewModel);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ClearLink(string slug, int featureId)
        {
            var hasPermission = await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission) { return RedirectToUnauthorized(); }

            var location = await _locationService.GetLocationByStubAsync(slug);
            if (location == null) { return NotFound(); }

            var locationFeature = await _locationFeatureService
                .GetByFeatureIdLocationIdAsync(featureId, location.Id);
            if (locationFeature == null) { return NotFound(); }

            locationFeature.RedirectUrl = null;

            await _locationFeatureService.EditAsync(locationFeature);

            return RedirectToAction(nameof(LocationFeature), new
            {
                slug = location.Stub,
                featureId
            });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ClearSegment(string slug, int featureId)
        {
            var hasPermission = await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.LocationManagement)
                && await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.WebPageContentManagement);
            if (!hasPermission) { return RedirectToUnauthorized(); }

            var location = await _locationService.GetLocationByStubAsync(slug);
            if (location == null) { return NotFound(); }

            var locationFeature = await _locationFeatureService
                .GetByFeatureIdLocationIdAsync(featureId, location.Id);
            if (locationFeature?.SegmentId == null) { return NotFound(); }

            await _segmentService.DeleteAsync(locationFeature.SegmentId.Value);

            locationFeature.SegmentId = null;
            await _locationFeatureService.EditAsync(locationFeature);

            return RedirectToAction(nameof(LocationFeature), new
            {
                slug = location.Stub,
                featureId
            });
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> Details(string slug)
        {
            var location = await _locationService.GetLocationByStubAsync(slug);
            if (location == null)
            {
                return NotFound();
            }

            var defaultLanguageId = await _languageService.GetDefaultLanguageId();

            if (location.DescriptionSegmentId != default)
            {
                location.DescriptionSegment = await _segmentService
                    .GetBySegmentAndLanguageAsync(location.DescriptionSegmentId, defaultLanguageId);
            }

            var features = await _featureService.GetAllFeaturesAsync();

            var locationFeatures = await _locationFeatureService
                .GetLocationFeaturesByLocationAsync(location.Id);

            var featuresHere = features
                .Where(_ => locationFeatures.Select(_ => _.FeatureId).Contains(_.Id));

            var viewModel = new DetailsViewModel
            {
                AtThisLocation = featuresHere.Where(_ => _.IsAtThisLocation).OrderBy(_ => _.SortOrder).ToList(),
                Location = location,
                ServicesAvailable = featuresHere.Where(_ => !_.IsAtThisLocation).OrderBy(_ => _.SortOrder).ToList(),
                LocationManager = await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.LocationManagement),
                SegmentEditor = await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.WebPageContentManagement)
            };

            viewModel.DescriptionLanguages.AddRange(await _segmentService
                .GetSegmentLanguagesByIdAsync(location.DescriptionSegmentId));
            viewModel.AllLanguages.AddRange(await _languageService.GetActiveNamesAsync());

            return View(viewModel);
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(int page)
        {
            var filter = new LocationFilter(page == 0 ? 1 : page, 60);

            var locationList = await _locationService.GetPaginatedListAsync(filter);

            var viewModel = new IndexViewModel
            {
                CurrentPage = filter.Page,
                ItemCount = locationList.Count,
                ItemsPerPage = filter.Take.Value,
                Locations = locationList.Data
            };

            if (viewModel.PastMaxPage)
            {
                return RedirectToRoute(new { page = viewModel.LastPage ?? 1 });
            }

            return View(viewModel);
        }

        [HttpGet("[action]/{slug}/{featureId}")]
        public async Task<IActionResult> LocationFeature(string slug, int featureId)
        {
            Location location;
            Feature feature;
            try
            {
                (feature, location) = await GetFeatureLocation(featureId, slug);
            }
            catch (OcudaException)
            {
                return NotFound();
            }

            var locationFeature = await _locationFeatureService
                .GetByFeatureIdLocationIdAsync(featureId, location.Id);

            if (locationFeature == null)
            {
                return NotFound();
            }

            var viewModel = new LocationFeatureViewModel
            {
                Feature = feature,
                Location = location,
                LocationFeature = locationFeature,
                CanManageLocations = await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.LocationManagement),
                CanEditSegments = await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.WebPageContentManagement)
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

            if (locationFeature.SegmentId.HasValue)
            {
                var locationFeatureText = await _segmentService
                    .GetBySegmentAndLanguageAsync(locationFeature.SegmentId.Value, defaultLanguageId);
                locationFeature.Text = CommonMark.CommonMarkConverter.Convert(locationFeatureText?.Text);
                viewModel.LocationFeatureLanguages.AddRange(await _segmentService
                    .GetSegmentLanguagesByIdAsync(locationFeature.SegmentId.Value));
            }

            return View(viewModel);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> RemoveFeature(string slug, int featureId)
        {
            var hasPermission = await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission) { return RedirectToUnauthorized(); }

            var location = await _locationService.GetLocationByStubAsync(slug);
            if (location == null) { return NotFound(); }

            var locationFeature = await _locationFeatureService
                .GetByFeatureIdLocationIdAsync(featureId, location.Id);
            if (locationFeature == null) { return NotFound(); }

            if (locationFeature.SegmentId.HasValue)
            {
                await _segmentService
                    .DeleteWithTextsAlreadyVerifiedAsync(locationFeature.SegmentId.Value);
            }

            await _locationFeatureService.DeleteAsync(featureId, location.Id);

            return RedirectToAction(nameof(Details), new { slug });
        }

        [HttpPost("[action]/{slug}/{featureId}")]
        public async Task<IActionResult> UpdateLink(LinkViewModel viewModel)
        {
            if (viewModel == null) { return BadRequest(); }

            var hasPermission = await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission) { return RedirectToUnauthorized(); }

            Location location;
            Feature feature;
            try
            {
                (feature, location) = await GetFeatureLocation(viewModel.FeatureId,
                    viewModel.LocationStub);
            }
            catch (OcudaException)
            {
                return NotFound();
            }

            var locationFeature = await _locationFeatureService
                .GetByFeatureIdLocationIdAsync(feature.Id, location.Id);
            if (locationFeature == null) { return NotFound(); }

            locationFeature.RedirectUrl = viewModel.Link?.Trim();
            locationFeature.NewTab = viewModel.NewTab;

            await _locationFeatureService.EditAsync(locationFeature);

            return RedirectToAction(nameof(LocationFeature), new
            {
                slug = location.Stub,
                featureId = feature.Id
            });
        }

        [HttpGet("[action]/{slug}/{featureId}")]
        public async Task<IActionResult> UpdateLink(string slug, int featureId)
        {
            var hasPermission = await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission) { return RedirectToUnauthorized(); }

            Location location;
            Feature feature;
            try
            {
                (feature, location) = await GetFeatureLocation(featureId, slug);
            }
            catch (OcudaException)
            {
                return NotFound();
            }

            var viewModel = new LinkViewModel
            {
                Location = location,
                Feature = feature
            };

            var locationFeature = await _locationFeatureService
                .GetByFeatureIdLocationIdAsync(featureId, location.Id);

            if (locationFeature != null)
            {
                viewModel.Link = locationFeature.RedirectUrl;
                viewModel.NewTab = locationFeature.NewTab;
            }

            return View(nameof(UpdateLink), viewModel);
        }

        private async Task<(Feature, Location)> GetFeatureLocation(int featureId, string stub)
        {
            var location = await _locationService.GetLocationByStubAsync(stub);
            if (location == null)
            {
                _logger.LogError("Unable to find location with stub: {Stub}", stub);
                throw new OcudaException($"Unable to find location with stub: {stub}");
            }

            var feature = await _featureService.GetFeatureByIdAsync(featureId);
            if (feature == null)
            {
                _logger.LogError("Unable to find feature with id: {FeatureId}", featureId);
                throw new OcudaException($"Unable to find feature with id: {featureId}");
            }

            return (feature, location);
        }
    }
}