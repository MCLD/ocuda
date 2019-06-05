using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.ViewModels.Links;
using Ocuda.Ops.Models.Entities;
using Ocuda.Ops.Service.Filters;
using Ocuda.Ops.Service.Interfaces.Ops.Services;
using Ocuda.Utility.Keys;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers
{
    public class LinksController : BaseController<LinksController>
    {
        private readonly ILinkService _linkService;
        private readonly ISectionService _sectionService;

        public const string DefaultCategoryDisplayName = "[No Category]";

        public LinksController(ServiceFacades.Controller<LinksController> context,
            ILinkService linkService,
            ISectionService sectionService) : base(context)
        {
            _linkService = linkService ?? throw new ArgumentNullException(nameof(linkService));
            _sectionService = sectionService ?? throw new ArgumentNullException(nameof(sectionService));
        }

        public async Task<IActionResult> Library(string section, int id, int page = 1)
        {
            var currentSection = await _sectionService.GetByPathAsync(section);
            var currentLibrary = await _linkService.GetLibraryByIdAsync(id);

            if (currentLibrary?.SectionId != currentSection.Id)
            {
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }

            var itemsPerPage = await _siteSettingService
                .GetSettingIntAsync(Models.Keys.SiteSetting.UserInterface.ItemsPerPage);

            var filter = new BlogFilter(page, itemsPerPage)
            {
                SectionId = currentSection.Id,
                LinkLibraryId = id
            };

            var linkList = await _linkService.GetPaginatedListAsync(filter);

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

            var viewModel = new LibraryViewModel()
            {
                PaginateModel = paginateModel,
                Links = linkList.Data,
                Library = currentLibrary
            };

            return View(viewModel);
        }
    }
}
