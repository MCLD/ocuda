using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Volunteer;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement
{
    [Area("SiteManagement")]
    [Route("[area]/[controller]")]
    public class VolunteerController : BaseController<VolunteerController>
    {
        private readonly ILanguageService _languageService;
        private readonly ILocationService _locationService;
        private readonly IPermissionGroupService _permissionGroupService;
        private readonly ISegmentService _segmentService;
        private readonly IVolunteerFormService _volunteerFormService;

        public VolunteerController(ServiceFacades.Controller<VolunteerController> context,
            IVolunteerFormService volunteerFormService,
            ILanguageService languageService,
            ILocationService locationService,
            IPermissionGroupService permissionGroupService,
            ISegmentService segmentService) : base(context)
        {
            _volunteerFormService = volunteerFormService
                ?? throw new ArgumentNullException(nameof(volunteerFormService));
            _languageService = languageService
                ?? throw new ArgumentNullException(nameof(languageService));
            _locationService = locationService
                ?? throw new ArgumentNullException(nameof(locationService));
            _permissionGroupService = permissionGroupService
                ?? throw new ArgumentNullException(nameof(permissionGroupService));
            _segmentService = segmentService
                ?? throw new ArgumentNullException(nameof(segmentService));
        }

        public static string Area
        { get { return "SiteManagement"; } }

        public static string Name
        { get { return "Volunteer"; } }

        [Route("[action]")]
        [RestoreModelState]
        public IActionResult AddForm()
        {
            var volunteerTypes = _volunteerFormService.GetAllVolunteerFormTypes();
            return View(new AddFormViewModel
            {
                VolunteerFormsSelectList = volunteerTypes.Select(_ => new SelectListItem
                {
                    Text = _.Key,
                    Value = _.Value.ToString(CultureInfo.InvariantCulture)
                })
            });
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState]
        public async Task<IActionResult> AddForm(AddFormViewModel viewModel)
        {
            if (viewModel != null && ModelState.IsValid)
            {
                try
                {
                    var newForm = new VolunteerForm
                    {
                        VolunteerFormType = (VolunteerFormType)Enum.Parse(typeof(VolunteerFormType), viewModel.TypeName)
                    };
                    var form = await _volunteerFormService.AddUpdateFormAsync(newForm);
                    ShowAlertSuccess($"Added Form: {Enum.Parse(typeof(VolunteerFormType), viewModel.TypeName)}");
                    return RedirectToAction(nameof(Form), new { form.Id });
                }
                catch (OcudaException ex)
                {
                    ShowAlertDanger($"Unable to Create Form: {ex.Message}");
                    return RedirectToAction(nameof(AddForm));
                }
            }

            return RedirectToAction(nameof(AddForm));
        }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpPost]
        [Route("[action]/{type}")]
        public async Task<IActionResult> AddSegment(int type,
            string segmentText)
        {
            var form = await _volunteerFormService.GetFormByTypeAsync((VolunteerFormType)type);

            if (form == null)
            {
                ShowAlertDanger($"Record not found for form ID {type}.");
                return RedirectToAction(nameof(Index));
            }

            var languages = await _languageService.GetActiveAsync();

            var defaultLanguage = languages.SingleOrDefault(_ => _.IsActive && _.IsDefault)
                ?? languages.FirstOrDefault(_ => _.IsActive);

            if (defaultLanguage == null)
            {
                ShowAlertDanger("No default language configured.");
                return RedirectToAction(nameof(Form), new { form.Id });
            }

            var segment = await _segmentService.CreateAsync(new Segment
            {
                IsActive = true,
                Name = $"{form.VolunteerFormType} - Form Header",
            });

            await _segmentService.CreateSegmentTextAsync(new SegmentText
            {
                SegmentId = segment.Id,
                LanguageId = defaultLanguage.Id,
                Text = segmentText
            });

            form.HeaderSegmentId = segment.Id;

            await _volunteerFormService.EditAsync(form);

            ShowAlertSuccess($"Segment added.");
            return RedirectToAction(nameof(Form), new { form.Id });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> DisableForm(int id)
        {
            try
            {
                var form = await _volunteerFormService.GetFormByIdAsync(id);
                await _volunteerFormService.DisableAsync(form);
                ShowAlertSuccess($"Deleted form: {form.VolunteerFormType}");
            }
            catch (OcudaException ex)
            {
                _logger.LogError(ex, "Error deleting form: {Message}", ex.Message);
                ShowAlertDanger($"Error deleting form");
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Route("[action]")]
        [SaveModelState]
        public async Task<IActionResult> EditForm(VolunteerForm form)
        {
            if (form == null)
            {
                throw new ArgumentNullException(nameof(form));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _volunteerFormService.EditAsync(form);
                    ShowAlertSuccess($"Updated Form: {form.VolunteerFormType}");
                    return RedirectToAction(nameof(Form), new { form.Id });
                }
                catch (OcudaException ex)
                {
                    ShowAlertDanger($"Unable to Update Form: {form.VolunteerFormType}");
                    _logger.LogError(ex, "Problem updating form: {Message}", ex.Message);
                    return RedirectToAction(nameof(Form), new { form.Id });
                }
            }
            else
            {
                ShowAlertDanger($"Invalid Parameters: {form.VolunteerFormType}");
                return RedirectToAction(nameof(Form), new { form.Id });
            }
        }

        [Route("{Id}")]
        [RestoreModelState]
        public async Task<IActionResult> Form(int Id)
        {
            try
            {
                var form = await _volunteerFormService.GetFormByIdAsync(Id);

                string segmentName = string.Empty;
                int segmentId = 0;
                if (form.HeaderSegmentId.HasValue)
                {
                    segmentName = $"{form.VolunteerFormType} - Form Header";
                    segmentId = form.HeaderSegmentId.Value;
                }

                return View("Details", new DetailsViewModel
                {
                    TypeName = form.VolunteerFormType.ToString(),
                    FormId = form.Id,
                    FormTypeId = (int)form.VolunteerFormType,
                    HeaderSegmentName = segmentName,
                    HeaderSegmentId = segmentId
                });
            }
            catch (OcudaException ex)
            {
                ShowAlertDanger($"Unable to find Form {Id}: {ex.Message}");
                return RedirectToAction(nameof(VolunteerController.Index));
            }
        }

        [Route("")]
        [Route("[action]/{page}")]
        public async Task<IActionResult> Index(int page)
        {
            page = page == 0 ? 1 : page;
            if (!await HasAppPermissionAsync(_permissionGroupService,
                ApplicationPermission.FormManagement))
            {
                return RedirectToUnauthorized();
            }

            var itemsPerPage = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.UserInterface.ItemsPerPage);
            var filter = new BaseFilter(page, itemsPerPage);

            var formList = await _volunteerFormService.GetPaginatedListAsync(filter);

            var paginateModel = new PaginateModel
            {
                ItemCount = formList.Count,
                CurrentPage = page,
                ItemsPerPage = filter.Take.Value
            };
            if (paginateModel.PastMaxPage)
            {
                return RedirectToRoute(
                    new
                    {
                        page = paginateModel.LastPage ?? 1
                    });
            }

            var volunteerForms = new List<DetailsViewModel>();
            foreach (var vform in formList.Data)
            {
                var tempForm = new DetailsViewModel
                {
                    FormId = vform.Id,
                    FormTypeId = (int)vform.VolunteerFormType,
                    TypeName = vform.VolunteerFormType.ToString()
                };
                volunteerForms.Add(tempForm);
            }

            var viewModel = new IndexViewModel
            {
                VolunteerForms = volunteerForms,
                PaginateModel = paginateModel
            };

            return View(viewModel);
        }

        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        [HttpPost]
        [Route("[action]/{type}")]
        public async Task<IActionResult> RemoveSegment(int type)
        {
            var form = await _volunteerFormService.GetFormByTypeAsync((VolunteerFormType)type);

            if (form == null)
            {
                ShowAlertDanger($"Form not found for name {type}.");
                return RedirectToAction(nameof(Index));
            }

            int segmentId = form.HeaderSegmentId.Value;
            form.HeaderSegmentId = null;

            await _volunteerFormService.EditAsync(form);

            try
            {
                await _segmentService.DeleteAsync(segmentId);
                ShowAlertSuccess("Segment removed and deleted.");
            }
            catch (OcudaException oex)
            {
                string message = oex.Message;
                ShowAlertWarning($"Segment removed from this form but not deleted: {message}");
            }

            return RedirectToAction(nameof(Form), new { form.Id });
        }
    }
}