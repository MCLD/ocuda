using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonMark;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Controllers.ViewModels.Locations;
using Ocuda.Promenade.Service;

namespace Ocuda.Promenade.Controllers
{
    [Route("[Controller]")]
    [Route("{culture:cultureConstraint?}/[Controller]")]
    public class LocationsController : BaseController<LocationsController>
    {
        private readonly string _apiKey;
        private readonly LocationService _locationService;
        private readonly SegmentService _segmentService;

        public LocationsController(ServiceFacades.Controller<LocationsController> context,
            LocationService locationService,
            SegmentService segmentService) : base(context)
        {
            _locationService = locationService
                ?? throw new ArgumentNullException(nameof(locationService));
            _segmentService = segmentService
                ?? throw new ArgumentNullException(nameof(segmentService));

            _apiKey = _config[Utility.Keys.Configuration.OcudaGoogleAPI];
        }

        public static string Name { get { return "Locations"; } }

        public IFormatProvider CurrentCulture { get; }

        [HttpGet("{locationStub}/[action]/{featureStub}")]
        public async Task<IActionResult> FeatureInfo(string locationStub, string featureStub)
        {
            var locationFeature
                 = await _locationService.GetLocationFullFeatureAsync(locationStub, featureStub);

            if (locationFeature != null)
            {
                var viewModel = new FeatureInfoViewModel
                {
                    BodyText = CommonMarkConverter.Convert(locationFeature.Feature.BodyText),
                    Text = CommonMarkConverter.Convert(locationFeature.Text)
                };

                return Json(viewModel);
            }
            else
            {
                _logger.LogWarning("Location Feature not found for location {locationStub} and feature {featureStub}",
                        locationStub,
                        featureStub);
                return NotFound();
            }
        }

        [HttpGet("")]
        [HttpGet("[action]")]
        [HttpGet("[action]/{Zip}")]
        [HttpGet("[action]/{latitude}/{longitude}")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design",
            "CA1031:Do not catch general exception types",
            Justification = "Show end user error message rather than exception")]
        public async Task<IActionResult> Find(string zip, double? latitude, double? longitude)
        {
            string issue = null;

            if (!string.IsNullOrEmpty(zip))
            {
                if (!long.TryParse(zip, out long _) || zip.Length != 5)
                {
                    issue = _localizer[i18n.Keys.Promenade.ErrorZipCode];
                }
                else
                {
                    try
                    {
                        return await FindAsync(zip);
                    }
                    catch (Exception)
                    {
                        issue = _localizer[i18n.Keys.Promenade.ErrorItemZipCode, zip];
                    }
                }
            }
            else if (latitude.HasValue && longitude.HasValue)
            {
                try
                {
                    return await FindAsync(latitude, longitude);
                }
                catch (Exception)
                {
                    issue = _localizer[i18n.Keys.Promenade.CoordinatesErrorItem,
                        latitude,
                        longitude];
                }
            }

            var viewModel = await CreateLocationViewModelAsync(default, default);
            viewModel.Warning = issue;
            return await ShowNearestAsync(viewModel);
        }

        [HttpGet("{locationStub}")]
        public async Task<IActionResult> Locations(string locationStub)
        {
            if (string.IsNullOrEmpty(locationStub))
            {
                return RedirectToAction(nameof(Find));
            }

            var viewModel = new LocationDetailViewModel
            {
                CanonicalUrl = await GetCanonicalUrlAsync(),
                Location = await _locationService.GetLocationByStubAsync(locationStub)
            };

            if (viewModel.Location == null)
            {
                return NotFound();
            }

            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

            if (viewModel.Location.HoursSegmentId.HasValue)
            {
                viewModel.HoursSegment
                    = await _segmentService.GetSegmentTextBySegmentIdAsync(
                        viewModel.Location.HoursSegmentId.Value,
                        forceReload);

                if (viewModel.HoursSegment != null)
                {
                    viewModel.HoursSegment.Text
                        = CommonMarkConverter.Convert(viewModel.HoursSegment.Text);
                }
            }

            if (viewModel.Location.PreFeatureSegmentId.HasValue)
            {
                viewModel.PreFeatureSegment = await _segmentService.GetSegmentTextBySegmentIdAsync(
                    viewModel.Location.PreFeatureSegmentId.Value, forceReload);
                if (viewModel.PreFeatureSegment != null)
                {
                    viewModel.PreFeatureSegment.Text
                        = CommonMarkConverter.Convert(viewModel.PreFeatureSegment.Text);
                }
            }

            if (viewModel.Location.PostFeatureSegmentId.HasValue)
            {
                viewModel.PostFeatureSegment = await _segmentService.GetSegmentTextBySegmentIdAsync(
                    viewModel.Location.PostFeatureSegmentId.Value, forceReload);
                if (viewModel.PostFeatureSegment != null)
                {
                    viewModel.PostFeatureSegment.Text
                        = CommonMarkConverter.Convert(viewModel.PostFeatureSegment.Text);
                }
            }

            viewModel.Location.DescriptionSegment = await _segmentService
                .GetSegmentTextBySegmentIdAsync(viewModel.Location.DescriptionSegmentId,
                    forceReload);

            if (viewModel.Location.DescriptionSegment?.Text.Length > 0)
            {
                viewModel.Location.DescriptionSegment.Text = CommonMarkConverter
                    .Convert(viewModel.Location.DescriptionSegment.Text);
            }

            viewModel.Location.LocationHours
                = await _locationService.GetFormattedWeeklyHoursAsync(viewModel.Location.Id);

            if (viewModel.Location.LocationHours != null)
            {
                var hours = await _locationService
                    .GetFormattedWeeklyHoursAsync(viewModel.Location.Id, true);

                viewModel.StructuredLocationHours = hours.ConvertAll(_ => $"{_.Days} {_.Time}");
            }

            var locationFeatures
                = await _locationService.GetFullLocationFeaturesAsync(locationStub);

            viewModel.LocationFeatures = locationFeatures
                .Select(_ => new LocationsFeaturesViewModel(_));

            if (viewModel.Location.DisplayGroupId.HasValue)
            {
                viewModel.LocationNeighborGroup = await _locationService
                    .GetLocationsNeighborGroup(viewModel.Location.DisplayGroupId.Value);

                var neighbors = await _locationService
                    .GetLocationsNeighborsAsync(viewModel.Location.DisplayGroupId.Value);
                if (neighbors.Count > 0)
                {
                    viewModel.NearbyLocationGroups = neighbors;
                    viewModel.NearbyCount = viewModel.NearbyLocationGroups.Count;
                    viewModel.NearbyEventsCount = viewModel.NearbyLocationGroups
                        .Count(_ => _.Location.HasEvents);
                }
            }

            PageTitle = viewModel.Location.Name;

            return View("LocationDetails", viewModel);
        }

