using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Incident.ViewModel;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Controllers.ServiceFacades;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Services.Interfaces;

namespace Ocuda.Ops.Controllers.Areas.Incident
{
    [Area(nameof(Incident))]
    [Route("[area]")]
    public class HomeController : BaseController<HomeController>
    {
        private const int CacheLocationPermissionsMinutes = 5;
        private const string PageTitle = "Incident Reports";
        private readonly IOcudaCache _cache;
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly IIncidentService _incidentService;
        private readonly ILocationService _locationService;
        private readonly IPermissionGroupService _permissionGroupService;
        private readonly IUserService _userService;

        public HomeController(Controller<HomeController> context,
            IDateTimeProvider dateTimeProvider,
            IIncidentService incidentService,
            ILocationService locationService,
            IOcudaCache cache,
            IPermissionGroupService permissionGroupService,
            IUserService userService) : base(context)
        {
            ArgumentNullException.ThrowIfNull(cache);
            ArgumentNullException.ThrowIfNull(dateTimeProvider);
            ArgumentNullException.ThrowIfNull(incidentService);
            ArgumentNullException.ThrowIfNull(locationService);
            ArgumentNullException.ThrowIfNull(permissionGroupService);
            ArgumentNullException.ThrowIfNull(userService);

            _cache = cache;
            _dateTimeProvider = dateTimeProvider;
            _incidentService = incidentService;
            _locationService = locationService;
            _permissionGroupService = permissionGroupService;
            _userService = userService;

            SetPageTitle(PageTitle);
        }

        public static string Name
        { get { return "Home"; } }

        [HttpPost("[action]")]
        public async Task<IActionResult> ActivateType(int typeId)
        {
            if (!IsSiteManager())
            {
                return RedirectToUnauthorized();
            }

            await _incidentService.AdjustTypeStatusAsync(typeId, true);
            return RedirectToAction(nameof(Configuration));
        }

        [HttpGet("[action]")]
        [RestoreModelState]
        public async Task<IActionResult> Add()
        {
            var activeIncidentTypes = await _incidentService.GetActiveIncidentTypesAsync();

            if (activeIncidentTypes.Count == 0)
            {
                ShowAlertWarning("You must configure incident types before you can enter an incident");
                return RedirectToAction(nameof(Configuration));
            }

            var associatedLocation = await _userService.GetAssociatedLocation(CurrentUserId);

            var viewModel = new AddViewModel
            {
                IncidentDocumentLink = await _siteSettingService
                    .GetSettingStringAsync(Models.Keys.SiteSetting.Incident.Documentation),
                Incident = new Models.Entities.Incident
                {
                    LocationId = associatedLocation ?? default
                },
                IncidentTypes = activeIncidentTypes.Select(_ => new SelectListItem
                {
                    Value = _.Key.ToString(CultureInfo.InvariantCulture),
                    Text = _.Value
                }),
                Locations = await GetLocationsDropdownAsync(_locationService),
                MultiUserAccount = await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.MultiUserAccount)
            };

            return View(viewModel);
        }

