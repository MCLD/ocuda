using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonMark;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Controllers.ViewModels.Home;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Promenade.Service;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Exceptions;

namespace Ocuda.Promenade.Controllers
{
    [Route("")]
    [Route("{culture:cultureConstraint?}")]
    public class HomeController : GeneralBasePageController<HomeController>
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly LocationService _locationService;
        private readonly PageService _pageService;
        private readonly VolunteerFormService _volunteerFormService;

        public HomeController(IDateTimeProvider dateTimeProvider,
           LocationService locationService,
           PageService pageService,
           ServiceFacades.Controller<HomeController> context,
           ServiceFacades.PageController pageContext,
           VolunteerFormService volunteerFormService)
            : base(context, pageContext)
        {
            _dateTimeProvider = dateTimeProvider
                ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _locationService = locationService
                ?? throw new ArgumentNullException(nameof(locationService));
            _pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
            _volunteerFormService = volunteerFormService
                ?? throw new ArgumentNullException(nameof(volunteerFormService));
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

            if (HasAlertInfo)
            {
                viewModel.ShowMessage = AlertInfo;
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

        #region Volunteer form handling

        [HttpGet("{locationSlug:locationSlugConstraint}/[action]")]
        public async Task<IActionResult> Volunteer(string locationSlug)
        {
            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

            var locationId = await _locationService.GetLocationIdAsync(locationSlug, forceReload);

            if (!locationId.HasValue)
            {
                return NotFound();
            }

            var adultLocationMapping = await _volunteerFormService
                .FindLocationFormAsync(VolunteerFormType.Adult, locationId.Value, forceReload);

            if (adultLocationMapping != null)
            {
                return RedirectToAction(nameof(VolunteerAdult), new { locationSlug });
            }

            var teenVolunteerForm = await _volunteerFormService
                .FindLocationFormAsync(VolunteerFormType.Teen, locationId.Value, forceReload);

            if (teenVolunteerForm != null)
            {
                return RedirectToAction(nameof(VolunteerTeen), new { locationSlug });
            }

            SetAlertInfo(i18n.Keys.Promenade.ErrorVolunteerNotAccepting);
            return RedirectToAction(nameof(Location), new { locationSlug });
        }

        [HttpGet("{locationSlug:locationSlugConstraint}/volunteer/adult")]
        public async Task<IActionResult> VolunteerAdult(string locationSlug)
        {
            try
            {
                var viewModel = await PopulateAdultVolunteerViewModelAsync(locationSlug, null);
                PageTitle = _localizer[i18n.Keys.Promenade.VolunteerPageTitle];
                return View("AdultVolunteerForm", viewModel);
            }
            catch (OcudaException oex)
            {
                if (string.IsNullOrEmpty(oex.Message))
                {
                    return NotFound();
                }
                else
                {
                    SetAlertInfo(oex.Message);
                    return RedirectToAction(nameof(Location), new { locationSlug });
                }
            }
        }

        [HttpPost("{locationSlug:locationSlugConstraint}/volunteer/adult")]
        public async Task<IActionResult> VolunteerAdult(string locationSlug,
            AdultVolunteerFormViewModel viewModel)
        {
            if (viewModel == null)
            {
                return RedirectToAction(nameof(Location), new { locationSlug });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _volunteerFormService.SaveSubmissionAsync(viewModel.ToFormSubmission());
                }
                catch (OcudaException oex)
                {
                    _logger.LogError(oex,
                        "Error saving adult volunteer form: {ErrorMessage}",
                        oex.Message);
                    viewModel.WarningText = _localizer[i18n.Keys.Promenade.ErrorCouldNotSubmit];
                    var returnViewModel = await PopulateAdultVolunteerViewModelAsync(locationSlug,
                        viewModel);
                    return View("AdultVolunteerForm", returnViewModel);
                }
            }

            return await ReturnThanks(locationSlug, VolunteerFormType.Adult);
        }

        [HttpPost("{locationSlug:locationSlugConstraint}/volunteer/teen")]
        public async Task<IActionResult> VolunteerTeen(string locationSlug,
            TeenVolunteerFormViewModel viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            if (string.IsNullOrWhiteSpace(viewModel.GuardianName))
            {
                ModelState.AddModelError(nameof(viewModel.GuardianName),
                    _localizer[i18n.Keys.Promenade.RequiredFieldItem,
                        i18n.Keys.Promenade.PromptGuardianName]);
            }
            if (string.IsNullOrWhiteSpace(viewModel.GuardianPhone))
            {
                ModelState.AddModelError(nameof(viewModel.GuardianPhone),
                    _localizer[i18n.Keys.Promenade.RequiredFieldItem,
                        i18n.Keys.Promenade.PromptGuardianPhone]);
            }
            if (string.IsNullOrWhiteSpace(viewModel.GuardianEmail))
            {
                ModelState.AddModelError(nameof(viewModel.GuardianEmail),
                    _localizer[i18n.Keys.Promenade.RequiredFieldItem,
                        i18n.Keys.Promenade.PromptGuardianEmail]);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _volunteerFormService.SaveSubmissionAsync(viewModel.ToFormSubmission());
                }
                catch (OcudaException oex)
                {
                    _logger.LogError(oex,
                        "Error saving teen volunteer form: {ErrorMessage}",
                        oex.Message);
                    viewModel.WarningText = _localizer[i18n.Keys.Promenade.ErrorCouldNotSubmit];
                    PageTitle = _localizer[i18n.Keys.Promenade.VolunteerPageTitle];
                    var returnViewModel = await PopulateTeenVolunteerViewModelAsync(locationSlug,
                        viewModel);
                    return View("TeenVolunteerForm", returnViewModel);
                }
            }

            return await ReturnThanks(locationSlug, VolunteerFormType.Teen);
        }

