using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Controllers.ViewModels.Locations;
using Ocuda.Promenade.Service;
using Ocuda.Utility;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Promenade.Controllers
{
    [Route("[Controller]")]
    [Route("{culture:cultureConstraint?}/[Controller]")]
    public class LocationsController : BaseController<LocationsController>
    {
        private readonly string _apiKey;
        private readonly LocationService _locationService;
        private readonly IPathResolverService _pathResolverService;

        public LocationsController(ServiceFacades.Controller<LocationsController> context,
            IPathResolverService pathResolverService,
            LocationService locationService) : base(context)
        {
            ArgumentNullException.ThrowIfNull(pathResolverService);
            ArgumentNullException.ThrowIfNull(locationService);

            _pathResolverService = pathResolverService;
            _locationService = locationService;

            _apiKey = _config[Utility.Keys.Configuration.OcudaGoogleAPI];
        }

        public static string Name
        { get { return "Locations"; } }

        public IFormatProvider CurrentCulture { get; }

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
            viewModel.WarningText = issue;
            return await ShowNearestAsync(viewModel);
        }

        private async Task<LocationViewModel> CreateLocationViewModelAsync()
        {
            return await CreateLocationViewModelAsync(null, null);
        }

        private async Task<LocationViewModel> CreateLocationViewModelAsync(double? latitude,
            double? longitude)
        {
            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

            var locations = await _locationService
                    .GetLocationsStatusAsync(latitude, longitude, forceReload);

            var builder = BaseUriBuilder;
            foreach (var location in locations)
            {
                builder.Path = _pathResolverService.GetPublicContentLink(UriPaths.Images,
                    UriPaths.Locations,
                    location?.ImagePath);
                location.ImagePath = builder.Uri.LocalPath;
                builder.Path = _pathResolverService.GetPublicContentLink(UriPaths.Images,
                    UriPaths.Locations,
                    UriPaths.Maps,
                    location.MapImagePath);
                location.MapImagePath = builder.Uri.LocalPath;
            }

            return new LocationViewModel
            {
                Locations = locations,
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
                    viewModel.InfoText = _localizer[i18n.Keys.Promenade.ErrorCouldNotGeocode];
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
            viewModel ??= await CreateLocationViewModelAsync();

            PageTitle = _localizer[i18n.Keys.Promenade.LocationFind];

            if (!string.IsNullOrWhiteSpace(viewModel.Zip))
            {
                viewModel.InfoText = _localizer[i18n.Keys.Promenade.ZipCodeClosest,
                    viewModel.Zip.Trim()];
            }

            return View("Index", viewModel);
        }
    }
}