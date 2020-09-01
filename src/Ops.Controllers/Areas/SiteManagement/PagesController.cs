using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.SiteManagement.ViewModels.Pages;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Models;
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
    public class PagesController : BaseController<PagesController>
    {
        private readonly ILanguageService _languageService;
        private readonly IPageService _pageService;
        private readonly IPermissionGroupService _permissionGroupService;
        private readonly ISocialCardService _socialCardService;

        public static string Name { get { return "Pages"; } }
        public static string Area { get { return "SiteManagement"; } }

        public PagesController(ServiceFacades.Controller<PagesController> context,
            ILanguageService languageService,
            IPageService pageService,
            IPermissionGroupService permissionGroupService,
            ISocialCardService socialCardService) : base(context)
        {
            _languageService = languageService
                ?? throw new ArgumentNullException(nameof(languageService));
            _pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
            _permissionGroupService = permissionGroupService
                ?? throw new ArgumentNullException(nameof(permissionGroupService));
            _socialCardService = socialCardService
                ?? throw new ArgumentNullException(nameof(socialCardService));
        }

        [Route("")]
        [Route("[action]/{page}")]
        public async Task<IActionResult> Index(int page = 1, PageType? Type = null)
        {
            var filter = new PageFilter(page)
            {
                PageType = Type ?? PageType.Home
            };

            var headerList = await _pageService.GetPaginatedHeaderListAsync(filter);

            var paginateModel = new PaginateModel
            {
                ItemCount = headerList.Count,
                CurrentPage = page,
                ItemsPerPage = filter.Take.Value
            };
            if (paginateModel.PastMaxPage)
            {
                return RedirectToRoute(
                    new
                    {
                        page = paginateModel.LastPage ?? 1,
                        Type
                    });
            }

            foreach (var header in headerList.Data)
            {
                header.PageLanguages = await _pageService.GetHeaderLanguagesByIdAsync(header.Id);
            }

            var viewModel = new IndexViewModel
            {
                PageHeaders = headerList.Data,
                PaginateModel = paginateModel,
                PageType = filter.PageType.Value,
                IsSiteManager = !string.IsNullOrEmpty(UserClaim(ClaimType.SiteManager)),
                PermissionIds = UserClaims(ClaimType.PermissionId)
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route("[action]")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> Create(IndexViewModel model)
        {
            JsonResponse response;

            var checkStub = new Regex(@"^[\w\-]*$");
            if (!checkStub.IsMatch(model.PageHeader.Stub))
            {
                ModelState.AddModelError("PageHeader.Stub", "Invalid stub; only letters, numbers, hyphens and underscores are allowed.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var header = await _pageService.CreateHeaderAsync(model.PageHeader);
                    response = new JsonResponse
                    {
                        Success = true
                    };

                    if (header.IsLayoutPage)
                    {
                        response.Url = Url.Action(nameof(Layouts), new { id = header.Id });
                    }
                    else
                    {
                        response.Url = Url.Action(nameof(Detail), new { id = header.Id });
                    }

                    ShowAlertSuccess($"Created page: {header.PageName}");
                }
                catch (OcudaException ex)
                {
                    response = new JsonResponse
                    {
                        Success = false,
                        Message = ex.Message
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
                    Message = string.Join(Environment.NewLine, errors)
                };
            }

            return Json(response);
        }

        [HttpPost]
        [Route("[action]")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> Edit(IndexViewModel model)
        {
            JsonResponse response;

            if (ModelState.IsValid)
            {
                try
                {
                    var header = await _pageService.EditHeaderAsync(model.PageHeader);
                    response = new JsonResponse
                    {
                        Success = true
                    };
                    ShowAlertSuccess($"Updated page: {header.PageName}");
                }
                catch (OcudaException ex)
                {
                    response = new JsonResponse
                    {
                        Success = false,
                        Message = ex.Message
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
                    Message = string.Join(Environment.NewLine, errors)
                };
            }

            return Json(response);
        }

        [HttpPost]
        [Route("[action]")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> Delete(IndexViewModel model)
        {
            try
            {
                await _pageService.DeleteHeaderAsync(model.PageHeader.Id);
                ShowAlertSuccess($"Deleted page: {model.PageHeader.PageName}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting page header: {Message}", ex.Message);
                ShowAlertDanger("Unable to delete page: ", ex.Message);
            }

            return RedirectToAction(nameof(Index), new
            {
                page = model.PaginateModel.CurrentPage,
                Type = model.PageType
            });
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<JsonResult> StubInUse(PageHeader pageHeader)
        {
            var response = new JsonResponse
            {
                Success = !(await _pageService.StubInUseAsync(pageHeader))
            };

            if (!response.Success)
            {
                response.Message = "The chosen stub is already in use for that page type. Please choose a different stub.";
            }

            return Json(response);
        }

        private async Task<bool> HasPagePermissionAsync(int pageHeaderId)
        {
            if (!string.IsNullOrEmpty(UserClaim(ClaimType.SiteManager)))
            {
                return true;
            }
            else
            {
                var permissionClaims = UserClaims(ClaimType.PermissionId);
                if (permissionClaims.Count > 0)
                {
                    var permissionGroups = await _permissionGroupService
                        .GetPagePermissionsAsync(pageHeaderId);
                    var permissionGroupsStrings = permissionGroups
                        .Select(_ => _.PermissionGroupId.ToString(CultureInfo.InvariantCulture));

                    return permissionClaims.Any(_ => permissionGroupsStrings.Contains(_));
                }
                return false;
            }
        }

        [Route("[action]/{id}")]
        [RestoreModelState]
        public async Task<IActionResult> Detail(int id, string language)
        {
            if (!await HasPagePermissionAsync(id))
            {
                return RedirectToUnauthorized();
            }

            var header = await _pageService.GetHeaderByIdAsync(id);

            if (header.IsLayoutPage)
            {
                return RedirectToAction(nameof(Layouts), new { id = header.Id });
            }

            var languages = await _languageService.GetActiveAsync();

            var selectedLanguage = languages
                .FirstOrDefault(_ => _.Name.Equals(language, StringComparison.OrdinalIgnoreCase))
                ?? languages.Single(_ => _.IsDefault);

            var page = await _pageService.GetByHeaderAndLanguageAsync(id, selectedLanguage.Id);

            var viewModel = new DetailViewModel
            {
                Page = page,
                HeaderId = header.Id,
                HeaderName = header.PageName,
                HeaderStub = header.Stub,
                HeaderType = header.Type,
                NewPage = page == null,
                LanguageId = selectedLanguage.Id,
                LanguageDescription = selectedLanguage.Description,
                LanguageList = new SelectList(languages, nameof(Language.Name),
                    nameof(Language.Description), selectedLanguage.Name),
                SocialCardList = new SelectList(await _socialCardService.GetListAsync(),
                    nameof(SocialCard.Id), nameof(SocialCard.Title), page?.SocialCardId),
                IsSiteManager = !string.IsNullOrEmpty(UserClaim(ClaimType.SiteManager))
            };

            if (page?.IsPublished == true)
            {
                var baseUrl = await _siteSettingService
                    .GetSettingStringAsync(Models.Keys.SiteSetting.SiteManagement.PromenadeUrl);

                if (!string.IsNullOrWhiteSpace(baseUrl))
                {
                    viewModel.PageUrl = $"{baseUrl}{selectedLanguage.Name}/{page.PageHeader.Type}/{page.PageHeader.Stub}";
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        [Route("[action]/{id?}")]
        [SaveModelState]
        public async Task<IActionResult> Detail(DetailViewModel model)
        {
            if (model == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if (!await HasPagePermissionAsync(model.HeaderId))
            {
                return RedirectToUnauthorized();
            }

            var language = await _languageService.GetActiveByIdAsync(model.LanguageId);

            if (ModelState.IsValid)
            {
                var page = model.Page;
                page.LanguageId = language.Id;
                page.PageHeaderId = model.HeaderId;

                var currentPage = await _pageService.GetByHeaderAndLanguageAsync(
                    model.HeaderId, language.Id);

                if (currentPage == null)
                {
                    await _pageService.CreateAsync(page);

                    ShowAlertSuccess("Added page content!");
                }
                else
                {
                    await _pageService.EditAsync(page);

                    ShowAlertSuccess("Updated page content!");
                }
            }

            return RedirectToAction(nameof(Detail), new
            {
                id = model.HeaderId,
                language = language.Name
            });
        }

        [HttpPost]
        [Route("[action]")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> DeletePage(DetailViewModel model)
        {
            var page = await _pageService.GetByHeaderAndLanguageAsync(model.HeaderId,
                model.LanguageId);

            await _pageService.DeleteAsync(page);

            var language = await _languageService.GetActiveByIdAsync(model.LanguageId);

            ShowAlertSuccess($"Deleted page {language.Description} content!");

            return RedirectToAction(nameof(Detail),
                new
                {
                    id = model.HeaderId,
                    language = language.Name
                });
        }

        [Route("[action]/{headerId}")]
        public async Task<IActionResult> Preview(int headerId, int languageId)
        {
            try
            {
                var page = await _pageService.GetByHeaderAndLanguageAsync(headerId, languageId);
                var language = await _languageService.GetActiveByIdAsync(languageId);

                var viewModel = new PreviewViewModel
                {
                    HeaderId = headerId,
                    Language = language.Name,
                    Content = CommonMark.CommonMarkConverter.Convert(page.Content),
                    Stub = page.PageHeader.Stub
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error previewing header {Header} in language {Language}: {Message}",
                    headerId,
                    languageId,
                    ex.Message);
                ShowAlertWarning("Unable to preview page");
                return RedirectToAction("Index");
            }
        }
        [Route("[action]/{id}")]
        [RestoreModelState]
        public async Task<IActionResult> Layouts(int id, int page = 1)
        {
            if (!await HasPagePermissionAsync(id))
            {
                return RedirectToUnauthorized();
            }

            var header = await _pageService.GetHeaderByIdAsync(id);

            if (!header.IsLayoutPage)
            {
                return RedirectToAction(nameof(Detail), new { id = header.Id });
            }

            var filter = new BaseFilter(page);

            var layoutList = await _pageService.GetPaginatedLayoutListForHeaderAsync(id, filter);

            var paginateModel = new PaginateModel
            {
                ItemCount = layoutList.Count,
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

            var viewModel = new LayoutsViewModel
            {
                PageLayouts = layoutList.Data,
                PaginateModel = paginateModel,
                HeaderId = header.Id,
                HeaderName = header.PageName,
                HeaderStub = header.Stub,
                HeaderType = header.Type,
                SocialCardList = new SelectList(await _socialCardService.GetListAsync(),
                    nameof(SocialCard.Id), nameof(SocialCard.Title))
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> CreateLayout(LayoutsViewModel model)
        {
            JsonResponse response;

            if (await HasPagePermissionAsync(model.PageLayout.PageHeaderId))
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        var layout = await _pageService.CreateLayoutAsync(model.PageLayout);
                        response = new JsonResponse
                        {
                            Success = true,
                            Url = Url.Action(nameof(LayoutDetail), new { id = layout.Id })
                        };

                        ShowAlertSuccess($"Created layout: {layout.Name}");
                    }
                    catch (OcudaException ex)
                    {
                        response = new JsonResponse
                        {
                            Success = false,
                            Message = ex.Message
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
                        Message = string.Join(Environment.NewLine, errors)
                    };
                }
            }
            else
            {
                response = new JsonResponse
                {
                    Message = "Unauthorized",
                    Success = false
                };
            }

            return Json(response);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> EditLayout(LayoutsViewModel model)
        {
            JsonResponse response;

            var pageLayout = await _pageService.GetLayoutByIdAsync(model.PageLayout.Id);

            if (await HasPagePermissionAsync(pageLayout.PageHeaderId))
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        var layout = await _pageService.EditLayoutAsync(model.PageLayout);
                        response = new JsonResponse
                        {
                            Success = true
                        };

                        ShowAlertSuccess($"Updated layout: {layout.Name}");
                    }
                    catch (OcudaException ex)
                    {
                        response = new JsonResponse
                        {
                            Success = false,
                            Message = ex.Message
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
                        Message = string.Join(Environment.NewLine, errors)
                    };
                }
            }
            else
            {
                response = new JsonResponse
                {
                    Message = "Unauthorized",
                    Success = false
                };
            }

            return Json(response);
        }

        [HttpPost]
        [Route("[action]")]
        public async Task<IActionResult> DeleteLayout(LayoutsViewModel model)
        {
            var pageLayout = await _pageService.GetLayoutByIdAsync(model.PageLayout.Id);

            if (!await HasPagePermissionAsync(pageLayout.PageHeaderId))
            {
                return RedirectToUnauthorized();
            }

            try
            {
                await _pageService.DeleteLayoutAsync(model.PageLayout.Id);
                ShowAlertSuccess($"Deleted layout: {model.PageLayout.Name}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting page layout: {Message}", ex.Message);
                ShowAlertDanger("Unable to delete layout: ", ex.Message);
            }

            return RedirectToAction(nameof(Layouts), new
            {
                id = pageLayout.PageHeaderId,
                page = model.PaginateModel.CurrentPage
            });
        }

        [Route("[action]/{id}")]
        public async Task<IActionResult> LayoutDetail(int id)
        {
            return View();
        }

        [Route("[action]/{id}")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> ContentPermissions(int id)
        {
            var header = await _pageService.GetHeaderByIdAsync(id);

            var permissionGroups = await _permissionGroupService.GetAllAsync();
            var pagePermissions = await _permissionGroupService.GetPagePermissionsAsync(id);

            var availableGroups = new Dictionary<int, string>();
            var assignedGroups = new Dictionary<int, string>();

            foreach (var permissionGroup in permissionGroups)
            {
                var permission = pagePermissions
                    .SingleOrDefault(_ => _.PermissionGroupId == permissionGroup.Id);
                if (permission == null)
                {
                    availableGroups.Add(permissionGroup.Id, permissionGroup.PermissionGroupName);
                }
                else
                {
                    assignedGroups.Add(permissionGroup.Id, permissionGroup.PermissionGroupName);
                }
            }

            return View(new ContentPermissionsViewModel
            {
                HeaderName = header.PageName,
                HeaderStub = header.Stub,
                HeaderType = header.Type,
                HeaderId = header.Id,
                AvailableGroups = availableGroups,
                AssignedGroups = assignedGroups
            });
        }

        [HttpPost]
        [Route("[action]/{headerId}/{permissionGroupId}")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> AddPermissionGroup(int headerId, int permissionGroupId)
        {
            try
            {
                await _permissionGroupService
                    .AddPageHeaderPermissionGroupAsync(headerId, permissionGroupId);
                AlertInfo = "Content permission added.";
            }
            catch (Exception ex)
            {
                AlertDanger = $"Problem adding permission: {ex.Message}";
            }

            return RedirectToAction(nameof(ContentPermissions), new { id = headerId });
        }

        [HttpPost]
        [Route("[action]/{headerId}/{permissionGroupId}")]
        [Authorize(Policy = nameof(ClaimType.SiteManager))]
        public async Task<IActionResult> RemovePermissionGroup(int headerId, int permissionGroupId)
        {
            try
            {
                await _permissionGroupService
                    .RemovePageHeaderPermissionGroupAsync(headerId, permissionGroupId);
                AlertInfo = "Content permission removed.";
            }
            catch (Exception ex)
            {
                AlertDanger = $"Problem removing permission: {ex.Message}";
            }

            return RedirectToAction(nameof(ContentPermissions), new { id = headerId });
        }
    }
}
