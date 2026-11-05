using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImageOptimApi;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Location;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Controllers.ServiceFacades;
using Ocuda.Ops.Controllers.ViewModels.Locations;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Extensions;
using Ocuda.Utility.Filters;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers
{
    [Route("[controller]")]
    public class LocationsController(Controller<LocationsController> context,
        IConfiguration configuration,
        IDigitalDisplayService digitalDisplayService,
        IFeatureService featureService,
        IImageService imageService,
        ILanguageService languageService,
        ILocationFeatureService locationFeatureService,
        ILocationService locationService,
        IPermissionGroupService permissionGroupService,
        ISegmentService segmentService,
        IVolunteerFormService volunteerFormService)
        : BaseController<LocationsController>(context)
    {
        public static string Name
        {
            get { return "Locations"; }
        }

        [HttpGet("[action]/{slug}/{featureId}")]
        public async Task<IActionResult> AddDescription(string slug, int featureId)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return BadRequest();
            }

            var hasPermission = await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.LocationManagement)
                && await HasAppPermissionAsync(permissionGroupService,
                    ApplicationPermission.WebPageContentManagement);
            if (!hasPermission)
            {
                return RedirectToUnauthorized();
            }

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

            var locationFeature = await locationFeatureService
                .GetByFeatureIdLocationIdAsync(featureId, location.Id);
            if (locationFeature == null)
            {
                return NotFound();
            }

            if (locationFeature.SegmentId != null)
            {
                return RedirectToAction(nameof(SegmentsController.Detail),
                    SegmentsController.Name,
                    new
                    {
                        area = SegmentsController.Area,
                        id = locationFeature.SegmentId,
                    });
            }

            var segment = await segmentService.CreateAsync(new Segment
            {
                IsActive = true,
                Name = $"Location {location.Name} feature {feature.Name} custom text",
            });

            if (segment == null)
            {
                _logger.LogError(
                    "Unable to create segment for {LocationName} feature {FeatureName}",
                    location.Name,
                    feature.Name);
                ShowAlertDanger("Unable to create segment. Please contact an administrator.");
                return RedirectToAction(nameof(LocationFeature), new { slug, featureId });
            }

            locationFeature.SegmentId = segment.Id;
            await locationFeatureService.EditAsync(locationFeature);

            return RedirectToAction(nameof(SegmentsController.Detail),
                SegmentsController.Name,
                new
                {
                    area = Areas.SiteManagement.SegmentsController.Area,
                    id = segment.Id,
                });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> AddDescription(string stub)
        {
            var location = await locationService.GetLocationByStubAsync(stub);
            if (location != null)
            {
                if (location.DescriptionSegmentId == default)
                {
                    var segment = new Segment
                    {
                        Name = $"{location.Name} description",
                    };
                    segment = await segmentService.CreateAsync(segment);
                    location.DescriptionSegmentId = segment.Id;
                    await locationService.EditAsync(location);
                    return RedirectToAction(nameof(SegmentsController.Detail),
                        SegmentsController.Name,
                        new { area = SegmentsController.Area, id = segment.Id });
                }
                else
                {
                    ShowAlertDanger("There is already a location description segment attached to this location.");
                }

                return RedirectToAction(nameof(Details), new { slug = location.Stub });
            }
            else
            {
                ShowAlertDanger("Location not found.");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost("[action]/{slug}")]
        public async Task<IActionResult> AddFeature(string slug, int featureId)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return BadRequest();
            }

            var hasPermission = await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission)
            {
                return RedirectToUnauthorized();
            }

            var location = await locationService.GetLocationByStubAsync(slug);
            if (location == null)
            {
                return NotFound();
            }

            var locationFeature = await locationFeatureService
                .GetByFeatureIdLocationIdAsync(featureId, location.Id);

            if (locationFeature == null)
            {
                await locationFeatureService.AddLocationFeatureAsync(new LocationFeature
                {
                    FeatureId = featureId,
                    LocationId = location.Id,
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
            if (string.IsNullOrEmpty(slug))
            {
                return BadRequest();
            }

            var hasPermission = await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission)
            {
                return RedirectToUnauthorized();
            }

            var location = await locationService.GetLocationByStubAsync(slug);
            if (location == null)
            {
                return NotFound();
            }

            var features = await featureService.GetAllFeaturesAsync();

            var locationFeatures = await locationFeatureService
                .GetLocationFeaturesByLocationAsync(location.Id);

            var locationHasFeatureIds = locationFeatures.Select(_ => _.FeatureId);

            var viewModel = new AddFeatureViewModel
            {
                Location = location,
            };

            viewModel.AvailableFeatures
                .AddRange([.. features.Where(_ => !locationHasFeatureIds.Contains(_.Id))]);

            return View(viewModel);
        }

        [HttpGet("[action]/{slug}")]
        [RestoreModelState]
        public async Task<IActionResult> AddInteriorImage(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return BadRequest();
            }

            var hasPermission = await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission)
            {
                return RedirectToUnauthorized();
            }

            var location = await locationService.GetLocationByStubAsync(slug);
            if (location == null)
            {
                return NotFound();
            }

            var viewModel = new InteriorImageViewModel
            {
                CropHeight = locationService.InteriorImageHeight,
                CropWidth = locationService.InteriorImageWidth,
                LocationName = location.Name,
                Slug = location.Stub,
            };

            foreach (var languageItem in await languageService.GetActiveAsync())
            {
                viewModel.Languages.Add(languageItem.Id, languageItem.Description);
                viewModel.AltTexts.Add(languageItem.Id, string.Empty);
            }

            return View("AddInteriorImage", viewModel);
        }

        [HttpPost("[action]/{slug}")]
        [SaveModelState]
        public async Task<IActionResult> AddInteriorImage(
            InteriorImageViewModel interiorImageViewModel,
            string slug)
        {
            var hasPermission = await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission)
            {
                return RedirectToUnauthorized();
            }

            if (interiorImageViewModel == null)
            {
                return BadRequest();
            }

            if (string.IsNullOrEmpty(slug))
            {
                return BadRequest();
            }

            var location = await locationService.GetLocationByStubAsync(slug);
            if (location == null)
            {
                return NotFound();
            }

            var languages = await languageService.GetActiveAsync();

            foreach (var language in languages)
            {
                if (!interiorImageViewModel.AltTexts.TryGetValue(language.Id, out string value)
                    || string.IsNullOrWhiteSpace(value))
                {
                    ShowAlertDanger("You must supply Alt Text in all requested languages.");
                    return RedirectToAction(nameof(AddInteriorImage), new { slug });
                }
            }

            try
            {
                await locationService.UploadAddInteriorImageAsync(location.Id,
                    interiorImageViewModel.Filename,
                    interiorImageViewModel.Image,
                    interiorImageViewModel.AltTexts);
            }
            catch (OcudaException oex)
            {
                ShowAlertDanger($"An error occurred: {oex.Message}");
                return RedirectToAction(nameof(AddInteriorImage), new { slug });
            }

            return RedirectToAction(nameof(UpdateInteriorImages), new { slug });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> AddLocationNotice(string stub)
        {
            var location = await locationService.GetLocationByStubAsync(stub);
            if (location != null)
            {
                if (!location.PreFeatureSegmentId.HasValue)
                {
                    var segment = new Segment
                    {
                        IsActive = false,
                        Name = $"{location.Name} location notice",
                    };
                    segment = await segmentService.CreateAsync(segment);
                    location.PreFeatureSegmentId = segment.Id;
                    await locationService.EditAsync(location);
                    return RedirectToAction(nameof(SegmentsController.Detail),
                        SegmentsController.Name,
                        new { area = SegmentsController.Area, id = segment.Id });
                }
                else
                {
                    ShowAlertDanger("There is already a location notice segment attached to this location.");
                }

                return RedirectToAction(nameof(Details), new { slug = location.Stub });
            }
            else
            {
                ShowAlertDanger("Location not found.");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> AddPostHoursNotice(string stub)
        {
            var location = await locationService.GetLocationByStubAsync(stub);
            if (location != null)
            {
                if (!location.PostFeatureSegmentId.HasValue)
                {
                    var segment = new Segment
                    {
                        IsActive = false,
                        Name = $"{location.Name} below-hours notice",
                    };
                    segment = await segmentService.CreateAsync(segment);
                    location.PostFeatureSegmentId = segment.Id;
                    await locationService.EditAsync(location);
                    return RedirectToAction(nameof(SegmentsController.Detail),
                        SegmentsController.Name,
                        new { area = SegmentsController.Area, id = segment.Id });
                }
                else
                {
                    ShowAlertDanger("There is already a below-hours notice segment attached to this location.");
                }

                return RedirectToAction(nameof(Details), new { slug = location.Stub });
            }
            else
            {
                ShowAlertDanger("Location not found.");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> AddReplaceHoursNotice(string stub)
        {
            var location = await locationService.GetLocationByStubAsync(stub);
            if (location != null)
            {
                if (!location.HoursSegmentId.HasValue)
                {
                    var segment = new Segment
                    {
                        IsActive = false,
                        Name = $"{location.Name} hours replacement notice",
                    };
                    segment = await segmentService.CreateAsync(segment);
                    location.HoursSegmentId = segment.Id;
                    await locationService.EditAsync(location);
                    return RedirectToAction(nameof(SegmentsController.Detail),
                        SegmentsController.Name,
                        new { area = SegmentsController.Area, id = segment.Id });
                }
                else
                {
                    ShowAlertDanger("There is already a hours replacement notice segment attached to this location.");
                }

                return RedirectToAction(nameof(Details), new { slug = location.Stub });
            }
            else
            {
                ShowAlertDanger("Location not found.");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost("[action]/{slug}")]
        public async Task<IActionResult> ChangeInternalImageSortOrder(int interiorImageId,
            int increment,
            string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return BadRequest();
            }

            var hasPermission = await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission)
            {
                return RedirectToUnauthorized();
            }

            await locationService.UpdateInteriorImageSortAsync(slug, interiorImageId, increment);

            return RedirectToAction(nameof(UpdateInteriorImages), new { slug });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ClearLink(string slug, int featureId)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return BadRequest();
            }

            var hasPermission = await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission)
            {
                return RedirectToUnauthorized();
            }

            var location = await locationService.GetLocationByStubAsync(slug);
            if (location == null)
            {
                return NotFound();
            }

            var locationFeature = await locationFeatureService
                .GetByFeatureIdLocationIdAsync(featureId, location.Id);
            if (locationFeature == null)
            {
                return NotFound();
            }

            locationFeature.RedirectUrl = null;

            await locationFeatureService.EditAsync(locationFeature);

            return RedirectToAction(nameof(LocationFeature), new
            {
                slug = location.Stub,
                featureId,
            });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ClearSegment(string slug, int featureId)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return BadRequest();
            }

            var hasPermission = await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.LocationManagement)
                && await HasAppPermissionAsync(permissionGroupService,
                    ApplicationPermission.WebPageContentManagement);
            if (!hasPermission)
            {
                return RedirectToUnauthorized();
            }

            var location = await locationService.GetLocationByStubAsync(slug);
            if (location == null)
            {
                return NotFound();
            }

            var locationFeature = await locationFeatureService
                .GetByFeatureIdLocationIdAsync(featureId, location.Id);
            if (locationFeature?.SegmentId == null)
            {
                return NotFound();
            }

            await segmentService.DeleteAsync(locationFeature.SegmentId.Value);

            locationFeature.SegmentId = null;
            await locationFeatureService.EditAsync(locationFeature);

            return RedirectToAction(nameof(LocationFeature), new
            {
                slug = location.Stub,
                featureId,
            });
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> Details(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return BadRequest();
            }

            var location = await locationService.GetLocationByStubAsync(slug);
            if (location == null)
            {
                return NotFound();
            }

            var defaultLanguageId = await languageService.GetDefaultLanguageId();

            if (location.DescriptionSegmentId != default)
            {
                location.DescriptionSegment = await segmentService
                    .GetBySegmentAndLanguageAsync(location.DescriptionSegmentId, defaultLanguageId);
            }

            if (location.PostFeatureSegmentId.HasValue)
            {
                location.PostFeatureSegmentText = await segmentService
                    .GetBySegmentAndLanguageAsync(location.PostFeatureSegmentId.Value,
                        defaultLanguageId);
            }

            if (location.PreFeatureSegmentId.HasValue)
            {
                location.PreFeatureSegmentText = await segmentService
                    .GetBySegmentAndLanguageAsync(location.PreFeatureSegmentId.Value,
                        defaultLanguageId);
            }

            if (location.HoursSegmentId.HasValue)
            {
                location.HoursSegmentText = await segmentService
                    .GetBySegmentAndLanguageAsync(location.HoursSegmentId.Value,
                        defaultLanguageId);
            }

            var features = await featureService.GetAllFeaturesAsync();

            var locationFeatures = await locationFeatureService
                .GetLocationFeaturesByLocationAsync(location.Id);

            var featuresHere = features
                .Where(_ => locationFeatures.Select(_ => _.FeatureId).Contains(_.Id));

            var languages = await languageService.GetActiveAsync();

            location.InteriorImages = await locationService
                .GetLocationInteriorImagesAsync(location.Id);

            var viewModel = new DetailsViewModel
            {
                AtThisLocation
                    = [.. featuresHere.Where(_ => _.IsAtThisLocation).OrderBy(_ => _.SortOrder)],
                Displays = await digitalDisplayService.GetByLocationAsync(location.Id),
                IsSiteManager = !string.IsNullOrEmpty(UserClaim(ClaimType.SiteManager)),
                Location = location,
                LocationManager = await HasAppPermissionAsync(permissionGroupService,
                    ApplicationPermission.LocationManagement),
                SegmentEditor = await HasAppPermissionAsync(permissionGroupService,
                    ApplicationPermission.WebPageContentManagement),
                ServicesAvailable
                    = [.. featuresHere.Where(_ => !_.IsAtThisLocation).OrderBy(_ => _.SortOrder)],
            };

            var volunteerFeature = await featureService
                .GetFeatureBySlugAsync("volunteer");
            if (volunteerFeature != null)
            {
                var forms = await volunteerFormService.GetVolunteerFormsAsync();
                if (forms.Count != 0)
                {
                    var formsViewModel = new List<LocationVolunteerFormViewModel>();
                    foreach (var form in forms)
                    {
                        var mappings = await volunteerFormService
                            .GetFormUserMappingsAsync(form.Id, location.Id);
                        var newForm = new LocationVolunteerFormViewModel
                        {
                            TypeId = (int)form.VolunteerFormType,
                            TypeName = form.VolunteerFormType.ToString(),
                            FormMappings = mappings
                                .ToList()
                                .ConvertAll(_ => new LocationVolunteerMappingViewModel(_)),
                            IsDisabled = form.IsDisabled,
                        };
                        if (form.IsDisabled)
                        {
                            newForm.AlertWarning = $"The {form.VolunteerFormType} volunteer form is not active.";
                        }

                        formsViewModel.Add(newForm);
                    }

                    var locationFeature = await locationFeatureService
                        .GetByFeatureIdLocationIdAsync(volunteerFeature.Id, location.Id);
                    var hasForms = formsViewModel
                        .Any(_ => _.FormMappings.Count != 0 && !_.IsDisabled);
                    var hasLocationFeature = locationFeature != null;

                    if (hasForms && !hasLocationFeature)
                    {
                        await volunteerFormService
                            .AddVolunteerLocationFeature(volunteerFeature.Id,
                                location.Id,
                                location.Stub);
                    }
                    else if (!hasForms && hasLocationFeature)
                    {
                        await locationFeatureService
                            .DeleteAsync(volunteerFeature.Id, location.Id);
                    }

                    viewModel.VolunteerForms.AddRange(formsViewModel);
                }

                // don't show the volunteer forms at all if there are non and not manager
                if (!viewModel.LocationManager
                    && viewModel.VolunteerForms.Sum(_ => _.FormMappings?.Count) == 0)
                {
                    viewModel.VolunteerForms.Clear();
                }
            }

            foreach (var display in viewModel.Displays)
            {
                var assets = await digitalDisplayService.GetNonExpiredAssetsAsync(display.Id);
                display.SlideCount = assets.Count();
            }

            if (location.PreFeatureSegmentId.HasValue)
            {
                viewModel.LocationNoticeSegment
                    = await segmentService.GetByIdAsync(location.PreFeatureSegmentId.Value);
                viewModel.LocationNoticeLanguages.AddRange(await segmentService
                    .GetSegmentLanguagesByIdAsync(location.PreFeatureSegmentId.Value));
            }

            if (location.PostFeatureSegmentId.HasValue)
            {
                viewModel.PostHoursNoticeSegment
                    = await segmentService.GetByIdAsync(location.PostFeatureSegmentId.Value);
                viewModel.PostHoursNoticeLanguages.AddRange(await segmentService
                    .GetSegmentLanguagesByIdAsync(location.PostFeatureSegmentId.Value));
            }

            if (location.HoursSegmentId.HasValue)
            {
                viewModel.HoursReplacementNoticeSegment
                    = await segmentService.GetByIdAsync(location.HoursSegmentId.Value);
                viewModel.HoursReplacementNoticeLanguages.AddRange(await segmentService
                    .GetSegmentLanguagesByIdAsync(location.HoursSegmentId.Value));
            }

            if (location.ImageAltTextSegmentId.HasValue)
            {
                foreach (var language in await languageService.GetActiveAsync())
                {
                    location.ImageAltTextSegmentTexts.Add(await segmentService
                        .GetBySegmentAndLanguageAsync(location.ImageAltTextSegmentId.Value,
                            language.Id));
                }
            }

            if (location.MapAltTextSegmentId.HasValue)
            {
                foreach (var language in await languageService.GetActiveAsync())
                {
                    location.MapAltTextSegmentTexts.Add(await segmentService
                        .GetBySegmentAndLanguageAsync(location.MapAltTextSegmentId.Value,
                            language.Id));
                }
            }

            viewModel.DescriptionLanguages.AddRange(await segmentService
                .GetSegmentLanguagesByIdAsync(location.DescriptionSegmentId));
            viewModel.AllLanguages.AddRange(await languageService.GetActiveNamesAsync());

            return View(viewModel);
        }

        [HttpGet("[action]/{slug}")]
        public async Task<IActionResult> ExteriorImage(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return BadRequest();
            }

            var location = await locationService.GetLocationByStubAsync(slug);
            if (string.IsNullOrEmpty(location?.ImagePath))
            {
                return NotFound();
            }

            var fullImagePath = await locationService
                .GetExteriorImageFilePathAsync(location.ImagePath);

            if (!System.IO.File.Exists(fullImagePath))
            {
                return NotFound();
            }

            new FileExtensionContentTypeProvider()
                .TryGetContentType(fullImagePath, out string fileType);

            return PhysicalFile(fullImagePath,
                fileType ?? System.Net.Mime.MediaTypeNames.Application.Octet);
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(int page)
        {
            var filter = new LocationFilter(page == 0 ? 1 : page, 60);

            var locationList = await locationService.GetPaginatedListAsync(filter);

            var viewModel = new IndexViewModel
            {
                CurrentPage = filter.Page,
                ItemCount = locationList.Count,
                ItemsPerPage = filter.Take.Value,
                Locations = locationList.Data,
            };

            return viewModel.PastMaxPage ? RedirectToRoute(new { page = viewModel.LastPage ?? 1 }) : View(viewModel);
        }

        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> InteriorImage(int id)
        {
            var interiorImage = await locationService.GetInteriorImageByIdAsync(id);

            if (string.IsNullOrEmpty(interiorImage?.ImagePath))
            {
                return NotFound();
            }

            var fullImagePath = await locationService
                .GetInteriorImageFilePathAsync(interiorImage.ImagePath);

            if (!System.IO.File.Exists(fullImagePath))
            {
                return NotFound();
            }

            new FileExtensionContentTypeProvider()
                .TryGetContentType(fullImagePath, out string fileType);

            return PhysicalFile(fullImagePath,
                fileType ?? System.Net.Mime.MediaTypeNames.Application.Octet);
        }

        [HttpGet("[action]/{slug}/{featureId}")]
        public async Task<IActionResult> LocationFeature(string slug, int featureId)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return BadRequest();
            }

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

            var locationFeature = await locationFeatureService
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
                CanManageLocations = await HasAppPermissionAsync(permissionGroupService,
                    ApplicationPermission.LocationManagement),
                CanEditSegments = await HasAppPermissionAsync(permissionGroupService,
                    ApplicationPermission.WebPageContentManagement),
            };

            viewModel.AllLanguages.AddRange(await languageService.GetActiveNamesAsync());

            var defaultLanguageId = await languageService.GetDefaultLanguageId();

            var nameSegment = await segmentService
                .GetBySegmentAndLanguageAsync(feature.NameSegmentId, defaultLanguageId);
            feature.DisplayName = nameSegment.Text;
            viewModel.FeatureNameLanguages.AddRange(await segmentService
                .GetSegmentLanguagesByIdAsync(feature.NameSegmentId));

            if (feature.TextSegmentId.HasValue)
            {
                var featureText = await segmentService
                    .GetBySegmentAndLanguageAsync(feature.TextSegmentId.Value, defaultLanguageId);
                feature.BodyText = CommonMark.CommonMarkConverter.Convert(featureText?.Text);
                viewModel.FeatureTextLanguages.AddRange(await segmentService
                    .GetSegmentLanguagesByIdAsync(feature.TextSegmentId.Value));
            }

            if (locationFeature.SegmentId.HasValue)
            {
                var locationFeatureText = await segmentService
                    .GetBySegmentAndLanguageAsync(locationFeature.SegmentId.Value,
                        defaultLanguageId);
                locationFeature.Text = CommonMark
                    .CommonMarkConverter
                    .Convert(locationFeatureText?.Text);
                viewModel.LocationFeatureLanguages.AddRange(await segmentService
                    .GetSegmentLanguagesByIdAsync(locationFeature.SegmentId.Value));
            }

            return View(viewModel);
        }

        [HttpGet("[action]/{slug}")]
        public async Task<IActionResult> MapImage(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return BadRequest();
            }

            var location = await locationService.GetLocationByStubAsync(slug);
            if (string.IsNullOrEmpty(location?.MapImagePath))
            {
                return NotFound();
            }

            var fullImagePath = await locationService
                .GetMapImageFilePathAsync(location.MapImagePath);

            if (!System.IO.File.Exists(fullImagePath))
            {
                return NotFound();
            }

            new FileExtensionContentTypeProvider()
                .TryGetContentType(fullImagePath, out string fileType);

            return PhysicalFile(fullImagePath,
                fileType ?? System.Net.Mime.MediaTypeNames.Application.Octet);
        }

        [HttpPost]
        [Route("{slug}/[action]")]
        public async Task<IActionResult> MapVolunteerCoordinator(string slug, int type, int userId)
        {
            var location = await locationService.GetLocationByStubAsync(slug);
            try
            {
                await volunteerFormService
                    .AddFormUserMapping(location.Id, (VolunteerFormType)type, userId);
                ShowAlertSuccess($"Added staff member to receive {(VolunteerFormType)type} Volunteer form submissions.");
            }
            catch (OcudaException oex)
            {
                ShowAlertDanger($"Unable to add staff member for for {location.Name}: {oex.Message}");
            }

            return RedirectToAction(nameof(Details), new { slug });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> RemoveFeature(string slug, int featureId)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return BadRequest();
            }

            var hasPermission = await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission)
            {
                return RedirectToUnauthorized();
            }

            var location = await locationService.GetLocationByStubAsync(slug);
            if (location == null)
            {
                return NotFound();
            }

            var locationFeature = await locationFeatureService
                .GetByFeatureIdLocationIdAsync(featureId, location.Id);
            if (locationFeature == null)
            {
                return NotFound();
            }

            if (locationFeature.SegmentId.HasValue)
            {
                await segmentService
                    .DeleteWithTextsAlreadyVerifiedAsync(locationFeature.SegmentId.Value);
            }

            await locationFeatureService.DeleteAsync(featureId, location.Id);

            return RedirectToAction(nameof(Details), new { slug });
        }

        [HttpPost]
        [Route("{slug}/[action]")]
        public async Task<IActionResult> RemoveFormUserMapping(string slug, int userId, int type)
        {
            var location = await locationService.GetLocationByStubAsync(slug);
            try
            {
                await volunteerFormService
                    .RemoveFormUserMapping(location.Id, userId, (VolunteerFormType)type);
                ShowAlertSuccess($"Removed staff member from receiving {(VolunteerFormType)type} Volunteer form submissions.");
            }
            catch (OcudaException oex)
            {
                ShowAlertDanger($"Unable to remove staff member for {location.Name}: {oex.Message}");
            }

            return RedirectToAction(nameof(Details), new { slug });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> RemoveHoursReplacementNotice(int id)
        {
            var location = await locationService.GetLocationByIdAsync(id);
            if (location != null)
            {
                if (location.HoursSegmentId.HasValue)
                {
                    var segmentId = location.HoursSegmentId.Value;
                    location.HoursSegmentId = null;
                    await locationService.EditAsync(location);
                    await segmentService.DeleteAsync(segmentId);
                    ShowAlertSuccess("Hours replacement notice removed.");
                }
                else
                {
                    ShowAlertDanger("No hours replacement notice segment attached to this location.");
                }

                return RedirectToAction(nameof(Details), new { slug = location.Stub });
            }
            else
            {
                ShowAlertDanger("Location not found.");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost("[action]/{slug}")]
        public async Task<IActionResult> RemoveInteriorImage(int interiorImageId, string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return BadRequest();
            }

            var hasPermission = await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission)
            {
                return RedirectToUnauthorized();
            }

            await locationService.DeleteInteriorImageAsync(interiorImageId);

            ShowAlertSuccess("Image deleted successfully");
            return RedirectToAction(nameof(UpdateInteriorImages), new { slug });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> RemoveLocationNotice(int id)
        {
            var location = await locationService.GetLocationByIdAsync(id);
            if (location != null)
            {
                if (location.PreFeatureSegmentId.HasValue)
                {
                    var segmentId = location.PreFeatureSegmentId.Value;
                    location.PreFeatureSegmentId = null;
                    await locationService.EditAsync(location);
                    await segmentService.DeleteAsync(segmentId);
                    ShowAlertSuccess("Location notice removed.");
                }
                else
                {
                    ShowAlertDanger("No location notice segment attached to this location.");
                }

                return RedirectToAction(nameof(Details), new { slug = location.Stub });
            }
            else
            {
                ShowAlertDanger("Location not found.");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> RemovePostHoursNotice(int id)
        {
            var location = await locationService.GetLocationByIdAsync(id);
            if (location != null)
            {
                if (location.PostFeatureSegmentId.HasValue)
                {
                    var segmentId = location.PostFeatureSegmentId.Value;
                    location.PostFeatureSegmentId = null;
                    await locationService.EditAsync(location);
                    await segmentService.DeleteAsync(segmentId);
                    ShowAlertSuccess("Below-hours notice removed.");
                }
                else
                {
                    ShowAlertDanger("No below-hours notice segment attached to this location.");
                }

                return RedirectToAction(nameof(Details), new { slug = location.Stub });
            }
            else
            {
                ShowAlertDanger("Location not found.");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost("[action]")]
        public async Task<JsonResult> UpdateAltText(UpdatedAltText newAltText)
        {
            ArgumentNullException.ThrowIfNull(newAltText);

            var response = new JsonResponse();

            try
            {
                await locationService.UpdateAltTextAsync(newAltText.LocationId,
                    newAltText.LanguageId,
                    newAltText.Field,
                    newAltText.Text?.Trim());
                response.Success = true;
                response.Message = newAltText.Text?.Trim();
            }
            catch (OcudaException oex)
            {
                response.Success = false;
                response.Message = oex.Message;
            }

            return Json(response);
        }

        [HttpGet("[action]/{slug}")]
        public async Task<IActionResult> UpdateExteriorImage(string slug)
        {
            var hasPermission = await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission)
            {
                return RedirectToUnauthorized();
            }

            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            var location = await locationService.GetLocationByStubAsync(slug);
            return location == null
                ? NotFound()
                : View("ExteriorImage", new ExteriorImageViewModel
                {
                    CropHeight = locationService.ExteriorImageHeight,
                    CropWidth = locationService.ExteriorImageWidth,
                    LocationName = location.Name,
                    Slug = location.Stub,
                });
        }

        [HttpPost("[action]/{slug}")]
        public async Task<IActionResult> UpdateExteriorImage(ExteriorImageViewModel viewModel,
            string slug)
        {
            if (viewModel == null)
            {
                return BadRequest();
            }

            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            var hasPermission = await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission)
            {
                return RedirectToUnauthorized();
            }

            if (viewModel.Image == null)
            {
                ShowAlertDanger("Please provide an exterior image");
                return RedirectToAction(nameof(ExteriorImage), new { slug });
            }

            var location = await locationService.GetLocationByStubAsync(slug);

            try
            {
                await locationService
                    .UpdateExteriorImageAsync(viewModel.Image, viewModel.Filename, slug);
            }
            catch (OcudaException oex)
            {
                ShowAlertDanger($"Unable to update exterior photo for location {location.Name}: {oex.Message}");

                return RedirectToAction(nameof(UpdateExteriorImage), new { slug });
            }

            ShowAlertSuccess($"Location {location.Name} exterior image updated successfully!");
            return RedirectToAction(nameof(Details), new { slug });
        }

        [HttpPost("[action]/{slug}")]
        public async Task<IActionResult> UpdateInteriorImage(InteriorImageViewModel viewModel,
            string slug)
        {
            if (viewModel == null)
            {
                return BadRequest();
            }

            var hasPermission = await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission)
            {
                return RedirectToUnauthorized();
            }

            if (!viewModel.ImageId.HasValue)
            {
                return NotFound();
            }

            if (viewModel.Image == null)
            {
                ShowAlertDanger("Please provide an interior image");
                return RedirectToAction(nameof(UpdateInteriorImage), new { slug });
            }

            var interiorImage = await locationService
                .GetInteriorImageByIdAsync(viewModel.ImageId.Value);
            if (interiorImage == null)
            {
                return NotFound();
            }

            var location = await locationService.GetLocationByStubAsync(slug);
            if (location == null)
            {
                return NotFound();
            }

            try
            {
                await locationService.UpdateInteriorImageAsync(interiorImage,
                    viewModel.Image.FileName,
                    viewModel.Image);
            }
            catch (OcudaException oex)
            {
                ShowAlertDanger($"Unable to update interior photo for location {location.Name}: {oex.Message}");

                return RedirectToAction(nameof(UpdateInteriorImage),
                    new { interiorImageId = interiorImage.Id, slug });
            }

            ShowAlertSuccess($"Location {location.Name} interior image updated successfully!");
            return RedirectToAction(nameof(UpdateInteriorImage),
                new { interiorImageId = interiorImage.Id, slug });
        }

        [HttpGet("[action]/{slug}")]
        [RestoreModelState]
        public async Task<IActionResult> UpdateInteriorImage(int interiorImageId, string slug)
        {
            var hasPermission = await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission)
            {
                return RedirectToUnauthorized();
            }

            var location = await locationService.GetLocationByStubAsync(slug);
            if (location == null)
            {
                return NotFound();
            }

            var interiorImage = await locationService.GetInteriorImageByIdAsync(interiorImageId);
            if (interiorImage == null)
            {
                return NotFound();
            }

            var viewModel = new InteriorImageViewModel
            {
                CropHeight = locationService.InteriorImageHeight,
                CropWidth = locationService.InteriorImageWidth,
                ImageId = interiorImage.Id,
                LocationName = location.Name,
                Slug = location.Stub,
            };

            var altTexts = await locationService
                .GetAllLanguageImageAltTextsAsync(interiorImage.Id);

            foreach (var languageItem in await languageService.GetActiveAsync())
            {
                var altText = altTexts.SingleOrDefault(_ => _.LanguageId == languageItem.Id);
                viewModel.Languages.Add(languageItem.Id, languageItem.Description);
                viewModel.AltTexts.Add(languageItem.Id, altText.AltText);
            }

            return View("InteriorImage", viewModel);
        }

        [HttpPost("[action]/{slug}")]
        [SaveModelState]
        public async Task<IActionResult> UpdateInteriorImageData(InteriorImageViewModel viewModel,
            string slug)
        {
            if (viewModel == null)
            {
                return BadRequest();
            }

            var hasPermission = await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission)
            {
                return RedirectToUnauthorized();
            }

            var location = await locationService.GetLocationByStubAsync(slug);
            if (location == null)
            {
                return NotFound();
            }

            var interiorImage = await locationService
                .GetInteriorImageByIdAsync(viewModel.ImageId.Value);
            if (interiorImage == null)
            {
                return NotFound();
            }

            var allAltTexts = await locationService
                .GetAllLanguageImageAltTextsAsync(viewModel.ImageId.Value);

            int updates = 0;

            foreach (var altText in viewModel.AltTexts)
            {
                if (string.IsNullOrWhiteSpace(altText.Value))
                {
                    ShowAlertDanger("Unable to save empty Alt Texts.");
                    continue;
                }

                var inDatabase = allAltTexts.SingleOrDefault(_ => _.LanguageId == altText.Key);

                if (inDatabase == null)
                {
                    // add
                    await locationService.AddImageAltTextAsync(new LocationInteriorImageAltText
                    {
                        AltText = altText.Value?.Trim(),
                        LanguageId = altText.Key,
                        LocationInteriorImageId = viewModel.ImageId.Value,
                    });
                    updates++;
                }
                else if (inDatabase.AltText?.Trim() != altText.Value?.Trim())
                {
                    // changed
                    await locationService.UpdateImageAltTextAsync(viewModel.ImageId.Value,
                        altText.Key,
                        altText.Value?.Trim());
                    updates++;
                }
            }

            if (updates == 0)
            {
                ShowAlertWarning("No updates were made.");
            }
            else
            {
                ShowAlertSuccess($"Updates made: {updates}");
            }

            return RedirectToAction(nameof(UpdateInteriorImage),
                new { interiorImageId = viewModel.ImageId, slug });
        }

        [HttpGet("[action]/{slug}")]
        public async Task<IActionResult> UpdateInteriorImages(string slug)
        {
            var hasPermission = await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission)
            {
                return RedirectToUnauthorized();
            }

            if (string.IsNullOrEmpty(slug))
            {
                return BadRequest();
            }

            var location = await locationService.GetLocationByStubAsync(slug);
            if (location == null)
            {
                return NotFound();
            }

            var interiorImages = await locationService.GetLocationInteriorImagesAsync(location.Id);
            foreach (var interiorImage in interiorImages)
            {
                interiorImage.AllAltTexts = await locationService
                    .GetAllLanguageImageAltTextsAsync(interiorImage.Id);
            }

            return View("InteriorImages", new InteriorImagesViewModel
            {
                InteriorImages = interiorImages,
                LocationName = location.Name,
                Slug = location.Stub,
            });
        }

        [HttpPost("[action]/{slug}/{featureId}")]
        public async Task<IActionResult> UpdateLink(LinkViewModel viewModel)
        {
            if (viewModel == null)
            {
                return BadRequest();
            }

            var hasPermission = await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission)
            {
                return RedirectToUnauthorized();
            }

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

            var locationFeature = await locationFeatureService
                .GetByFeatureIdLocationIdAsync(feature.Id, location.Id);
            if (locationFeature == null)
            {
                return NotFound();
            }

            locationFeature.RedirectUrl = viewModel.Link?.Trim();
            locationFeature.NewTab = viewModel.NewTab;

            await locationFeatureService.EditAsync(locationFeature);

            return RedirectToAction(nameof(LocationFeature), new
            {
                slug = location.Stub,
                featureId = feature.Id,
            });
        }

        [HttpGet("[action]/{slug}/{featureId}")]
        public async Task<IActionResult> UpdateLink(string slug, int featureId)
        {
            var hasPermission = await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission)
            {
                return RedirectToUnauthorized();
            }

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
                Feature = feature,
            };

            var locationFeature = await locationFeatureService
                .GetByFeatureIdLocationIdAsync(featureId, location.Id);

            if (locationFeature != null)
            {
                viewModel.Link = locationFeature.RedirectUrl;
                viewModel.NewTab = locationFeature.NewTab;
            }

            return View(nameof(UpdateLink), viewModel);
        }

        [HttpGet("[action]/{slug}")]
        public async Task<IActionResult> UpdateMapImage(string slug)
        {
            var hasPermission = await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission)
            {
                return RedirectToUnauthorized();
            }

            var viewModel = new UpdateMapImageViewModel
            {
                Height = 250,
                Location = await locationService.GetLocationByStubAsync(slug),
                MapApiKey = configuration[Configuration.OcudaGoogleAPI],
                Width = 500,
            };
            return View(viewModel);
        }

        [HttpPost("[action]/{slug}")]
        public async Task<IActionResult> UpdateMapImage([FromBody] string imageBase64, string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            var hasPermission = await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.LocationManagement);
            if (!hasPermission)
            {
                return RedirectToUnauthorized();
            }

            var location = await locationService.GetLocationByStubAsync(slug);
            if (location == null)
            {
                return NotFound();
            }

            try
            {
                var imageWithInfo = await imageService.ConvertFromBase64Async(imageBase64);

                // TODO: fix this using slugify the way the other image processes work?
                var fixedBase = location.Name
                    .ToLowerInvariant()
                    .Replace(" ", "-", StringComparison.InvariantCultureIgnoreCase)
                    .Replace(".", string.Empty, StringComparison.InvariantCultureIgnoreCase);
                var filename = $"{fixedBase}-map{imageWithInfo.ImageInformation.Extension}";

                try
                {
                    await locationService.UpdateMapImageAsync(
                        imageWithInfo.ImageBytes,
                        filename,
                        slug);
                }
                catch (OcudaException oex)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, oex.Message);
                }

                return new JsonResult("Image updated successfully!");
            }
            catch (ParameterException pex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, pex.Message);
            }
        }

        private async Task<FeatureLocation> GetFeatureLocation(int featureId, string stub)
        {
            var location = await locationService.GetLocationByStubAsync(stub);
            if (location == null)
            {
                _logger.LogError("Unable to find location with stub: {Stub}", stub);
                throw new OcudaException($"Unable to find location with stub: {stub}");
            }

            var feature = await featureService.GetFeatureByIdAsync(featureId);
            if (feature == null)
            {
                _logger.LogError("Unable to find feature with id: {FeatureId}", featureId);
                throw new OcudaException($"Unable to find feature with id: {featureId}");
            }

            return (feature, location);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.OrderingRules",
        "SA1201:Elements should appear in the correct order",
        Justification = "Internal struct is only used in this file")]
    internal record struct FeatureLocation(Feature Feature, Location Location)
    {
        public static implicit operator (Feature Feature, Location Location)(FeatureLocation value)
        {
            return (value.Feature, value.Location);
        }

        public static implicit operator FeatureLocation((Feature Feature, Location Location) value)
        {
            return new FeatureLocation(value.Feature, value.Location);
        }
    }
}
