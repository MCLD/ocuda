using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Controllers.ViewModels.Locations;
using Ocuda.Promenade.Controllers.ViewModels.Locations.Volunteer;
using Ocuda.Promenade.Models.Entities;
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
        private readonly VolunteerFormService _volunteerFormService;

        public LocationsController(ServiceFacades.Controller<LocationsController> context,
            LocationService locationService,
            VolunteerFormService volunteerFormService,
            SegmentService segmentService) : base(context)
        {
            _locationService = locationService
                ?? throw new ArgumentNullException(nameof(locationService));
            _volunteerFormService = volunteerFormService
                ?? throw new ArgumentNullException(nameof(volunteerFormService));
            _segmentService = segmentService
                ?? throw new ArgumentNullException(nameof(segmentService));

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
            viewModel.Warning = issue;
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

            return new LocationViewModel
            {
                Locations = await _locationService
                    .GetLocationsStatusAsync(latitude, longitude, forceReload),
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
            viewModel ??= await CreateLocationViewModelAsync();

            PageTitle = _localizer[i18n.Keys.Promenade.LocationFind];

            if (!string.IsNullOrWhiteSpace(viewModel.Zip))
            {
                viewModel.Info = _localizer[i18n.Keys.Promenade.ZipCodeClosest,
                    viewModel.Zip.Trim()];
            }

            return View("Index", viewModel);
        }


        [Route("{locationStub}/volunteer")]
        [Route("{locationStub}/volunteer/adult")]
        public async Task<IActionResult> AdultVolunteerForm(string locationStub)
        {
            var location = await _locationService.GetLocationByStubAsync(locationStub);
            if (location == null)
            {
                return RedirectToAction(nameof(Find));
            }

            try
            {
                var adultVolunteerForm = await _volunteerFormService.FindVolunteerFormAsync(VolunteerFormType.Adult);
                var teenVolunteerForm = await _volunteerFormService.FindVolunteerFormAsync(VolunteerFormType.Teen);

                if ((adultVolunteerForm?.IsDisabled != false)
                    && (teenVolunteerForm?.IsDisabled != false))
                {
                    return RedirectToAction(nameof(Locations), new { locationStub });
                }

                LocationForm adultLocationMapping = null;
                LocationForm teenLocationMapping = null;

                if (adultVolunteerForm != null)
                {
                    adultLocationMapping
                        = await _volunteerFormService.FindLocationFormAsync(adultVolunteerForm.Id, location.Id);
                }
                if (teenVolunteerForm != null)
                {
                    teenLocationMapping = await _volunteerFormService.FindLocationFormAsync(teenVolunteerForm.Id, location.Id);
                }

                if (adultLocationMapping == null)
                {
                    if (teenLocationMapping == null)
                    {
                        return RedirectToAction(nameof(Locations), new { locationStub });
                    }
                    else
                    {
                        return RedirectToAction(nameof(TeenVolunteerForm), new { locationStub });
                    }
                }

                var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;
                var viewModel = new AdultVolunteerFormViewModel
                {
                    LocationStub = locationStub,
                    LocationId = location.Id,
                    TeenFormAvailable = teenLocationMapping != null,
                    FormId = adultVolunteerForm.Id
                };

                if (adultVolunteerForm.HeaderSegmentId.HasValue)
                {
                    viewModel.HeaderSegment = await _segmentService.GetSegmentTextBySegmentIdAsync(adultVolunteerForm.HeaderSegmentId.Value, forceReload);
                }

                if (viewModel.HeaderSegment != null)
                {
                    viewModel.HeaderSegment.Text = CommonMarkConverter.Convert(viewModel.HeaderSegment.Text);
                }
                return View(viewModel);
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Locations), new { locationStub = location.Stub });
            }
        }

        [HttpPost]
        [Route("{locationStub}/volunteer/adult")]
        public async Task<IActionResult> AdultVolunteerForm(AdultVolunteerFormViewModel viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _volunteerFormService.SaveSubmission(viewModel.ToFormSubmission());
                }
                catch (Exception ex)
                {
                    viewModel.WarningText = _localizer[i18n.Keys.Promenade.Error];
                    return View(viewModel);
                }
            }

            return RedirectToAction(nameof(FormSubmissionSuccess), new { locationStub = viewModel.LocationStub, volunteerType = VolunteerFormType.Adult });
        }


        [HttpPost]
        [Route("{locationStub}/volunteer/teen")]
        public async Task<IActionResult> TeenVolunteerForm(TeenVolunteerFormViewModel viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            if (string.IsNullOrWhiteSpace(viewModel.GuardianName))
            {
                ModelState.AddModelError(nameof(viewModel.GuardianName), "Please include the name of your parent or guardian");
            }
            if (string.IsNullOrWhiteSpace(viewModel.GuardianPhone))
            {
                ModelState.AddModelError(nameof(viewModel.GuardianPhone), "Please include the phone number of your parent or guardian");
            }
            if (string.IsNullOrWhiteSpace(viewModel.GuardianEmail))
            {
                ModelState.AddModelError(nameof(viewModel.GuardianEmail), "Please include the email of your parent or guardian");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _volunteerFormService.SaveSubmission(viewModel.ToFormSubmission());
                }
                catch (Exception)
                {
                    viewModel.WarningText = _localizer[i18n.Keys.Promenade.Error];
                    return View(viewModel);
                }
            }

            return RedirectToAction(nameof(FormSubmissionSuccess), new { locationStub = viewModel.LocationStub, volunteerType = VolunteerFormType.Teen });
        }

        [Route("{locationStub}/volunteer/teen")]
        public async Task<IActionResult> TeenVolunteerForm(string locationStub)
        {
            var location = await _locationService.GetLocationByStubAsync(locationStub);
            if (location == null)
            {
                return RedirectToAction(nameof(Find));
            }

            try
            {
                var teenVolunteerForm = await _volunteerFormService.FindVolunteerFormAsync(VolunteerFormType.Teen);
                var adultVolunteerForm = await _volunteerFormService.FindVolunteerFormAsync(VolunteerFormType.Adult);

                if (teenVolunteerForm?.IsDisabled != false
                    && adultVolunteerForm?.IsDisabled != false)
                {
                    // Alert: volunteers are not being accepted atm
                    return RedirectToAction(nameof(Locations), locationStub);
                }

                LocationForm teenLocationMapping = null;
                LocationForm adultLocationMapping = null;

                if (teenVolunteerForm != null)
                {
                    teenLocationMapping = await _volunteerFormService.FindLocationFormAsync(teenVolunteerForm.Id, location.Id);
                }
                if (adultVolunteerForm != null)
                {
                    adultLocationMapping
                        = await _volunteerFormService.FindLocationFormAsync(adultVolunteerForm.Id, location.Id);
                }

                if (teenLocationMapping == null)
                {
                    if (adultLocationMapping == null)
                    {
                        return RedirectToAction(nameof(Locations), new { locationStub });
                    }
                    else
                    {
                        return RedirectToAction(nameof(AdultVolunteerForm), new { locationStub });
                    }
                }

                var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;
                var viewModel = new TeenVolunteerFormViewModel
                {
                    LocationStub = locationStub,
                    LocationId = location.Id,
                    AdultFormAvailable = adultLocationMapping != null,
                    FormId = teenVolunteerForm.Id
                };

                if (teenVolunteerForm.HeaderSegmentId.HasValue)
                {
                    viewModel.HeaderSegment = await _segmentService.GetSegmentTextBySegmentIdAsync(teenVolunteerForm.HeaderSegmentId.Value, forceReload);
                }

                if (viewModel.HeaderSegment != null)
                {
                    viewModel.HeaderSegment.Text = CommonMarkConverter.Convert(viewModel.HeaderSegment.Text);
                }
                return View(viewModel);
            }
            catch (Exception)
            {
                return RedirectToAction(nameof(Locations), new { locationStub = location.Stub });
            }
        }
    }
}