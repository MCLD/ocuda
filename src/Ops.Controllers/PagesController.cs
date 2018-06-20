using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.ViewModels.Pages;
using Ocuda.Ops.Service;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers
{
    public class PagesController : BaseController
    {
        private readonly PageService _pageService;
        
        public PagesController(PageService pageService)
        {
            _pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var pageList = await _pageService.GetPagesAsync();

            var paginateModel = new PaginateModel
            {
                ItemCount = await _pageService.GetPageCountAsync(),
                CurrentPage = page,
                ItemsPerPage = 2
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
                Pages = pageList.Skip((page - 1) * paginateModel.ItemsPerPage)
                                .Take(paginateModel.ItemsPerPage)
            };

            return View(viewModel);
        }

        //TODO use stub instead of id
        public async Task<IActionResult> View(int id)
        {
            var page = await _pageService.GetPageByIdAsync(id);

            page.Content = CommonMark.CommonMarkConverter.Convert(page.Content);

            return View(page);
        }
    }
}
