﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Ocuda.Ops.Controllers.Abstract;
using Ocuda.Ops.Controllers.ViewModels.Files;
using Ocuda.Ops.Models;
using Ocuda.Ops.Service;
using Ocuda.Ops.Service.Filters;
using Ocuda.Utility.Models;

namespace Ocuda.Ops.Controllers
{
    public class FilesController : BaseController<FilesController>
    {
        private readonly CategoryService _categoryService;
        private readonly FileService _fileService;
        private readonly SectionService _sectionService;

        public FilesController(ServiceFacade.Controller<FilesController> context,
            CategoryService categoryService,
            FileService fileService,
            SectionService sectionService
            ) : base(context)
        {
            _categoryService = categoryService
                ?? throw new ArgumentNullException(nameof(categoryService));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _sectionService = sectionService
                ?? throw new ArgumentNullException(nameof(sectionService));
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
                CategoryType = CategoryType.File
            };

            var fileList = await _fileService.GetPaginatedListAsync(filter);
            var categoryList = await _categoryService.GetBySectionIdAsync(filter);

            var paginateModel = new PaginateModel()
            {
                ItemCount = fileList.Count,
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
                Files = fileList.Data,
                Categories = categoryList
            };

            if (categoryId.HasValue)
            {
                viewModel.CategoryName =
                    (await _categoryService.GetCategoryByIdAsync(categoryId.Value)).Name;
            }

            return View(viewModel);
        }

        public async Task<IActionResult> ViewPrivateFile(int id)
        {
            var file = await _fileService.GetByIdAsync(id);
            var fileBytes = await _fileService.ReadPrivateFileAsync(file);
            string fileName = $"{file.Name}{file.Extension}";
            try
            {
                var typeProvider = new FileExtensionContentTypeProvider();
                typeProvider.TryGetContentType(fileName, out string fileType);

                Response.Headers.Add("Content-Disposition", "inline; filename=" + fileName);
                return File(fileBytes, fileType);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error viewing file {file.Id} : {ex}", ex);
                return StatusCode(StatusCodes.Status404NotFound);
            }
        }
    }
}
