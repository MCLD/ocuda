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
using Ocuda.Utility;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Promenade.Controllers
{
    [Route("")]
    [Route("{culture:cultureConstraint?}")]
    public class HomeController : GeneralBasePageController<HomeController>
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly LocationService _locationService;
        private readonly PageService _pageService;
        private readonly IPathResolverService _pathResolverService;
        private readonly VolunteerFormService _volunteerFormService;

        public HomeController(IDateTimeProvider dateTimeProvider,
           IPathResolverService pathResolverService,
           LocationService locationService,
           PageService pageService,
           ServiceFacades.Controller<HomeController> context,
           ServiceFacades.PageController pageContext,
           VolunteerFormService volunteerFormService)
            : base(context, pageContext)
        {
            ArgumentNullException.ThrowIfNull(dateTimeProvider);
            ArgumentNullException.ThrowIfNull(locationService);
            ArgumentNullException.ThrowIfNull(pageService);
            ArgumentNullException.ThrowIfNull(pathResolverService);
            ArgumentNullException.ThrowIfNull(volunteerFormService);

            _dateTimeProvider = dateTimeProvider;
            _locationService = locationService;
            _pageService = pageService;
            _pathResolverService = pathResolverService;
            _volunteerFormService = volunteerFormService;
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

            var location = await _locationService.GetLocationAsync(locationId.Value, forceReload);

            if (Location == null)
            {
                return NotFound();
            }

            var builder = BaseUriBuilder;
            builder.Path = _pathResolverService.GetPublicContentLink(UriPaths.Images,
                UriPaths.Locations,
                location.ImagePath);
            location.ImagePath = builder.Uri.LocalPath;
            builder.Path = _pathResolverService.GetPublicContentLink(UriPaths.Images,
                UriPaths.Locations,
                UriPaths.Maps,
                location.MapImagePath);
            location.MapImagePath = builder.Uri.LocalPath;

            if (location.InteriorImages?.Count > 0)
            {
                foreach (var interiorImage in location.InteriorImages)
                {
                    builder.Path = _pathResolverService.GetPublicContentLink(UriPaths.Images,
                        UriPaths.Locations,
                        UriPaths.Interior,
                        interiorImage.ImagePath);
                    interiorImage.ImagePath = builder.Uri.LocalPath;
                    builder.Path = _pathResolverService.GetPublicContentLink(UriPaths.Images,
                        UriPaths.Locations,
                        UriPaths.Maps,
                        location.MapImagePath);
                    location.MapImagePath = builder.Uri.LocalPath;
                }
            }

            var viewModel = new LocationDetailViewModel
            {
                DayOfWeek = _dateTimeProvider.Now.DayOfWeek,
                CanonicalLink = await GetCanonicalLinkAsync(),
                Location = location,
                SocialLink = location.Facebook ?? await _siteSettingService
                    .GetSettingStringAsync(Models.Keys.SiteSetting.Social.FacebookUrl, forceReload),
                SeeServicesAtAllLink = await _siteSettingService
                    .GetSettingStringAsync(Models.Keys.SiteSetting.Site.ServicesAtAllLink)
            };

            // TODO: fix so that social link is selectable
            if (!string.IsNullOrEmpty(viewModel.SocialLink))
            {
                viewModel.SocialIcon = "fa-brands fa-facebook";
                viewModel.SocialName = nameof(location.Facebook);
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
                = await _locationService.GetHoursAsync(viewModel.Location.Id, forceReload);

            var locationFeatures = await _locationService
                .GetFullLocationFeaturesAsync(locationId.Value, null, forceReload);

            viewModel.LocationFeatures = locationFeatures
                .Select(_ => new LocationsFeaturesViewModel(_));

            viewModel.AtThisLocation = viewModel.LocationFeatures.Where(_ => _.IsAtThisLocation);
            viewModel.ServicesAvailable = viewModel.LocationFeatures.Where(_ => !_.IsAtThisLocation);

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

            viewModel.Schema = await GetSchemaAsync(viewModel.Location);

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

        private async Task<Schema.NET.Thing> GetSchemaAsync(Location location)
        {
            var locationUri = new Uri(await GetCanonicalLinkAsync());
            Schema.NET.LocalBusiness schema = null;

            if (string.Equals(location.Type, nameof(Schema.NET.Library),
                StringComparison.OrdinalIgnoreCase))
            {
                schema = new Schema.NET.Library();
            }

            schema ??= new Schema.NET.LocalBusiness();

            if (string.Equals(location.AddressType, nameof(Schema.NET.PostalAddress),
                StringComparison.OrdinalIgnoreCase))

            {
                schema.Address = new Schema.NET.PostalAddress
                {
                    AddressCountry = location.Country,
                    AddressLocality = location.City,
                    AddressRegion = location.State,
                    ContactType = location.ContactType,
                    Id = locationUri,
                    PostalCode = location.Zip,
                    StreetAddress = location.Address
                };
            }
            if (string.Equals(location.AreaServedType, nameof(Schema.NET.AdministrativeArea),
                StringComparison.OrdinalIgnoreCase))
            {
                schema.AreaServed = new Schema.NET.AdministrativeArea
                {
                    Name = location.AreaServedName
                };
            }
            schema.BranchCode = location.Stub;
            schema.Email = location.Email;
            schema.HasMap = new Uri(location.MapLink);
            schema.IsAccessibleForFree = location.IsAccessibleForFree;
            schema.Location = new Schema.NET.Place
            {
                Id = locationUri
            };
            schema.Name = location.Name;
            schema.ParentOrganization = new Schema.NET.Organization
            {
                Name = location.ParentOrganization,
                Url = new Uri(location.ParentOrganization),
            };
            schema.Telephone = location.Phone;
            schema.Url = locationUri;

            if (location.IsAccessibleForFree)
            {
                schema.PriceRange = "$0";
            }
            else if (!string.IsNullOrEmpty(location.PriceRange))
            {
                schema.PriceRange = location.PriceRange;
            }

            if (!location.IsClosed && location.LocationHours?.Count > 0)
            {
                var openingHoursList = new List<Schema.NET.IOpeningHoursSpecification>();
                var locationHoursOpen = location.LocationHours
                        .Where(_ => _.Open != default && _.Close != default);

                foreach (var locationHour in locationHoursOpen)
                {
                    var openingHours = new Schema.NET.OpeningHoursSpecification();
                    var daysOfweek = new List<Schema.NET.DayOfWeek?>();
                    foreach (var dayOfWeek in locationHour.DaysOfWeek)
                    {
                        if (Enum.TryParse(dayOfWeek.ToString(),
                            out Schema.NET.DayOfWeek parsedDayOfWeek))
                        {
                            daysOfweek.Add(parsedDayOfWeek);
                        }
                    }
                    if (daysOfweek.Count > 0)
                    {
                        openingHoursList.Add(new Schema.NET.OpeningHoursSpecification
                        {
                            DayOfWeek = daysOfweek,
                            Opens = locationHour.Open,
                            Closes = locationHour.Close
                        });
                    }
                }
                schema.OpeningHoursSpecification = openingHoursList;
            }

            if (location.ImagePath != null)
            {
                var builder = BaseUriBuilder;
                builder.Path = location.ImagePath;
                schema.Image = builder.Uri;
                schema.Photo = new Schema.NET.Photograph
                {
                    Url = builder.Uri
                };
            }

            var sameAs = new List<Uri>
            {
                new Uri(location.MapLink)
            };

            if (!string.IsNullOrEmpty(location.Facebook))
            {
                sameAs.Add(new Uri(location.Facebook));
            }

            schema.SameAs = sameAs;

            return schema;
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
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
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