        [HttpPost("[action]")]
        [SaveModelState]
        public async Task<IActionResult> Add(AddViewModel viewModel)
        {
            if (viewModel == null) { return RedirectToAction(nameof(Add)); }

            if (viewModel.IncidentDate < _dateTimeProvider.Now.AddDays(-14))
            {
                ModelState.AddModelError(nameof(viewModel.IncidentDate), "Please choose a date in the last 2 weeks");
            }
            else if (viewModel.IncidentDate > _dateTimeProvider.Now)
            {
                ModelState.AddModelError(nameof(viewModel.IncidentDate), "Please choose a date not in the future.");
            }
            else if (viewModel.IncidentDate.HasValue && viewModel.IncidentTime.HasValue)
            {
                viewModel.Incident.IncidentAt = viewModel.IncidentDate.Value
                    + viewModel.IncidentTime.Value.TimeOfDay;
            }

            if (ModelState.IsValid)
            {
                var affectedStaff = new List<IncidentStaff>();
                var affectedPeople = new List<IncidentParticipant>();

                var witnessStaff = new List<IncidentStaff>();
                var witnessPeople = new List<IncidentParticipant>();

                if (!string.IsNullOrEmpty(viewModel.AffectedJson))
                {
                    (affectedStaff, affectedPeople) = SortParticipants(viewModel.AffectedJson,
                        IncidentParticipantType.Affected);
                }

                if (!string.IsNullOrEmpty(viewModel.WitnessJson))
                {
                    (witnessStaff, witnessPeople) = SortParticipants(viewModel.WitnessJson,
                        IncidentParticipantType.Witness);
                }

                var baseUri = BaseUriBuilder;
                baseUri.Path = Url.Action(nameof(Details), new { id = 0 });

                int? incidentId = null;

                try
                {
                    incidentId = await _incidentService.AddAsync(viewModel.Incident,
                        affectedStaff.Union(witnessStaff).ToList(),
                        affectedPeople.Union(witnessPeople).ToList(),
                        baseUri.Uri);
                }
                catch (OcudaException oex)
                {
                    _logger.LogCritical(oex, "Issue creating incident report.");
                }

                if (incidentId.HasValue)
                {
                    ShowAlertSuccess($"Incident {incidentId} created.");
                    return RedirectToAction(nameof(Details), new { id = incidentId });
                }
                else
                {
                    ShowAlertWarning("There was an issue creating your incident. Please verify it was created correctly.");
                    return RedirectToAction(nameof(Index));
                }
            }
            else
            {
                return RedirectToAction(nameof(Add));
            }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> AddFollowup(int incidentId, string followupText)
        {
            var incident = await _incidentService.GetAsync(incidentId);
            if (incident == null)
            {
                ShowAlertDanger($"Unable to find incident id {incidentId}");
                return RedirectToAction(nameof(Mine));
            }

            await _incidentService.AddFollowupAsync(incidentId, followupText);
            return RedirectToAction(nameof(Details), new { id = incidentId });
        }

        [HttpPost("[action]/{locationId:int}/{permissionGroupId:int}")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> AddPermissionGroup(int locationId, int permissionGroupId)
        {
            try
            {
                await _permissionGroupService
                    .AddToPermissionGroupAsync<PermissionGroupIncidentLocation>(locationId,
                        permissionGroupId);
                var permissionInfo = await _permissionGroupService.GetGroupsAsync([permissionGroupId]);
                var location = await _locationService.GetLocationByIdAsync(locationId);
                AlertInfo = $"Group <strong>{permissionInfo.FirstOrDefault()?.PermissionGroupName}</strong> added to view incidents at <strong>{location.Name}</strong>. This will take effect after {CacheLocationPermissionsMinutes} minutes.";
            }
            catch (OcudaException oex)
            {
                AlertDanger = $"Problem adding permission: {oex.Message}";
            }

            return RedirectToAction(nameof(Permissions), new { locationId });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> AddRelationship(int incidentId, int relatedIncidentId)
        {
            var incident = await _incidentService.GetAsync(incidentId);
            if (incident == null)
            {
                ShowAlertDanger($"Unable to find incident id {incidentId}");
                return RedirectToAction(nameof(Mine));
            }

            var relatedIncident = await _incidentService.GetAsync(relatedIncidentId);
            if (relatedIncident == null)
            {
                ShowAlertDanger($"Unable to find related incident id {relatedIncidentId}");
                return RedirectToAction(nameof(Details), new { id = incidentId });
            }

            await _incidentService.AddRelationshipAsync(incidentId, relatedIncidentId);
            return RedirectToAction(nameof(Details), new { id = incidentId });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> AddType(string incidentTypeDescription)
        {
            if (!IsSiteManager())
            {
                return RedirectToUnauthorized();
            }

            var exists = await _incidentService.GetTypeAsync(incidentTypeDescription);
            if (exists != null)
            {
                ShowAlertWarning("That incident type name already exists.");
            }
            else
            {
                await _incidentService.AddTypeAsync(incidentTypeDescription);
            }
            return RedirectToAction(nameof(Configuration));
        }

        [HttpGet("[action]")]
        [HttpGet("[action]/{page:int}")]
        public async Task<IActionResult> All(int page, string searchText)
        {
            var hasPermission = await CanViewAllAsync();

            if (!hasPermission)
            {
                return RedirectToUnauthorized();
            }

            int currentPage = page != 0 ? page : 1;

            var filter = new IncidentFilter(currentPage)
            {
                SearchText = searchText
            };

            var viewModel = await GetIncidentsAsync(filter, currentPage);

            if (viewModel.PastMaxPage)
            {
                return RedirectToRoute(new { page = viewModel.LastPage ?? 1 });
            }

            viewModel.ViewingAll = true;
            viewModel.SecondaryHeading = nameof(All);

            foreach (var incident in viewModel.Incidents)
            {
                incident.CreatedByUser
                    = await _userService.GetByIdIncludeDeletedAsync(incident.CreatedBy);
            }

            return View("Index", viewModel);
        }

        [HttpGet("[action]")]
        [HttpGet("[action]/{page:int}")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> Configuration(int page)
        {
            int currentPage = page != 0 ? page : 1;

            var filter = new IncidentFilter(currentPage)
            {
                CreatedById = CurrentUserId,
            };

            var incidentTypes = await _incidentService.GetIncidentTypesAsync(filter);

            var viewModel = new ConfigurationViewModel
            {
                CanConfigureEmails = IsSiteManager(),
                CanConfigureIncidents = IsSiteManager(),
                EmailTemplateId = await _siteSettingService
                    .GetSettingIntAsync(Models.Keys.SiteSetting.Incident.EmailTemplateId),
                IncidentDocumentLink = await _siteSettingService
                    .GetSettingStringAsync(Models.Keys.SiteSetting.Incident.Documentation),
                IncidentTypes = incidentTypes.Data,
                ItemCount = incidentTypes.Count,
                ItemsPerPage = filter.Take.Value,
                LawEnforcementAddresses = await _siteSettingService
                    .GetSettingStringAsync(Models.Keys.SiteSetting.Incident.LawEnforcementAddresses),
                Locations = await _locationService.GetAllLocationsAsync()
            };

            foreach (var location in viewModel.Locations)
            {
                var locationPermissions = await _permissionGroupService
                    .GetPermissionsAsync<PermissionGroupIncidentLocation>(location.Id);
                viewModel.LocationPermissions.Add(location.Id, locationPermissions.Count);
            }

            if (viewModel.PastMaxPage)
            {
                return RedirectToRoute(new { page = viewModel.LastPage ?? 1 });
            }

            return View(viewModel);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> DeactivateType(int typeId)
        {
            if (!IsSiteManager())
            {
                return RedirectToUnauthorized();
            }

            await _incidentService.AdjustTypeStatusAsync(typeId, false);
            return RedirectToAction(nameof(Configuration));
        }

        [HttpGet("[action]/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var incident = await _incidentService.GetAsync(id);

            if (incident == null)
            {
                ShowAlertWarning($"Unable to find incident number {id}");
                return RedirectToAction(nameof(Mine));
            }

            var hasPermission = incident.CreatedBy == CurrentUserId || await CanViewAllAsync();

            if (!hasPermission)
            {
                return RedirectToUnauthorized();
            }

            if (!incident.IsVisible && !IsSiteManager())
            {
                return StatusCode(404);
            }

            var viewModel = new DetailsViewModel
            {
                AllLocationNames = await _locationService.GetAllNamesIncludingDeletedAsync(),
                CanAdd = hasPermission,
                CanHide = IsSiteManager(),
                Incident = incident,
                IncidentDocumentLink = await _siteSettingService
                    .GetSettingStringAsync(Models.Keys.SiteSetting.Incident.Documentation),
                IncidentTypes = await _incidentService.GetActiveIncidentTypesAsync(),
                SecondaryHeading = $"#{incident.Id}"
            };

            SetPageTitle($"Incident Report #{incident.Id}");

            return View(viewModel);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> HideIncident(int incidentId)
        {
            if (!IsSiteManager())
            {
                return RedirectToUnauthorized();
            }
            await _incidentService.SetVisibilityAsync(incidentId, false);
            return RedirectToAction(nameof(Details), new { id = incidentId });
        }

        [HttpGet("[action]")]
        [HttpGet("[action]/{page:int}")]
        public async Task<IActionResult> Locations(int page, string searchText)
        {
            var authorizedLocations = await GetLocationAuthorizationsAsync();

            if (!authorizedLocations.Any())
            {
                return RedirectToUnauthorized();
            }

            int currentPage = page != 0 ? page : 1;

            var filter = new IncidentFilter(currentPage)
            {
                LocationIds = authorizedLocations,
                SearchText = searchText
            };

            var viewModel = await GetIncidentsAsync(filter, currentPage);

            if (viewModel.PastMaxPage)
            {
                return RedirectToRoute(new { page = viewModel.LastPage ?? 1 });
            }

            viewModel.SecondaryHeading = "My Location(s)";

            foreach (var incident in viewModel.Incidents)
            {
                incident.CreatedByUser
                    = await _userService.GetByIdIncludeDeletedAsync(incident.CreatedBy);
            }

            return View("Index", viewModel);
        }

        [HttpGet("")]
        [HttpGet("[action]/{page:int}")]
        public async Task<IActionResult> Mine(int page, string searchText)
        {
            int currentPage = page != 0 ? page : 1;

            var filter = new IncidentFilter(currentPage)
            {
                CreatedById = CurrentUserId,
                SearchText = searchText,
            };

            var viewModel = await GetIncidentsAsync(filter, currentPage);

            if (viewModel.PastMaxPage)
            {
                return RedirectToRoute(new { page = viewModel.LastPage ?? 1 });
            }

            viewModel.SecondaryHeading = nameof(Mine);

            SetPageTitle($"{PageTitle}: {nameof(Mine)}");

            return View("Index", viewModel);
        }

        [HttpGet("[action]/{locationId:int}")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> Permissions(int locationId)
        {
            var location = await _locationService.GetLocationByIdAsync(locationId);
            if (location == null)
            {
                return NotFound();
            }

            var permissionGroups = await _permissionGroupService.GetAllAsync();
            var locationPermissions = await _permissionGroupService
                .GetPermissionsAsync<PermissionGroupIncidentLocation>(locationId);

            var viewModel = new PermissionsViewModel
            {
                LocationId = locationId,
                LocationName = location.Name
            };

            foreach (var permissionGroup in permissionGroups)
            {
                if (locationPermissions.Any(_ => _.PermissionGroupId == permissionGroup.Id))
                {
                    viewModel.AssignedGroups.Add(permissionGroup.Id,
                        permissionGroup.PermissionGroupName);
                }
                else
                {
                    viewModel.AvailableGroups.Add(permissionGroup.Id,
                        permissionGroup.PermissionGroupName);
                }
            }

            return View(viewModel);
        }

        [HttpPost("[action]/{locationId:int}/{permissionGroupId:int}")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> RemovePermissionGroup(int locationId, int permissionGroupId)
        {
            try
            {
                await _permissionGroupService
                    .RemoveFromPermissionGroupAsync<PermissionGroupIncidentLocation>(locationId,
                        permissionGroupId);
                var permissionInfo = await _permissionGroupService.GetGroupsAsync([permissionGroupId]);
                var location = await _locationService.GetLocationByIdAsync(locationId);
                AlertInfo = $"Group <strong>{permissionInfo.FirstOrDefault()?.PermissionGroupName}</strong> removed from viewing incidents at <strong>{location.Name}</strong>. This will take effect after {CacheLocationPermissionsMinutes} minutes.";
            }
            catch (OcudaException oex)
            {
                AlertDanger = $"Problem removing permission: {oex.Message}";
            }

            return RedirectToAction(nameof(Permissions), new { locationId });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ShowIncident(int incidentId)
        {
            if (!IsSiteManager())
            {
                return RedirectToUnauthorized();
            }
            await _incidentService.SetVisibilityAsync(incidentId, true);
            return RedirectToAction(nameof(Details), new { id = incidentId });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> UpdateEmailSettings(int emailTemplateId,
            string lawEnforcementAddresses)
        {
            var issues = new List<string>();
            if (emailTemplateId == default)
            {
                issues.Add("No email template set, emails will not be sent.");
            }
            await _siteSettingService
                .UpdateAsync(Models.Keys.SiteSetting.Incident.EmailTemplateId,
                    emailTemplateId.ToString(CultureInfo.InvariantCulture));

            if (string.IsNullOrEmpty(lawEnforcementAddresses))
            {
                issues.Add("No law enforcement destination email addresses set, emails will not be sent.");
            }
            await _siteSettingService
                .UpdateAsync(Models.Keys.SiteSetting.Incident.LawEnforcementAddresses,
                    lawEnforcementAddresses);

            if (issues.Count > 0)
            {
                var sb = new System.Text.StringBuilder("Some issues were encountered:<ul>");
                issues.ForEach(_ => sb.Append("<li>").Append(_).AppendLine("</li>"));
                sb.Append("</ul>");
                ShowAlertWarning(sb.ToString());
            }
            else
            {
                ShowAlertSuccess("Updated incident email configuration.");
            }

            return RedirectToAction(nameof(Configuration));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> UpdateType(int incidentTypeId,
            string incidentTypeDescription)
        {
            if (!IsSiteManager())
            {
                return RedirectToUnauthorized();
            }

            try
            {
                await _incidentService.UpdateIncidentTypeAsync(incidentTypeId,
                    incidentTypeDescription);
            }
            catch (OcudaException oex)
            {
                ShowAlertWarning($"Unable to update incident type: {oex.Message}");
            }
            return RedirectToAction(nameof(Configuration));
        }

        private static (List<IncidentStaff>, List<IncidentParticipant>)
            SortParticipants(string jsonParticipants, IncidentParticipantType type)
        {
            var staff = new List<IncidentStaff>();
            var people = new List<IncidentParticipant>();

            var participants = JsonSerializer.Deserialize<IncidentStaffPublic[]>(jsonParticipants,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

            foreach (var party in participants)
            {
                if (party.Id != default)
                {
                    staff.Add(new IncidentStaff
                    {
                        UserId = party.Id,
                        IncidentParticipantType = type
                    });
                }
                else
                {
                    people.Add(new IncidentParticipant
                    {
                        Barcode = string.IsNullOrEmpty(party.Barcode)
                            ? null
                            : party.Barcode,
                        Description = string.IsNullOrEmpty(party.Description)
                            ? null
                            : party.Description,
                        IncidentParticipantType = type,
                        Name = string.IsNullOrEmpty(party.Name) ? null : party.Name
                    });
                }
            }

            return (staff, people);
        }

        private async Task<bool> CanViewAllAsync()
        {
            return !string.IsNullOrEmpty(UserClaim(ClaimType.SiteManager))
                || await HasAppPermissionAsync(_permissionGroupService,
                    ApplicationPermission.ViewAllIncidentReports);
        }

        private async Task<bool> CanViewLocationsAsync()
        {
            var authorizedLocations = await GetLocationAuthorizationsAsync();

            return authorizedLocations.Any();
        }

        private async Task<IndexViewModel> GetIncidentsAsync(IncidentFilter filter, int currentPage)
        {
            var incidents = await _incidentService.GetPaginatedAsync(filter);

            var allIncidentTypes = await _incidentService.GetAllIncidentTypesAsync();

            return new IndexViewModel
            {
                AllLocationNames = await _locationService.GetAllNamesIncludingDeletedAsync(),
                CanConfigureIncidents = IsSiteManager(),
                CanViewAll = await CanViewAllAsync(),
                CanViewLocation = await CanViewLocationsAsync(),
                CurrentPage = currentPage,
                IncidentDocumentLink = await _siteSettingService
                    .GetSettingStringAsync(Models.Keys.SiteSetting.Incident.Documentation),
                Incidents = incidents.Data,
                IncidentTypes = allIncidentTypes,
                ItemCount = incidents.Count,
                ItemsPerPage = filter.Take.Value,
                SearchText = filter.SearchText
            };
        }

        private async Task<IEnumerable<int>> GetLocationAuthorizationsAsync()
        {
            var cacheKey = string.Format(CultureInfo.InvariantCulture,
                Cache.OpsIncidentLocationAuthorizations,
                CurrentUserId);

            var list = await _cache.GetObjectFromCacheAsync<IEnumerable<int>>(cacheKey);

            if (list != null)
            {
                return list;
            }

            var authorizedLocations = new List<int>();
            var locations = await _locationService.GetAllLocationsAsync();

            foreach (var location in locations)
            {
                var locationGroups = await _permissionGroupService
                    .GetPermissionsAsync<PermissionGroupIncidentLocation>(location.Id);
                if (locationGroups?.Count > 0)
                {
                    var locationAuthorized = await _permissionGroupService
                        .HasAPermissionAsync<PermissionGroupIncidentLocation>(locationGroups
                            .Select(_ => _.PermissionGroupId));
                    if (locationAuthorized)
                    {
                        authorizedLocations.Add(location.Id);
                    }
                }
            }

            await _cache.SaveToCacheAsync(cacheKey,
                authorizedLocations.AsEnumerable(),
                TimeSpan.FromMinutes(CacheLocationPermissionsMinutes));

            return authorizedLocations;
        }
    }
}