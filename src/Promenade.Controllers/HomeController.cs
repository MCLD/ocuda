using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommonMark;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Promenade.Controllers.Abstract;
using Ocuda.Promenade.Controllers.ViewModels.Home;
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
        private readonly VolunteerFormService _volunteerFormService;

        public HomeController(IDateTimeProvider dateTimeProvider,
           LocationService locationService,
           ServiceFacades.Controller<HomeController> context,
           ServiceFacades.PageController pageContext,
           VolunteerFormService volunteerFormService)
            : base(context, pageContext)
        {
            _dateTimeProvider = dateTimeProvider
                ?? throw new ArgumentNullException(nameof(dateTimeProvider));
            _locationService = locationService
                ?? throw new ArgumentNullException(nameof(locationService));
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











        [HttpGet("{locationSlug:locationSlugConstraint}/volunteer")]
        [HttpGet("{locationSlug:locationSlugConstraint}/volunteer/adult")]
        public async Task<IActionResult> VolunteerAdult(string locationSlug)
        {
            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

            var locationId = await _locationService.GetLocationIdAsync(locationSlug, forceReload);

            if (!locationId.HasValue)
            {
                return NotFound();
            }

            var adultVolunteerForm = await _volunteerFormService.FindVolunteerFormAsync(VolunteerFormType.Adult, forceReload);
            var teenVolunteerForm = await _volunteerFormService.FindVolunteerFormAsync(VolunteerFormType.Teen, forceReload);

            if ((adultVolunteerForm?.IsDisabled != false)
                && (teenVolunteerForm?.IsDisabled != false))
            {
                return RedirectToAction(nameof(Location), new { locationSlug });
            }

            LocationForm adultLocationMapping = null;
            LocationForm teenLocationMapping = null;

            if (adultVolunteerForm != null)
            {
                adultLocationMapping
                    = await _volunteerFormService.FindLocationFormAsync(adultVolunteerForm.Id, locationId.Value);
            }
            if (teenVolunteerForm != null)
            {
                teenLocationMapping = await _volunteerFormService.FindLocationFormAsync(teenVolunteerForm.Id, locationId.Value);
            }

            if (adultLocationMapping == null)
            {
                if (teenLocationMapping == null)
                {
                    return RedirectToAction(nameof(Location), new { locationSlug });
                }
                else
                {
                    return RedirectToAction(nameof(VolunteerTeen), new { locationSlug });
                }
            }

            var viewModel = new AdultVolunteerFormViewModel
            {
                LocationSlug = locationSlug,
                LocationId = locationId.Value,
                TeenFormAvailable = teenLocationMapping != null,
                FormId = adultVolunteerForm.Id
            };

            if (adultVolunteerForm.HeaderSegmentId.HasValue)
            {
                viewModel.SegmentHeader
                    = adultVolunteerForm.HeaderSegment.Header;
                viewModel.SegmentText
                    = FormatForDisplay(adultVolunteerForm.HeaderSegment);
            }

            return View(viewModel);
        }


        [HttpPost("{locationSlug:locationSlugConstraint}/volunteer/adult")]
        public async Task<IActionResult> VolunteerAdult(AdultVolunteerFormViewModel viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _volunteerFormService.SaveSubmissionAsync(viewModel.ToFormSubmission());
                }
                catch (Exception ex)
                {
                    viewModel.WarningText = _localizer[i18n.Keys.Promenade.Error];
                    return View(viewModel);
                }
            }
            return RedirectToAction(nameof(Index));
            //return RedirectToAction(nameof(FormSubmissionSuccess), new { locationSlug = viewModel.LocationSlug, volunteerType = VolunteerFormType.Adult });
        }


        [HttpPost("{locationSlug:locationSlugConstraint}/volunteer/teen")]
        public async Task<IActionResult> VolunteerTeen(TeenVolunteerFormViewModel viewModel)
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
                    await _volunteerFormService.SaveSubmissionAsync(viewModel.ToFormSubmission());
                }
                catch (Exception)
                {
                    viewModel.WarningText = _localizer[i18n.Keys.Promenade.Error];
                    return View(viewModel);
                }
            }

            return RedirectToAction(nameof(Index));
            //return RedirectToAction(nameof(FormSubmissionSuccess), new { locationSlug = viewModel.LocationSlug, volunteerType = VolunteerFormType.Teen });
        }

        [HttpGet("{locationSlug:locationSlugConstraint}/volunteer/teen")]
        public async Task<IActionResult> VolunteerTeen(string locationSlug)
        {
            var forceReload = HttpContext.Items[ItemKey.ForceReload] as bool? ?? false;

            var locationId = await _locationService.GetLocationIdAsync(locationSlug, forceReload);

            if (!locationId.HasValue)
            {
                return NotFound();
            }

            try
            {
                var teenVolunteerForm = await _volunteerFormService.FindVolunteerFormAsync(VolunteerFormType.Teen, forceReload);
                var adultVolunteerForm = await _volunteerFormService.FindVolunteerFormAsync(VolunteerFormType.Adult, forceReload);

                if (teenVolunteerForm?.IsDisabled != false
                    && adultVolunteerForm?.IsDisabled != false)
                {
                    // Alert: volunteers are not being accepted atm
                    return RedirectToAction(nameof(Location), new { locationSlug });
                }

                LocationForm teenLocationMapping = null;
                LocationForm adultLocationMapping = null;

                if (teenVolunteerForm != null)
                {
                    teenLocationMapping = await _volunteerFormService.FindLocationFormAsync(teenVolunteerForm.Id, locationId.Value);
                }
                if (adultVolunteerForm != null)
                {
                    adultLocationMapping
                        = await _volunteerFormService.FindLocationFormAsync(adultVolunteerForm.Id, locationId.Value);
                }

                if (teenLocationMapping == null)
                {
                    if (adultLocationMapping == null)
                    {
                        return RedirectToAction(nameof(Location), new { locationSlug });
                    }
                    else
                    {
                        return RedirectToAction(nameof(VolunteerAdult), new { locationSlug });
                    }
                }

                var viewModel = new TeenVolunteerFormViewModel
                {
                    LocationSlug = locationSlug,
                    LocationId = locationId.Value,
                    AdultFormAvailable = adultLocationMapping != null,
                    FormId = teenVolunteerForm.Id
                };


                if (adultVolunteerForm.HeaderSegmentId.HasValue)
                {
                    viewModel.SegmentHeader
                        = teenVolunteerForm.HeaderSegment.Header;
                    viewModel.SegmentText
                        = FormatForDisplay(teenVolunteerForm.HeaderSegment);
                }

                return View(viewModel);
            }
            catch (Exception)
            {
                return RedirectToAction(nameof(Location), new { locationSlug });
            }
        }

    }
}