﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.ViewModels.Pages;
using Ocuda.Ops.Service;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers
{
    public class PagesController : BaseController
    {
        private readonly PageService _pageService;
        private readonly SectionService _sectionService;

        public PagesController(PageService pageService, SectionService sectionService)
        {
            _pageService = pageService ?? throw new ArgumentNullException(nameof(pageService));
            _sectionService = sectionService ?? throw new ArgumentNullException(nameof(sectionService));
        }

        public async Task<IActionResult> Index(string section, int page = 1)
        {
            var currentSection = await _sectionService.GetByPathAsync(section);

            var filter = new BlogFilter(page)
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

        //TODO use stub instead of id
        public async Task<IActionResult> View(int id)
        {
            var page = await _pageService.GetByIdAsync(id);

            page.Content = CommonMark.CommonMarkConverter.Convert(page.Content);

            return View(page);
        }
    }
}