        [HttpGet("{locationSlug:locationSlugConstraint}/volunteer/teen")]
        public async Task<IActionResult> VolunteerTeen(string locationSlug)
        {
            try
            {
                var viewModel = await PopulateTeenVolunteerViewModelAsync(locationSlug, null);
                PageTitle = _localizer[i18n.Keys.Promenade.VolunteerPageTitle];
                return View("TeenVolunteerForm", viewModel);
            }
            catch (OcudaException oex)
            {
                if (string.IsNullOrEmpty(oex.Message))
                {
                    return NotFound();
                }
                else
                {
                    SetAlertInfo(oex.Message);
                    return RedirectToAction(nameof(Location), new { locationSlug });
                }
            }
        }

        private async Task<AdultVolunteerFormViewModel> PopulateAdultVolunteerViewModelAsync(
            string locationSlug,
            AdultVolunteerFormViewModel viewModel)
        {
            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

            var locationId = await _locationService.GetLocationIdAsync(locationSlug, forceReload);

            if (!locationId.HasValue)
            {
                throw new OcudaException();
            }

            var adultLocationMapping = await _volunteerFormService
                .FindLocationFormAsync(VolunteerFormType.Adult, locationId.Value, forceReload)
                ?? throw new OcudaException(i18n.Keys.Promenade.ErrorVolunteerNotAcceptingAdult);

            var teenLocationMapping = await _volunteerFormService
                .FindLocationFormAsync(VolunteerFormType.Teen, locationId.Value, forceReload);

            viewModel ??= new AdultVolunteerFormViewModel();

            viewModel.LocationSlug = locationSlug;
            viewModel.LocationId = locationId.Value;
            viewModel.TeenFormAvailable = teenLocationMapping != null;
            viewModel.FormId = adultLocationMapping.Form.Id;

            if (adultLocationMapping.Form.HeaderSegmentId.HasValue)
            {
                viewModel.SegmentHeader
                    = adultLocationMapping.Form.HeaderSegment.Header;
                viewModel.SegmentText
                    = FormatForDisplay(adultLocationMapping.Form.HeaderSegment);
            }

            return viewModel;
        }

        private async Task<TeenVolunteerFormViewModel> PopulateTeenVolunteerViewModelAsync(
                    string locationSlug,
            TeenVolunteerFormViewModel viewModel)
        {
            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

            var locationId = await _locationService.GetLocationIdAsync(locationSlug, forceReload);

            if (!locationId.HasValue)
            {
                throw new OcudaException();
            }

            var teenLocationMapping = await _volunteerFormService
                .FindLocationFormAsync(VolunteerFormType.Teen, locationId.Value, forceReload)
                ?? throw new OcudaException(i18n.Keys.Promenade.ErrorVolunteerNotAcceptingTeen);

            var adultLocationMapping = await _volunteerFormService
                .FindLocationFormAsync(VolunteerFormType.Teen, locationId.Value, forceReload);

            viewModel ??= new TeenVolunteerFormViewModel();

            viewModel.LocationSlug = locationSlug;
            viewModel.LocationId = locationId.Value;
            viewModel.AdultFormAvailable = adultLocationMapping != null;
            viewModel.FormId = teenLocationMapping.Form.Id;

            if (teenLocationMapping.Form.HeaderSegmentId.HasValue)
            {
                viewModel.SegmentHeader
                    = teenLocationMapping.Form.HeaderSegment.Header;
                viewModel.SegmentText
                    = FormatForDisplay(teenLocationMapping.Form.HeaderSegment);
            }

            return viewModel;
        }

        private async Task<IActionResult> ReturnThanks(string locationSlug, VolunteerFormType volunteerFormType)
        {
            var locationId = await _locationService.GetLocationIdAsync(locationSlug, false);

            if (!locationId.HasValue)
            {
                return NotFound();
            }

            var locationMapping = await _volunteerFormService
                .FindLocationFormAsync(volunteerFormType, locationId.Value, false);

            if (locationMapping.Form.ThanksPageHeaderId.HasValue)
            {
                var pageStub = await _pageService
                    .GetStubByHeaderIdTypeAsync(locationMapping.Form.ThanksPageHeaderId.Value,
                        PageType.Thanks,
                        false);

                if (!string.IsNullOrEmpty(pageStub))
                {
                    return RedirectToAction(nameof(ThanksController.Page),
                        ThanksController.Name,
                        new { stub = pageStub });
                }
            }

            SetAlertInfo(Ocuda.i18n.Keys.Promenade.VolunteerThankYou);
            return RedirectToAction(nameof(Location), new { locationSlug });
        }

        #endregion Volunteer form handling
    }
}