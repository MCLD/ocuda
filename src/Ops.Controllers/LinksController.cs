using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.ViewModels.Links;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers
{
    public class LinksController : BaseController<LinksController>
    {
        private readonly ICategoryService _categoryService;
        private readonly ILinkService _linkService;
        private readonly ISectionService _sectionService;

        public const string DefaultCategoryDisplayName = "[No Category]";

        public LinksController(ServiceFacade.Controller<LinksController> context,
            ILinkService linkService,
            ICategoryService categoryService,
            ISectionService sectionService) : base(context)
        {
            _linkService = linkService ?? throw new ArgumentNullException(nameof(linkService));
            _categoryService = categoryService
                ?? throw new ArgumentNullException(nameof(categoryService));
            _sectionService = sectionService ?? throw new ArgumentNullException(nameof(sectionService));
        }

        public async Task<IActionResult> Index(string section, int? categoryId = null, int page = 1)
        {
            var currentSection = await _sectionService.GetByPathAsync(section);
            var itemsPerPage = await _siteSettingService.GetSetting(SiteSettingKey.Pagination.ItemsPerPage);
            int.TryParse(itemsPerPage, out int take);

            var filter = new BlogFilter(page, take)
            {
                SectionId = currentSection.Id,
                CategoryId = categoryId,
                CategoryType = CategoryType.Link
            };

            var linkList = await _linkService.GetPaginatedListAsync(filter);
            var categoryList = await _categoryService.GetBySectionIdAsync(filter);

            var paginateModel = new PaginateModel()
            {
                ItemCount = linkList.Count,
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

            var viewModel = new IndexViewModel()
            {
                PaginateModel = paginateModel,
                Links = linkList.Data,
                Categories = categoryList
            };

            if (categoryId.HasValue)
            {
                var name = (await _categoryService.GetCategoryByIdAsync(categoryId.Value)).Name;
                viewModel.CategoryName =
                    string.IsNullOrWhiteSpace(name) ? DefaultCategoryDisplayName : name;
            }

            return View(viewModel);
        }
    }
}
