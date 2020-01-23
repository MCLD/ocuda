using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.Areas.Admin.ViewModels.Pages;
using Ocuda.Ops.Controllers.Filters;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Promenade.Services;
using Ocuda.Promenade.Models.Entities;
using Ocuda.Utility.Exceptions;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers.Areas.Admin
{
    [Area("Admin")]
    [Authorize(Policy = nameof(ClaimType.SiteManager))]
    [Route("[area]/[controller]")]
    public class PagesController : BaseController<PagesController>
    {
        private readonly ILanguageService _languageService;
        private readonly IPageService _pageService;
        private readonly ISocialCardService _socialCardService;

        public static string Name { get { return "Pages"; } }
        public static string Area { get { return "Admin"; } }

        public PagesController(ServiceFacades.Controller<PagesController> context,
            ILanguageService languageService,
            IPageService pageService,
            ISocialCardService socialCardService) : base(context)
        {
            _languageService = languageService
                ?? throw new ArgumentNullException(nameof(languageService));
            _pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
            _socialCardService = socialCardService
                ?? throw new ArgumentNullException(nameof(socialCardService));
        }

        [Route("")]
        [Route("[action]/{page}")]
        public async Task<IActionResult> Index(int page = 1)
        {
            var filter = new BaseFilter(page);

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
                        page = paginateModel.LastPage ?? 1
                    });
            }

            foreach (var header in headerList.Data)
            {
                header.PageLanguages = await _pageService.GetHeaderLanguagesByIdAsync(header.Id);
            }

            var viewModel = new IndexViewModel
            {
                PageHeaders = headerList.Data,
                PaginateModel = paginateModel
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route("[action]")]
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
                        Success = true,
                        Url = Url.Action(nameof(Detail), new { id = header.Id })
                    };
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

            return RedirectToAction(nameof(Index), new { page = model.PaginateModel.CurrentPage });
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

        [Route("[action]/{id}")]
        [RestoreModelState]
        public async Task<IActionResult> Detail(int id, string language)
        {
            var header = await _pageService.GetHeaderByIdAsync(id);

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
                NewPage = page == null,
                SelectedLanguageId = selectedLanguage.Id,
                LanguageList = new SelectList(languages, nameof(Language.Name),
                    nameof(Language.Description), selectedLanguage.Name),
                SocialCardList = new SelectList(await _socialCardService.GetListAsync(),
                    nameof(SocialCard.Id), nameof(SocialCard.Title), page?.SocialCardId)
            };

            if (page?.IsPublished == true)
            {
                var baseUrl = await _siteSettingService
                    .GetSettingStringAsync(Models.Keys.SiteSetting.SiteManagement.PromenadeUrl);

                if (!string.IsNullOrWhiteSpace(baseUrl))
                {
                    viewModel.PageUrl = $"{baseUrl}{selectedLanguage.Name}/{page.PageHeader.Type.ToString()}/{page.PageHeader.Stub}";
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        [Route("[action]/{id?}")]
        [SaveModelState]
        public async Task<IActionResult> Detail(DetailViewModel model)
        {
            var language = await _languageService.GetActiveByIdAsync(model.SelectedLanguageId);

            var currentPage = await _pageService.GetByHeaderAndLanguageAsync(
                    model.HeaderId, language.Id);

            if (ModelState.IsValid)
            {
                var page = model.Page;
                page.LanguageId = language.Id;
                page.PageHeaderId = model.HeaderId;

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
        public async Task<IActionResult> DeletePage(DetailViewModel model)
        {
            var page = await _pageService.GetByHeaderAndLanguageAsync(model.HeaderId,
                model.SelectedLanguageId);

            await _pageService.DeleteAsync(page);

            var language = await _languageService.GetActiveByIdAsync(model.SelectedLanguageId);

            ShowAlertSuccess($"Deleted page content!");

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
    }
}
