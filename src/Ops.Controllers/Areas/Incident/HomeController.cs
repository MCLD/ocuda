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
        private readonly IIncidentService _incidentService;
        private readonly ILocationService _locationService;
        private readonly IPermissionGroupService _permissionGroupService;
        private readonly IUserService _userService;

        private const string PageTitle = "Incident Reports";

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

            await _incidentService.AdjustTypeStatus(typeId, true);
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

                var incidentId = await _incidentService.AddAsync(viewModel.Incident,
                    affectedStaff.Union(witnessStaff).ToList(),
                    affectedPeople.Union(witnessPeople).ToList());

                // eventually redirect to the deatils page for this incident
                ShowAlertSuccess($"Incident {incidentId} created.");
            }
            else
            {
                ShowAlertDanger($"Could not add incident report: {ModelState.ErrorCount} validation errors.");
                return RedirectToAction(nameof(Add));
            }
            return RedirectToAction(nameof(Mine));
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
                CanConfigureIncidents = IsSiteManager(),
                IncidentTypes = incidentTypes.Data,
                ItemCount = incidentTypes.Count,
                ItemsPerPage = filter.Take.Value
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

            await _incidentService.AdjustTypeStatus(typeId, false);
            return RedirectToAction(nameof(Configuration));
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

        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var incident = await _incidentService.GetAsync(id);

            if (incident == null)
            {
                ShowAlertWarning($"Unable to find incident number {id}");
                return RedirectToAction(nameof(Mine));
            }

            var viewModel = new DetailsViewModel
            {
                Incident = incident,
                IncidentTypes = await _incidentService.GetActiveIncidentTypesAsync(),
                Locations = await GetLocationsAsync(_locationService),
                SecondaryHeading = $"#{incident.Id}"
            };

            SetPageTitle($"Incident Report #{incident.Id}");

            return View(viewModel);
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
                await _incidentService.UpdateIncidentType(incidentTypeId, incidentTypeDescription);
            }
            catch (OcudaException oex)
            {
                ShowAlertWarning($"Unable to update incident type: {oex.Message}");
            }
            return RedirectToAction(nameof(Configuration));
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

        private (List<IncidentStaff>, List<IncidentParticipant>)
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
    }
}
