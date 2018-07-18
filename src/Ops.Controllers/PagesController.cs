using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.ViewModels.Pages;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers
{
    public class PagesController : BaseController<PagesController>
    {
        private readonly IPageService _pageService;
        private readonly ISectionService _sectionService;

        public PagesController(ServiceFacade.Controller<PagesController> context,
            IPageService pageService, 
            ISectionService sectionService) : base(context)
        {
            _pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
            _sectionService = sectionService 
                ?? throw new ArgumentNullException(nameof(sectionService));
        }

        public async Task<IActionResult> Index(string section, int page = 1)
        {
            var currentSection = await _sectionService.GetByPathAsync(section);
            var itemsPerPage = await _siteSettingService
                .GetSettingIntAsync(SiteSettingKey.Pagination.ItemsPerPage);

            var filter = new BlogFilter(page, itemsPerPage)
            {
                SectionId = currentSection.Id
            };

            var pageList = await _pageService.GetPaginatedListAsync(filter);

            var paginateModel = new PaginateModel
            {
                ItemCount = pageList.Count,
                CurrentPage = page,
                ItemsPerPage = filter.Take.Value
            };

            if (paginateModel.MaxPage > 0 && paginateModel.CurrentPage > paginateModel.MaxPage)
            {
                return RedirectToRoute(
                    new
                    {
                        page = paginateModel.LastPage ?? 1
                    });
            }

            var viewModel = new IndexViewModel
            {
                PaginateModel = paginateModel,
                Pages = pageList.Data
            };

            return View(viewModel);
        }

        public new async Task<IActionResult> Display(string section, string id)
        {
            var currentSection = await _sectionService.GetByPathAsync(section);
            var page = await _pageService.GetByStubAndSectionIdAsync(id, currentSection.Id);

            if(page != null)
            {
                page.Content = CommonMark.CommonMarkConverter.Convert(page.Content);
                return View(page);
            }
            else
            {
                ShowAlertDanger($"Could not find page '{id}' in '{currentSection.Name}'.");
                return RedirectToAction(nameof(PagesController.Index));
            }

            
        }
    }
}
