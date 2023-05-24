using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonMark;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Controllers.ViewModels.Locations;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Promenade.Controllers
{
    [Route("")]
    [Route("{culture:cultureConstraint?}")]
    public class HomeController : BasePageController<HomeController>
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly LocationService _locationService;

        public HomeController(IDateTimeProvider dateTimeProvider,
           LocationService locationService,
           ServiceFacades.Controller<HomeController> context,
           ServiceFacades.PageController pageContext)
            : base(context, pageContext)
        {
            _dateTimeProvider = dateTimeProvider
                ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _locationService = locationService
                ?? throw new ArgumentNullException(nameof(locationService));
        }

        public static string Name
        { get { return "Home"; } }

        protected override PageType PageType
        { get { return PageType.Home; } }

        [HttpGet("{locationSlug:locationSlugConstraint}/[action]/{featureSlug}")]
        public async Task<IActionResult> Feature(string locationSlug, string featureSlug)
        {
            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

            LocationFeature locationFeature = null;
            try
            {
                locationFeature = await GetFeatureDetailsAsync(locationSlug, featureSlug, forceReload);
            }
            catch (OcudaException ex)
            {
                if (ex.Data.Contains(nameof(RedirectToAction)))
                {
                    return ex.Data[nameof(RedirectToAction)] as IActionResult;
                }
            }

            if (locationFeature?.Feature != null)
            {
                var location = await _locationService
                    .GetLocationAsync(locationFeature.LocationId, forceReload);
                PageTitle = _localizer[i18n.Keys.Promenade.LocationFeatureAt,
                    locationFeature.Feature.Name,
                    location.Name];

                return View("Feature", new LocationDetailViewModel
                {
                    CanonicalLink = await GetCanonicalLinkAsync(),
                    DayOfWeek = _dateTimeProvider.Now.DayOfWeek,
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

        [HttpGet("{locationSlug:locationSlugConstraint}/[action]/{featureSlug}")]
        public async Task<IActionResult> FeatureDetails(string locationSlug, string featureSlug)
        {
            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

            LocationFeature locationFeature = null;
            try
            {
                locationFeature = await GetFeatureDetailsAsync(locationSlug,
                    featureSlug,
                    forceReload);
            }
            catch (OcudaException ex)
            {
                if (ex.Data.Contains(nameof(RedirectToAction)))
                {
                    return ex.Data[nameof(RedirectToAction)] as IActionResult;
                }
            }

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
                return NotFound();
            }
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            return await ReturnPageAsync(nameof(Index));
        }

        [HttpGet("{locationSlug:locationSlugConstraint}")]
        public async Task<IActionResult> Location(string locationSlug)
        {
            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

            var locationId = await _locationService.GetLocationIdAsync(locationSlug, forceReload);

            if (locationId == null)
            {
                return NotFound();
            }

            var viewModel = new LocationDetailViewModel
            {
                DayOfWeek = _dateTimeProvider.Now.DayOfWeek,
                CanonicalLink = await GetCanonicalLinkAsync(),
                Location = await _locationService.GetLocationAsync(locationId.Value, forceReload)
            };

            if (viewModel.Location == null)
            {
                return NotFound();
            }

            if (viewModel.Location.HoursSegmentText != null)
            {
                viewModel.HoursSegmentText = FormatForDisplay(viewModel.Location.HoursSegmentText);
            }

            if (viewModel.Location.PreFeatureSegmentText != null)
            {
                viewModel.PreFeatureSegmentHeader
                    = viewModel.Location.PreFeatureSegmentText.Header;
                viewModel.PreFeatureSegmentText
                    = FormatForDisplay(viewModel.Location.PreFeatureSegmentText);
            }

            if (viewModel.Location.PostFeatureSegmentText != null)
            {
                viewModel.PostFeatureSegmentHeader
                    = viewModel.Location.PostFeatureSegmentText.Header;
                viewModel.PostFeatureSegmentText
                    = FormatForDisplay(viewModel.Location.PostFeatureSegmentText);
            }

            if (viewModel.Location.DescriptionSegment != null)
            {
                viewModel.DescriptionSegmentText
                    = FormatForDisplay(viewModel.Location.DescriptionSegment);
            }

            viewModel.Location.LocationHours
                = await _locationService.GetHoursAsync(viewModel.Location.Id, forceReload, false);

            if (viewModel.Location.LocationHours != null)
            {
                var hours = await _locationService
                    .GetHoursAsync(viewModel.Location.Id, forceReload, true);

                ((List<string>)viewModel.StructuredLocationHours).AddRange(hours.ToList()
                    .ConvertAll(_ => $"{_.Days} {_.Time}"));
            }

            var locationFeatures = await _locationService
                .GetFullLocationFeaturesAsync(locationId.Value, forceReload);

            viewModel.LocationFeatures = locationFeatures
                .Select(_ => new LocationsFeaturesViewModel(_));

            if (viewModel.Location.DisplayGroupId.HasValue)
            {
                viewModel.LocationNeighborGroup = await _locationService
                    .GetLocationsNeighborGroup(viewModel.Location.DisplayGroupId.Value,
                        forceReload);

                var neighbors = await _locationService
                    .GetLocationsNeighborsAsync(viewModel.Location.DisplayGroupId.Value,
                        forceReload);

                if (neighbors?.Count > 0)
                {
                    ((List<LocationGroup>)viewModel.NearbyLocationGroups).AddRange(neighbors);
                    viewModel.NearbyCount = viewModel.NearbyLocationGroups.Count;
                    viewModel.NearbyEventsCount = viewModel.NearbyLocationGroups
                        .Count(_ => _.Location.HasEvents);
                }
            }

            PageTitle = viewModel.Location.Name;

            return View(nameof(Location), viewModel);
        }

        private async Task<LocationFeature> GetFeatureDetailsAsync(string locationSlug,
            string featureSlug,
            bool forceReload)
        {
            if (string.IsNullOrEmpty(locationSlug))
            {
                var e = new OcudaException("Missing location");
                e.Data.Add(nameof(RedirectToAction), RedirectToAction(nameof(Index)));
                throw e;
            }
            else if (string.IsNullOrEmpty(featureSlug))
            {
                var e = new OcudaException("Missing location");
                e.Data.Add(nameof(RedirectToAction), RedirectToAction(nameof(Location),
                    new { locationSlug }));
                throw e;
            }

            var locationId = await _locationService.GetLocationIdAsync(locationSlug, forceReload);

            if (!locationId.HasValue)
            {
                var e = new OcudaException("Missing location");
                e.Data.Add(nameof(RedirectToAction), RedirectToAction(nameof(Index)));
                throw e;
            }

            return await _locationService
                .GetLocationFullFeatureAsync(locationId.Value, featureSlug, forceReload);
        }
    }
}