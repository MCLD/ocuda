using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Incident.ViewModel;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Controllers.ServiceFacades;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;

namespace Ocuda.Ops.Controllers.Areas.Incident
{
    [Area(nameof(Incident))]
    [Route("[area]")]
    public class HomeController : BaseController<HomeController>
    {
        private const string PageTitle = "Incident Reports";
        private readonly IIncidentService _incidentService;
        private readonly ILocationService _locationService;
        private readonly IPermissionGroupService _permissionGroupService;
        private readonly IUserService _userService;

        public HomeController(Controller<HomeController> context,
            IIncidentService incidentService,
            ILocationService locationService,
            IPermissionGroupService permissionGroupService,
            IUserService userService) : base(context)
        {
            _incidentService = incidentService
                ?? throw new ArgumentNullException(nameof(incidentService));
            _locationService = locationService
                ?? throw new ArgumentNullException(nameof(locationService));
            _permissionGroupService = permissionGroupService
                ?? throw new ArgumentNullException(nameof(permissionGroupService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));

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

            if (viewModel.IncidentDate.HasValue && viewModel.IncidentTime.HasValue)
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

                var incidentId = await _incidentService.AddAsync(viewModel.Incident,
                    affectedStaff.Union(witnessStaff).ToList(),
                    affectedPeople.Union(witnessPeople).ToList(),
                    baseUri.Uri);

                ShowAlertSuccess($"Incident {incidentId} created.");
                return RedirectToAction(nameof(Details), new { id = incidentId });
            }
            else
            {
                ShowAlertDanger($"Could not add incident report: {ModelState.ErrorCount} validation errors.");
                return RedirectToAction(nameof(Add));
            }
            return RedirectToAction(nameof(Mine));
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
        [HttpGet("[action]/{page}")]
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
                incident.CreatedByUser = await _userService.GetByIdAsync(incident.CreatedBy);
            }

            return View("Index", viewModel);
        }

        [HttpGet("[action]")]
        [HttpGet("[action]/{page}")]
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
                IncidentTypes = incidentTypes.Data,
                ItemCount = incidentTypes.Count,
                ItemsPerPage = filter.Take.Value,
                LawEnforcementAddresses = await _siteSettingService
                    .GetSettingStringAsync(Models.Keys.SiteSetting.Incident.LawEnforcementAddresses),
            };

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

        [HttpGet("[action]/{id}")]
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

            var viewModel = new DetailsViewModel
            {
                CanAdd = hasPermission,
                Incident = incident,
                IncidentTypes = await _incidentService.GetActiveIncidentTypesAsync(),
                Locations = await GetLocationsAsync(_locationService),
                SecondaryHeading = $"#{incident.Id}"
            };

            SetPageTitle($"Incident Report #{incident.Id}");

            return View(viewModel);
        }

        [HttpGet("")]
        [HttpGet("[action]/{page}")]
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

            if (issues.Any())
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
                await _incidentService.UpdateIncidentTypeAsync(incidentTypeId, incidentTypeDescription);
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

        private async Task<IndexViewModel> GetIncidentsAsync(IncidentFilter filter, int currentPage)
        {
            var incidents = await _incidentService.GetPaginatedAsync(filter);

            var allIncidentTypes = await _incidentService.GetAllIncidentTypesAsync();

            return new IndexViewModel
            {
                CanViewAll = await CanViewAllAsync(),
                CurrentPage = currentPage,
                Incidents = incidents.Data,
                IncidentTypes = allIncidentTypes,
                ItemCount = incidents.Count,
                ItemsPerPage = filter.Take.Value,
                Locations = await _locationService.GetAllLocationsIdNameAsync(),
                SearchText = filter.SearchText,
            };
        }
    }
}