        [HttpGet("{locationStub}/{featureStub}")]
        public async Task<IActionResult> Locations(string locationStub, string featureStub)
        {
            if (string.IsNullOrEmpty(locationStub))
            {
                return RedirectToAction(nameof(Find));
            }
            else if (string.IsNullOrEmpty(featureStub))
            {
                return RedirectToAction(nameof(Locations), new
                {
                    locationStub,
                    featureStub = string.Empty
                });
            }

            var locationFeature
                = await _locationService.GetLocationFullFeatureAsync(locationStub, featureStub);

            if (locationFeature?.Feature != null)
            {
                var location = await _locationService.GetLocationByStubAsync(locationStub);
                PageTitle = _localizer[i18n.Keys.Promenade.LocationFeatureAt,
                    locationFeature.Feature.Name,
                    location.Name];

                return View("LocationFeatureDetails", new LocationDetailViewModel
                {
                    CanonicalUrl = await GetCanonicalUrlAsync(),
                    LocationFeatures = new List<LocationsFeaturesViewModel>
                    {
                        new LocationsFeaturesViewModel(locationFeature)
                    },
                    Location = location
                });
            }
            else
            {
                return NotFound();
            }
        }

        private async Task<LocationViewModel> CreateLocationViewModelAsync()
        {
            return await CreateLocationViewModelAsync(null, null);
        }

        private async Task<LocationViewModel> CreateLocationViewModelAsync(double? latitude,
            double? longitude)
        {
            return new LocationViewModel
            {
                Locations = await _locationService.GetLocationsStatusAsync(latitude, longitude),
                CanSearchAddress = !string.IsNullOrWhiteSpace(_apiKey),
                Latitude = latitude,
                Longitude = longitude
            };
        }

        private async Task<IActionResult> FindAsync(string zip)
        {
            LocationViewModel viewModel = null;

            if (!string.IsNullOrWhiteSpace(_apiKey) && !string.IsNullOrEmpty(zip))
            {
                var (latitude, longitude) = await _locationService.GeocodeAddressAsync(zip);

                if (longitude.HasValue && latitude.HasValue)
                {
                    viewModel = await CreateLocationViewModelAsync(latitude, longitude);
                    viewModel.Zip = zip.Trim();
                }
                else
                {
                    viewModel = await CreateLocationViewModelAsync();
                }
            }

            return await ShowNearestAsync(viewModel);
        }

        private async Task<IActionResult> FindAsync(double? latitude, double? longitude)
        {
            LocationViewModel viewModel = null;

            if (!string.IsNullOrWhiteSpace(_apiKey) && latitude.HasValue && longitude.HasValue)
            {
                viewModel = await CreateLocationViewModelAsync(latitude.Value, longitude.Value);

                viewModel.Zip = await _locationService
                    .GetZipCodeAsync(latitude.Value, longitude.Value);
            }

            return await ShowNearestAsync(viewModel);
        }

        private async Task<IActionResult> ShowNearestAsync(LocationViewModel viewModel)
        {
            if (viewModel == null)
            {
                viewModel = await CreateLocationViewModelAsync();
            }

            PageTitle = _localizer[i18n.Keys.Promenade.LocationFind];

            if (!string.IsNullOrWhiteSpace(viewModel.Zip))
            {
                viewModel.Info = _localizer[i18n.Keys.Promenade.ZipCodeClosest,
                    viewModel.Zip.Trim()];
            }

            return View("Locations", viewModel);
        }
    }
}