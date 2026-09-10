using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ocuda.Models;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Emedia;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Models;
using Ocuda.Ops.Models.Keys;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility;
using Ocuda.Utility.Abstract;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Extensions;
using Ocuda.Utility.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.SiteManagement
{
    [Area(nameof(SiteManagement))]
    [Route("[area]/[controller]")]
    public class EmediaController(ServiceFacades.Controller<EmediaController> context,
        ICategoryService categoryService,
        IDateTimeProvider dateTimeProvider,
        IEmediaService emediaService,
        ILanguageService languageService,
        IPermissionGroupService permissionGroupService,
        IReportingService reportingService,
        ISegmentService segmentService,
        ISiteSettingPromService siteSettingPromService,
        ISubjectService subjectService)
        : BaseController<EmediaController>(context)
    {
        public static string Area
        {
            get { return nameof(SiteManagement); }
        }

        public static string Name
        {
            get { return "Emedia"; }
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> AddReferer(ConfigureViewModel viewModel)
        {
            ArgumentNullException.ThrowIfNull(viewModel);

            if (string.IsNullOrWhiteSpace(viewModel.Referer))
            {
                ShowAlertWarning("Unable to add empty referer.");
                return RedirectToAction(nameof(Configure));
            }

            string referer = viewModel.Referer.Trim();

            var current = await siteSettingPromService
                .GetSettingStringAsync(Promenade.Models.Keys.SiteSetting.Emedia.ValidReferers);

            var asList = current.Split(',').ToList();

            if (asList.Contains(referer))
            {
                ShowAlertWarning("Referer is already present in list");
                return RedirectToAction(nameof(Configure));
            }

            var hostnameType = Uri.CheckHostName(referer);

            if (hostnameType != UriHostNameType.Dns)
            {
                ShowAlertWarning("Referer added, though it does not appear to be a valid hostname");
            }

            asList.Add(referer);

            await siteSettingPromService
                .UpdateAsync(Promenade.Models.Keys.SiteSetting.Emedia.ValidReferers,
                    string.Join(',', asList).Trim(','));

            ShowAlertSuccess($"Added referer: {referer}");
            return RedirectToAction(nameof(Configure));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ChangeCategories(int id, ICollection<int> categories)
        {
            JsonResponse response;

            if (!await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.EmediaManagement))
            {
                response = new JsonResponse
                {
                    Message = "Unauthorized",
                    Success = false,
                };
            }
            else
            {
                try
                {
                    await emediaService.UpdateCategoriesAsync(id, categories);
                    response = new JsonResponse
                    {
                        Success = true,
                    };
                }
                catch (OcudaException ex)
                {
                    response = new JsonResponse
                    {
                        Message = ex.Message,
                        Success = false,
                    };
                }
            }

            return Json(response);
        }

        [HttpPost("[action]")]
        public async Task<JsonResult> ChangeGroupSort(int id, bool increase)
        {
            JsonResponse response;

            if (!await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.EmediaManagement))
            {
                response = new JsonResponse
                {
                    Message = "Unauthorized",
                    Success = false,
                };
            }
            else
            {
                try
                {
                    await emediaService.UpdateGroupSortOrder(id, increase);
                    response = new JsonResponse
                    {
                        Success = true,
                    };
                }
                catch (OcudaException ex)
                {
                    response = new JsonResponse
                    {
                        Message = ex.Message,
                        Success = false,
                    };
                }
            }

            return Json(response);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> ChangeSubjects(int id, ICollection<int> subjects)
        {
            JsonResponse response;

            if (!await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.EmediaManagement))
            {
                response = new JsonResponse
                {
                    Message = "Unauthorized",
                    Success = false,
                };
            }
            else
            {
                try
                {
                    await emediaService.UpdateSubjectsAsync(id, subjects);
                    response = new JsonResponse
                    {
                        Success = true,
                    };
                }
                catch (OcudaException ex)
                {
                    response = new JsonResponse
                    {
                        Message = ex.Message,
                        Success = false,
                    };
                }
            }

            return Json(response);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Configure()
        {
            var validReferers = await siteSettingPromService
                .GetSettingStringAsync(Promenade.Models.Keys.SiteSetting.Emedia.ValidReferers);

            var invalidRefererShowResource = await siteSettingPromService.GetSettingBoolAsync(
                Promenade.Models.Keys.SiteSetting.Emedia.InvalidRefererShowResource);

            var launchDelay = await siteSettingPromService
                .GetSettingIntAsync(Promenade.Models.Keys.SiteSetting.Emedia.LaunchDelay);

            var viewModel = new ConfigureViewModel
            {
                AllSegmentId = await siteSettingPromService
                    .GetSettingIntAsync(Promenade.Models.Keys.SiteSetting.Emedia.AllSegment),
                ButtonAllSegmentId = await siteSettingPromService
                    .GetSettingIntAsync(Promenade.Models.Keys.SiteSetting.Emedia.ButtonAllSegment),
                ButtonGroupSegmentId = await siteSettingPromService.GetSettingIntAsync(
                    Promenade.Models.Keys.SiteSetting.Emedia.ButtonGroupSegment),
                HasReferers = !string.IsNullOrEmpty(validReferers),
                InvalidRefererBehavior =
                    [
                        new()
                        {
                             Text = "Redirect to index",
                             Value = "False",
                        },
                        new()
                        {
                            Selected = invalidRefererShowResource,
                            Text = "Launch resource",
                            Value = "True",
                        },
                    ],
                LaunchDelay = launchDelay,
                LaunchSegmentId = await siteSettingPromService
                    .GetSettingIntAsync(Promenade.Models.Keys.SiteSetting.Emedia.LaunchSegment),
                ValidReferers = validReferers.Split(','),
            };

            foreach (var language in await languageService.GetActiveAsync())
            {
                viewModel.Languages.Add(language.Name, language.Description);
            }

            viewModel.AllSegmentLanguages = viewModel.AllSegmentId > 0
                ? await segmentService.GetSegmentLanguagesByIdAsync(viewModel.AllSegmentId)
                : null;
            viewModel.ButtonAllSegmentLanguages = viewModel.ButtonAllSegmentId > 0
                ? await segmentService.GetSegmentLanguagesByIdAsync(viewModel.ButtonAllSegmentId)
                : null;
            viewModel.ButtonGroupSegmentLanguages = viewModel.ButtonGroupSegmentId > 0
                ? await segmentService.GetSegmentLanguagesByIdAsync(viewModel.ButtonGroupSegmentId)
                : null;
            viewModel.LaunchSegmentLanguages = viewModel.LaunchSegmentId > 0
                ? await segmentService.GetSegmentLanguagesByIdAsync(viewModel.LaunchSegmentId)
                : null;

            return View(viewModel);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CreateAllSegment()
        {
            return await RedirectToNewSegmentAsync("Emedia All Page Header",
                i18n.Keys.Promenade.EmediaAll,
                null,
                Promenade.Models.Keys.SiteSetting.Emedia.AllSegment);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CreateAllViewButton()
        {
            return await RedirectToNewSegmentAsync("Emedia All View Button",
                null,
                i18n.Keys.Promenade.EmediaAll,
                Promenade.Models.Keys.SiteSetting.Emedia.ButtonAllSegment);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CreateEmedia(GroupDetailsViewModel model)
        {
            JsonResponse response;

            if (model == null)
            {
                response = new JsonResponse
                {
                    Success = false,
                    Message = "Unable to create empty emedia item.",
                };
            }
            else if (!await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.EmediaManagement))
            {
                response = new JsonResponse
                {
                    Message = "Unauthorized",
                    Success = false,
                };
            }
            else if (ModelState.IsValid)
            {
                try
                {
                    var group = await emediaService.CreateAsync(model.Emedia);
                    response = new JsonResponse
                    {
                        Success = true,
                        Url = Url.Action(nameof(Details), new { id = group.Id }),
                    };
                }
                catch (OcudaException ex)
                {
                    response = new JsonResponse
                    {
                        Success = false,
                        Message = ex.Message,
                    };
                }
            }
            else
            {
                var errors = ModelState.Values
                    .SelectMany(_ => _.Errors)
                    .Select(_ => _.ErrorMessage);

                response = new JsonResponse
                {
                    Success = false,
                    Message = string.Join(Environment.NewLine, errors),
                };
            }

            return Json(response);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CreateGroup(IndexViewModel model)
        {
            JsonResponse response;

            if (model == null)
            {
                response = new JsonResponse
                {
                    Success = false,
                    Message = "Unable to create empty group.",
                };
            }
            else if (!await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.EmediaManagement))
            {
                response = new JsonResponse
                {
                    Message = "Unauthorized",
                    Success = false,
                };
            }
            else if (ModelState.IsValid)
            {
                try
                {
                    var group = await emediaService.CreateGroupAsync(model.EmediaGroup);
                    response = new JsonResponse
                    {
                        Success = true,
                        Url = Url.Action(nameof(GroupDetails), new { id = group.Id }),
                    };
                }
                catch (OcudaException ex)
                {
                    response = new JsonResponse
                    {
                        Success = false,
                        Message = ex.Message,
                    };
                }
            }
            else
            {
                var errors = ModelState.Values
                    .SelectMany(_ => _.Errors)
                    .Select(_ => _.ErrorMessage);

                response = new JsonResponse
                {
                    Success = false,
                    Message = string.Join(Environment.NewLine, errors),
                };
            }

            return Json(response);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CreateGroupViewButton()
        {
            return await RedirectToNewSegmentAsync("Emedia Group View Button",
                null,
                i18n.Keys.Promenade.EmediaGroups,
                Promenade.Models.Keys.SiteSetting.Emedia.ButtonGroupSegment);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> CreateLaunchSegment()
        {
            return await RedirectToNewSegmentAsync("Emedia Launch Text",
                null,
                i18n.Keys.Promenade.LaunchSegment,
                Promenade.Models.Keys.SiteSetting.Emedia.LaunchSegment);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> DeleteEmedia(GroupDetailsViewModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            if (!await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.EmediaManagement))
            {
                return RedirectToUnauthorized();
            }

            try
            {
                await emediaService.DeleteAsync(model.Emedia.Id);
                ShowAlertSuccess($"Deleted emedia : {model.Emedia.Name}");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error deleting emedia: {Message}", ex.Message);
                ShowAlertDanger($"Error deleting emedia: {model.Emedia.Name}");
            }

            return RedirectToAction(nameof(GroupDetails),
                new
                {
                    id = model.EmediaGroup.Id,
                    page = model.PaginateModel.CurrentPage,
                });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> DeleteGroup(IndexViewModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            if (!await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.EmediaManagement))
            {
                return RedirectToUnauthorized();
            }

            try
            {
                await emediaService.DeleteGroupAsync(model.EmediaGroup.Id);
                ShowAlertSuccess($"Deleted emedia group: {model.EmediaGroup.Name}");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error deleting emedia group: {Message}", ex.Message);
                ShowAlertDanger($"Error deleting emedia group: {model.EmediaGroup.Name}");
            }

            return RedirectToAction(nameof(Index), new { page = model.CurrentPage });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> DeleteGroupSegment(GroupDetailsViewModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            if (!await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.EmediaManagement))
            {
                return RedirectToUnauthorized();
            }

            try
            {
                await emediaService.DeleteGroupSegmentAsync(model.EmediaGroupId);
                ShowAlertSuccess($"Deleted group segment: {model.Segment.Name}");
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error deleting group segment: {Message}", ex.Message);
                ShowAlertDanger($"Error deleting group segment: {model.Segment.Name}");
            }

            return RedirectToAction(nameof(GroupDetails), new
            {
                id = model.EmediaGroupId,
                page = model.PaginateModel.CurrentPage,
            });
        }

        [HttpGet("[action]/{id}")]
        [RestoreModelState]
        public async Task<IActionResult> Details(int id, string language)
        {
            if (!await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.EmediaManagement))
            {
                return RedirectToUnauthorized();
            }

            var emedia = await emediaService.GetIncludingGroupAsync(id);

            if (emedia == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var languages = await languageService.GetActiveAsync();

            var selectedLanguage = languages
                .FirstOrDefault(_ => _.Name.Equals(language, StringComparison.OrdinalIgnoreCase))
                ?? languages.Single(_ => _.IsDefault);

            var emediaText = await emediaService.GetTextByEmediaAndLanguageAsync(emedia.Id,
                selectedLanguage.Id);

            var selectedTopics = await emediaService.GetSubjectsForEmediaAsync(emedia.Id);

            var selectedCategories = await emediaService.GetCategoriesForEmediaAsync(emedia.Id);

            var viewModel = new DetailsViewModel
            {
                CategorySelectionText = string.Join(", ",
                    selectedCategories.Select(_ => _.Name).Order()),
                Emedia = emedia,
                EmediaText = emediaText,
                LanguageId = selectedLanguage.Id,
                LanguageList = new SelectList(languages,
                    nameof(Language.Name),
                    nameof(Language.Description),
                    selectedLanguage.Name),
                SubjectSelectionText = string.Join(", ",
                    selectedTopics.Select(_ => _.Name).Order()),
            };

            viewModel.CategoryList.AddRange(await categoryService.GetAllAsync());
            viewModel.CategorySelection.AddRange([.. selectedCategories.Select(_ => _.Id)]);

            viewModel.SubjectList.AddRange(await subjectService.GetAllAsync());
            viewModel.SubjectSelection.AddRange([.. selectedTopics.Select(_ => _.Id)]);

            return View(viewModel);
        }

        [HttpPost("[action]/{id}")]
        [SaveModelState]
        public async Task<IActionResult> Details(int id, DetailsViewModel model)
        {
            ArgumentNullException.ThrowIfNull(model?.EmediaText);

            if (!await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.EmediaManagement))
            {
                return RedirectToUnauthorized();
            }

            model.EmediaText.EmediaId = id;

            if (ModelState.IsValid)
            {
                try
                {
                    await emediaService.SetEmediaTextAsync(model.EmediaText);
                    ShowAlertSuccess("Updated emedia text");
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Error updating emedia text: {Message}", ex.Message);
                    ShowAlertDanger("Error updating emedia text");
                }
            }

            var language = await languageService.GetActiveByIdAsync(model.EmediaText.LanguageId);

            return RedirectToAction(nameof(Details), new
            {
                id = model.EmediaText.EmediaId,
                language = language.IsDefault ? null : language.Name,
            });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> EditEmedia(GroupDetailsViewModel model)
        {
            JsonResponse response;

            if (model == null)
            {
                response = new JsonResponse
                {
                    Success = false,
                    Message = "Unable to edit empty emedia item.",
                };
            }
            else if (!await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.EmediaManagement))
            {
                response = new JsonResponse
                {
                    Message = "Unauthorized",
                    Success = false,
                };
            }
            else if (ModelState.IsValid)
            {
                try
                {
                    var emedia = await emediaService.EditAsync(model.Emedia);
                    response = new JsonResponse
                    {
                        Success = true,
                    };

                    ShowAlertSuccess($"Updated emedia: {emedia.Name}");
                }
                catch (OcudaException ex)
                {
                    response = new JsonResponse
                    {
                        Success = false,
                        Message = ex.Message,
                    };
                }
            }
            else
            {
                var errors = ModelState.Values
                    .SelectMany(_ => _.Errors)
                    .Select(_ => _.ErrorMessage);

                response = new JsonResponse
                {
                    Success = false,
                    Message = string.Join(Environment.NewLine, errors),
                };
            }

            return Json(response);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> EditGroup(IndexViewModel model)
        {
            JsonResponse response;

            if (model == null)
            {
                response = new JsonResponse
                {
                    Success = false,
                    Message = "Unable to edit empty group.",
                };
            }
            else if (!await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.EmediaManagement))
            {
                response = new JsonResponse
                {
                    Message = "Unauthorized",
                    Success = false,
                };
            }
            else if (ModelState.IsValid)
            {
                try
                {
                    var group = await emediaService.EditGroupAsync(model.EmediaGroup);
                    response = new JsonResponse
                    {
                        Success = true,
                    };

                    ShowAlertSuccess($"Updated group: {group.Name}");
                }
                catch (OcudaException ex)
                {
                    response = new JsonResponse
                    {
                        Success = false,
                        Message = ex.Message,
                    };
                }
            }
            else
            {
                var errors = ModelState.Values
                    .SelectMany(_ => _.Errors)
                    .Select(_ => _.ErrorMessage);

                response = new JsonResponse
                {
                    Success = false,
                    Message = string.Join(Environment.NewLine, errors),
                };
            }

            return Json(response);
        }

        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> Export(int id)
        {
            if (!await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.EmediaManagement))
            {
                return RedirectToUnauthorized();
            }

            string publicSitePath = await _siteSettingService
                .GetSettingStringAsync(Models.Keys.SiteSetting.SiteManagement.PromenadeUrl);

            string intranetPath = await _siteSettingService
                .GetSettingStringAsync(Models.Keys.SiteSetting.UserInterface.BaseIntranetLink);

            return File(JsonSerializer.SerializeToUtf8Bytes(new PortableList<ESourceImport>
            {
                ExportedAt = dateTimeProvider.Now,
                ExportedBy = CurrentUsername,
                Items = await emediaService.ExportItemsAsync(id),
                Source = $"{publicSitePath} (via {intranetPath})",
                Type = nameof(Navigation),
                Version = 1,
            }),
                System.Net.Mime.MediaTypeNames.Application.Json,
                $"{dateTimeProvider.Now:yyyyMMdd}-{nameof(Emedia)}.json");
        }

        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> GroupDetails(int id, int page = 1)
        {
            if (!await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.EmediaManagement))
            {
                return RedirectToUnauthorized();
            }

            var group = await emediaService.GetGroupIncludingSegmentAsync(id);

            if (group == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var itemsPerPage = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.UserInterface.ItemsPerPage);

            var filter = new BaseFilter(page, itemsPerPage);

            var emediaList = await emediaService.GetPaginatedListForGroupAsync(group.Id, filter);

            var paginateModel = new PaginateModel
            {
                ItemCount = emediaList.Count,
                CurrentPage = page,
                ItemsPerPage = filter.Take.Value,
            };
            if (paginateModel.PastMaxPage)
            {
                return RedirectToRoute(
                    new
                    {
                        page = paginateModel.LastPage ?? 1,
                    });
            }

            foreach (var emedia in emediaList.Data)
            {
                var languages = await emediaService.GetEmediaLanguagesAsync(emedia.Id);
                emedia.EmediaLanguages.AddRange(languages);
            }

            var viewModel = new GroupDetailsViewModel
            {
                EmediaGroup = group,
                EmediaGroupId = id,
                Emedias = emediaList.Data,
                PaginateModel = paginateModel,
            };

            if (group.SegmentId.HasValue)
            {
                viewModel.SegmentLanguages = await segmentService
                    .GetSegmentLanguagesByIdAsync(group.SegmentId.Value);
            }

            return View(viewModel);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Import(int groupId, IFormFile jsonImportFile)
        {
            if (!await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.EmediaManagement))
            {
                return RedirectToUnauthorized();
            }

            if (jsonImportFile == null)
            {
                return StatusCode(500);
            }

            var jsonStream = jsonImportFile.OpenReadStream();

            var dataFromFile = await JsonSerializer
                .DeserializeAsync<PortableList<ESourceImport>>(jsonStream);

            await emediaService.ImportItemsAsync(groupId, dataFromFile.Items);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(int? page)
        {
            page ??= 1;

            if (!await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.EmediaManagement))
            {
                return RedirectToUnauthorized();
            }

            var itemsPerPage = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.UserInterface.ItemsPerPage);

            var filter = new BaseFilter(page, itemsPerPage);

            var groupList = await emediaService.GetPaginatedGroupListAsync(filter);

            var viewModel = new IndexViewModel
            {
                ItemCount = groupList.Count,
                CurrentPage = page.Value,
                ItemsPerPage = filter.Take.Value,
            };

            if (viewModel.PastMaxPage)
            {
                return RedirectToRoute(
                    new
                    {
                        page = viewModel.LastPage ?? 1,
                    });
            }

            viewModel.EmediaGroups.AddRange(groupList.Data);

            var reportsNoPermissions = reportingService.GetList(new BaseFilter<string>
            {
                Data = Models.Definitions.ReportDefinitions.ReportTypeDigitalLibrary,
            });

            var reports = await PopulatePermissionsAsync(permissionGroupService,
                reportsNoPermissions.Data);

            viewModel.ReportLinks.AddRange(reports
                .Where(_ => _.IsPermittedView)
                .ToDictionary(k => k.Name,
                    v => Url.Action(nameof(Reporting.HomeController.Details),
                        Reporting.HomeController.Name,
                        new
                        {
                            Reporting.HomeController.Area,
                            ReportId = v.Id,
                        })));

            return View(viewModel);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> RemoveAllSegment()
        {
            await RemoveSegment(Promenade.Models.Keys.SiteSetting.Emedia.AllSegment);
            return RedirectToAction(nameof(Configure));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> RemoveAllViewButton()
        {
            await RemoveSegment(Promenade.Models.Keys.SiteSetting.Emedia.ButtonAllSegment);
            return RedirectToAction(nameof(Configure));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> RemoveGroupViewButton()
        {
            await RemoveSegment(Promenade.Models.Keys.SiteSetting.Emedia.ButtonGroupSegment);
            return RedirectToAction(nameof(Configure));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> RemoveLaunchSegment()
        {
            await RemoveSegment(Promenade.Models.Keys.SiteSetting.Emedia.LaunchSegment);
            return RedirectToAction(nameof(Configure));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> RemoveReferer(string referer)
        {
            if (string.IsNullOrWhiteSpace(referer))
            {
                ShowAlertWarning("Unable to remove empty referer.");
                return RedirectToAction(nameof(Configure));
            }

            var current = await siteSettingPromService
                .GetSettingStringAsync(Promenade.Models.Keys.SiteSetting.Emedia.ValidReferers);

            var asList = current.Split(',').ToList();

            if (!asList.Contains(referer))
            {
                ShowAlertWarning("Referer is not present in list");
                return RedirectToAction(nameof(Configure));
            }

            asList.Remove(referer);

            await siteSettingPromService
                .UpdateAsync(Promenade.Models.Keys.SiteSetting.Emedia.ValidReferers,
                    string.Join(',', asList).Trim(','));

            ShowAlertSuccess($"Removed referer: {referer}");
            return RedirectToAction(nameof(Configure));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> SetInvalidRefererBehavior(bool invalidRefererBehavior)
        {
            await siteSettingPromService.UpdateAsync(
                Promenade.Models.Keys.SiteSetting.Emedia.InvalidRefererShowResource,
                invalidRefererBehavior.ToString(CultureInfo.InvariantCulture));
            string name = Promenade.Models.Defaults.SiteSettings
                .Get
                .Single(_ => _.Id
                    == Promenade.Models.Keys.SiteSetting.Emedia.InvalidRefererShowResource)
                .Name;
            ShowAlertSuccess($"Updated setting: {name}");

            return RedirectToAction(nameof(Configure));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> SetLaunchDelay(int launchDelay)
        {
            await siteSettingPromService.UpdateAsync(
                Promenade.Models.Keys.SiteSetting.Emedia.LaunchDelay,
                launchDelay.ToString(CultureInfo.InvariantCulture));
            string name = Promenade.Models.Defaults.SiteSettings
                .Get
                .Single(_ => _.Id == Promenade.Models.Keys.SiteSetting.Emedia.LaunchDelay)
                .Name;
            ShowAlertSuccess($"Updated setting: {name}");
            return RedirectToAction(nameof(Configure));
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> UpdateGroupSegment(GroupDetailsViewModel model)
        {
            JsonResponse response;

            if (model == null)
            {
                response = new JsonResponse
                {
                    Success = false,
                    Message = "Unable to update empty group.",
                };
            }
            else if (!await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.EmediaManagement))
            {
                response = new JsonResponse
                {
                    Message = "Unauthorized",
                    Success = false,
                };
            }
            else if (ModelState.IsValid)
            {
                try
                {
                    string url = null;

                    var group = await emediaService.GetGroupByIdAsync(model.EmediaGroupId);
                    if (group.SegmentId.HasValue)
                    {
                        var segment = await segmentService.GetByIdAsync(group.SegmentId.Value);
                        segment.Name = model.Segment.Name;
                        segment = await segmentService.EditAsync(segment);
                        ShowAlertSuccess($"Updated group segment: {segment.Name}");
                    }
                    else
                    {
                        group.Segment = model.Segment;
                        group.Segment.IsActive = true;
                        await emediaService.AddGroupSegmentAsync(group);
                        url = Url.Action(
                            nameof(SegmentsController.Detail),
                            SegmentsController.Name,
                            new { id = group.SegmentId });
                    }

                    response = new JsonResponse
                    {
                        Success = true,
                        Url = url,
                    };
                }
                catch (DbUpdateException ex)
                {
                    response = new JsonResponse
                    {
                        Success = false,
                        Message = ex.Message,
                    };
                }
            }
            else
            {
                var errors = ModelState.Values
                    .SelectMany(_ => _.Errors)
                    .Select(_ => _.ErrorMessage);

                response = new JsonResponse
                {
                    Success = false,
                    Message = string.Join(Environment.NewLine, errors),
                };
            }

            return Json(response);
        }

        [HttpPost("[action]")]
        [SaveModelState]
        public async Task<IActionResult> UpdateSortAs(int id, string sortAs)
        {
            if (!await HasAppPermissionAsync(permissionGroupService,
                ApplicationPermission.EmediaManagement))
            {
                return RedirectToUnauthorized();
            }

            try
            {
                await emediaService.SetSortAsAsync(id, sortAs);
            }
            catch (OcudaException oex)
            {
                _logger.LogError(oex,
                    "Error updating emedia {Id} sort as text to {SortAs}: {ErrorMessage}",
                    id,
                    sortAs,
                    oex.Message);
                ShowAlertDanger($"Could not update Sort As text: {oex.Message}");
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task<IActionResult> RedirectToNewSegmentAsync(string name,
            string header,
            string text,
            string promSiteSettingKey)
        {
            ArgumentNullException.ThrowIfNull(name);

            if (string.IsNullOrWhiteSpace(text) && string.IsNullOrWhiteSpace(header))
            {
                throw new ArgumentException("Header or text must be provided to create a segment.");
            }

            var segment = await segmentService.CreateAsync(new Segment
            {
                IsActive = true,
                Name = name,
            });

            var defaultLanguageId = await languageService.GetDefaultLanguageId();

            await segmentService.CreateSegmentTextAsync(new SegmentText
            {
                Header = string.IsNullOrWhiteSpace(header) ? null : header.Trim(),
                LanguageId = defaultLanguageId,
                SegmentId = segment.Id,
                Text = string.IsNullOrWhiteSpace(text) ? null : text.Trim(),
            });

            if (!string.IsNullOrWhiteSpace(promSiteSettingKey))
            {
                await siteSettingPromService.UpdateAsync(promSiteSettingKey,
                    segment.Id.ToString(CultureInfo.InvariantCulture));
            }

            var defaultLanguage = await languageService.GetActiveByIdAsync(defaultLanguageId);

            return RedirectToAction(nameof(SegmentsController.Detail),
                SegmentsController.Name,
                new
                {
                    area = SegmentsController.Area,
                    id = segment.Id,
                    language = defaultLanguage.Name,
                });
        }

        private async Task RemoveSegment(string promSiteSettingKey)
        {
            var segmentId = await siteSettingPromService.GetSettingIntAsync(promSiteSettingKey);

            if (segmentId > 0)
            {
                await siteSettingPromService.UpdateAsync(promSiteSettingKey, "-1");
                await segmentService.DeleteWithTextsAlreadyVerifiedAsync(segmentId);
            }
            else
            {
                ShowAlertWarning("Unable to delete invalid text segment.");
            }
        }
    }
}
